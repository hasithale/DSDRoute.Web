using DSDRoute.Attributes;
using DSDRoute.Data;
using DSDRoute.Hubs;
using DSDRoute.Models;
using DSDRoute.Models.ViewModels;
using DSDRoute.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DSDRoute.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<OrderHub> _hubContext;

        public OrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            
            var canViewAll = await _userManager.HasPermissionAsync(currentUser, Permissions.Orders_ViewAll);
            var canViewOwn = await _userManager.HasPermissionAsync(currentUser, Permissions.Orders_ViewOwn);

            if (!canViewAll && !canViewOwn)
            {
                return Forbid();
            }

            IQueryable<Order> ordersQuery = _context.Orders
                .Include(o => o.Shop)
                .Include(o => o.SalesRep)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product);

            // Users can only see their own orders unless they have ViewAll permission
            if (!canViewAll)
            {
                ordersQuery = ordersQuery.Where(o => o.SalesRepId == currentUser.Id);
            }

            // Apply date filters if provided
            if (startDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.OrderDate.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.OrderDate.Date <= endDate.Value.Date);
            }

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.CanViewAll = canViewAll;
            ViewBag.CanCreate = await _userManager.HasPermissionAsync(currentUser, Permissions.Orders_Create);
            ViewBag.CanEdit = await _userManager.HasPermissionAsync(currentUser, Permissions.Orders_Edit);
            ViewBag.CanDelete = await _userManager.HasPermissionAsync(currentUser, Permissions.Orders_Delete);
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            
            var canViewAll = await _userManager.HasPermissionAsync(currentUser, Permissions.Orders_ViewAll);
            var canViewOwn = await _userManager.HasPermissionAsync(currentUser, Permissions.Orders_ViewOwn);

            if (!canViewAll && !canViewOwn)
            {
                return Forbid();
            }

            var order = await _context.Orders
                .Include(o => o.Shop)
                .Include(o => o.SalesRep)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.Payments)
                .Include(o => o.Returns).ThenInclude(r => r.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            // Users can only view their own orders unless they have ViewAll permission
            if (!canViewAll && order.SalesRepId != currentUser.Id)
                return Forbid();

            return View(order);
        }

        [RequirePermission(Permissions.Orders_Create)]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var shops = await _context.Shops.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
            var products = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
            
            var salesRepName = "Unknown";
            if (!string.IsNullOrEmpty(currentUser.Email))
            {
                salesRepName = currentUser.Email.Split('@')[0];
            }
            else if (!string.IsNullOrEmpty(currentUser.UserName))
            {
                salesRepName = currentUser.UserName.Split('@')[0];
            }

            var viewModel = new CreateOrderViewModel
            {
                SalesRepId = currentUser.Id,
                SalesRepName = salesRepName,
                OrderNumber = await GenerateOrderNumber(),
                OrderTime = DateTime.Now.TimeOfDay, // Set current time when form loads
                Shops = new SelectList(shops, "Id", "Name"),
                Products = new SelectList(products, "Id", "Name")
            };
            
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permissions.Orders_Create)]
        public async Task<IActionResult> Create(CreateOrderViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Create the order
                    var order = new Order
                    {
                        ShopId = model.ShopId,
                        SalesRepId = currentUser.Id,
                        OrderDate = model.OrderDate.Add(model.OrderTime),
                        Status = OrderStatus.Pending,
                        Notes = model.Notes,
                        TotalAmount = model.NetTotal,
                        TaxPercentage = model.TaxPercentage,
                        InvoiceDiscount = model.InvoiceDiscount,
                        DeliveryDate = model.DeliveryDate
                    };

                    // Add order items and update product quantities
                    foreach (var item in model.OrderItems.Where(oi => oi.ProductId > 0 && oi.Quantity > 0))
                    {
                        // Check product availability and update stock
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product == null)
                        {
                            ModelState.AddModelError("", $"Product with ID {item.ProductId} not found.");
                            throw new InvalidOperationException($"Product with ID {item.ProductId} not found.");
                        }

                        if (product.StockQty < item.Quantity)
                        {
                            ModelState.AddModelError("", $"Insufficient stock for {product.Name}. Available: {product.StockQty}, Requested: {item.Quantity}");
                            throw new InvalidOperationException($"Insufficient stock for {product.Name}");
                        }

                        // Decrease product quantity
                        product.StockQty -= item.Quantity;
                        _context.Products.Update(product);

                        order.OrderItems.Add(new OrderItem
                        {
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = item.UnitPrice
                        });
                    }

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    // Add payment if any
                    if (model.TodayPayment > 0)
                    {
                        var paymentType = model.PaymentMode switch
                        {
                            Models.ViewModels.PaymentMode.Cash => PaymentType.Cash,
                            Models.ViewModels.PaymentMode.Credit => PaymentType.Credit,
                            Models.ViewModels.PaymentMode.Card => PaymentType.Cash, // Treating card as cash for now
                            Models.ViewModels.PaymentMode.BankTransfer => PaymentType.Cheque, // Treating bank transfer as cheque
                            _ => PaymentType.Cash
                        };

                        var payment = new Payment
                        {
                            OrderId = order.Id,
                            Amount = model.TodayPayment,
                            PaymentDate = DateTime.Now,
                            PaymentType = paymentType,
                            RecordedById = currentUser.Id,
                            IsVerified = true
                        };
                        _context.Payments.Add(payment);
                    }

                    // Add returns if any and update return items table with invoice details
                    foreach (var returnItem in model.ReturnItems.Where(ri => ri.ProductId > 0 && ri.Quantity > 0))
                    {
                        var returnRecord = new Return
                        {
                            OrderId = order.Id,
                            ShopId = model.ShopId,
                            ProductId = returnItem.ProductId,
                            Quantity = returnItem.Quantity,
                            Reason = returnItem.ReturnReason == Models.ViewModels.ReturnReason.Other 
                                ? (returnItem.CustomReason ?? "Other") 
                                : returnItem.ReturnReason.ToString(),
                            RefundAmount = returnItem.ReturnAmount,
                            ProcessedById = currentUser.Id,
                            ReturnDate = DateTime.Now,
                            Status = ReturnStatus.Approved
                        };
                        _context.Returns.Add(returnRecord);

                        // Increase product quantity back for returns
                        var returnProduct = await _context.Products.FindAsync(returnItem.ProductId);
                        if (returnProduct != null)
                        {
                            returnProduct.StockQty += returnItem.Quantity;
                            _context.Products.Update(returnProduct);
                        }
                    }

                    // Handle credit bill logic
                    var totalPreviousCredit = await _context.CreditBills
                        .Where(cb => cb.ShopId == model.ShopId && !cb.IsSettled)
                        .SumAsync(cb => cb.CreditAmount);

                    var effectivePayment = model.TodayPayment;
                    var totalOwed = model.NetTotal;

                    if (effectivePayment >= totalOwed)
                    {
                        // Full payment: settle all previous credit bills
                        var previousCreditBills = await _context.CreditBills
                            .Where(cb => cb.ShopId == model.ShopId && !cb.IsSettled)
                            .ToListAsync();

                        foreach (var creditBill in previousCreditBills)
                        {
                            creditBill.IsSettled = true;
                            creditBill.SettledDate = DateTime.Now;
                            creditBill.Notes += $" | Settled with Order #{order.Id} by {currentUser.Email}";
                        }
                        _context.CreditBills.UpdateRange(previousCreditBills);
                    }
                    else
                    {
                        // Partial payment: create new credit bill for outstanding balance
                        var outstandingAmount = totalOwed - effectivePayment;
                        
                        if (outstandingAmount > 0)
                        {
                            var previousCreditBills = await _context.CreditBills
                            .Where(cb => cb.ShopId == model.ShopId && !cb.IsSettled)
                            .ToListAsync();

                            foreach (var creditBill1 in previousCreditBills)
                            {
                                creditBill1.IsSettled = true;
                                creditBill1.SettledDate = DateTime.Now;
                                creditBill1.Notes += $" | Balance added for Order #{order.Id} by {currentUser.Email}";
                            }
                            _context.CreditBills.UpdateRange(previousCreditBills);

                            var creditBill = new CreditBill
                            {
                                ShopId = model.ShopId,
                                InvoiceId = model.OrderNumber ?? $"ORD{order.Id}",
                                SalesRepId = currentUser.Id,
                                CreditAmount = outstandingAmount,
                                IsSettled = false,
                                CreatedDate = DateTime.Now,
                                Notes = $"Outstanding balance from Order #{order.Id}. Total owed: Rs. {totalOwed:0.00}, Paid: Rs. {effectivePayment:0.00}"
                            };
                            _context.CreditBills.Add(creditBill);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Notify admin via SignalR
                    await _hubContext.Clients.Group("Admin").SendAsync("NewOrderCreated", new
                    {
                        OrderId = order.Id,
                        ShopName = (await _context.Shops.FindAsync(order.ShopId))?.Name,
                        SalesRep = currentUser.Email,
                        TotalAmount = order.TotalAmount,
                        OrderDate = order.OrderDate
                    });

                    TempData["Success"] = "Order created successfully and submitted for approval.";
                    return RedirectToAction(nameof(Details), new { id = order.Id });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = $"Failed to create order: {ex.Message}";
                    
                    // Reload data for view
                    var reloadShops = await _context.Shops.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
                    var reloadProducts = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
                    
                    model.Shops = new SelectList(reloadShops, "Id", "Name");
                    model.Products = new SelectList(reloadProducts, "Id", "Name");
                    
                    return View(model);
                }
            }

            // Reload data for view if validation fails
            var shops = await _context.Shops.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
            var products = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
            
            model.Shops = new SelectList(shops, "Id", "Name");
            model.Products = new SelectList(products, "Id", "Name");
            
            return View(model);
        }

        [HttpPost]
        [RequirePermission(Permissions.Orders_Edit)]
        public async Task<IActionResult> Approve(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Shop)
                .Include(o => o.SalesRep)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            order.Status = OrderStatus.Approved;
            order.ApprovalDate = DateTime.UtcNow;
            order.ApprovedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _context.SaveChangesAsync();

            // Notify sales rep
            await _hubContext.Clients.User(order.SalesRepId).SendAsync("OrderApproved", new
            {
                OrderId = order.Id,
                ShopName = order.Shop.Name,
                Message = "Your order has been approved and is ready for delivery."
            });

            TempData["Success"] = "Order approved successfully.";
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }

        [HttpPost]
        [RequirePermission(Permissions.Orders_Edit)]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            var order = await _context.Orders
                .Include(o => o.Shop)
                .Include(o => o.SalesRep)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            order.Status = OrderStatus.Rejected;
            order.RejectionReason = reason;

            await _context.SaveChangesAsync();

            // Notify sales rep
            await _hubContext.Clients.User(order.SalesRepId).SendAsync("OrderRejected", new
            {
                OrderId = order.Id,
                ShopName = order.Shop.Name,
                Reason = reason,
                Message = "Your order has been rejected."
            });

            TempData["Success"] = "Order rejected.";
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }

        [HttpPost]
        [RequirePermission(Permissions.Orders_Edit)]
        public async Task<IActionResult> MarkDelivered(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            
            var canViewAll = await _userManager.HasPermissionAsync(currentUser, Permissions.Orders_ViewAll);

            var order = await _context.Orders
                .Include(o => o.Shop)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            // Users can only mark their own orders as delivered unless they have ViewAll permission
            if (!canViewAll && order.SalesRepId != currentUser.Id)
                return Forbid();

            if (order.Status != OrderStatus.Approved)
            {
                TempData["Error"] = "Only approved orders can be marked as delivered.";
                return RedirectToAction(nameof(Details), new { id = order.Id });
            }

            order.Status = OrderStatus.Delivered;
            order.DeliveryDate = DateTime.UtcNow;
            order.DeliveredById = currentUser.Id;

            await _context.SaveChangesAsync();

            // Notify admin if delivered by sales rep
            if (!canViewAll)
            {
                await _hubContext.Clients.Group("Admin").SendAsync("OrderDelivered", new
                {
                    OrderId = order.Id,
                    ShopName = order.Shop.Name,
                    SalesRep = currentUser.Email,
                    DeliveryDate = order.DeliveryDate
                });
            }

            TempData["Success"] = "Order marked as delivered.";
            return RedirectToAction(nameof(Details), new { id = order.Id });
        }

        [HttpGet]
        public async Task<IActionResult> GetProductInfo(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound();

            return Json(new
            {
                id = product.Id,
                name = product.Name,
                unitPrice = product.Price,
                stockQty = product.StockQty,
                category = product.Category
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetShopInfo(int shopId)
        {
            var shop = await _context.Shops.FindAsync(shopId);
            if (shop == null)
                return NotFound();

            // Get previous unsettled credit bills
            var creditAmount = await _context.CreditBills
                .Where(cb => cb.ShopId == shopId && !cb.IsSettled)
                .SumAsync(cb => cb.CreditAmount);

            return Json(new
            {
                id = shop.Id,
                name = shop.Name,
                location = shop.Location,
                contact = shop.Contact,
                email = shop.Email,
                previousCreditAmount = creditAmount
            });
        }

        private async Task<string> GenerateOrderNumber()
        {
            var today = DateTime.Today;
            var todayOrderCount = await _context.Orders
                .Where(o => o.OrderDate.Date == today)
                .CountAsync();

            return $"ORD{today:yyyyMMdd}{(todayOrderCount + 1):D3}";
        }

        [HttpGet]
        public async Task<IActionResult> Print(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            var order = await _context.Orders
                .Include(o => o.Shop)
                .Include(o => o.SalesRep)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.Payments)
                .Include(o => o.Returns).ThenInclude(r => r.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            // Non-admin users can only print their own orders
            if (!isAdmin && order.SalesRepId != currentUser.Id)
                return Forbid();

            // Get previous credit bills amount for this shop (settled ones that were active when this order was created)
            var previousCreditAmount = await _context.CreditBills
                .Where(cb => cb.ShopId == order.ShopId && 
                            cb.CreatedDate < order.OrderDate &&
                            cb.IsSettled && 
                            cb.SettledDate >= order.OrderDate)
                .SumAsync(cb => cb.CreditAmount);

            ViewBag.IsPrintView = true;
            ViewBag.PreviousCreditAmount = previousCreditAmount;
            return View("PrintOrder", order);
        }

        [HttpGet]
        public async Task<IActionResult> PrintAll(DateTime? startDate, DateTime? endDate)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            
            var canViewAll = await _userManager.HasPermissionAsync(currentUser, Permissions.Orders_ViewAll);

            IQueryable<Order> ordersQuery = _context.Orders
                .Include(o => o.Shop)
                .Include(o => o.SalesRep)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .Include(o => o.Payments)
                .Include(o => o.Returns).ThenInclude(r => r.Product);

            if (!canViewAll)
            {
                ordersQuery = ordersQuery.Where(o => o.SalesRepId == currentUser.Id);
            }

            // Apply date filters if provided
            if (startDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.OrderDate.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.OrderDate.Date <= endDate.Value.Date);
            }

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.IsPrintView = true;
            ViewBag.CanViewAll = canViewAll;
            return View("PrintAllOrders", orders);
        }

        // API endpoint for mobile app
        [HttpGet]
        [Route("api/orders")]
        public async Task<IActionResult> GetOrdersApi()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            IQueryable<Order> ordersQuery = _context.Orders
                .Include(o => o.Shop)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Product);

            if (!isAdmin)
            {
                ordersQuery = ordersQuery.Where(o => o.SalesRepId == currentUser.Id);
            }

            var orders = await ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new
                {
                    o.Id,
                    o.OrderDate,
                    o.Status,
                    o.TotalAmount,
                    ShopName = o.Shop.Name,
                    ItemCount = o.OrderItems.Count
                })
                .ToListAsync();

            return Json(orders);
        }
    }
}

using DSDRoute.Data;
using DSDRoute.Hubs;
using DSDRoute.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DSDRoute.Controllers
{
    [Authorize]
    public class ReturnController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<OrderHub> _hubContext;

        public ReturnController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHubContext<OrderHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            IQueryable<Return> returnsQuery = _context.Returns
                .Include(r => r.Shop)
                .Include(r => r.Product)
                .Include(r => r.Order).ThenInclude(o => o!.SalesRep);

            if (!isAdmin)
            {
                returnsQuery = returnsQuery.Where(r => r.Order != null && r.Order.SalesRepId == currentUser.Id);
            }

            var returns = await returnsQuery
                .OrderByDescending(r => r.ReturnDate)
                .ToListAsync();

            ViewBag.IsAdmin = isAdmin;
            return View(returns);
        }

        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            var returnItem = await _context.Returns
                .Include(r => r.Shop)
                .Include(r => r.Product)
                .Include(r => r.Order).ThenInclude(o => o!.SalesRep)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (returnItem == null)
                return NotFound();

            // Non-admin users can only view their own returns
            if (!isAdmin && (returnItem.Order == null || returnItem.Order.SalesRepId != currentUser.Id))
                return Forbid();

            return View(returnItem);
        }

        [Authorize(Roles = "Admin,SalesRep")]
        public async Task<IActionResult> Create(int? orderId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            // Get shops
            var shops = await _context.Shops.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
            ViewBag.Shops = shops;

            // Get products
            var products = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
            ViewBag.Products = products;

            var returnItem = new Return();

            if (orderId.HasValue)
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId.Value);

                if (order != null && (isAdmin || order.SalesRepId == currentUser.Id))
                {
                    returnItem.OrderId = orderId.Value;
                    returnItem.ShopId = order.ShopId;
                    ViewBag.Order = order;
                    ViewBag.OrderItems = order.OrderItems;
                }
            }

            return View(returnItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SalesRep")]
        public async Task<IActionResult> Create(Return returnItem)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            if (ModelState.IsValid)
            {
                // Verify order belongs to current user if not admin
                if (returnItem.OrderId.HasValue)
                {
                    var order = await _context.Orders.FindAsync(returnItem.OrderId.Value);
                    if (order != null && !isAdmin && order.SalesRepId != currentUser.Id)
                        return Forbid();
                }

                returnItem.ReturnDate = DateTime.UtcNow;
                returnItem.ProcessedById = currentUser.Id;
                returnItem.Status = ReturnStatus.Pending;

                _context.Returns.Add(returnItem);
                await _context.SaveChangesAsync();

                // Update product stock
                var product = await _context.Products.FindAsync(returnItem.ProductId);
                if (product != null)
                {
                    product.StockQty += returnItem.Quantity;
                    await _context.SaveChangesAsync();
                }

                // Notify admin if return created by sales rep
                if (!isAdmin)
                {
                    await _hubContext.Clients.Group("Admin").SendAsync("ReturnCreated", new
                    {
                        ReturnId = returnItem.Id,
                        ShopName = (await _context.Shops.FindAsync(returnItem.ShopId))?.Name,
                        ProductName = product?.Name,
                        Quantity = returnItem.Quantity,
                        SalesRep = currentUser.Email,
                        ReturnDate = returnItem.ReturnDate
                    });
                }

                TempData["Success"] = "Return recorded successfully.";
                return RedirectToAction(nameof(Details), new { id = returnItem.Id });
            }

            // Reload data for view
            var shops = await _context.Shops.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();
            var products = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
            
            ViewBag.Shops = shops;
            ViewBag.Products = products;

            if (returnItem.OrderId.HasValue)
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                    .FirstOrDefaultAsync(o => o.Id == returnItem.OrderId.Value);

                if (order != null && (isAdmin || order.SalesRepId == currentUser.Id))
                {
                    ViewBag.Order = order;
                    ViewBag.OrderItems = order.OrderItems;
                }
            }

            return View(returnItem);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var returnItem = await _context.Returns
                .Include(r => r.Shop)
                .Include(r => r.Product)
                .Include(r => r.Order).ThenInclude(o => o!.SalesRep)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (returnItem == null)
                return NotFound();

            returnItem.Status = ReturnStatus.Approved;
            returnItem.ApprovedDate = DateTime.UtcNow;
            returnItem.ApprovedById = User.FindFirstValue(ClaimTypes.NameIdentifier);

            await _context.SaveChangesAsync();

            // Notify sales rep if order exists
            if (returnItem.Order?.SalesRep != null)
            {
                await _hubContext.Clients.User(returnItem.Order.SalesRepId).SendAsync("ReturnApproved", new
                {
                    ReturnId = returnItem.Id,
                    ShopName = returnItem.Shop.Name,
                    ProductName = returnItem.Product.Name,
                    Quantity = returnItem.Quantity,
                    Message = "Your return has been approved."
                });
            }

            TempData["Success"] = "Return approved successfully.";
            return RedirectToAction(nameof(Details), new { id = returnItem.Id });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            var returnItem = await _context.Returns
                .Include(r => r.Shop)
                .Include(r => r.Product)
                .Include(r => r.Order).ThenInclude(o => o!.SalesRep)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (returnItem == null)
                return NotFound();

            returnItem.Status = ReturnStatus.Rejected;
            returnItem.RejectionReason = reason;

            // Reverse stock update
            var product = await _context.Products.FindAsync(returnItem.ProductId);
            if (product != null)
            {
                product.StockQty -= returnItem.Quantity;
                await _context.SaveChangesAsync();
            }

            await _context.SaveChangesAsync();

            // Notify sales rep if order exists
            if (returnItem.Order?.SalesRep != null)
            {
                await _hubContext.Clients.User(returnItem.Order.SalesRepId).SendAsync("ReturnRejected", new
                {
                    ReturnId = returnItem.Id,
                    ShopName = returnItem.Shop.Name,
                    ProductName = returnItem.Product.Name,
                    Reason = reason,
                    Message = "Your return has been rejected."
                });
            }

            TempData["Success"] = "Return rejected.";
            return RedirectToAction(nameof(Details), new { id = returnItem.Id });
        }

        // API endpoint for mobile app
        [HttpGet]
        [Route("api/returns")]
        public async Task<IActionResult> GetReturnsApi()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            IQueryable<Return> returnsQuery = _context.Returns
                .Include(r => r.Shop)
                .Include(r => r.Product)
                .Include(r => r.Order);

            if (!isAdmin)
            {
                returnsQuery = returnsQuery.Where(r => r.Order != null && r.Order.SalesRepId == currentUser.Id);
            }

            var returns = await returnsQuery
                .OrderByDescending(r => r.ReturnDate)
                .Select(r => new
                {
                    r.Id,
                    r.ReturnDate,
                    r.Quantity,
                    r.Status,
                    r.Reason,
                    ShopName = r.Shop.Name,
                    ProductName = r.Product.Name,
                    OrderId = r.OrderId
                })
                .ToListAsync();

            return Json(returns);
        }
    }
}
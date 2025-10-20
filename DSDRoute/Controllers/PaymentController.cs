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
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<OrderHub> _hubContext;

        public PaymentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IHubContext<OrderHub> hubContext)
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

            IQueryable<Payment> paymentsQuery = _context.Payments
                .Include(p => p.Order).ThenInclude(o => o.Shop)
                .Include(p => p.Order).ThenInclude(o => o.SalesRep);

            if (!isAdmin)
            {
                paymentsQuery = paymentsQuery.Where(p => p.Order.SalesRepId == currentUser.Id);
            }

            var payments = await paymentsQuery
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            ViewBag.IsAdmin = isAdmin;
            return View(payments);
        }

        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            var payment = await _context.Payments
                .Include(p => p.Order).ThenInclude(o => o.Shop)
                .Include(p => p.Order).ThenInclude(o => o.SalesRep)
                .Include(p => p.Order).ThenInclude(o => o.OrderItems).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
                return NotFound();

            // Non-admin users can only view their own payments
            if (!isAdmin && payment.Order.SalesRepId != currentUser.Id)
                return Forbid();

            return View(payment);
        }

        [Authorize(Roles = "Admin,SalesRep")]
        public async Task<IActionResult> Create(int orderId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            var order = await _context.Orders
                .Include(o => o.Shop)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return NotFound();

            // Non-admin users can only create payments for their own orders
            if (!isAdmin && order.SalesRepId != currentUser.Id)
                return Forbid();

            // Check if order is delivered
            if (order.Status != OrderStatus.Delivered)
            {
                TempData["Error"] = "Payments can only be recorded for delivered orders.";
                return RedirectToAction("Details", "Order", new { id = orderId });
            }

            var totalPaid = order.Payments.Sum(p => p.Amount);
            var remainingAmount = order.TotalAmount - totalPaid;

            if (remainingAmount <= 0)
            {
                TempData["Error"] = "This order has been fully paid.";
                return RedirectToAction("Details", "Order", new { id = orderId });
            }

            var payment = new Payment
            {
                OrderId = orderId,
                Amount = remainingAmount,
                PaymentDate = DateTime.Now
            };

            ViewBag.Order = order;
            ViewBag.RemainingAmount = remainingAmount;
            
            return View(payment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SalesRep")]
        public async Task<IActionResult> Create(Payment payment)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            var order = await _context.Orders
                .Include(o => o.Shop)
                .Include(o => o.SalesRep)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == payment.OrderId);

            if (order == null)
                return NotFound();

            // Non-admin users can only create payments for their own orders
            if (!isAdmin && order.SalesRepId != currentUser.Id)
                return Forbid();

            if (ModelState.IsValid)
            {
                var totalPaid = order.Payments.Sum(p => p.Amount);
                var remainingAmount = order.TotalAmount - totalPaid;

                if (payment.Amount > remainingAmount)
                {
                    ModelState.AddModelError("Amount", $"Payment amount cannot exceed remaining balance of Rs. {remainingAmount:0.00}");
                }
                else
                {
                    payment.RecordedById = currentUser.Id;
                    payment.PaymentDate = DateTime.UtcNow;
                    
                    _context.Payments.Add(payment);
                    await _context.SaveChangesAsync();

                    // Notify admin if payment recorded by sales rep
                    if (!isAdmin)
                    {
                        await _hubContext.Clients.Group("Admin").SendAsync("PaymentRecorded", new
                        {
                            PaymentId = payment.Id,
                            OrderId = order.Id,
                            ShopName = order.Shop.Name,
                            Amount = payment.Amount,
                            SalesRep = currentUser.Email,
                            PaymentDate = payment.PaymentDate
                        });
                    }

                    TempData["Success"] = "Payment recorded successfully.";
                    return RedirectToAction(nameof(Details), new { id = payment.Id });
                }
            }

            // Reload data for view
            var totalPaidReload = order.Payments.Sum(p => p.Amount);
            var remainingAmountReload = order.TotalAmount - totalPaidReload;
            
            ViewBag.Order = order;
            ViewBag.RemainingAmount = remainingAmountReload;
            
            return View(payment);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Verify(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Order).ThenInclude(o => o.SalesRep)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (payment == null)
                return NotFound();

            payment.IsVerified = true;
            payment.VerificationDate = DateTime.UtcNow;
            payment.VerifiedById = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);

            await _context.SaveChangesAsync();

            // Notify sales rep
            await _hubContext.Clients.User(payment.Order.SalesRepId).SendAsync("PaymentVerified", new
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                Message = "Your payment record has been verified."
            });

            TempData["Success"] = "Payment verified successfully.";
            return RedirectToAction(nameof(Details), new { id = payment.Id });
        }

        // API endpoint for mobile app
        [HttpGet]
        [Route("api/payments")]
        public async Task<IActionResult> GetPaymentsApi()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            IQueryable<Payment> paymentsQuery = _context.Payments
                .Include(p => p.Order).ThenInclude(o => o.Shop);

            if (!isAdmin)
            {
                paymentsQuery = paymentsQuery.Where(p => p.Order.SalesRepId == currentUser.Id);
            }

            var payments = await paymentsQuery
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new
                {
                    p.Id,
                    p.Amount,
                    p.PaymentDate,
                    p.PaymentType,
                    p.IsVerified,
                    OrderId = p.Order.Id,
                    ShopName = p.Order.Shop.Name
                })
                .ToListAsync();

            return Json(payments);
        }
    }
}

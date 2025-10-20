using DSDRoute.Attributes;
using DSDRoute.Data;
using DSDRoute.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DSDRoute.Controllers
{
    [RequirePermission(Permissions.Admin_FullAccess)]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var users = await _userManager.Users.ToListAsync();
            
            var dashboardData = new AdminDashboardViewModel
            {
                TotalShops = await _context.Shops.CountAsync(s => s.IsActive),
                TotalProducts = await _context.Products.CountAsync(p => p.IsActive),
                PendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending),
                TotalSalesReps = (await _userManager.GetUsersInRoleAsync("User")).Count(u => ((ApplicationUser)u).IsActive),
                RecentOrders = await _context.Orders
                    .Include(o => o.Shop)
                    .Include(o => o.SalesRep)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(10)
                    .ToListAsync(),
                TodaysOrderValue = await _context.Orders
                    .Where(o => o.OrderDate.Date == DateTime.Today)
                    .SumAsync(o => o.TotalAmount),
                OutstandingCredits = await _context.CreditBills
                    .Where(cb => !cb.IsSettled)
                    .SumAsync(cb => cb.CreditAmount),
                Users = users
            };

            return View(dashboardData);
        }

        [HttpGet]
        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                IsActive = true
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                TempData["Success"] = "User created successfully.";
                return RedirectToAction("Dashboard");
            }
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email!,
                IsActive = user.IsActive,
                Role = roles.FirstOrDefault() ?? "User"
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            user.Email = model.Email;
            user.UserName = model.Email;
            user.IsActive = model.IsActive;
            await _userManager.UpdateAsync(user);

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(model.Role))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            TempData["Success"] = "User updated successfully.";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> DeactivateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.IsActive = false;
            await _userManager.UpdateAsync(user);
            TempData["Success"] = "User deactivated successfully.";
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var model = new ResetPasswordViewModel { Id = user.Id, Email = user.Email! };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["Success"] = "Password reset successfully.";
                return RedirectToAction("Dashboard");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
    }
}
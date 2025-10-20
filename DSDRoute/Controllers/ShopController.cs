using DSDRoute.Attributes;
using DSDRoute.Data;
using DSDRoute.Models;
using DSDRoute.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DSDRoute.Controllers
{
    [Authorize]
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShopController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [RequirePermission(Permissions.Shops_View)]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var shops = await _context.Shops
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();
            
            ViewBag.CanAdd = await _userManager.HasPermissionAsync(currentUser, Permissions.Shops_Add);
            ViewBag.CanEdit = await _userManager.HasPermissionAsync(currentUser, Permissions.Shops_Edit);
            ViewBag.CanDelete = await _userManager.HasPermissionAsync(currentUser, Permissions.Shops_Delete);
            
            return View(shops);
        }

        [RequirePermission(Permissions.Shops_View)]
        public async Task<IActionResult> Details(int id)
        {
            var shop = await _context.Shops
                .Include(s => s.Orders).ThenInclude(o => o.SalesRep)
                .Include(s => s.CreditBills)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (shop == null)
                return NotFound();

            return View(shop);
        }

        [RequirePermission(Permissions.Shops_Add)]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permissions.Shops_Add)]
        public async Task<IActionResult> Create(Shop shop)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                shop.CreatedBy = user?.Id ?? "";
                shop.CreatedDate = DateTime.UtcNow;
                
                _context.Shops.Add(shop);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Shop created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(shop);
        }

        [RequirePermission(Permissions.Shops_Edit)]
        [RequirePermission(Permissions.Shops_Edit)]
        public async Task<IActionResult> Edit(int id)
        {
            var shop = await _context.Shops.FindAsync(id);
            if (shop == null)
                return NotFound();

            return View(shop);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permissions.Shops_Edit)]
        public async Task<IActionResult> Edit(int id, Shop shop)
        {
            if (id != shop.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shop);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Shop updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShopExists(shop.Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(shop);
        }

        [HttpPost]
        [RequirePermission(Permissions.Shops_Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var shop = await _context.Shops.FindAsync(id);
            if (shop != null)
            {
                shop.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Shop deactivated successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permissions.Shops_Edit)]
        public async Task<IActionResult> Activate(int id)
        {
            var shop = await _context.Shops.FindAsync(id);
            if (shop != null)
            {
                shop.IsActive = true;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Shop activated successfully.";
            }
            return RedirectToAction(nameof(Details), new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(Permissions.Shops_Edit)]
        public async Task<IActionResult> Deactivate(int id)
        {
            var shop = await _context.Shops.FindAsync(id);
            if (shop != null)
            {
                shop.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Shop deactivated successfully.";
            }
            return RedirectToAction(nameof(Details), new { id = id });
        }

        private bool ShopExists(int id)
        {
            return _context.Shops.Any(e => e.Id == id);
        }
    }
}

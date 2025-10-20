using DSDRoute.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace DSDRoute.Services
{
    public static class PermissionExtensions
    {
        public static async Task<bool> HasPermissionAsync(this UserManager<ApplicationUser> userManager, ApplicationUser user, string permission)
        {
            var roles = await userManager.GetRolesAsync(user);
            
            foreach (var role in roles)
            {
                var rolePermissions = Permissions.GetPermissionsForRole(role);
                if (rolePermissions.Contains(permission))
                {
                    return true;
                }
            }
            
            return false;
        }

        public static async Task<bool> HasPermissionAsync(this UserManager<ApplicationUser> userManager, ClaimsPrincipal user, string permission)
        {
            var applicationUser = await userManager.GetUserAsync(user);
            if (applicationUser == null) return false;
            
            return await userManager.HasPermissionAsync(applicationUser, permission);
        }

        public static async Task<List<string>> GetUserPermissionsAsync(this UserManager<ApplicationUser> userManager, ApplicationUser user)
        {
            var roles = await userManager.GetRolesAsync(user);
            var permissions = new HashSet<string>();
            
            foreach (var role in roles)
            {
                var rolePermissions = Permissions.GetPermissionsForRole(role);
                foreach (var permission in rolePermissions)
                {
                    permissions.Add(permission);
                }
            }
            
            return permissions.ToList();
        }
    }
}
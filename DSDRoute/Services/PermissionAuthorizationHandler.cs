using DSDRoute.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace DSDRoute.Services
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PermissionAuthorizationHandler(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User == null || !context.User.Identity!.IsAuthenticated)
            {
                context.Fail();
                return;
            }

            var user = await _userManager.GetUserAsync(context.User);
            if (user == null)
            {
                context.Fail();
                return;
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var hasPermission = false;

            foreach (var role in userRoles)
            {
                var rolePermissions = Permissions.GetPermissionsForRole(role);
                if (rolePermissions.Contains(requirement.Permission))
                {
                    hasPermission = true;
                    break;
                }
            }

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
    }

    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string Permission { get; }

        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }
    }
}
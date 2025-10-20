using Microsoft.AspNetCore.Authorization;

namespace DSDRoute.Attributes
{
    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        public RequirePermissionAttribute(string permission)
        {
            Policy = permission;
        }
    }
}
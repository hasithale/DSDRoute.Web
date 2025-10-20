using Microsoft.AspNetCore.Identity;

namespace DSDRoute.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsActive { get; set; } = true;
    }
}
using System.ComponentModel.DataAnnotations;

namespace DSDRoute.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email or Username")]
        public string EmailOrUsername { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }
}
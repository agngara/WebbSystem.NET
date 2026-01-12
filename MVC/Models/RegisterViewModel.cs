
using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{

    public class RegisterViewModel
    {

        [Required(ErrorMessage = "Please Enter a Username")]
        [StringLength(30)]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username can only contain letters and numbers")]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "Please Enter a Name")]
        [StringLength(30)]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Name can only contain letters and numbers")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Please Enter a Address")]
        [StringLength(50)]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Please Enter a Password")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Please confirm Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password")]
        public required string ConfirmPassword { get; set; }
    }
}


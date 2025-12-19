
using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{

    public class RegisterViewModel
    {

        [Required(ErrorMessage = "Please Enter a Username")]
        [StringLength(30)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please Enter a Name")]
        [StringLength(30)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please Enter a Address")]
        [StringLength(50)]
        public string Address { get; set; }

        [Required(ErrorMessage = "Please Enter a Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please confirm Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}


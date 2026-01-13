using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace MVC.Models
{
    public class SettingsViewModel
    {
        [StringLength(30)]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Name can only contain letters and numbers")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Please Enter an Email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100)]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Please Enter a Address")]
        [StringLength(50)]
        public required string Address { get; set; }

        public bool IsPrivate { get; set; }

        [StringLength(30)]
        [DataType(DataType.Password)]
        [Display(Name = "Update Password")]
        public string? Password { get; set; }

    }
}

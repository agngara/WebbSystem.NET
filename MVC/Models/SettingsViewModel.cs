using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace MVC.Models
{
    public class SettingsViewModel
    {
        [StringLength(30)]
        public string? Name { get; set; }

        [StringLength(50)]
        public string? Address { get; set; }

        public bool IsPrivate { get; set; }

        [StringLength(30)]
        [DataType(DataType.Password)]
        [Display(Name = "Update Password")]
        public string? Password { get; set; }

    }
}

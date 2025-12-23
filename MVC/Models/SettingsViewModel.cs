

using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{

    public class SettingsViewModel
    {
        [StringLength(30)]
        public required string Name { get; set; }

        [StringLength(50)]
        public required string Address { get; set; }

        public bool IsPrivate { get; set; }

        [DataType(DataType.Password)]
        public required string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}

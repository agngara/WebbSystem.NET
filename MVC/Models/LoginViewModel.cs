
using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{

    public class LoginViewModel
    {

        [Required(ErrorMessage = "Please Enter a Username")]
        [StringLength(30)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please Enter a Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}

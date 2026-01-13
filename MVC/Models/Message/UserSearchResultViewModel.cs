using System.ComponentModel.DataAnnotations;

namespace MVC.Models.Message
{
    public class UserSearchResultViewModel
    {
        [Required]

        public string UserId { get; set; } = null!;

        [Required(ErrorMessage = "Enter a name")]
        public string DisplayName { get; set; } = null!;
        public string? Email { get; set; }

        
    }
}

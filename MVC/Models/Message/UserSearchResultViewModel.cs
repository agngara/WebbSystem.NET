namespace MVC.Models.Message
{
    public class UserSearchResultViewModel
    {
        public string UserId { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? Email { get; set; }
    }
}

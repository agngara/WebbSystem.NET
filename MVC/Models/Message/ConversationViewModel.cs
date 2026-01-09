namespace MVC.Models.Message
{
    public class ConversationViewModel
    {
        public bool IsGuest { get; set; }

        public string? SearchTerm { get; set; }

        public List<UserSearchResultViewModel> UserResults { get; set; } = new();

        public List<ChatViewModel> Items { get; set; } = new();
    }
}

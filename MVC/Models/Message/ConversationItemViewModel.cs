namespace MVC.Models.Message
{
    public class ConversationItemViewModel
    {
        public bool IsGuest { get; set; }

        public int? GuestId { get; set; }
        public string? GuestName { get; set; }

        public int? ConversationId { get; set; }

        public string? OtherUserName { get; set; }
        public string? OtherUserEmail { get; set; }

        public string? LastMessageText { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public string? OtherUserProfileImageUrl { get; set; }
        public int UnreadMessage { get; set; }
    }
}

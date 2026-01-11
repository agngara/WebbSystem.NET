namespace MVC.Models.Message
{
    public class MessageBubbleViewModel
    {
        public int MessageId { get; set; }
        public bool IsMine { get; set; }
        public string Text { get; set; }
        public DateTime SentAt { get; set; }
        public string? GuestName { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }
}

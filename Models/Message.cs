
namespace Models
{
    public class Message
    {
        public int Id { get; set; }

        public string? SenderId { get; set; }  // string because references User.Id
        public User? Sender { get; set; }

        public string ReceiverId { get; set; } // string because references User.Id
        public User Receiver { get; set; }

        public string? SenderName { get; set; }

        public string Content { get; set; }

        public bool IsRead { get; set; }

        public DateTime SendDateTime { get; set; }
    }
}

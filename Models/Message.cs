
namespace Models
{
    public class Message
    {
        public int Id { get; set; }

        //string cuz Userid
        public string? SenderId { get; set; }
        public User? Sender { get; set; }

        //string cuz Userid
        public required string ReceiverId { get; set; }
        public required User Receiver { get; set; }

        public required string SenderName { get; set; }

        public required string Content { get; set; }

        //default value is false so no ? or required
        public bool IsRead { get; set; }

        public DateTime? SendDateTime { get; set; }
    }
}

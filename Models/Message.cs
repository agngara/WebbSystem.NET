
using Microsoft.VisualBasic;

namespace Models
{
    public class Message
    {
        public int Id { get; set; }
        public required string Text { get; set; }
        public DateTime SendDateTime { get; set; }

        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }

        //string cuz Userid
        public string? SenderId { get; set; }
        public User? Sender { get; set; }

        //string cuz Userid


        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }



    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models
{
    public class Message
    {
        public int Id { get; set; }
        public bool IsRead { get; set; }
        public string? SenderName { get; set; }
        public int? SenderId { get; set; }
        [ForeignKey(nameof(SenderId))]
        public User? Sender { get; set; }  
        public int ReceiverId { get; set; }
        [ForeignKey(nameof(ReceiverId))]
        public User Receiver { get; set; }
        public string Content { get; set; }
        public DateTime SendDateTime {  get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class ConversationUser
    {
        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
    }
}

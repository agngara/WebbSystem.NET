using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class Conversation
    {
        public int Id { get; set; }

        public virtual List<Message> Messages { get; set; } = [];
        public virtual List<ConversationUser> Participants { get; set; } = [];
    }
}

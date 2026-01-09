using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class GuestMessage
    {
        public int Id { get; set; }

        public string ToUserId { get; set; }

        public User ToUser { get; set; }

        public string GuestName { get; set; }

        public string Text { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; }

        public DateTime? ReadAt { get; set; }
    }
}

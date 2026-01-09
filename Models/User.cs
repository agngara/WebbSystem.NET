using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class User : IdentityUser
    {
        public required string Name { get; set; }

        public string? Address { get; set; }

        public bool? IsPrivate { get; set; }

        public bool IsActive { get; set; } = true;

        public int? CvId { get; set; }
        [ForeignKey(nameof(CvId))]
        public CV? Cv { get; set; }

        public List<Message> SentMessages { get; set; } = [];
        public List<ConversationUser> ConversationUsers { get; set; } = [];
        public virtual List<ProjectUser> ProjectUsers { get; set; } = [];
    }
}

using Microsoft.AspNetCore.Identity;

namespace Models
{
    public class User : IdentityUser
    {
        public required string Name { get; set; }

        public string? Address { get; set; }

        public bool? IsPrivate { get; set; }

        public CV? Cv { get; set; }

        public List<Message> SentMessages { get; set; } = [];
        public List<Message> ReceivedMessages { get; set; } = [];
        public virtual List<ProjectUser> ProjectUsers { get; set; } = [];
    }
}

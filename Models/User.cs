using Microsoft.AspNetCore.Identity;

namespace Models
{
    public class User : IdentityUser
    {
        public string Name { get; set; }

        public string Adress { get; set; }

        public bool? IsPrivate { get; set; }

        public CV? Cv { get; set; }

        public List<Message> SentMessages { get; set; } = new List<Message>();
        public List<Message> ReceivedMessages { get; set; } = new List<Message>();
        public virtual List<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();
    }
}

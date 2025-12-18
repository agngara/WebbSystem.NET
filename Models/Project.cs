using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Project
    {
        public int Id { get; set; }

        public string CreatorId { get; set; }

        [ForeignKey(nameof(CreatorId))]
        public User Creator { get; set; }

        public string Titel { get; set; }

        public string Description { get; set; }

        public virtual List<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();

    }
}

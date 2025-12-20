using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Project
    {
        public int Id { get; set; }

        //Required just change it to someone else if needed
        public required string CreatorId { get; set; }

        [ForeignKey(nameof(CreatorId))]
        public required User Creator { get; set; }

        public required string Titel { get; set; }

        public string? Description { get; set; }

        public virtual List<ProjectUser> ProjectUsers { get; set; } = [];

    }
}

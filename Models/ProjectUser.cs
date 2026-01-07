using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class ProjectUser
    {
        public required int ProjectId { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public  Project Project { get; set; }

        public required string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }
    }
}

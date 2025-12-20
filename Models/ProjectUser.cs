using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class ProjectUser
    {
        public required int ProjectId { get; set; }

        [ForeignKey(nameof(ProjectId))]
        public required Project Project { get; set; }

        public required string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public required User User { get; set; }
    }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class CV
    {
        public int Id { get; set; }

        public required string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public required User User { get; set; }

        public string? Description { get; set; }

        public string? ProfileImage { get; set; }

        public int ProfileViews { get; set; } = 0;

        public virtual List<WorkExperience> WorkExperiences { get; set; } = [];
        public virtual List<Competence> Competences { get; set; } = [];
        public virtual List<Education> Educations { get; set; } = [];
    }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class CV
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public string? Description { get; set; }

        public string? ProfileImage { get; set; }

        public int ProfileViews { get; set; } = 0;

        public virtual List<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();
        public virtual List<Competence> Competences { get; set; } = new List<Competence>();
        public virtual List<Education> Educations { get; set; } = new List<Education>();
    }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Education
    {
        public int Id { get; set; }

        public string ProgramOrCourseName { get; set; }

        public int InstitutionId { get; set; }

        [ForeignKey(nameof(InstitutionId))]
        public Institution Institution { get; set; }

        public string? Description { get; set; }

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public int CvId { get; set; }

        [ForeignKey(nameof(CvId))]
        public CV Cv { get; set; }
    }
}

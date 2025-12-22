using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class WorkExperience
    {
        public int Id { get; set; }

        public required string Title { get; set; }

        public string? Description { get; set; }

        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        public int? WorkPlaceId { get; set; }

        [ForeignKey(nameof(WorkPlaceId))]
        public WorkPlace? WorkPlace { get; set; }

        public required int CvId { get; set; }

        [ForeignKey(nameof(CvId))]
        public required CV Cv { get; set; }
    }
}

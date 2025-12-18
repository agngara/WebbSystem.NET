using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class WorkExperience
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public int WorkPlaceId { get; set; }

        [ForeignKey(nameof(WorkPlaceId))]
        public WorkPlace WorkPlace { get; set; }

        public int CvId { get; set; }

        [ForeignKey(nameof(CvId))]
        public CV Cv { get; set; }
    }
}

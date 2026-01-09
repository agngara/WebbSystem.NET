using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace Models
{
    public class WorkExperience
    {
        [XmlIgnore]
        public int Id { get; set; }

        public required string Title { get; set; }

        public string? Description { get; set; }

        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        [XmlIgnore]
        public int? WorkPlaceId { get; set; }

        [ForeignKey(nameof(WorkPlaceId))]
        public WorkPlace? WorkPlace { get; set; }

        [XmlIgnore]
        public required int CvId { get; set; }
        [XmlIgnore]
        [ForeignKey(nameof(CvId))]
        public required CV Cv { get; set; }
    }
}

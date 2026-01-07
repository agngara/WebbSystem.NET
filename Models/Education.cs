using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace Models
{
    public class Education
    {
        [XmlIgnore]
        public int Id { get; set; }

        public string? ProgramOrCourseName { get; set; }
        [XmlIgnore]
        public int? InstitutionId { get; set; }

        [ForeignKey(nameof(InstitutionId))]
        public Institution? Institution { get; set; }

        public string? Description { get; set; }

        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        [XmlIgnore]
        public required int CvId { get; set; }

        [ForeignKey(nameof(CvId))]
        [XmlIgnore]
        public required CV Cv { get; set; }
    }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace Models
{
    public class Competence
    {
        [XmlIgnore]
        public int Id { get; set; }

        public required string Name { get; set; }
        [XmlIgnore]
        public required int CvId { get; set; }
        [XmlIgnore]
        [ForeignKey(nameof(CvId))]
        public required CV Cv { get; set; }
    }
}

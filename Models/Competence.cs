using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Competence
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public required int CvId { get; set; }

        [ForeignKey(nameof(CvId))]
        public required CV Cv { get; set; }
    }
}

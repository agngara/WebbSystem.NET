using System.ComponentModel.DataAnnotations.Schema;

namespace Models
{
    public class Competence
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int CvId { get; set; }

        [ForeignKey(nameof(CvId))]
        public CV Cv { get; set; }
    }
}

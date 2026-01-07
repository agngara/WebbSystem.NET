using Models;

namespace MVC.Models.Projects
{
    public class CvFileModel
    {
        public string Name {  get; set; }
        public String Description { get; set; }
        public virtual List<WorkExperience> WorkExperiences { get; set; } = [];
        public virtual List<Competence> Competences { get; set; } = [];
        public virtual List<Education> Educations { get; set; } = [];
    }
}

using Models;

namespace MVC.Models.CVs
{
    public class CvFileModel
    {
        public string Name {  get; set; }
        public String Description { get; set; }
        public virtual List<WorkExperienceFileModel> WorkExperiences { get; set; } = [];
        public virtual List<Competence> Competences { get; set; } = [];
        public virtual List<EducationFileModel> Educations { get; set; } = [];
    }
}

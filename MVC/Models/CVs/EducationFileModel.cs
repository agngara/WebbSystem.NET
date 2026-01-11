namespace MVC.Models.CVs
{
    public class EducationFileModel
    {

        public string? Description { get; set; }

        public string ProgramOrCourseName { get; set; }

        public string? InstitutionName { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

    }
}

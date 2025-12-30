// Models/CVViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class CvEditModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Beskrivning krävs")]
        [StringLength(500, ErrorMessage = "Beskrivning får max vara 500 tecken")]
        public string Description { get; set; } = string.Empty;

        public IFormFile? ProfileImageFile { get; set; }
        public string? ExistingProfileImage { get; set; }

        // Kompetenser
        public List<CompetenceViewModel> Competences { get; set; } = new();

        // Utbildningar
        public List<EducationViewModel> Educations { get; set; } = new();

        // Arbetserfarenheter
        public List<WorkExperienceViewModel> WorkExperiences { get; set; } = new();

        // Projekt
        public List<SelectedProjectViewModel> SelectedProjects { get; set; } = new();
        public List<ProjectOptionViewModel> AvailableProjects { get; set; } = new();
    }

    public class CompetenceViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Kompetensnamn krävs")]
        [StringLength(100, ErrorMessage = "Kompetensnamn får max vara 100 tecken")]
        public string Name { get; set; } = string.Empty;
    }

    public class EducationViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Program/kursnamn krävs")]
        [StringLength(100, ErrorMessage = "Program/kursnamn får max vara 100 tecken")]
        public string ProgramOrCourseName { get; set; } = string.Empty;

        public string? InstitutionName { get; set; }
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? EndDate { get; set; }
    }

    public class WorkExperienceViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Titel krävs")]
        [StringLength(100, ErrorMessage = "Titel får max vara 100 tecken")]
        public string Title { get; set; } = string.Empty;

        public string? CompanyName { get; set; }
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? EndDate { get; set; }
    }

    public class SelectedProjectViewModel
    {
        public int ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    public class ProjectOptionViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    public class ProfileViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public bool IsPrivate { get; set; }
        public string? Email { get; set; }
    }
}
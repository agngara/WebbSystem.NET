using Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace MVC.Models.CVs
{
    public class WorkExperienceFileModel
    {
        public required string Title { get; set; }

        public string? Description { get; set; }

        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string WorkPlaceName { get; set; }
    }
}

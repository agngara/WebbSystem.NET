using Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC.Models.Projects
{
    public class ProjectViewModel
    {
        public required int ProjectId { get; set; }

        public required string CreatorId { get; set; }
        public required User Creator { get; set; }
        public  string Titel { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public List<User> Users { get; set; } = [];
    }
}

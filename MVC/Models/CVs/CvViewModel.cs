using Models;

namespace MVC.Models.CVs
{
    public class CvViewModel
    {
        public User User { get; set; }

        public List<Project> UserProjects { get; set; }
    }
}
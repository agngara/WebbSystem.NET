
namespace Models
{
    public class WorkPlace
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();
    }
}

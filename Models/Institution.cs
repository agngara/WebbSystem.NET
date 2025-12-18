
namespace Models
{
    public class Institution
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<Education> Educations { get; set; } = new List<Education>();
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models
{
    public class CV
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public User User {  get; set; }
        public string? ProfileImage { get; set; }
        
        public virtual List<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();
        public virtual List<Competence> Competences { get; set; } = new List<Competence>();
        public virtual List<Education> Educations { get; set; } = new List<Education>();






    }
}

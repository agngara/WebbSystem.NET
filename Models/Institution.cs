using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class Institution
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Education> Educations { get; set; } = new List<Education>();
    }
}

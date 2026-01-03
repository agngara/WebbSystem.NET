using System.ComponentModel.DataAnnotations;

namespace MVC.Models
{
    public class CreateProjectViewModel
    {
        [Required(ErrorMessage = "Please Enter a Project Name.")]
        [StringLength(30)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please Enter a Project Description.")]
        [StringLength(2000)]
        public string Description { get; set; }
        public IFormFile? ImageFile { get; set; }

    }

    
}

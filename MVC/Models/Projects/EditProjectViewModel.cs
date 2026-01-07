using System.ComponentModel.DataAnnotations;

namespace MVC.Models.Projects
{
    public class EditProjectViewModel
    {
        public int ProjectId { get; set; }
        
        [Required(ErrorMessage = "Please Enter a Project Name.")]
        [StringLength(30)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please Enter a Project Description.")]
        [StringLength(2000)]
        public string Description { get; set; }
        
        public string ImageUrl { get; set; }

        public IFormFile ImageFile {  get; set; }
    }

    
}

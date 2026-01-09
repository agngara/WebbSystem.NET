using System.ComponentModel.DataAnnotations;

public class DeactivateAccountViewModel
{
    [Required]
    [Display(Name = "Enter password to deactivate")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
}

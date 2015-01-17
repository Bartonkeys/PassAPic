using System.ComponentModel.DataAnnotations;

namespace PassAPic.Models.Models
{
    public class RegisterExternalBindingModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }
    }
}
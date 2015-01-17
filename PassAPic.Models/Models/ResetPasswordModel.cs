using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PassAPic.Models.Models
{
    public class ResetPasswordModel
    {
        [EmailAddress]
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "newPassword")]
        public string NewPassword { get; set; }
    }
}
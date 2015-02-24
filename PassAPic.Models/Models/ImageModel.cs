using System;
using Newtonsoft.Json;

namespace PassAPic.Models.Models
{
    public class ImageModel : GameBaseModel
    {
        [JsonProperty(PropertyName = "image")]
        public String Image { get; set; }

        

    }
}
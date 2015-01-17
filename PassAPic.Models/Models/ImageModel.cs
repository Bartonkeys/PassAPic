using System;
using Newtonsoft.Json;

namespace PassAPic.Models.Models
{
    public class ImageModel : GameBaseModel
    {
        [JsonProperty(PropertyName = "image")]
        public String Image { get; set; }

        [JsonProperty(PropertyName = "nextUserId")]
        public int NextUserId { get; set; }

        [JsonProperty("sentFromUsername")]
        public string SentFromUsername { get; set; }

    }
}
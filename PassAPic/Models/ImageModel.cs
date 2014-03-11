using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using PassAPic.Models;

namespace PassAPic.Models
{
    public class ImageModel : GameBaseModel
    {
        [JsonProperty(PropertyName = "image")]
        public String Image { get; set; }

        [JsonProperty(PropertyName = "nextUserId")]
        public int NextUserId { get; set; }

    }
}
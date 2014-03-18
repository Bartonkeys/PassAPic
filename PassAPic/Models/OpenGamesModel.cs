
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace PassAPic.Models
{
    public class OpenGamesModel
    {
        [JsonProperty(PropertyName = "wordModelList")]
        public List<WordModel> WordModelList { get; set; }

        [JsonProperty(PropertyName = "imageModelList")]
        public List<ImageModel> ImageModelList { get; set; }
    }
}
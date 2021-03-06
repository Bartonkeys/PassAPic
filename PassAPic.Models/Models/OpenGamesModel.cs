﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace PassAPic.Models.Models
{
    public class OpenGamesModel
    {
        [JsonProperty(PropertyName = "wordModelList")]
        public List<WordModel> WordModelList { get; set; }

        [JsonProperty(PropertyName = "imageModelList")]
        public List<ImageModel> ImageModelList { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using PassAPic.Models;

namespace PassAPic.Models
{
    public class WordModel: GameBaseModel
    {
        [JsonProperty(PropertyName = "word")]
        public string Word { get; set; }

        [JsonProperty(PropertyName = "nextUserId")]
        public int NextUserId { get; set; }

        [JsonProperty("sentFromUsername")]
        public string SentFromUsername { get; set; }
    }
    
}
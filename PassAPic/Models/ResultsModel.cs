using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Web;
using Newtonsoft.Json;

namespace PassAPic.Models
{
    public class ResultsModel
    {
        [JsonProperty(PropertyName = "gameId")]
        public int GameId { get; set; }

        [JsonProperty(PropertyName = "guesses")]
        public List<GameBaseModel> Guesses { get; set; } 
    }
}
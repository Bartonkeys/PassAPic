using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Web;
using Newtonsoft.Json;

namespace PassAPic.Models
{
    public class GamesModel
    {
        [JsonProperty(PropertyName = "gameId")]
        public int GameId { get; set; }

        [JsonProperty(PropertyName = "startingWord")]
        public string StartingWord { get; set; }

        [JsonProperty(PropertyName = "numberOfGuesses")]
        public int NumberOfGuesses { get; set; }

        [JsonProperty(PropertyName = "gameOverMan")]
        public Boolean GameOverMan { get; set; } 

        [JsonProperty(PropertyName = "guesses")]
        public List<GameBaseModel> Guesses { get; set; } 
    }
}
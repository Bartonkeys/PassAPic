using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PassAPic.Models.Models
{
    public class GamesModel
    {
        [JsonProperty(PropertyName = "gameId")]
        public int GameId { get; set; }

        [JsonProperty(PropertyName = "creatorId")]
        public int CreatorId { get; set; }

        [JsonProperty(PropertyName = "creatorName")]
        public virtual string CreatorName { get; set; }

        [JsonProperty(PropertyName = "startingWord")]
        public string StartingWord { get; set; }

        [JsonProperty(PropertyName = "numberOfGuesses")]
        public int NumberOfGuesses { get; set; }

        [JsonProperty(PropertyName = "gameOverMan")]
        public Boolean GameOverMan { get; set; } 

        [JsonProperty(PropertyName = "guesses")]
        public List<GameBaseModel> Guesses { get; set; }

        [JsonProperty(PropertyName = "animation")]
        public string Animation { get; set; }

        [JsonProperty(PropertyName = "dateCreated")]
        public DateTime DateCreated { get; set; }

        [JsonProperty(PropertyName = "dateCompleted")]
        public DateTime? DateCompleted { get; set; }

        [JsonProperty(PropertyName = "comments")]
        public virtual List<GameCommentClientModel> Comments { get; set; }

        [JsonProperty(PropertyName = "timerInSeconds")]
        public int TimerInSeconds { get; set; }

       

    }
}
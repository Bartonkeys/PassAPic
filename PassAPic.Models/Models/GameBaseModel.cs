﻿using System;
using Newtonsoft.Json;
using PassAPic.Models.Models.Models;

namespace PassAPic.Models.Models
{
    public class GameBaseModel
    {
        [JsonProperty(PropertyName = "guessId")]
        public int GuessId { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public int UserId { get; set; }

        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "creatorId")]
        public int CreatorId { get; set; }

        [JsonProperty(PropertyName = "gameId")]
        public int GameId { get; set; }

        [JsonProperty(PropertyName = "order")]
        public int Order { get; set; }

        [JsonProperty(PropertyName = "isLastTurn")]
        public bool IsLastTurn { get; set; }

        [JsonProperty(PropertyName = "latitude")]
        public double Latitude { get; set; }

        [JsonProperty(PropertyName = "longitude")]
        public double Longitude { get; set; }

        [JsonProperty(PropertyName = "mode")]
        public Mode Mode { get; set; }

        [JsonProperty(PropertyName = "dateCreated")]
        public DateTime DateCreated { get; set; }

        [JsonProperty(PropertyName = "timerInSeconds")]
        public int TimerInSeconds { get; set; }

        [JsonProperty(PropertyName = "exchanges")]
        public int Exchanges { get; set; }

        [JsonProperty(PropertyName = "nextUserId")]
        public int NextUserId { get; set; }

        [JsonProperty("sentFromUsername")]
        public string SentFromUsername { get; set; }

    }
}

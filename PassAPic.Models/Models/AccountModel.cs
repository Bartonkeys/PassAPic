﻿using System;
using Newtonsoft.Json;

namespace PassAPic.Models.Models
{
    public class AccountModel
    {
        [JsonProperty(PropertyName = "userId")]
        public int UserId { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "openGames")]
        public OpenGamesModel OpenGames { get; set; }

         [JsonProperty(PropertyName = "numberOfCompletedGames")]
        public int NumberOfCompletedGames { get; set; }

         [JsonProperty(PropertyName = "lastActivity")]
        public DateTime? LastActivity { get; set; }

         [JsonProperty(PropertyName = "hasPlayedWithUserBefore")]
        public bool HasPlayedWithUserBefore { get; set; }

         [JsonProperty(PropertyName = "gamesPlayedWithUserBefore")]
         public int GamesPlayedWithUserBefore { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }

    }
}

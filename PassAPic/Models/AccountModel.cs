using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace PassAPic.Models
{
    public class AccountModel
    {
        [JsonProperty(PropertyName = "userId")]
        public int UserId { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "games")]
        public List<OpenGamesModel> OpenGames { get; set; }

         [JsonProperty(PropertyName = "numberOfCompletedGames")]
        public int NumberOfCompletedGames { get; set; }

         [JsonProperty(PropertyName = "lastActivity")]
        public DateTime? LastActivity { get; set; }

         [JsonProperty(PropertyName = "hasPlayedWithUserBefore")]
        public bool HasPlayedWithUserBefore { get; set; }

    }
}
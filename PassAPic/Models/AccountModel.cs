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
        public List<GamesModel> OpenGames { get; set; }

        // [JsonProperty(PropertyName = "facebookFriends")]
        //public List<FacebookFriendModel> FacebookFriends { get; set; }

    }
}
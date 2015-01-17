using System;
using Newtonsoft.Json;

namespace PassAPic.Models.Models
{
    public class FacebookFriendModel
    {
         [JsonProperty("name")]
        public string Name { get; set; }

         [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("isPaPUser")]
        public Boolean IsPaPUser { get; set; }

        [JsonProperty("papUserId")]
        public int PaPUserId { get; set; }
    }
}
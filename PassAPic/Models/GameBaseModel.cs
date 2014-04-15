using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace PassAPic.Models
{
    public class GameBaseModel
    {
        [JsonProperty(PropertyName = "userId")]
        public int UserId { get; set; }

        [JsonProperty(PropertyName = "creatorId")]
        public int CreatorId { get; set; }

        [JsonProperty(PropertyName = "gameId")]
        public int GameId { get; set; }

        [JsonProperty(PropertyName = "order")]
        public int Order { get; set; }
        [JsonProperty(PropertyName = "isLastTurn")]
        public bool IsLastTurn { get; set; }
    }
}

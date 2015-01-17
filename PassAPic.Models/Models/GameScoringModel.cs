using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PassAPic.Models.Models
{
    public class GameScoringModel
    {
         
        [JsonProperty(PropertyName = "gameId")]
        public int GameId { get; set; }
        [JsonProperty(PropertyName = "userId")]
        public int UserId { get; set; }
        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }
        [JsonProperty(PropertyName = "score")]
        public int Score { get; set; }

    }
}

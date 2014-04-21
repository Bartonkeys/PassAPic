using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace PassAPic.Models
{
    public class GameSetupModel: GameBaseModel
    {
        [JsonProperty(PropertyName = "numberOfPlayers")]
        public int NumberOfPlayers { get; set; }

        [JsonProperty(PropertyName = "isEasyMode")]
        public bool IsEasyMode { get; set; }

    }
}
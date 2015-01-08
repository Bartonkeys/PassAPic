using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace PassAPic.Models
{
    public class GameCommentClientModel : GameCommentModel
    {
        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }
    }
}
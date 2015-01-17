using Newtonsoft.Json;

namespace PassAPic.Models.Models
{
    public class GameCommentClientModel : GameCommentModel
    {
        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }
    }
}
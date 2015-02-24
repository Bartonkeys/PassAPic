using Newtonsoft.Json;

namespace PassAPic.Models.Models
{
    public class WordModel: GameBaseModel
    {
        [JsonProperty(PropertyName = "word")]
        public string Word { get; set; }

    }
    
}
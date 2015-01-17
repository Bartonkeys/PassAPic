namespace PassAPic.Models
{
    public class WordModel: GameBaseModel
    {
        [JsonProperty(PropertyName = "word")]
        public string Word { get; set; }

        [JsonProperty(PropertyName = "nextUserId")]
        public int NextUserId { get; set; }

        [JsonProperty("sentFromUsername")]
        public string SentFromUsername { get; set; }
    }
    
}
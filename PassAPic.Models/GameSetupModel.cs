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
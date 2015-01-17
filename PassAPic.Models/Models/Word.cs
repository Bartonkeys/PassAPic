using Newtonsoft.Json;

namespace PassAPic.Models.Models
{
    public class Word
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "word")]
        public string RandomWord { get; set; }
    }
}

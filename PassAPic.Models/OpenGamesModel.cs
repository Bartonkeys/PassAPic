using System.Collections.Generic;

namespace PassAPic.Models
{
    public class OpenGamesModel
    {
        [JsonProperty(PropertyName = "wordModelList")]
        public List<WordModel> WordModelList { get; set; }

        [JsonProperty(PropertyName = "imageModelList")]
        public List<ImageModel> ImageModelList { get; set; }
    }
}
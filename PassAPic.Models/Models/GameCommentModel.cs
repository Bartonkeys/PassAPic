using System;
using Newtonsoft.Json;

namespace PassAPic.Models.Models
{
    public class GameCommentModel
    {

        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "likes")]
        public long Likes { get; set; }

        [JsonProperty(PropertyName = "gameId")]
        public int GameId { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public int UserId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty(PropertyName = "dateCreated")]
        public DateTime DateCreated { get; set; }


    }
}
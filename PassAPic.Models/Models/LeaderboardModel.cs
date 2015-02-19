using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PassAPic.Models.Models
{
    public class LeaderboardModel
    {
        [Key]
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "totalScore")]
        public int TotalScore { get; set; }

        [JsonProperty(PropertyName = "weekNumber")]
        public int WeekNumber { get; set; }

    }

}

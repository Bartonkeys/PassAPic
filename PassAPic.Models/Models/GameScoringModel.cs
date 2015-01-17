using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassAPic.Models.Models
{
    public class GameScoringModel
    {
        public int GameId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int Score { get; set; }

    }
}

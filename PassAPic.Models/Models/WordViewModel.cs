using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PassAPic.Models.Models.Models;

namespace PassAPic.Models.Models
{
    public class WordViewModel
    {
        public string Word { get; set;}
        public int? Games { get; set; }
        public int? Exchanges { get; set; }
        public Mode Mode { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PassAPic.Models
{
    public class RegisterItem
    {
        public string ApplicationGuid { get; set; }
        public int UserId { get; set; }
        public int DeviceType { get; set; }
        public String DeviceToken { get; set; }

    }
}
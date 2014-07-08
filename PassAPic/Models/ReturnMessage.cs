using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PassAPic.Models
{
    public class ReturnMessage
    {
        public String Message { get; set; }

        public ReturnMessage()
        {

        }

        public ReturnMessage(String newMessage)
        {
            Message = newMessage;
        }
    }
}
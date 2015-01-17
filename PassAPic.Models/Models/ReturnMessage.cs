using System;

namespace PassAPic.Models.Models
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
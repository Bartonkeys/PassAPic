using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassAPic.Core.PushRegistration
{
    public class PushMessageException : ApplicationException
    {
        public PushMessageException()
        {}

        public PushMessageException(string message, Exception innerException): base(message, innerException)
        {}
    }
}

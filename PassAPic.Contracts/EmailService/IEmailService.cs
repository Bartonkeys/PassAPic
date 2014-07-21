using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassAPic.Contracts.EmailService
{
    public interface IEmailService
    {
        void SendPasswordToEmail(string password, string email);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PassAPic.Contracts.EmailService;

namespace PassAPic.Core.Email
{
    public class PlaceHolderEmailService: IEmailService
    {
        public void SendPasswordToEmail(string password, string email)
        {
            //todo Sort this out
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PassAPic.Contracts.EmailService;
using System;
using System.Net;
using System.Net.Mail;
using SendGrid;

namespace PassAPic.Core.Email
{
    public class SendGridEmailService: IEmailService
    {
        public void SendPasswordToEmail(string password, string email)
        {
            
            // Create the email object first, then add the properties.
            var myMessage = new SendGridMessage();

            // Add the message properties.
            myMessage.From = new MailAddress("system@passapic.com");

            // Add multiple addresses to the To field.
            List<String> recipients = new List<String>
            {email};

            myMessage.AddTo(recipients);
            myMessage.Subject = "Passapic password reset";

            //Add the HTML and Text bodies
            myMessage.Html = "<p>Your new Passapic password is: " + password + "</p>";
            myMessage.Text = "Your new Passapic password is: " + password;
            myMessage.EnableClickTracking(true);

            // Create network credentials to access your SendGrid account.
            var username = "yerma";
            var pswd = "Y)rm91234";
            
            var credentials = new NetworkCredential(username, pswd);

            // Create an Web transport for sending email.
            var transportWeb = new Web(credentials);

            // Send the email.
            transportWeb.Deliver(myMessage);
        }
    }
}

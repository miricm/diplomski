using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace Users.Infrastructure
{
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            string sendGridUserName  = "apikey";
            string sentFrom          = "miricja@gmail.com";
            string sendGridPassword  = ConfigurationManager.AppSettings["SendGridApiKey"];

            // Konfiguracija klijenta
            var client = new SmtpClient(host: "smtp.sendgrid.net", port: 587);

            // Nepotrebno?
            // client.Port = 587; 

            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;

            // Kreiranje akreditiva (credentials)
            NetworkCredential credentials = new NetworkCredential(sendGridUserName, sendGridPassword);
            client.EnableSsl    = true; 
            client.Credentials  = credentials;

            // Kreiranje poruke
            var mail      = new MailMessage(sentFrom, message.Destination);
            mail.Subject  = message.Subject;
            mail.Body     = message.Body;

            // Slanje
            return client.SendMailAsync(mail);
        }
    }
}
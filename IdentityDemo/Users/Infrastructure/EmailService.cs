using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
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
            var sendGridUserName = "apikey";
            var sentFrom = "miricja@gmail.com";
            var sendGridPassword = "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";

            // Konfiguracija klijenta
            var client = new SmtpClient("smtp.sendgrid.net", Convert.ToInt32(587));

            client.Port = 587;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;

            // Kreiranje akreditiva (credentials)
            NetworkCredential credentials = new NetworkCredential(sendGridUserName, sendGridPassword);
            client.EnableSsl = true; client.Credentials = credentials;

            // Kreiranje poruke
            var mail = new MailMessage(sentFrom, message.Destination); 
            mail.Subject = message.Subject;
            mail.Body = message.Body;

            // Slanje
            return client.SendMailAsync(mail);
        }
    }
}
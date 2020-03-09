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
            string smtpName = ConfigurationManager.AppSettings["smtpUserName"];
            string smtpPass = ConfigurationManager.AppSettings["smtpPassword"];

            MailAddress to = new MailAddress(message.Destination);
            MailAddress from = new MailAddress("miricja@gmail.com");

            MailMessage mail = new MailMessage(from, to);
            mail.Subject = message.Subject;
            mail.Body = message.Body;
            mail.IsBodyHtml = true;

            SmtpClient client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(smtpName, smtpPass),
                EnableSsl = true
            };

            return client.SendMailAsync(mail);
        }
    }
}
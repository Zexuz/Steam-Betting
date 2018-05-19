using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Betting.Backend.Services.Impl
{
    public interface IGmailService
    {
        Task SendEmail(List<MailAddress> to, string body, string subject, List<MailAddress> cc = null);
    }

    public class GmailService : IGmailService
    {
        private readonly string _appPassword;
        private readonly string _username;

        public GmailService(IConfiguration configuration)
        {
            var googleEmail = configuration.GetSection("GoogleEmail");
            _appPassword    = googleEmail.GetSection("AppPassword").Value;
            _username       = googleEmail.GetSection("UserName").Value;
        }


        public async Task SendEmail(List<MailAddress> to, string body, string subject, List<MailAddress> cc = null)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(_username, _appPassword),
                EnableSsl   = true
            };

            var mail     = new MailMessage();
            mail.Body    = body;
            mail.Subject = subject;

            mail.From = new MailAddress("example@example.com"); //this does not matter!
            AddRange(to, mail.To.Add);

            if (cc != null)
                AddRange(cc, mail.CC.Add);
            await smtpClient.SendMailAsync(mail);
        }

        private static void AddRange(IEnumerable<MailAddress> list, Action<MailAddress> action)
        {
            foreach (var item in list)
            {
                action(item);
            }
        }
    }
}
using HRManager.WebAPI.Domain.Interfaces;
using System.Net;
using System.Net.Mail;

namespace HRManager.WebAPI.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var settings = _config.GetSection("EmailSettings");

            var fromEmail = settings["FromEmail"];
            var fromName = settings["FromName"];
            var password = settings["Password"];
            var host = settings["Host"];
            var port = int.Parse(settings["Port"]);

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true // Permite usar HTML no email
            };

            message.To.Add(new MailAddress(toEmail));

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
        }
    }
}

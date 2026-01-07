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
            // Validações de nulo
            var host = _config["Smtp:Host"];
            var portStr = _config["Smtp:Port"];
            var username = _config["Smtp:Username"];
            var password = _config["Smtp:Password"];

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(portStr) || 
                string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                Console.WriteLine("Configurações de SMTP incompletas.");
                return; 
            }

            int port = int.Parse(portStr);

            var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(username, "HR Manager"), // Usar username como remetente
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}

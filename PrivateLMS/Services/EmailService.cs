using Mailjet.Client;
using Mailjet.Client.Resources;
using Mailjet.Client.TransactionalEmails;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace PrivateLMS.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var apiKey = _configuration["Mailjet:ApiKey"];
            var secretKey = _configuration["Mailjet:SecretKey"];
            var senderEmail = _configuration["Mailjet:SenderEmail"];
            var senderName = _configuration["Mailjet:SenderName"];

            var client = new MailjetClient(apiKey, secretKey);
            var request = new MailjetRequest { Resource = Send.Resource };
            var email = new TransactionalEmailBuilder()
                .WithFrom(new SendContact(senderEmail, senderName))
                .WithSubject(subject)
                .WithHtmlPart(body)
                .WithTo(new SendContact(toEmail))
                .Build();

            var response = await client.SendTransactionalEmailAsync(email);
            if (response.Messages.Length == 0 || response.Messages[0].Status != "success")
            {
                throw new Exception("Failed to send email via Mailjet.");
            }
        }
    }
}
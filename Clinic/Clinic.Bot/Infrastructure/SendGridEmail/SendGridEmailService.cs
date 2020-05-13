using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace Clinic.Bot.Infrastructure.SendGridEmail
{
    public class SendGridEmailService : ISendGridEmailService
    {
        IConfiguration _configuration;
        public SendGridEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<bool> Execute(
            string fromEmail,
            string fromName,
            string toEmail,
            string toName,
            string subject,
            string plainTextContent,
            string htmlContent)
        {
            var apiKey = _configuration["SendGridEmailKey"];
            var client = new SendGridClient(apiKey);
            var From = new EmailAddress(fromEmail, fromName);
            var To = new EmailAddress(toEmail, toName);
            var Subject = subject;
            var PlainTextContent = plainTextContent;
            var HtmlContent = htmlContent;
            var email = MailHelper.CreateSingleEmail(From, To, Subject, PlainTextContent, HtmlContent);
            var response = await client.SendEmailAsync(email);
            if (response.StatusCode.ToString().ToLower() == "unauthorized")
                return false;

            return true;
        }
    }
}

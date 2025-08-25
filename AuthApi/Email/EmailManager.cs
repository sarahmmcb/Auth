using AuthApi.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AuthApi.Email
{
    public class EmailManager
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailManager(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task<bool> SendEmail(
            string from,
            string fromEmailDisplayName,
            List<string> to,
            string subject,
            string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromEmailDisplayName, from));
            to.ForEach(address => { 
                message.To.Add(new MailboxAddress("User", address));
            });
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();

            try
            {
                client.Connect(_smtpSettings.Host, _smtpSettings.Port, _smtpSettings.UseSsl);
                await client.SendAsync(message);
            }
            catch (Exception ex) {
                // TODO: Logging
                return false;
            }
            finally
            {
                client.Disconnect(true);
            }

            return true;
        }
    }
}

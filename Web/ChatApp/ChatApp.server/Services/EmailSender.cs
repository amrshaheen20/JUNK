using System.Net;
using System.Net.Mail;

namespace ChatApi.server.Services
{ public class EmailSender
    {
        private readonly IConfiguration configuration;
        public EmailSender(IConfiguration Configuration)
        {
            configuration = Configuration;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpUser = configuration["SMTP:SMTPUser"];
            var smtpPassword = configuration["SMTP:SMTPPassword"];
            var smtpServer = configuration["SMTP:SMTPServer"];
            var smtpPort = configuration["SMTP:SMTPPort"];

            if (string.IsNullOrWhiteSpace(smtpUser) ||
                string.IsNullOrWhiteSpace(smtpPassword) ||
                string.IsNullOrWhiteSpace(smtpServer) ||
                string.IsNullOrWhiteSpace(smtpPort))
            {
                throw new InvalidOperationException("SMTP configuration is missing or incomplete in appsettings.json.");
            }

            if (!int.TryParse(smtpPort, out var port))
            {
                throw new InvalidOperationException("Invalid SMTP port specified in appsettings.json.");
            }

            var smtpClient = new SmtpClient(smtpServer)
            {
                Port = port,
                Credentials = new NetworkCredential(smtpUser, smtpPassword),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage(smtpUser, email, subject, htmlMessage)
            {
                IsBodyHtml = true
            };


            return smtpClient.SendMailAsync(mailMessage);
        }



        public Task SendEmailConfirmationAsync(string email, string token)
        {
            var emailBody = System.IO.File.ReadAllText(Constants.EMAIL_CONFIRMATION_TEMPLATE);

            emailBody = emailBody.Replace("{{verification_code}}", token);

            return SendEmailAsync(email, "Email Confirmation", emailBody);
        }

        public Task SendPasswordResetAsync(string email, string reset_link)
        {
            var emailBody = System.IO.File.ReadAllText(Constants.PASSWORD_RESET_TEMPLATE);

            emailBody = emailBody.Replace("{{reset_link}}", reset_link);

            return SendEmailAsync(email, "Password Reset", emailBody);
        }

    }
}

using System;
using System.Net;
using System.Net.Mail;

namespace SUPPLY_API
{
    public class EmailSender
    {
        private readonly EmailSettings _settings;

        public EmailSender(EmailSettings settings)
        {
            _settings = settings;
        }

        public bool SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                var smtpClient = new SmtpClient(_settings.SmtpServer)
                {
                    Port = _settings.Port,
                    Credentials = new NetworkCredential(_settings.FromEmail, _settings.Password),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage(_settings.FromEmail, toEmail, subject, body);
                smtpClient.Send(mailMessage);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при отправке письма: " + ex.Message);
                return false;
            }
        }
    }
}

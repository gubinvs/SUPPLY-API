using System;
using System.Net;
using System.Net.Mail;

public class EmailSender
{
    private string smtpServer; // Например, для Gmail это "smtp.gmail.com"
    private int port; // Порт(для Gmail 587)
    private string fromEmail; // Ваш email
    private string password; // Ваш пароль (не храните пароль в коде!)

    // Конструктор для инициализации данных
    //public EmailSender(string smtpServer, int port, string fromEmail, string password)
    public EmailSender()
    {
        this.smtpServer = "smtp.beget.com";
        this.port = 2525;
        this.fromEmail = "support@encomponent.ru";
        this.password = "JRk*upFTqk5b";
    }

    /// <summary>
    /// Метод для отправки сообщения
    /// </summary>
    /// <param name="toEmail">Кому отправляем</param>
    /// <param name="subject">Заголовок письма</param>
    /// <param name="body">Сообщение</param>
    /// <returns></returns>
    public bool SendEmail(string toEmail, string subject, string body)
    {
        try
        {
            // Создание клиента SMTP
            SmtpClient smtpClient = new SmtpClient(smtpServer)
            {
                Port = port,
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true
            };

            // Создание сообщения
            MailMessage mailMessage = new MailMessage(fromEmail, toEmail, subject, body);

            // Отправка сообщения
            smtpClient.Send(mailMessage);

            return true; // Если отправлено успешно
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при отправке письма: " + ex.Message);
            return false; // Если произошла ошибка
        }
    }
}

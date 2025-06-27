namespace SUPPLY_API
{
   public class EmailSettings
        {
            public required string SmtpServer { get; set; }
            public int Port { get; set; }
            public required string FromEmail { get; set; }
            public required string Password { get; set; }
        }

}

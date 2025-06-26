namespace SUPPLY_API
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = null!;
        public int Port { get; set; }
        public string FromEmail { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}

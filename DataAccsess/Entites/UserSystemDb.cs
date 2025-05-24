using System;

namespace SUPPLY_API
{
    public class UserSystemDb
    {
        public int Id { get; set; }

        public string? GuidIdUser { get; set; }

        public string? EmailUser { get; set; }

        public string? PasswordUser { get; set; }

        public string? TokenUser { get; set; }

        public DateTime DataRegistrationUser { get; set; }

        public bool ActivationEmailUser { get; set; }

        public bool AdminUserSystem { get; set; }

        // Пустой конструктор для EF
        public UserSystemDb() { }

        public UserSystemDb(
            string email,
            string password,
            string token,
            bool activationEmailUser,
            bool adminUserSystem,
            DateTime registrationDate)
        {
            GuidIdUser = Guid.NewGuid().ToString();
            EmailUser = email;
            PasswordUser = password;
            TokenUser = token;
            DataRegistrationUser = registrationDate;
            ActivationEmailUser = activationEmailUser;
            AdminUserSystem = adminUserSystem;
        }
    }
}

// Модель данный о пользователях системы


namespace SUPPLY_API
{
    public class CollaboratorSystemDb
    {
        public int Id { get; set; }

        // Идентификатор пользователя 
        public string? GuidIdCollaborator { get; set; }

        // Идентификатор роли пользователя в системе (админ, поставшик, заказчик)
        public string? GuidIdRoleSystem { get; set; }

        // Временный токер выдаваемый при авторизации пользователя
        public string? TokenSystem { get; set; }

        // ФИО пользователя
        public string? NameCollaborator { get; set; }

        // E-mail пользователя
        public string? EmailCollaborator { get; set; }

        // Пароль пользователя
        public string? PasswordCollaborator { get; set; }

        // Телефон пользователя
        public string? PhoneCollaborator { get; set; }


        // Дата регистрации пользователя в системе
        public DateTime DataRegistrationCollaborator { get; set; }

        // Подтверждение e-mail пользователем
        public bool ActivationEmailCollaborator { get; set; }


        public CollaboratorSystemDb() { }
        public CollaboratorSystemDb(
                    string guidIdCollaborator,
                    string guidIdRoleSystem,
                    string tokenSystem,
                    string nameCollaborator,
                    string emailCollaborator,
                    string phoneCollaborator,
                    DateTime dataRegistrationCollaborator,
                    bool activationEmailCollaborator
                )
        {
            GuidIdCollaborator = guidIdCollaborator;
            GuidIdRoleSystem = guidIdRoleSystem;
            TokenSystem = tokenSystem;
            NameCollaborator = nameCollaborator;
            EmailCollaborator = emailCollaborator;
            PhoneCollaborator = phoneCollaborator;
            DataRegistrationCollaborator = dataRegistrationCollaborator;
            ActivationEmailCollaborator = activationEmailCollaborator;
        }
    }
}

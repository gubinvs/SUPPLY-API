// Класс содержит данные о пользователе компании
namespace SUPPLY_API
{
    public class CollaboratorProvider
    {
        public int Id { get; set; }

        public string? GuidIdCollaborator { get; set; }

        public string? GuidIdProvider { get; set; }

        public string? NameCollaborator { get; set; }

        public string? EmailCollaborator { get; set; }

        public string? PhoneCollaborator { get; set; }

        // Пустой конструктор для EF Core
        public CollaboratorProvider() { }

        public CollaboratorProvider
                                    (
                                        string guidCollaborator,
                                        string guidProvider,
                                        string name,
                                        string email,
                                        string phone
                                    )
        {
            GuidIdCollaborator = guidCollaborator;
            GuidIdProvider = guidProvider;
            NameCollaborator = name;
            EmailCollaborator = email;
            PhoneCollaborator = phone;
        }
        

    }
}
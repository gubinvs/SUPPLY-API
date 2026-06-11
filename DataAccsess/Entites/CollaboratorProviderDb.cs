using DocumentFormat.OpenXml.Drawing;
using Microsoft.EntityFrameworkCore;

namespace SUPPLY_API
{
    public class CollaboratorProviderDb
    {
        public int Id {get; set;}

        public string GuidIdCollaboratorProvider {get; set;} = String.Empty;
        public string GuidIdCompanyProvider {get; set;} = String.Empty;

        public string NameCollaboratorProvider {get; set;} = String.Empty;

        public string PhoneCollaboratorProvider {get; set;} = String.Empty;

        public string EmailCollaboratorProvider {get; set;} = String.Empty;

        public CollaboratorProviderDb (){}

    }
}
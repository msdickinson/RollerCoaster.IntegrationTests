using System.Diagnostics.CodeAnalysis;

namespace RollerCoaster.IntegrationTests.Services.AccountDB.Models
{
    [ExcludeFromCodeCoverage]
    public class SelectAccountByEmailRequest
    {
        public string Email { get; set; }
    }
}

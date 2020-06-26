using System.Diagnostics.CodeAnalysis;

namespace RollerCoaster.IntegrationTests.Services.AccountDB.Models
{
    [ExcludeFromCodeCoverage]
    public class RollerCoasterDBOptions
    {
        public string ConnectionString { get; set; }
    }
}

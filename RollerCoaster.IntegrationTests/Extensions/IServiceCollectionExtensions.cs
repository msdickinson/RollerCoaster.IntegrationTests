using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RollerCoaster.IntegrationTests.Account;
using RollerCoaster.IntegrationTests.Services.AccountDB;

namespace RollerCoaster.IntegrationTests.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddIntegrationTests(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton<IAccountAPIIntegrationTests, AccountAPIIntegrationTests>();
            serviceCollection.TryAddSingleton<IAccountDBService, AccountDBService>();
            
            return serviceCollection;
        }
    }
}

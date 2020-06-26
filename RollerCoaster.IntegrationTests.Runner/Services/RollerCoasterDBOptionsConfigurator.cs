using DickinsonBros.Encryption.Certificate.Abstractions;
using DickinsonBros.Telemetry.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RollerCoaster.IntegrationTests.Runner.Models;
using RollerCoaster.IntegrationTests.Services.AccountDB.Models;

namespace RollerCoaster.IntegrationTests.Runner.Services
{
    public class RollerCoasterDBOptionsConfigurator : IConfigureOptions<RollerCoasterDBOptions>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public RollerCoasterDBOptionsConfigurator(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }
        void IConfigureOptions<RollerCoasterDBOptions>.Configure(RollerCoasterDBOptions options)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var provider = scope.ServiceProvider;
            var configuration = provider.GetRequiredService<IConfiguration>();
            var certificateEncryptionService = provider.GetRequiredService<ICertificateEncryptionService<RunnerCertificateEncryptionServiceOptions>>();
            var rollerCoasterDBOptions = configuration.GetSection(nameof(RollerCoasterDBOptions)).Get<RollerCoasterDBOptions>();
            rollerCoasterDBOptions.ConnectionString = certificateEncryptionService.Decrypt(rollerCoasterDBOptions.ConnectionString);
            configuration.Bind($"{nameof(TelemetryServiceOptions)}", options);

            options.ConnectionString = rollerCoasterDBOptions.ConnectionString;
        }
    }
}

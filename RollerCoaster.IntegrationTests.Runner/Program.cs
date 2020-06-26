using DickinsonBros.DateTime.Extensions;
using DickinsonBros.DurableRest.Extensions;
using DickinsonBros.Encryption.Certificate.Extensions;
using DickinsonBros.Encryption.Certificate.Models;
using DickinsonBros.Guid.Abstractions;
using DickinsonBros.Guid.Extensions;
using DickinsonBros.Logger.Abstractions;
using DickinsonBros.Logger.Extensions;
using DickinsonBros.Redactor.Extensions;
using DickinsonBros.Redactor.Models;
using DickinsonBros.SQL.Extensions;
using DickinsonBros.Stopwatch.Extensions;
using DickinsonBros.Telemetry.Abstractions;
using DickinsonBros.Telemetry.Extensions;
using DickinsonBros.Telemetry.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RollerCoaster.Account.API.Proxy.Extensions;
using RollerCoaster.Account.API.Proxy.Models;
using RollerCoaster.IntegrationTests.Account;
using RollerCoaster.IntegrationTests.Extensions;
using RollerCoaster.IntegrationTests.Runner.Models;
using RollerCoaster.IntegrationTests.Runner.Services;
using RollerCoaster.IntegrationTests.Services.AccountDB.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RollerCoaster.IntegrationTests.Runner
{
    class Program
    {
        const string SHOW_LOGS = "SHOW_LOGS";
        const string REG_EXP_TEST_NAMES = "REG_EXP_TEST_NAMES";
        const string SHOW_TEST_EXCEPTION = "SHOW_TEST_EXCEPTION";

        const string SHOW_SUCCESS_LOGS_ON_SUCCESS = "SHOW_SUCCESS_LOGS_ON_SUCCESS";
        const string SHOW_SUCCESS_LOGS_ON_FAILURE = "SHOW_SUCCESS_LOGS_ON_FAILURE";

        IConfiguration _configuration;

        List<Task> tasks = new List<Task>();
        ConcurrentStack<IntegreationTestResult> integreationTestResults = new ConcurrentStack<IntegreationTestResult>();

        async static Task<int> Main()
        {
            return await new Program().DoMain();
        }
        async Task<int> DoMain()
        {
            try
            {
                var showSuccessLogsOnSuccess = Convert.ToBoolean(Environment.GetEnvironmentVariable(SHOW_SUCCESS_LOGS_ON_SUCCESS));
                var showSuccessLogsOnFailure = Convert.ToBoolean(Environment.GetEnvironmentVariable(SHOW_SUCCESS_LOGS_ON_FAILURE));
                var showTestException = Convert.ToBoolean(Environment.GetEnvironmentVariable(SHOW_TEST_EXCEPTION));
                var showLogs = Convert.ToBoolean(Environment.GetEnvironmentVariable(SHOW_LOGS));
                var regExp = Environment.GetEnvironmentVariable(REG_EXP_TEST_NAMES) != null ? new Regex(Environment.GetEnvironmentVariable(REG_EXP_TEST_NAMES)) : new Regex(@".*");
    
                using var applicationLifetime = new ApplicationLifetime();
                var services = InitializeDependencyInjection();
                ConfigureServices(services, applicationLifetime, showLogs);
                using var provider = services.BuildServiceProvider();
                var telemetryService = provider.GetRequiredService<ITelemetryService>();
                var guidService = provider.GetRequiredService<IGuidService>();
                var accountAPIIntegrationTests = provider.GetRequiredService<IAccountAPIIntegrationTests>();
                var correlationService = provider.GetRequiredService<ICorrelationService>();

                AddToRunner<IAccountAPIIntegrationTests>(accountAPIIntegrationTests, correlationService, guidService, regExp);

                await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);

                var results = integreationTestResults.ToList();

                foreach(var result in results)
                {
                    var pass = result.Pass ? "PASS": "FAIL";
                    Console.WriteLine($"{ pass }  { result.TestName } - { result.CorrelationId }");

                    if(
                        result.SuccessLog.Any() && 
                        (result.Pass && showSuccessLogsOnSuccess) ||
                        (!result.Pass && showSuccessLogsOnFailure)
                      )
                    {
                        foreach (var log in result.SuccessLog)
                        {
                            Console.WriteLine($"+ {log}");
                        }
                    }
                    

                    if (!result.Pass && showTestException)
                    {
                        Console.WriteLine($"- {result.Exception.Message}");
                    }

                    if (
                        (result.SuccessLog.Any() &&
                        (result.Pass && showSuccessLogsOnSuccess) ||
                        (!result.Pass && showSuccessLogsOnFailure)) ||
                        (!result.Pass && showTestException)
                      )
                    {
                        Console.WriteLine();
                    }
                }

                Console.WriteLine($"{results.Count(e=>e.Pass)} Successful, {results.Count(e => !e.Pass)} Failed");

                try
                {
                   // await accountAPIIntegrationTests.CreateAccountAsync_InvaildEmail_ReturnBadRequestAsync().ConfigureAwait(false);
                }
                catch(Exception ex)
                {

                }
                finally
                {

                }
                await telemetryService.FlushAsync().ConfigureAwait(false);

                applicationLifetime.StopApplication();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                Console.WriteLine("End...");
            }

            return integreationTestResults.ToList().Any(e => !e.Pass) ? 1 : 0;
        }

        private void AddToRunner<T>(T tests, ICorrelationService correlationService, IGuidService guidService, Regex myRegex)
        {
            //Collect Methods
            var methods = tests
                 .GetType()
                 .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                 .Where(e => e.DeclaringType.Name == tests.GetType().Name && myRegex.IsMatch($"{e.ReflectedType.Name}.{e.Name}"))
                 .ToList();

            //Add Methods To Pool
            foreach (var methodInfo in methods)
            {
                tasks.Add(Process<T>(tests, methodInfo, correlationService, guidService));
            }
        }

        private async Task Process<T>(T tests, MethodInfo methodInfo, ICorrelationService correlationService, IGuidService guidService)
        {
            correlationService.CorrelationId = guidService.NewGuid().ToString();
            bool pass = false;
            Exception exception = null;
            var successLog = new List<string>();

            try
            {
                await (Task)methodInfo.Invoke(tests, new Object[] { successLog });
                pass = true;
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            integreationTestResults.Push(new IntegreationTestResult
            {
                TestName = $"{methodInfo.ReflectedType.Name}.{methodInfo.Name}",
                CorrelationId = correlationService.CorrelationId,
                Pass = pass,
                Exception = exception,
                SuccessLog = successLog
            });

        }

        private void ConfigureServices(IServiceCollection services, Services.ApplicationLifetime applicationLifetime, bool showLogs)
        {
            services.AddOptions();
            services.AddLogging(cfg => {
                if (showLogs)
                {
                    cfg.AddConsole();
                }
            });

            //Add ApplicationLifetime
            services.AddSingleton<IApplicationLifetime>(applicationLifetime);

            //Add DateTime Service
            services.AddDateTimeService();

            //Add Stopwatch Service
            services.AddStopwatchService();

            //Add Logging Service
            services.AddGuidService();

            //Add Logging Service
            services.AddLoggingService();

            //Add Redactor Service
            services.AddRedactorService();
            services.Configure<RedactorServiceOptions>(_configuration.GetSection(nameof(RedactorServiceOptions)));

            //Add Certificate Encryption Service
            services.AddCertificateEncryptionService<CertificateEncryptionServiceOptions>();
            services.Configure<CertificateEncryptionServiceOptions<RunnerCertificateEncryptionServiceOptions>>(_configuration.GetSection(nameof(RunnerCertificateEncryptionServiceOptions)));

            services.AddCertificateEncryptionService<RunnerCertificateEncryptionServiceOptions>();
            services.Configure<CertificateEncryptionServiceOptions<RunnerCertificateEncryptionServiceOptions>>(_configuration.GetSection(nameof(RunnerCertificateEncryptionServiceOptions)));

            //Add Telemetry Service
            services.AddTelemetryService();
            services.AddSingleton<IConfigureOptions<TelemetryServiceOptions>, TelemetryServiceOptionsConfigurator>();

            //Add DurableRest Service
            services.AddDurableRestService();

            //Add SQLService
            services.AddSQLService();

            //Add Account Proxy Service
            services.AddAccountProxyService
            (
                new Uri(_configuration[$"{nameof(AccountProxyOptions)}:{nameof(AccountProxyOptions.BaseURL)}"]),
                new TimeSpan(0, 0, Convert.ToInt32(_configuration[$"{nameof(AccountProxyOptions)}:{nameof(AccountProxyOptions.HttpClientTimeoutInSeconds)}"]))
            );
            services.Configure<AccountProxyOptions>(_configuration.GetSection(nameof(AccountProxyOptions)));

            //Add IntegrationsTests
            services.AddIntegrationTests();
            services.AddSingleton<IConfigureOptions<RollerCoasterDBOptions>, RollerCoasterDBOptionsConfigurator>();

        }

        IServiceCollection InitializeDependencyInjection()
        {
            var aspnetCoreEnvironment = Environment.GetEnvironmentVariable("BUILD_CONFIGURATION");
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{aspnetCoreEnvironment}.json", true);
            _configuration = builder.Build();
            var services = new ServiceCollection();
            services.AddSingleton(_configuration);
            return services;
        }
    }
}

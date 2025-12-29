using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.DomainServices;
using Agit.Domain;
using Agit.Domain.UnitOfWork;
using Serilog;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;


namespace TAMHR.ESS.SchedulerSHAStatus
{
    class Program
    {
        private static ServiceProvider _serviceProvider;
        static void Main(string[] args)
        {
            // Call setup configuration method.
            Setup();

            // Get and set reminder service object from DI container.
            var reminderService = _serviceProvider.GetService<ReminderService>();

            // Log for starting service.
            Log.Information("Starting Service...");

            // Job Update SHA Status vaccine.
            DoAction(async () => await reminderService.ScheduleUpdateSHAStatusVaccine(), "Success Update SHA Status Vaccine");

            // Job Update Status vaccine.
            DoAction(async () => await reminderService.SheduleUpdateStatusVaccine(), "Success Remind Schedule Update Status Vaccine");

            // Log for ending service.
            Log.Information("Ending Service...");

            // Close and flush the logs into file.
            Log.CloseAndFlush();

            // Dispose the DI container and all containing objects.
            Dispose();
        }

        /// <summary>
        /// Do an action and send to log.
        /// </summary>
        /// <param name="action">This asynchronous function delegate.</param>
        /// <param name="succesMessage">This success message.</param>
        private async static void DoAction(Func<Task> action, string succesMessage)
        {
            try
            {
                // Do an asynchronous action.
                await action();

                // Log information with success message.
                Log.Information(succesMessage);
            }
            catch (Exception ex)
            {
                // Log error with stack trace detail.
                Log.Error(ex, "Failed Remind");
            }
        }

        /// <summary>
        /// Dispose the DI container.
        /// </summary>
        private static void Dispose()
        {
            // Exit from this method if the DI container object is null.
            if (_serviceProvider == null) return;

            // If DI container is disposable then dispose the object.
            if (_serviceProvider is IDisposable)
            {
                // Dispose DI container object.
                _serviceProvider.Dispose();
            }
        }

        /// <summary>
        /// Setup configurations and dependency injections.
        /// </summary>
        private static void Setup()
        {
            // Load configuration builder.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            // Build configuration.
            var configuration = builder.Build();

            // Load connection string.
            var connectionString = configuration["ConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
                connectionString = Constanta.ConnectionString();

            // Load logging file path.
            var loggingFilePath = configuration["Logging:FilePath"];
            if (string.IsNullOrEmpty(loggingFilePath))
                loggingFilePath = Constanta.PathLog();

                // Setup and create logger object.
                Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(loggingFilePath, rollingInterval: RollingInterval.Month)
                .CreateLogger();

            // Add unit of work into DI container.
            var services = new ServiceCollection()
                .AddDbContext<IUnitOfWork, UnitOfWork>(options => options.UseSqlServer(connectionString));

            // Scan and register all domain service classes into DI container with transient lifetime.
            services.Scan(scanner => scanner.FromAssemblyOf<UnitOfWork>()
                .AddClasses(classes =>
                {
                    // Register all classes that inherit form DomainServiceBase abstract class into DI container.
                    classes.AssignableTo<DomainServiceBase>();
                })
                .AsSelf()
                .WithTransientLifetime()
            );

            // Build DI container.
            _serviceProvider = services.BuildServiceProvider();
        }
    }
}

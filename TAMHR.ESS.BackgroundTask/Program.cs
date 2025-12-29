using Agit.Domain.UnitOfWork;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TAMHR.ESS.Infrastructure;
using Serilog;
using System.Linq;
using System.Reflection;

namespace TAMHR.ESS.BackgroundTask
{
    public class Program
    {
        private static ServiceProvider _serviceProvider;
        public static void Main(string[] args)
        {
            // Call setup configuration method.
            Setup();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).ConfigureServices(services =>
                    services.AddHostedService<DerivedBackgroundWorker>()).UseWindowsService();
        //public static IHostBuilder CreateHostBuilder(string[] args) => 
        //    Host.CreateDefaultBuilder(args).UseWindowsService().ConfigureServices((hostContext, services) => 
        //        { services.AddHostedService<DerivedBackgroundWorker>(); });

        private static void Setup()
        {
            // Load configuration builder.
            var builder = new ConfigurationBuilder();
                //.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            builder.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).AddJsonFile("appsettings.json");
            //// Build configuration.
            //var configuration = builder.Build();

            //// Load connection string.
            //var connectionString = configuration["ConnectionString"];

            //// Load logging file path.
            //var loggingFilePath = configuration["Logging:FilePath"];

            //// Setup and create logger object.
            ////Log.Logger = new LoggerConfiguration()
            ////.MinimumLevel.Debug()
            ////.WriteTo.Console()
            ////.WriteTo.File(loggingFilePath, rollingInterval: RollingInterval.Month)
            ////.CreateLogger();

            //// Add unit of work into DI container.
            //var services = new ServiceCollection()
            //    .AddDbContext<IUnitOfWork, UnitOfWork>(options => options.UseSqlServer(connectionString));

            //// Scan and register all domain service classes into DI container with transient lifetime.
            //services.Scan(scanner => scanner.FromAssemblyOf<UnitOfWork>()
            //    .AddClasses(classes =>
            //    {
            //        // Register all classes that inherit form DomainServiceBase abstract class into DI container.
            //        classes.AssignableTo<DomainServiceBase>();
            //    })
            //    .AsSelf()
            //    .WithTransientLifetime()
            //);

            //// Build DI container.
            //_serviceProvider = services.BuildServiceProvider();
        }
    }
}

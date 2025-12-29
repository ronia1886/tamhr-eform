using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using TAMHR.ESS.RenamePDFService.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace TAMHR.ESS.RenamePDFService
{
    public class Program
    {
        private static IConfiguration Configuration;
        private static Serilog.ILogger Logger;

        public static void Main(string[] args)
        {
            // Set base path to application's directory
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Build configuration
            Configuration = new ConfigurationBuilder()
                .SetBasePath(baseDirectory)
                .AddJsonFile("appsettings-pdfservice.json", optional: false, reloadOnChange: true)
                .Build();

            // Configure logging
            ConfigureLogging();

            try
            {
                // Create host builder
                var host = CreateHostBuilder(args).Build();

                // Run the application
                var renamePdfService = host.Services.GetRequiredService<IRenamePdfService>();
                renamePdfService.RenamePDFs();
            }
            catch (Exception ex)
            {
                Logger.Error($"Host terminated unexpectedly: {ex.Message}");
            }
            finally
            {
                Logger.Information("Application closing.");
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Configure services
                    services.AddSingleton(Configuration);
                    services.AddLogging(loggingBuilder =>
                    {
                        loggingBuilder.AddSerilog(Logger, dispose: true); // Use Serilog's Logger directly
                        loggingBuilder.AddConsole(); // Add console logging
                    });
                    services.AddSingleton<IRenamePdfService, RenamePdfService>();
                });

        private static void ConfigureLogging()
        {
            // Specify log directory path relative to application's directory
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ebupotschedulerlog");

            Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .WriteTo.File(
                    GetLogFilePath(logDirectory),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    shared: true
                )
                .CreateLogger();

            // Set Serilog's logger
            Log.Logger = Logger;
        }

        private static string GetLogFilePath(string logDirectory)
        {
            // Ensure log directory exists
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Construct log file path based on current date
            string logFileName = $"ebupotschedulerlog-{DateTime.Today:yyyy-MM-dd}.txt";
            return Path.Combine(logDirectory, logFileName);
        }
    }
}

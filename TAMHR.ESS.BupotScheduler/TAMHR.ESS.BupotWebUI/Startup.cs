using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Data.SqlClient;
using TAMHR.ESS.RenamePDFService;
using TAMHR.ESS.RenamePDFService.Helpers;

namespace TAMHR.ESS.BupotWebUI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Register the RenamePdfService
            services.AddSingleton<IRenamePdfService, RenamePdfService>();

            // Add Hangfire services
            services.AddHangfire(config =>
            {
                config.UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection"));
            });

            // Add controllers if needed
            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            // Map Hangfire Dashboard as the default route
            app.UseHangfireDashboard(""); // No path specified to use root path

            // Remove default route mapping to prevent conflict
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // Add this line if you have controllers to map
            });

            // Ensure Hangfire server is started only once
            app.UseHangfireServer();

            // Schedule Hangfire jobs
            ScheduleHangfireJobs(app);

            // Log application stopping
            lifetime.ApplicationStopping.Register(() =>
            {
                Console.WriteLine("Application is stopping...");
            });
        }

        private void ScheduleHangfireJobs(IApplicationBuilder app)
        {
            var renamePdfService = app.ApplicationServices.GetService<IRenamePdfService>();

            if (renamePdfService != null)
            {
                string sqlConnectionString = Configuration.GetConnectionString("DefaultConnection");
                int jobIntervalMinutes = GetJobIntervalFromDatabase(sqlConnectionString);

                string jobId = "eBupot21Scheduler";
                string jobName = "Recurring job to rename eBupot 21";

                // Translate jobIntervalMinutes to a valid CRON expression
                string cronExpression = ConvertIntervalToCronExpression(jobIntervalMinutes);

                RecurringJob.AddOrUpdate(jobId, () => renamePdfService.RenamePDFs(), cronExpression);

                Console.WriteLine($"Job '{jobName}' with ID '{jobId}' has been scheduled based on the interval of {jobIntervalMinutes} minutes.");
            }
            else
            {
                Console.WriteLine($"Error: Unable to schedule job 'eBupot 21 Scheduler' because the RenamePdfService is not available.");
                throw new InvalidOperationException("RenamePdfService is not registered in the DI container.");
            }
        }

        private string ConvertIntervalToCronExpression(int intervalMinutes)
        {
            if (intervalMinutes <= 0)
            {
                throw new ArgumentException("Interval must be a positive integer.");
            }

            if (intervalMinutes % 60 == 0)
            {
                int hours = intervalMinutes / 60;
                if (hours > 0 && hours <= 24)
                {
                    return $"0 */{hours} * * *"; // Every 'hours' hours at the 0th minute
                }
                else
                {
                    throw new ArgumentException("Invalid interval: must be between 1 and 24 hours.");
                }
            }
            else
            {
                // Use the interval in minutes directly if it's not a multiple of 60
                return $"*/{intervalMinutes} * * * *";
            }
        }

        private int GetJobIntervalFromDatabase(string connectionString)
        {
            int jobIntervalMinutes = 10; // Default value if not found

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT ConfigValue FROM TB_M_CONFIG WHERE ConfigKey = 'Timeout.eBupotInterval'";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    object result = command.ExecuteScalar();
                    if (result != null && int.TryParse(result.ToString(), out int interval))
                    {
                        jobIntervalMinutes = interval;
                    }
                }
            }

            return jobIntervalMinutes;
        }
    }
}

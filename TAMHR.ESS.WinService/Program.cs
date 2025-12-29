using Agit.Domain;
using Agit.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.WinService.Jobs;

namespace TAMHR.ESS.WinService
{
    class Program
    {
        static void Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));
            var builder = BuildHost(args);

            if (isService)
            {
                builder.RunAsServiceAsync().GetAwaiter().GetResult();
            }
            else
            {
                builder.Build().RunAsync().GetAwaiter().GetResult();
            }
        }

        public static IHostBuilder BuildHost(string[] args) =>
            new HostBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    var dir = Directory.GetCurrentDirectory();
                    configHost.SetBasePath(dir);
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.AddJsonFile("appsettings.json", true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddOptions();

                    var connection = hostContext.Configuration["ConnectionString"];
                    services.AddDbContext<IUnitOfWork, UnitOfWork>(options => options.UseSqlServer(connection), ServiceLifetime.Transient);

                    //Scan assembly and register classes that implement domain service base class
                    services.Scan(scanner => scanner.FromAssemblyOf<UnitOfWork>()
                        .AddClasses(classes =>
                        {
                            classes.AssignableTo<DomainServiceBase>();
                        })
                        .AsSelf()
                        .WithTransientLifetime()
                    );

                    services.AddLogging();
                    services.AddQuartzHostedService(hostContext.Configuration);
                    services.AddTransient<QueueJob>();
                    services.AddTransient<ReminderJob>();
                })
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    configLogging.AddConsole();
                    configLogging.AddDebug();
                });

        //public static async Task RunScheduler()
        //{
        //    var props = new NameValueCollection
        //        {
        //            { "quartz.serializer.type", "json" }
        //        };
        //    var factory = new StdSchedulerFactory(props);
        //    var scheduler = await factory.GetScheduler();

        //    var job = JobBuilder.Create<QueueJob>()
        //        .WithIdentity("queueJob", "commonJobGroup")
        //        .Build();

        //    var trigger = TriggerBuilder.Create()
        //        .WithIdentity("commonTrigger", "commonJobGroup")
        //        .StartNow()
        //        .WithSimpleSchedule(x => x
        //            .WithIntervalInMinutes(5)
        //            .RepeatForever())
        //        .Build();

        //    await scheduler.ScheduleJob(job, trigger);
        //}
    }
}

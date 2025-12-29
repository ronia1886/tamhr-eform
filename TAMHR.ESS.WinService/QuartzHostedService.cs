using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TAMHR.ESS.WinService.Jobs;
using Quartz;
using System.Collections.Generic;

namespace TAMHR.ESS.WinService
{
    public class QuartzHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IScheduler _scheduler;

        public QuartzHostedService(ILogger<QuartzHostedService> logger, IScheduler scheduler)
        {
            _logger = logger;
            _scheduler = scheduler;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Quartz started...");

            var job = JobBuilder.Create<QueueJob>()
                .WithIdentity("queueJob", "commonJobGroup")
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithIdentity("commonTrigger", "commonJobGroup")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithInterval(TimeSpan.FromMinutes(90))
                    .RepeatForever()
                )
                .Build();

            var reminderJob = JobBuilder.Create<ReminderJob>()
                .WithIdentity("reminderJob", "reminderJobGroup")
                .Build();

            var reminderTrigger = TriggerBuilder.Create()
                .WithIdentity("reminderTrigger", "reminderJobGroup")
                .StartNow()
                //.WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromMinutes(20)).RepeatForever())
                .WithCronSchedule(string.Format("0 {0} {1} ? * *", 29, 13))
                .Build();

            var dictionary = new Dictionary<IJobDetail, IReadOnlyCollection<ITrigger>>();
            dictionary.Add(job, new HashSet<ITrigger>(new[] { trigger }));
            dictionary.Add(reminderJob, new HashSet<ITrigger>(new[] { reminderTrigger }));

            await _scheduler.ScheduleJobs(dictionary, false);
            await _scheduler.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Quartz stopped...");
            await _scheduler.Shutdown(cancellationToken);
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TAMHR.ESS.EmailReminderService.Jobs;
using Quartz;
using System.Collections.Generic;
using TAMHR.ESS.Infrastructure.DomainServices;
//using Microsoft.Extensions.Logging.Console.Internal;

namespace TAMHR.ESS.EmailReminderService
{
    public class QuartzHostedService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IScheduler _scheduler;
        private readonly ReminderService _reminderService;
        private readonly ConfigService _configService;

        public QuartzHostedService(ILogger<QuartzHostedService> logger, IScheduler scheduler,ReminderService reminderService, ConfigService configService)
        {
            _logger = logger;
            _scheduler = scheduler;
            _reminderService = reminderService;
            _configService = configService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Quartz started...");

            var getHour = _configService.GetConfigValue<string>("WeeklyWFHPlanning.HourReminder");
            var getMinutes = _configService.GetConfigValue<string>("WeeklyWFHPlanning.MinutesReminder");
            var getTestCron = _configService.GetConfig("test.cron");

            var cron = string.Format("0 {0} {1} ? * *", getMinutes, getHour);

            if (getTestCron != null && getTestCron.ConfigText == "True")
            {
                cron = getTestCron.ConfigValue;
            }

            var emailReminderJob = JobBuilder.Create<EmailReminderJob>()
                .WithIdentity("emailReminderJob", "emailReminderJobGroup")
                .Build();

            var reminderTrigger = TriggerBuilder.Create()
                .WithIdentity("emailReminderTrigger", "emailReminderJobGroup")
                .StartNow()
                .WithCronSchedule(cron)
                .Build();

            var dictionary = new Dictionary<IJobDetail, IReadOnlyCollection<ITrigger>>();
            dictionary.Add(emailReminderJob, new HashSet<ITrigger>(new[] { reminderTrigger }));

            await _scheduler.ScheduleJobs(dictionary, false);
            await _scheduler.Start(cancellationToken);

            // ======================
            // OHS EMAIL REMINDER
            // ======================
            _logger.LogInformation("Quartz OHS started...");

            var OhsGetTestCron = _configService.GetConfig("test.cron");
            var getCron = _configService.GetConfigValue<string>("CronReminderOHSAreaActivity");
            Console.WriteLine($"getCron>>{getCron}");
            var cronOhs = getCron;

            if (OhsGetTestCron != null && OhsGetTestCron.ConfigText == "True")
            {
                cronOhs = OhsGetTestCron.ConfigValue;
            }
            Console.WriteLine($"getCron2>>{cronOhs}");
            var ohsJobKey = new JobKey("OhsEmailReminderJob", "OhsEmailReminderJobGroup");
            //if (await _scheduler.CheckExists(ohsJobKey))
            //{
            //    await _scheduler.DeleteJob(ohsJobKey);
            //    _logger.LogInformation("Job OhsEmailReminderJob lama dihapus.");
            //}

            var OhsEmailReminderJob = JobBuilder.Create<OHSEmailReminderJob>()
                .WithIdentity(ohsJobKey)
                .Build();

            var OhsReminderTrigger = TriggerBuilder.Create()
                .WithIdentity("OhsEmailReminderTrigger", "OhsEmailReminderJobGroup")
                .StartNow()
                .WithCronSchedule(cronOhs)
                .Build();

            var dictionaryOhs = new Dictionary<IJobDetail, IReadOnlyCollection<ITrigger>>();
            dictionaryOhs.Add(OhsEmailReminderJob, new HashSet<ITrigger>(new[] { OhsReminderTrigger }));

            //await _scheduler.ScheduleJob(OhsEmailReminderJob, OhsReminderTrigger);
            await _scheduler.ScheduleJobs(dictionaryOhs, false);
            _logger.LogInformation("Job OhsEmailReminderJob berhasil dijadwalkan.");

            await _scheduler.Start(cancellationToken);
        }


        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Quartz stopped...");
            await _scheduler.Shutdown(cancellationToken);
        }

    }
}

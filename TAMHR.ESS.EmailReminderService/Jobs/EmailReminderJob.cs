using System.Threading.Tasks;
using TAMHR.ESS.Infrastructure.DomainServices;
using Quartz;
using System;
using Quartz.Impl.Triggers;
using Quartz.Impl;

namespace TAMHR.ESS.EmailReminderService.Jobs
{
    public class EmailReminderJob : IJob
    {
        private readonly EmailService _emailService;
        private readonly ConfigService _configService;
        private readonly ScheduleJobService _scheduleJobService;


        public EmailReminderJob(EmailService emailService,ConfigService configService, ScheduleJobService scheduleJobService)
        {
            _emailService = emailService;
            _configService = configService;
            _scheduleJobService = scheduleJobService;
        }

        public async Task Execute(IJobExecutionContext context)
        {

            var Id = await _scheduleJobService.CreateScheduleJob(nameof(EmailReminderJob));
            //await _emailService.sendEmailReminderWeeklyWFHPlanningAsync();
            var isTrue = _configService.GetConfig("UpdateOutstandingApprovalReminder").ConfigValue;
            if (isTrue.ToLower() == "true")
            {
                _emailService.UpdateOutstandingRoutingApprovalByOC();
            }

            //reschedule code after execution code:
            var getHour = _configService.GetConfigValue<string>("WeeklyWFHPlanning.HourReminder");
            var getMinutes = _configService.GetConfigValue<string>("WeeklyWFHPlanning.MinutesReminder");

            var getTestCron = _configService.GetConfig("test.cron");

            var newCron = string.Format("0 {0} {1} ? * *", getMinutes, getHour);

            if (getTestCron != null && getTestCron.ConfigText == "True")
            {
                newCron = getTestCron.ConfigValue;
            }
            // retrieve the trigger
            var oldTrigger = await context.Scheduler.GetTrigger(new TriggerKey("emailReminderTrigger", "emailReminderJobGroup"));

            // obtain a builder that would produce the trigger
            var tb = oldTrigger.GetTriggerBuilder();

            // update the schedule associated with the builder, and build the new trigger
            var newTrigger = tb.StartAt(DateTime.Now.AddSeconds(10)).WithCronSchedule(newCron).Build();

            await context.Scheduler.RescheduleJob(oldTrigger.Key, newTrigger);
            await _scheduleJobService.FinishScheduleJob(Id);
            
        }
    }
}

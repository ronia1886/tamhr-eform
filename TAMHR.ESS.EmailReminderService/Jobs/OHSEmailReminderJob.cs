using System;
using System.Threading.Tasks;
using Quartz;
using TAMHR.ESS.Infrastructure.DomainServices;
using Microsoft.Extensions.Logging;

namespace TAMHR.ESS.EmailReminderService.Jobs
{
    public class OHSEmailReminderJob : IJob
    {
        private readonly EmailService _emailService;
        private readonly ConfigService _configService;
        private readonly ScheduleJobService _scheduleJobService;
        private readonly ILogger<OHSEmailReminderJob> _logger;

        public OHSEmailReminderJob(
            EmailService emailService,
            ConfigService configService,
            ScheduleJobService scheduleJobService,
            ILogger<OHSEmailReminderJob> logger)
        {
            _emailService = emailService;
            _configService = configService;
            _scheduleJobService = scheduleJobService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("OHSEmailReminderJob mulai dieksekusi...");

            try
            {
                // 1️ Catat ID jadwal di database/log
                var jobId = await _scheduleJobService.CreateScheduleJob(nameof(OHSEmailReminderJob));

                // 2️ Kirim Email
                await _emailService.sendEmailReminderOHSAsync();

                // 3 Ambil konfigurasi cron baru
                var getTestCron = _configService.GetConfig("test.cron");
                var getCron = _configService.GetConfigValue<string>("CronReminderOHSAreaActivity");
                Console.WriteLine($"getCron3>>{getCron}");
                var newCron = getCron;

                if (getTestCron != null && getTestCron.ConfigText == "True")
                {
                    newCron = getTestCron.ConfigValue;
                }
                Console.WriteLine($"getCron4>>{newCron}");
                if (string.IsNullOrEmpty(newCron))
                {
                    _logger.LogWarning("Konfigurasi Cron tidak ditemukan, job tidak akan dijadwalkan ulang.");
                    return;
                }

                // 4 Reschedule Job dengan konfigurasi baru
                var triggerKey = new TriggerKey("OhsEmailReminderTrigger", "OhsEmailReminderJobGroup");
                var oldTrigger = await context.Scheduler.GetTrigger(triggerKey);

                if (oldTrigger == null)
                {
                    _logger.LogError("Trigger dengan key {TriggerKey} tidak ditemukan!", triggerKey);
                    return;
                }

                var newTrigger = oldTrigger.GetTriggerBuilder()
                    .StartAt(DateTime.UtcNow.AddSeconds(10))
                    .WithCronSchedule(newCron)
                    .Build();

                await context.Scheduler.RescheduleJob(oldTrigger.Key, newTrigger);
                _logger.LogInformation("Job OHSEmailReminder berhasil dijadwalkan ulang dengan cron baru: {Cron}", newCron);

                // 6️⃣ Tandai pekerjaan selesai
                await _scheduleJobService.FinishScheduleJob(jobId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Terjadi kesalahan saat mengeksekusi OHSEmailReminderJob.");
            }
        }
    }
}

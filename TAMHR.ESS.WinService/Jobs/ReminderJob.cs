using System.Threading.Tasks;
using TAMHR.ESS.Infrastructure.DomainServices;
using Quartz;

namespace TAMHR.ESS.WinService.Jobs
{
    public class ReminderJob : IJob
    {
        private readonly EmailService _emailService;

        public ReminderJob(EmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _emailService.SendReminderAsync();
            //await _emailService.SendBpkbDateReminderAsync();
            //await _emailService.SendGetBpkbReminderAsync();
        }
    }
}

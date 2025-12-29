using System.Threading.Tasks;
using TAMHR.ESS.Infrastructure.DomainServices;
using Quartz;
using System;

namespace TAMHR.ESS.WinService.Jobs
{
    public class QueueJob : IJob
    {
        private readonly EmailService _emailService;

        public QueueJob(EmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                //await _emailService.SendQueueAsync();
            }
            catch(Exception ex)
            {

            }
        }
    }
}

using System;
using System.Threading.Tasks;
using TAMHR.ESS.Infrastructure.DomainServices;
using Quartz;

namespace TAMHR.ESS.WinService.Jobs
{
    public class ClearTempJob : IJob
    {
        private readonly CoreService _coreService;

        public ClearTempJob(CoreService coreService)
        {
            _coreService = coreService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}

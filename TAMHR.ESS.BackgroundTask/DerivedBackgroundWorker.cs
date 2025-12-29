using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TAMHR.ESS.BackgroundTask
{
    public class DerivedBackgroundWorker: BackgroundService
    {
        public readonly IWorker worker;
        public DerivedBackgroundWorker(IWorker worker)
        {
            this.worker = worker;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await worker.DoWorkAsync(stoppingToken);
        }
    

    }

}
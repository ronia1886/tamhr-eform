using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TAMHR.ESS.BackgroundTask
{
    public class BackgroundWorker: IHostedService
    {
        private readonly ILogger<BackgroundWorker> logger;
        private readonly IWorker worker;
        public BackgroundWorker(ILogger<BackgroundWorker> logger, IWorker worker) {
            this.logger = logger;
            this.worker = worker;
        }

        public async Task StartAsync (CancellationToken cancellationToken)
        {
            await worker.DoWorkAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Background worker stopping");
            return Task.CompletedTask;
        }

    }
}

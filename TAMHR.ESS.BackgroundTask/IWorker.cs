using System.Threading;
using System.Threading.Tasks;

namespace TAMHR.ESS.BackgroundTask
{
    public interface IWorker
    {
        Task DoWorkAsync(CancellationToken cancellationToken);
    }
}
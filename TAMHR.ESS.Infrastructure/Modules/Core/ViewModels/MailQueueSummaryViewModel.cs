using System;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class MailQueueSummaryViewModel
    {
        public int Total { get { return TotalPending + TotalSent; } }
        public int TotalPending { get; set; }
        public int TotalSent { get; set; }
        public int TotalNew { get { return TotalNewPending + TotalNewSent; } }
        public int TotalNewPending { get; set; }
        public int TotalNewSent { get; set; }
        public int TotalAverage { get; set; }
        public int TotalPendingPercent { get { return (int)Math.Round((decimal)TotalPending / Total); } }
        public int TotalSentPercent { get { return 100 - TotalPendingPercent; } }
        public int TotalNewPendingPercent { get { return (int)Math.Round((decimal)TotalNewPending / TotalNew); } }
        public int TotalNewSentPercent { get { return 100 - TotalNewPendingPercent; } }
        public int TotalPendingTask { get; set; }
        public int TotalTemporaryFiles { get; set; }

        public MailQueueSummaryViewModel(int totalPending, int totalSent, int totalNewPending, int totalNewSent, int totalAverage, int totalPendingTask, int totalTemporaryFiles)
        {
            TotalPending = totalPending;
            TotalSent = totalSent;
            TotalNewPending = totalNewPending;
            TotalNewSent = totalNewSent;
            TotalAverage = totalAverage;
            TotalPendingTask = totalPendingTask;
            TotalTemporaryFiles = totalTemporaryFiles;
        }
    }
}

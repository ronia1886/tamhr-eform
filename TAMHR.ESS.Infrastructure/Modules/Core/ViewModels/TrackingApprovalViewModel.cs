using TAMHR.ESS.Domain;
using System.Collections.Generic;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class TrackingApprovalViewModel
    {
        public int Progress { get; }
        public string DocumentStatusCode { get; set; }
        public IEnumerable<TrackingApproval> TrackingApprovals { get; }

        public TrackingApprovalViewModel(int progress, string documentStatusCode, IEnumerable<TrackingApproval> trackingApprovals)
        {
            Progress = progress;
            DocumentStatusCode = documentStatusCode;
            TrackingApprovals = trackingApprovals;
        }
    }
}

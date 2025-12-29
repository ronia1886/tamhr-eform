using System;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class BdjkRequestUpdateViewModel
    {
        public Guid Id { get; set; }
        public DateTime WorkingDate { get; set; }
        public string BdjkCode { get; set; }
        public string ActivityCode { get; set; }
        public string BdjkReason { get; set; }
        public bool Taxi { get; set; }
        public bool UangMakanDinas { get; set; }
        public Guid DocumentApprovalId { get; set; }
        public Guid ParentId { get; set; }
    }

    public class BdjkRequestViewModel
    {
        public Guid Id { get; set; }
        public DateTime WorkingDate { get; set; }
        public string BdjkCode { get; set; }
        public string ActivityCode { get; set; }
        public string BdjkReason { get; set; }
        public bool Taxi { get; set; }
        public bool UangMakanDinas { get; set; }
    }

    public class BdjkPlanningViewModel
    {
        public DateTime Period { get; set; }
        public BdjkRequestViewModel[] Details { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class ShiftPlanningReportViewModel
    {
        public Guid Id { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentStatusCode { get; set; }
        public string ShiftCode { get; set; }
        public DateTime? DateShift { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Approver { get; set; }
        public dynamic listRequest { get; set; }

    }
}

using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("VW_ANNUAL_LEAVE_PLANNING_DETAIL")]
    public class AnnualLeavePlanningDetailView : IEntityMarker
    {
        public Guid Id { get; set; }
        public Guid AnnualLeavePlanningId { get; set; }
        public Guid DocumentApprovalId { get; set; }
        public Guid AbsentId { get; set; }
        public string AbsenceType { get; set; }
        public string Reason { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Days { get; set; }
        public string NoReg { get; set; }
        public int YearPeriod { get; set; }
        public int Version { get; set; }
        public bool IsUpdated { get; set; }
    }
}

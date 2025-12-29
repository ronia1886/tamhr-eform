
using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_ANNUAL_LEAVE_PLAN", DatabaseObjectType.StoredProcedure)]
    public class AnnualLeavePlanStoredEntity
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
        public string DisplayText { get; set; }
    }
}

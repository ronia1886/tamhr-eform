
using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GENERATE_TERMINATION_EXIT_CLEARANCE_PDF", DatabaseObjectType.StoredProcedure)]
    public class TerminationExitClearanceStoredEntity
    {
        public string About { get; set; }
        public string LeadTime { get; set; }
        public string Code { get; set; }
        public string RolePIC { get; set; }
        public string ApprovalActionCode { get; set; }
        public Guid? DocumentApprovalId { get; set; }
        public string NoReg { get; set; }
        public string PICName { get; set; }
        public string Salutation { get; set; }
        public string Division { get; set; }
        public string Email { get; set; }
        public DateTime? CompleteDate { get; set; }
        public string Place { get; set; }
        public string Remarks { get; set; }
    }
}

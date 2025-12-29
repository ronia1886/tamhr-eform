using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_TERMINATION_APPROVAL_HISTORY", DatabaseObjectType.StoredProcedure)]
    public class TerminationApprovalHistoryStoredEntity
    {
        public Guid Id { get; set; }
        public Guid DocumentApprovalId { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string JobName { get; set; }
        public string ApprovalActionCode { get; set; }
        public string RoleKey { get; set; }
    }
}

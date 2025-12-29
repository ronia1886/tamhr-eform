
using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_TERMINATION_APPROVER_LEVEL", DatabaseObjectType.StoredProcedure)]
    public class TerminationApproverLevelStoredEntity
    {
        public int ApproverLevel { get; set; }
    }
}

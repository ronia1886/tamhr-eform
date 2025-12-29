using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_TERMINATION_ROLLBACK", DatabaseObjectType.StoredProcedure)]
    public class TerminationRollback
    {
        public Guid DocumentApprovalId { get; set; }
    }
}

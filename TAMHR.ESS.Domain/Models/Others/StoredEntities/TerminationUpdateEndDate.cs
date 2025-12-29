using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_UPDATE_TERMINATION_DATE", DatabaseObjectType.StoredProcedure)]
    public class TerminationUpdateEndDate
    {
        public string Noreg { get; set; }
        public DateTime EndDate { get; set; }
        public Guid DocumentApprovalId { get; set; }
    }
}

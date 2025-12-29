using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_TERMINATION_UPDATE_ABNORMAL", DatabaseObjectType.StoredProcedure)]
    public class TerminationUpdateAbnormal
    {
        public Guid TerminationId { get; set; }
        public DateTime OldDate { get; set; }
    }
}

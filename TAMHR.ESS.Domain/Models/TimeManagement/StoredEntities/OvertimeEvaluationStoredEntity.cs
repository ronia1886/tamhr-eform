using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GENERATE_OVERTIME_EVALUATION", DatabaseObjectType.StoredProcedure)]
    public class OvertimeEvaluationStoredEntity
    {
        public string NoReg { get; set; }
        public DateTime OvertimeDate { get; set; }
        public decimal Duration { get; set; }
    }
}

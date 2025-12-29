using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GENERATE_TIME_EVALUATION_SPKL", DatabaseObjectType.StoredProcedure)]
    public class TimeEvaluationOvertimeStoredEntity
    {
        public string NoReg { get; set; }
        public string Name { get; set; }
        public int Day { get; set; }
        public decimal DurationAdjust { get; set; }
    }
}

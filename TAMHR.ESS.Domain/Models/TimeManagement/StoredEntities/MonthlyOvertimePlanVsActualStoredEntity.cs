
using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_MONTHLY_OT_PLAN_VS_ACTUAL", DatabaseObjectType.StoredProcedure)]
    public class MonthlyOvertimePlanVsActualStoredEntity
    {
        public string NoReg { get; set; }
        public int YearPeriod { get; set; }
        public int MonthPeriod { get; set; }
        public string Division { get; set; }
        public double AnnualPlan { get; set; }
        public double Used { get; set; }
        public double Remaining { get; set; }
        public double OTIndex { get; set; }
    }
}

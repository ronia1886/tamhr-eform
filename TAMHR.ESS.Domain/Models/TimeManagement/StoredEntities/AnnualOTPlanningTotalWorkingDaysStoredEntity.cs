
using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GENERATE_ANNUAL_OT_PLANNING_TOTAL_WORKING_DAYS", DatabaseObjectType.StoredProcedure)]
    public class AnnualOTPlanningTotalWorkingDaysStoredEntity
    {
        public string NoReg { get; set; }
        public string Name { get; set; }
        public int YearPeriod { get; set; }
        public string Month { get; set; }
        public int Days { get; set; }
    }
}

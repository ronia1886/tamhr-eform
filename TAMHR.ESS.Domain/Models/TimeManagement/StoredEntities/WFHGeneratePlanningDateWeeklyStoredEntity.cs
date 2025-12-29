
using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GENERATE_WEEKLY_WFH_PLANNING_DATE", DatabaseObjectType.StoredProcedure)]
    public class WFHGeneratePlanningDateWeekly
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}

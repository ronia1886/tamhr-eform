using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_PAYROLL_SUMMARY_REPORT", DatabaseObjectType.StoredProcedure)]
    public class PayrollSummaryReportStoredEntity
    {
        public string OrgCode { get; set; }
        public string Division { get; set; }
        public string TotalMP { get; set; }
        public string TotalWorkingDay { get; set; }
        public string LHTotalMP { get; set; }
        public string LHTotalLostHour { get; set; }
        public string OTTotalMP { get; set; }
        public string OTTotalOvertime { get; set; }
        public string ActualWorkHour { get; set; }
        public DateTime KeyDate { get; set; }
    }
}

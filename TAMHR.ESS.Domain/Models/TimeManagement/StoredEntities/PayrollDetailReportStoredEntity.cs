using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_PAYROLL_DETAIL_REPORT", DatabaseObjectType.StoredProcedure)]
    public class PayrollDetailReportStoredEntity
    {
        public string NoReg { get; set; }
        public string TotalWorkingDay { get; set; }
        public string LHTotalLostHour { get; set; }
        public string OTTotalOverTime { get; set; }
        public string ActualWorkHour { get; set; }
        public string Directorate { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string Line { get; set; }
        public string Group { get; set; }
        public string Class { get; set; }
        public string PostName { get; set; }
        public string JobName { get; set; }
        public string OTType { get; set; }
        public DateTime KeyDate { get; set; }
        public string Name { get; set; }
    }
}

using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_OVERTIME_LOG_TRACKING", DatabaseObjectType.StoredProcedure)]
    public class OvertimeLogTrackingStoredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? ProxyIn { get; set; }
        public DateTime? ProxyOut { get; set; }
        public string CodeOfHour { get; set; }
        public string Activity { get; set; }
        public string Reason { get; set; }
        public string CreatedName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Type { get; set; }
        public string InputBy { get; set; }
        public string Directorate { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string Line { get; set; }
        public string Group { get; set; }
        public string Class { get; set; }
        public string PostName { get; set; }
        public string JobName { get; set; }
        public string Name { get; set; }
        public string DocumentNumber { get; set; }
        public Guid? DocumentApprovalId { get; set; }
        public string DocumentURL { get; set; }
    }
}

using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_ABSENCE_LOG_TRACKING", DatabaseObjectType.StoredProcedure)]
    public class AbsenceLogTrackingStoredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public DateTime? ProxyIn { get; set; }
        public DateTime? ProxyOut { get; set; }
        public string AbsentStatus { get; set; }
        public string AbsentName { get; set; }
        public DateTime? MemoProxyIn { get; set; }
        public DateTime? MemoProxyOut { get; set; }
        public string MemoAbsentStatus { get; set; }
        public string MemoAbsentName { get; set; }
        public string CreatedName { get; set; }
        public DateTime? CreatedDate { get; set; }
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

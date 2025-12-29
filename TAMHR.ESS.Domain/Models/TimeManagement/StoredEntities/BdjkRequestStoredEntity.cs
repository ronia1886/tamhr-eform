using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_BDJK_REQUEST", DatabaseObjectType.StoredProcedure)]
    public class BdjkRequestStoredEntity
    {
        public Guid Id { get; set; }
        public Guid DocumentApprovalId { get; set; }
        public string DocumentStatusCode { get; set; }
        public DateTime WorkingDate { get; set; }
        public string ShiftCode { get; set; }
        public DateTime? NormalTimeIn { get; set; }
        public DateTime? NormalTimeOut { get; set; }
        public bool DinasLuar { get; set; }
        public DateTime? ProxyIn { get; set; }
        public DateTime? ProxyOut { get; set; }
        public int AbsentStatus { get; set; }
        public string BdjkCode { get; set; }
        public bool Selected { get; set; }
        public string ActivityCode { get; set; }
        public string ActivityName { get; set; }
        public string LastApproverName { get; set; }
        public string BdjkReason { get; set; }
        public bool Taxi { get; set; }
        public bool UangMakanDinas { get; set; }
        public bool CanClaimTaxi { get; set; }
        public bool CanClaimUangMakanDinas { get; set; }
        public int WorkDuration { get; set; }
        public int Overtime { get; set; }
        public int TotalRequest { get; set; }

        // Status field, based on comparison result with annual BDJK Plan
        public string Remark { get; set; }
    }
}

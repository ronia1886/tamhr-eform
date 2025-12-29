using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("VW_MASTER_DATA_BDJK")]
    public class MasterDataBdjkView : IEntityMarker
    {
        public Guid Id { get; set; }
        public Guid? AbnormalityBdjkId { get; set; }
        public Guid? DocumentApprovalId { get; set; }
        public Guid? ParentId { get; set; }
        public string DocumentNumber { get; set; }
        public string ParentDocumentNumber { get; set; }
        public string DocumentStatusCode { get; set; }
        public string DocumentStatusName { get; set; }
        public bool? EnableDocumentAction { get; set; }
        public string BdjkCode { get; set; }
        public int? Progress { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public DateTime WorkingDate { get; set; }
        public DateTime? WorkingTimeIn { get; set; }
        public DateTime? WorkingTimeOut { get; set; }
        public string ActivityCode { get; set; }
        public bool? UangMakanDinas { get; set; }
        public bool? Taxi { get; set; }
        public string ActivityName { get; set; }
        public string BdjkReason { get; set; }
        public string EmployeeSubgroup { get; set; }
        public decimal? Duration { get; set; }
        public string OrgCode { get; set; }
        [NotMapped]
        public int OrgLevel { get; set; }
        [NotMapped]
        public List<string> ListBdjkCode { get; set; }
        [NotMapped]
        public string NoRegEdit { get; set; }
        [NotMapped]
        public string ActivityCodeEdit { get; set; }
        [NotMapped]
        public bool IsEdit { get; set; }
        public DateTime CreatedOn { get; set; }
    }   
}

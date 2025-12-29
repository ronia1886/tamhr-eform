using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_TIME_MANAGEMENT_SPKL_REQUEST")]
    public partial class SpklRequest : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid TempId { get; set; }
        public Guid? DocumentApprovalId { get; set; }
        public string NoReg { get; set; }
        public DateTime OvertimeDate { get; set; }
        public DateTime OvertimeIn { get; set; }
        public DateTime OvertimeOut { get; set; }
        public DateTime? OvertimeInAdjust { get; set; }
        public DateTime? OvertimeOutAdjust { get; set; }
        public int OvertimeBreak { get; set; }
        public int? OvertimeBreakAdjust { get; set; }
        public decimal Duration { get; set; }
        public decimal? DurationAdjust { get; set; }
        public string OvertimeCategoryCode { get; set; }
        public string OvertimeReason { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        [JsonIgnore]
        public DocumentApproval DocumentApproval { get; set; }
        public int Progress { get { return DocumentApproval != null ? DocumentApproval.Progress : 0; } }
        public string DocumentNumber { get { return DocumentApproval?.DocumentNumber; } }
        public string DocumentStatusCode { get { return DocumentApproval?.DocumentStatusCode; } }
    }
}

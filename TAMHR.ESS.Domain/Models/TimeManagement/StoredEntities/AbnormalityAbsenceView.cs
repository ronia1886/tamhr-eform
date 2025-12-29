using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("VW_ABNORMALITY_ABSENCE")]
    public class AbnormalityAbsenceView : IEntityMarker
    {
        public Guid Id { get; set; }
        public Guid? AbnormalityAbsenceId { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string PostName { get; set; }
        public DateTime WorkingDate { get; set; }
        public DateTime AbnormalityWorkingDate { get; set; }
        public DateTime? NormalTimeIn { get; set; }
        public DateTime? NormalTimeOut { get; set; }
        public DateTime? WorkingTimeIn { get; set; }
        public DateTime? WorkingTimeOut { get; set; }
        public string ShiftCode { get; set; }
        public string AbsentStatus { get; set; }
        public string AbsentName { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool? RowStatus { get; set; }
        public string Reason { get; set; }
        public Guid? DocumentApprovalId { get; set; }
        public Guid? CommonFileId { get; set; }
        public DateTime? AbnormaityProxyIn { get; set; }
        public DateTime? AbnormalityProxyOut { get; set; }
        public string AbnormalityAbsenStatus { get; set; }
        public string AbnormalityAbsentName { get; set; }
        public string AbnormalityStatus { get; set; }
    }
}

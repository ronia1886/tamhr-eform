using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_TIME_MANAGEMENT_BDJK_REQUEST")]
    public partial class BdjkRequest : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public Guid DocumentApprovalId { get; set; }
        public string NoReg { get; set; }
        public DateTime WorkingDate { get; set; }
        public string BdjkCode { get; set; }
        public bool Taxi { get; set; }
        public bool UangMakanDinas { get; set; }
        public string ActivityCode { get; set; }
        public string BdjkReason { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public decimal? BdjkDuration { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;
using System.Collections.Generic;
namespace TAMHR.ESS.Domain
{
    [Table("TB_R_ABNORMALITY_BDJK")]
    public partial class AbnormalityBdjk : IEntityBase<Guid>
    {
       
        [Key]
        public Guid Id { get; set; }
        public Guid? TimeManagementBdjkId { get; set; }
        public Guid? DocumentApprovalId { get; set; }
        public string NoReg { get; set; }
        public string Status { get; set; }
        public DateTime WorkingDate { get; set; }
        public DateTime? WorkingTimeIn { get; set; }
        public DateTime? WorkingTimeOut { get; set; }
        public string BDJKCode { get; set; }
        public bool Taxi { get; set; }
        public bool UangMakanDinas { get; set; }
        public string ActivityCode { get; set; }
        public string BDJKReason { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public decimal? BDJKDuration { get; set; }
        [NotMapped]
        public List<string> ListBdjkCode { get; set; }
        [NotMapped]
        public int Level { get; set; }

    }

    public partial class ListBdjkCode
    {
        public string Name;
    }
}

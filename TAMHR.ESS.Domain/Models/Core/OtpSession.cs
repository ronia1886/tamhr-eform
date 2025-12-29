using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_OTP_SESSION")]
    public partial class OtpSession : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string SessionId { get; set; }
        public string ModuleCode { get; set; }
        public string Token { get; set; }
        public string Algorithm { get; set; }
        public bool IsActive { get; set; }
        public DateTime ExpiredOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}

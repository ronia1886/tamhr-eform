using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    
    [Table("TB_M_USER_IMPERSONATION")]
    public partial class UserImpersonation : IEntityBase<Guid>, IUserImpersonation
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string PostCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}

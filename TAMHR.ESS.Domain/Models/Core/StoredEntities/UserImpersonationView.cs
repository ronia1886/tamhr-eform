using System;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("VW_USER_IMPERSONATION")]
    public partial class UserImpersonationView : IUserImpersonation, IEntityMarker
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string PostCode { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = new DateTime(9999, 12, 31);
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public string ImpersonateNoReg { get; set; }
        public string ImpersonateName { get; set; }
        public string ImpersonateUsername { get; set; }
        public string PostName { get; set; }
        public string JobName { get; set; }
    }
}

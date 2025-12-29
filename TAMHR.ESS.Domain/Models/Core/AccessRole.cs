using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_ACCESS_ROLE")]
    public partial class AccessRole : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string AccessCode { get; set; }
        public string AccessTypeCode { get; set; }
        public Guid RoleId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public Role Role { get; set; }
    }
}

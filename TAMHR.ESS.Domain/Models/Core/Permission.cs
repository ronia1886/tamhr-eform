using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_PERMISSION")]
    public partial class Permission : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string PermissionKey { get; set; }
        public string Description { get; set; }
        public string PermissionTypeCode { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}

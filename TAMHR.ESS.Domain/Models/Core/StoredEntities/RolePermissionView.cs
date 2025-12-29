using System;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("VW_ROLE_PERMISSION")]
    public partial class RolePermissionView : IEntityMarker
    {
        public Guid Id { get; set; }
        public Guid PermissionId { get; set; }
        public Guid RoleId { get; set; }
        public string PermissionKey { get; set; }
        public string RoleKey { get; set; }
    }
}

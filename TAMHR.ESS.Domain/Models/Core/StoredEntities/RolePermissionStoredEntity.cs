using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_ROLE_PERMISSION", DatabaseObjectType.TableValued)]
    public partial class RolePermissionStoredEntity
    {
        public Guid Id { get; set; }
        public string PermissionKey { get; set; }
        public string Description { get; set; }
        public string PermissionTypeCode { get; set; }
        public bool Active { get; set; }
    }
}

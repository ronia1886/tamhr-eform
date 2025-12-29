using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_USER_ROLE", DatabaseObjectType.TableValued)]
    public partial class UserRoleStoredEntity
    {
        public Guid Id { get; set; }
        public string RoleKey { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string RoleTypeCode { get; set; }
        public bool Active { get; set; }
    }
}

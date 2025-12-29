
using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_TERMINATION_USER_ROLE_KEY", DatabaseObjectType.StoredProcedure)]
    public class TerminationUserRoleKeyStoredEntity
    {
        public string NoReg { get; set; }
        public string RoleKey { get; set; }
    }
}

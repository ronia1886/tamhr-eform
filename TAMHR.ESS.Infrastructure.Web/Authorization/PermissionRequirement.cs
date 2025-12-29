using Agit.Common.Attributes;
using Microsoft.AspNetCore.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.Infrastructure.Web.Authorization
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionKey RequiredPermission { get; }

        public PermissionRequirement(PermissionKey requiredPermission)
        {
            RequiredPermission = requiredPermission;
        }

        public override string ToString()
        {
            return StringEnum.GetStringValue(RequiredPermission);
        }
    }
}

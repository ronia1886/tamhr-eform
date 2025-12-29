using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Collections.Concurrent;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.ContextPrincipal;
using Agit.Common.Extensions;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Infrastructure.Web.Authorization
{
    /// <summary>
    /// Access control list helper class
    /// </summary>
    public class AclHelper
    {
        /// <summary>
        /// Thread safe list
        /// </summary>
        private readonly ConcurrentBag<string> _concurrentBags = new ConcurrentBag<string>();

        /// <summary>
        /// Readonly field that hold reference to CoreService object
        /// </summary>
        private readonly CoreService _coreService;

        /// <summary>
        /// Readonly field that hold reference to IHttpContextAccessor object
        /// </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Field that hold user claim data
        /// </summary>
        private UserClaim _userClaim;

        /// <summary>
        /// Check whether user has temporary permission or not
        /// </summary>
        /// <returns>True if exist, false otherwise</returns>
        public bool HasTemporaryPermission => _concurrentBags.Count > 0;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="coreService">CoreService Object</param>
        /// <param name="httpContextAccessor">IHttpContextAccessor Object</param>
        public AclHelper(CoreService coreService, IHttpContextAccessor httpContextAccessor)
        {
            _coreService = coreService;
            _httpContextAccessor = httpContextAccessor;
            _userClaim = UserClaim.CreateFrom(_httpContextAccessor.HttpContext.User);
        }

        /// <summary>
        /// Set temporary permission to concurrent bags
        /// </summary>
        /// <param name="permissions">List of Permissions</param>
        public void SetTemporaryPermissions(IEnumerable<string> permissions)
        {
            permissions.ForEach(x => _concurrentBags.Add(x));
        }

        /// <summary>
        /// Check whether user has permission
        /// </summary>
        /// <param name="permissionKey">Permission Key</param>
        /// <returns>True if exist, false otherwise</returns>
        public bool HasPermission(string permissionKey, bool checkInPermission = true)
        {
            return _concurrentBags.Any(x => x.ToLower() == permissionKey.ToLower()) || (checkInPermission && _coreService.HasPermission(permissionKey, _userClaim.Roles));
        }

        /// <summary>
        /// Check whether user has permission
        /// </summary>
        /// <param name="permissionKey">Permission Key</param>
        /// <returns>True if exist, false otherwise</returns>
        public bool HasPermission(PermissionKey permissionKey, bool checkInPermission = true)
        {
            var permissionKeyStr = StringEnum.GetStringValue(permissionKey);

            return HasPermission(permissionKeyStr, checkInPermission);
        }
    }
}

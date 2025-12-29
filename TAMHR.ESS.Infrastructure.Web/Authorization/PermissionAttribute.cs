using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Extensions;
using TAMHR.ESS.Infrastructure.Web.ContextPrincipal;
using TAMHR.ESS.Infrastructure.Web.Models;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.Infrastructure.Web.Authorization
{
    public class PermissionAttribute : TypeFilterAttribute
    {
        public PermissionAttribute(PermissionKey permission)
            : base(typeof(PermissionAttributeHandler))
        {
            Arguments = new[] { new PermissionRequirement(permission) };
        }

        private class PermissionAttributeHandler : Attribute, IAuthorizationFilter
        {
            private CoreService _coreService;
            private PermissionRequirement _permissionRequirement;

            public PermissionAttributeHandler(CoreService coreService, PermissionRequirement permissionRequirement)
            {
                _coreService = coreService;
                _permissionRequirement = permissionRequirement;
            }

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                var userClaim = UserClaim.CreateFrom(context.HttpContext.User);
                var permissionName = _permissionRequirement.ToString();

                if (!_coreService.HasPermission(permissionName, userClaim.Roles))
                {
                    if (context.HttpContext.Request.IsAjaxRequest())
                    {
                        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        context.Result = new JsonResult(new ExceptionInfo("Access Control List", $"You dont have permission to access permission with name '{permissionName}'"));
                    }
                    else
                    {
                        throw new Exception("You dont have permission to access this page");
                    }
                }
            }
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TAMHR.ESS.Infrastructure.Extensions;
using TAMHR.ESS.Infrastructure.Web.Extensions;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using Agit.Common.Utility;

namespace TAMHR.ESS.Infrastructure.Web.Filters
{
    public class DocumentApprovalFilterAttribute : TypeFilterAttribute
    {
        public DocumentApprovalFilterAttribute()
            : base(typeof(DocumentApprovalFilter))
        {
        }

        private class DocumentApprovalFilter : IAsyncActionFilter
        {
            private readonly ApprovalService _approvalService;
            private readonly AclHelper _aclHelper;
            public DocumentApprovalFilter(ApprovalService approvalService, AclHelper aclHelper)
            {
                _approvalService = approvalService;
                _aclHelper = aclHelper;
            }
            
            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                var contextInfo = context.RouteData.Extract();
                
                if (ObjectHelper.IsIn(contextInfo.Action.ToLower(), "view", "load"))
                {
                    var formKey = context.ActionArguments["formKey"].ToString();
                    var viewName = formKey.Replace("-", "");
                    var noreg = context.HttpContext.User.GetClaim("NoReg");
                    var permissions = _aclHelper.HasPermission($"Form.{viewName}.Create")
                        ? new string[] { "Core.Approval.Edit", "Core.Approval.Submit", "Core.Approval.Cancel", "Core.Approval.ViewAction" }
                        : new string[] { };

                    if (contextInfo.Action.ToLower() == "view" && context.ActionArguments.ContainsKey("docId"))
                    {
                        var docId = (Guid)context.ActionArguments["docId"];
                        var canViewAllDocumentApprovals = _aclHelper.HasPermission(ApplicationConstants.PermissionKey.ViewAllDocumentApprovals);

                        permissions = _approvalService.GeneratePermissions(noreg, docId, canViewAllDocumentApprovals).ToArray();
                    }

                    _aclHelper.SetTemporaryPermissions(permissions);
                }

                var result = await next();
            }
        }
    }
}

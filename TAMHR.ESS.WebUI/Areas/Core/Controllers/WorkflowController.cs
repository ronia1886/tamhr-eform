using System;
using System.Net;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Web.Models;
using TAMHR.ESS.Infrastructure.DomainServices;
using RestSharp;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
		/// <summary>
    /// Api controller for approval workflow
    /// </summary>
    [Route("api/workflow")]
    public class WorkflowApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Approval service
        /// </summary>
        protected ApprovalService ApprovalService => ServiceProxy.GetService<ApprovalService>();
        protected MdmService MdmService => ServiceProxy.GetService<MdmService>();
        #endregion

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IOptions<CookieAuthenticationOptions> _options;

        public WorkflowApiController(IOptions<CookieAuthenticationOptions> options, IServiceScopeFactory serviceScopeFactory)
        {
            _options = options;
            _serviceScopeFactory = serviceScopeFactory;
        }

        [HttpPost("{eventName}")]
        public async Task<IActionResult> Post(string eventName, [FromBody]DocumentApprovalRequest request)
        {
            if (!ApprovalService.HasApprovalMatrix(request.DocumentApprovalId))
            {
                throw new Exception("Approval matrix for this form is not defined. Please contact administrator for more information.");
            }

            var username = ServiceProxy.UserClaim.Username;
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;

            var actualOrganizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);

            var documentApproval = await ApprovalService.PostAsync(username, actualOrganizationStructure, eventName, request.DocumentApprovalId, request.Remarks, request.RefId, CompleteHandler);

            //TriggerAfter(documentApproval);

            LogAction($"Event Name <b>{eventName}</b>, Document ID <b>{request.DocumentApprovalId}</b>.");

            return NoContent();
        }

        [HttpPost("multiple/{eventName}")]
        public async Task<IActionResult> PostMultiple(string eventName, [FromBody]Guid[] ids)
        {
            var username = ServiceProxy.UserClaim.Username;
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;

            var actualOrganizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);

            foreach (var id in ids) {
                await ApprovalService.PostAsync(username, actualOrganizationStructure, eventName, id, completeHandler: CompleteHandler);
            }

            return NoContent();
        }

        [HttpPost("multipleapprove")]
        public async Task<IActionResult> PostMultipleApprove([FromBody]Guid[] ids)
        {
            return await PostMultiple(ApprovalAction.Approve, ids);
        }

        [HttpPost("multiplereject")]
        public async Task<IActionResult> PostMultipleReject([FromBody]Guid[] ids)
        {
            return await PostMultiple(ApprovalAction.Reject, ids);
        }

        private void TriggerAfter(DocumentApproval documentApproval)
        {
            if (documentApproval.Form?.FormKey == "health-declaration")
            {
                var backgroundTaskQueue = ServiceProxy.GetBackgroundTaskQueue();
                var documentApprovalId = documentApproval.Id;
                var creator = documentApproval.CreatedBy;

                backgroundTaskQueue.QueueBackgroundWorkItem(async (cancellationToken) =>
                {
                    try
                    {
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var coreService = scope.ServiceProvider.GetService<CoreService>();
                            var configService = scope.ServiceProvider.GetService<ConfigService>();
                            var webServiceUrl = configService.GetConfigValue<string>("AccessDoor.WebServiceUrl");
                            var webServiceMethod = configService.GetConfigValue<string>("AccessDoor.WebServiceMethod");
                            var employeeCard = coreService.QueryFirstOrDefault<EmployeeCard>(x => x.NoReg == creator);
                            var healthDeclaration = coreService.QueryFirstOrDefault<HealthDeclaration>(x => x.ReferenceDocumentApprovalId == documentApprovalId);
                            var accessDoorLog = new AccessDoorLog {
                                NoReg = healthDeclaration.NoReg,
                                Date = healthDeclaration.SubmissionDate
                            };

                            if (healthDeclaration.IsSick || healthDeclaration.WorkTypeCode == "wt-wfh") return;

                            if (string.IsNullOrEmpty(employeeCard.CardNumber))
                            {
                                accessDoorLog.Message = "Card number is not defined";
                                coreService.DynamicAdd(accessDoorLog);

                                return;
                            }
#if DEBUG
                            Debug.WriteLine("Start QUEUE");
#endif
                            var client = new RestClient(webServiceUrl);
                            var request = new RestRequest(webServiceMethod, Method.Post);
                            var parameters = new
                            {
                                CardNo = employeeCard.CardNumber,
                                CardStatus = true,
                                DownloadCard = true
                            };

                            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                            request.AddObject(parameters);

                            var response = client.Post(request);

                            accessDoorLog.Success = true;
                            accessDoorLog.Message = "Success update access door status";
                            coreService.DynamicAdd(accessDoorLog);
#if DEBUG
                            Debug.WriteLine("Release QUEUE");
#endif
                        }
                    }
                    catch (Exception ex)
                    {
                        var message = ex.Message;
                    }

                    await Task.FromResult(0);
                });
            }
        }

        private void CompleteHandler(DocumentApproval documentApproval)
        {
            var contextRequest = HttpContext.Request;
            var antiForgery = ServiceProxy.GetAntiForgery();
            var client = new RestClient($"{contextRequest.Scheme}://{contextRequest.Host}{contextRequest.PathBase}");
            var request = new RestRequest($"api/{documentApproval.FormKey}/complete/{{id}}", Method.Post);
            request.AddUrlSegment("id", documentApproval.Id);
            request.AddHeader("Content-Type", "application/json; charset=utf-8");
            request.AddHeader("X-Requested-With", "XMLHttpRequest");
            request.AddHeader("RequestVerificationToken", antiForgery.GetAndStoreTokens(HttpContext).RequestToken);

            foreach (var cookie in HttpContext.Request.Cookies)
            {
                request.AddCookie(cookie.Key, cookie.Value,null,null);
            }

            var response = client.Post(request);

            LogAction($"Complete Handler | Form Key <b>{documentApproval.FormKey}</b>, Document Number <b>{documentApproval.DocumentNumber}</b>.");

            if (response.StatusCode == HttpStatusCode.BadRequest) throw new Exception("Failed to complete the request");
        }
    }
	#endregion
}
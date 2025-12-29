using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Helpers;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using RestSharp;

namespace TAMHR.ESS.Infrastructure.Web
{
    public abstract class FormApiControllerBase<T> : ApiControllerBase
        where T : class
    {
        #region Domain Services
        /// <summary>
        /// Approval service
        /// </summary>
        public ApprovalService ApprovalService => ServiceProxy.GetService<ApprovalService>();

        /// <summary>
        /// Master data management service
        /// </summary>
        public MdmService MdmService => ServiceProxy.GetService<MdmService>();

        /// <summary>
        /// Form service
        /// </summary>
        public FormService FormService => ServiceProxy.GetService<FormService>();
        public LogService LogService => ServiceProxy.GetService<LogService>();
        #endregion

        #region Events
        /// <summary>
        /// Document updated event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void ApprovalService_DocumentUpdated(object sender, DocumentRequestDetailViewModel e) { }

        /// <summary>
        /// Document created event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void ApprovalService_DocumentCreated(object sender, DocumentRequestDetailViewModel e) { } 
        #endregion

        /// <summary>
        /// Get document approval by id
        /// </summary>
        /// <param name="id">Document Approval Id</param>
        /// <returns>Document Approval Object</returns>
        [HttpGet("document")]
        public DocumentApproval GetDocumentApproval(Guid id)
        {
            var documentApproval = ApprovalService.GetDocumentApprovalById(id);

            return documentApproval;
        }

        /// <summary>
        /// Generate document title
        /// </summary>
        /// <param name="title">Document Title</param>
        /// <param name="dicts">Parameters</param>
        /// <param name="documentRequestDetail">Document Request Detail Object</param>
        /// <returns>Formatted Document Title</returns>
        protected virtual string GenerateTitle(string title, Dictionary<string, object> dicts, DocumentRequestDetailViewModel<T> documentRequestDetail)
        {
            return StringHelper.Format(title, dicts);
        }

        /// <summary>
        /// Validate and create form data
        /// </summary>
        /// <param name="documentRequestDetail">Form Data</param>
        /// <returns>Created Form Data</returns>
        [HttpPost]
        public virtual IActionResult Create([FromBody]DocumentRequestDetailViewModel<T> documentRequestDetail)
        {
            ApprovalService.DocumentCreated += ApprovalService_DocumentCreated;

            var noreg = ServiceProxy.UserClaim.NoReg;
            var formKey = documentRequestDetail.FormKey;

            documentRequestDetail = ValidateViewModel(documentRequestDetail);
            ValidateOnCreate(formKey);
            ValidateOnPostCreate(documentRequestDetail);

            ApprovalService.CreateApprovalDocument(noreg, documentRequestDetail, (title, dicts) => GenerateTitle(title, dicts, documentRequestDetail));

            LogAction($"Create Document ID <b>{documentRequestDetail.DocumentApprovalId}</b>, Form Key <b>{documentRequestDetail.FormKey}</b>");

            return CreatedAtAction("GetDocumentApproval", new { id = documentRequestDetail.DocumentApprovalId, formKey = documentRequestDetail.FormKey });

        }

        /// <summary>
        /// Validate and create form data
        /// </summary>
        /// <param name="documentRequestDetail">Form Data</param>
        /// <returns>Created Form Data</returns>
        [HttpPost("multiple")]
        public IActionResult CreateMultiple([FromBody]ParentDocumentRequestDetailViewModel<T> parentDocumentRequestDetail)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var formKey = parentDocumentRequestDetail.FormKey;

            ValidateOnCreate(formKey);
            ValidateOnPostCreate(parentDocumentRequestDetail);

            var documentApproval = ApprovalService.CreateApprovalDocuments(noreg, parentDocumentRequestDetail);

            return CreatedAtAction("GetDocumentApproval", new { id = documentApproval.Id, formKey = documentApproval.FormKey });
        }

        [HttpPost("{eventName}")]
        public async Task<IActionResult> UpdateEvent(string eventName, [FromBody]DocumentRequestDetailViewModel<T> documentRequestDetail)
        {
            //update
            //ValidateOnPostUpdate(documentRequestDetail);
            ApprovalService.DocumentUpdated += ApprovalService_DocumentUpdated;
            ApprovalService.UpdateDocumentRequestDetail(documentRequestDetail, (title, dicts) => GenerateTitle(title, dicts, documentRequestDetail));
            //workflow
            if (!ApprovalService.HasApprovalMatrix(documentRequestDetail.DocumentApprovalId))
            {
                throw new Exception("Approval matrix for this form is not defined. Please contact administrator for more information.");
            }

            var username = ServiceProxy.UserClaim.Username;
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;

            var actualOrganizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);

            await ApprovalService.PostAsync(username, actualOrganizationStructure, eventName, documentRequestDetail.DocumentApprovalId, completeHandler: CompleteHandler);

            return NoContent();
        }

        protected async Task SubmitEvent(string username, string noreg, string postCode, string eventName, DocumentRequestDetailViewModel<T> documentRequestDetail)
        {
            ApprovalService.DocumentCreated += ApprovalService_DocumentCreated;

            var formKey = documentRequestDetail.FormKey;

            documentRequestDetail = ValidateViewModel(documentRequestDetail);

            ApprovalService.CreateApprovalDocument(noreg, documentRequestDetail, (title, dicts) => GenerateTitle(title, dicts, documentRequestDetail));

            var actualOrganizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);

            await ApprovalService.PostAsync(username, actualOrganizationStructure, eventName, documentRequestDetail.DocumentApprovalId, completeHandler: CompleteHandler);
        }

        /// <summary>
        /// Update the form data
        /// </summary>
        /// <param name="documentRequestDetail">Form Data</param>
        /// <returns>No Content</returns>
        [HttpPut]
        public IActionResult Update(DocumentRequestDetailViewModel<T> documentRequestDetail)
        {
            ApprovalService.DocumentUpdated += ApprovalService_DocumentUpdated;

            documentRequestDetail = ValidateViewModel(documentRequestDetail);

            ValidateOnPostUpdate(documentRequestDetail);

            ApprovalService.UpdateDocumentRequestDetail(documentRequestDetail, (title, dicts) => GenerateTitle(title, dicts, documentRequestDetail));

            LogAction($"Update Document ID <b>{documentRequestDetail.DocumentApprovalId}</b>, Form Key <b>{documentRequestDetail.FormKey}</b>");

            return CreatedAtAction("GetDocumentApproval", new { id = documentRequestDetail.DocumentApprovalId, formKey = documentRequestDetail.FormKey });
        }

        /// <summary>
        /// Update multiple form data
        /// </summary>
        /// <param name="documentRequestDetail">Form Data</param>
        /// <returns>No Content</returns>
        [HttpPut("multiple")]
        public IActionResult UpdateMultiple([FromBody]ParentDocumentRequestDetailViewModel<T> parentDocumentRequestDetail)
        {
            ValidateOnPostUpdate(parentDocumentRequestDetail);

            var documentApproval = ApprovalService.UpdateDocumentRequestDetails(parentDocumentRequestDetail);

            return CreatedAtAction("GetDocumentApproval", new { id = documentApproval.Id, formKey = documentApproval.FormKey });
        }

        [HttpPost("upload-attachment")]
        public IActionResult UploadAttachment([FromBody]GenericUploadViewModel viewModel)
        {
            ApprovalService.UploadAttachment(viewModel);

            return NoContent();
        }

        /// <summary>
        /// Validate if user can create this form or not
        /// </summary>
        /// <returns>Exception if not valid</returns>
        [HttpPost("validate")]
        public IActionResult Validate()
        {
            var paths = Request.Path.Value.Split(new [] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var formKey = paths[1];

            var username = ServiceProxy.UserClaim.Username;
            var isWebApi = !this.RouteData.DataTokens.Keys.Contains("area");
            var ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();
            var browser = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
            var area = RouteData.Values["area"];
            var controller = RouteData.Values["controller"];
            var action = RouteData.Values["action"];


            LogService.LogSuccess(username, ipAddress, browser, string.Format($"<b>Area: {area}</b><br/><b>Controller: {controller}</b><br/><b>Action: {action}</b>"), "call method validate");
            FormService.ValidateCreateForm(formKey);
           
            ValidateOnCreate(formKey);
            

            return NoContent();
        }

        /// <summary>
        /// Validate if user can create this form or not
        /// </summary>
        /// <param name="formKey">Form Key</param>
        protected virtual void ValidateOnCreate(string formKey)
        {
            var excludes = new[]
            {
                "bdjk-planning",
                "shift-planning",
                "shift-planning-report",
                "absence",
                "abnormality-absence",
                "abnormality-over-time",
                "abnormality-bdjk",
                "meal-allowance",
                "shift-meal-allowance",
                "reimbursement",
                "annual-leave-planning",
                "annual-wfh-planning",
                "annual-ot-planning",
                "annual-bdjk-planning",
                "termination",
                "weekly-wfh-planning"
            };

            if (excludes.Contains(formKey)) return;

            ApprovalService.ValidateCreateDocument(formKey, ServiceProxy.UserClaim.NoReg);
        }

        /// <summary>
        /// Validate the submitted form data
        /// </summary>
        /// <param name="requestDetailViewModel">Form Data</param>
        protected virtual void ValidateOnPostCreate(DocumentRequestDetailViewModel<T> requestDetailViewModel) { }

        /// <summary>
        /// Validate the submitted form data
        /// </summary>
        /// <param name="requestDetailViewModels">Multiple Form Data</param>
        protected virtual void ValidateOnPostCreate(ParentDocumentRequestDetailViewModel<T> requestDetailViewModels) { }

        /// <summary>
        /// Validate the updated form data
        /// </summary>
        /// <param name="requestDetailViewModel">Form Data</param>
        protected virtual void ValidateOnPostUpdate(DocumentRequestDetailViewModel<T> requestDetailViewModel)
        {
            if (!ApprovalService.CanUpdateDocument(requestDetailViewModel.DocumentApprovalId, ServiceProxy.UserClaim.NoReg))
            {
                throw new Exception("You dont have permission to update this document. Please contact administrator for more information.");
            }
        }
        protected virtual void ValidateOnPostUpdate(ParentDocumentRequestDetailViewModel<T> requestDetailViewModels) { }

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

            var cookieDomain = contextRequest.Host.Host;

            foreach (var cookie in HttpContext.Request.Cookies)
            {
                request.AddCookie(cookie.Key, cookie.Value,"/", cookieDomain);
            }
            
            var response = client.Post(request);

            LogAction($"Complete Handler | Form Key <b>{documentApproval.FormKey}</b>, Document Number <b>{documentApproval.DocumentNumber}</b>.");

            if (response.StatusCode == HttpStatusCode.BadRequest) throw new Exception("Failed to complete the request");
        }

        protected virtual DocumentRequestDetailViewModel<T> ValidateViewModel(DocumentRequestDetailViewModel<T> requestDetailViewModel)
        {
            return requestDetailViewModel;
        }
    }
}

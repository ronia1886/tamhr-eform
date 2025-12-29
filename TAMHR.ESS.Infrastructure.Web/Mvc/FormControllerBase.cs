using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using TAMHR.ESS.Infrastructure.Web.Models;
using TAMHR.ESS.Infrastructure.Web.Filters;
using TAMHR.ESS.Infrastructure.DomainServices;
using RestSharp;
using Newtonsoft.Json;
using Rotativa.AspNetCore;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Hosting;

namespace TAMHR.ESS.Infrastructure.Web
{
    [DocumentApprovalFilter]
    public abstract class FormControllerBase : MvcControllerBase
    {
        #region Services
        /// <summary>
        /// Core service
        /// </summary>
        public CoreService CoreService => ServiceProxy.GetService<CoreService>();

        /// <summary>
        /// Approval service
        /// </summary>
        public ApprovalService ApprovalService => ServiceProxy.GetService<ApprovalService>();

        /// <summary>
        /// Personal data service
        /// </summary>
        public PersonalDataService PersonalDataService => ServiceProxy.GetService<PersonalDataService>();

        public LogService LogService => ServiceProxy.GetService<LogService>();

        public FormService FormService => ServiceProxy.GetService<FormService>();
        #endregion

        /// <summary>
        /// Dictionary object that hold form view
        /// </summary>
        protected readonly IDictionary<string, Func<Guid, IActionResult>> Dicts = new Dictionary<string, Func<Guid, IActionResult>>();

        /// <summary>
        /// Dictionary object that hold pdf view
        /// </summary>
        protected readonly IDictionary<string, Func<Guid, IActionResult>> PdfDicts = new Dictionary<string, Func<Guid, IActionResult>>();

        /// <summary>
        /// Field that hold form key
        /// </summary>
        public string FormKey = "";

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public FormControllerBase()
            : base()
        {
        }
        #endregion

        /// <summary>
        /// Load form by key
        /// </summary>
        /// <param name="formKey">Form Key</param>
        /// <returns>Form View</returns>
        public IActionResult Load(string formKey)
        {
            this.FormKey = formKey;

            return Dicts[formKey](Guid.Empty);
        }

        /// <summary>
        /// View form by key and document approval id
        /// </summary>
        /// <param name="formKey">Form Key</param>
        /// <param name="docId">Document Approval Id</param>
        /// <returns>Form View</returns>
        public IActionResult View(string formKey, Guid docId)
        {
            this.FormKey = formKey;

            return Dicts[formKey](docId);
        }

        /// <summary>
        /// Download form by key and document approval id
        /// </summary>
        /// <param name="formKey">Form Key</param>
        /// <param name="docId">Document Approval Id</param>
        /// <returns>Pdf View</returns>
        public IActionResult Pdf(string formKey, Guid docId)
        {
            this.FormKey = formKey;

            return PdfDicts.ContainsKey(formKey) ? PdfDicts[formKey](docId) : NotFound();
        }

        /// <summary>
        /// Register view by form key
        /// </summary>
        /// <typeparam name="TViewModel">View Model</typeparam>
        /// <param name="formKey">Form Key</param>
        protected void RegisterView<TViewModel>(string formKey) where TViewModel : class
        {
            Dicts.Add(formKey, id => DefaultView<TViewModel>(formKey, id));
        }

        /// <summary>
        /// Register pdf view by view name and form keys
        /// </summary>
        /// <typeparam name="TViewModel">View Model</typeparam>
        /// <param name="viewName">View Name</param>
        /// <param name="formKeys">List of Form Key</param>
        protected void RegisterPdfView<TViewModel>(string viewName, string[] formKeys) where TViewModel : class
        {    
            foreach (var key in formKeys)
            {
                PdfDicts.Add(key, id => DefaultPdfView<TViewModel>(id, $"pdf/{viewName}"));
            }
        }

        /// <summary>
        /// Register pdf view by view name and form key
        /// </summary>
        /// <typeparam name="TViewModel">View Model</typeparam>
        /// <param name="viewName">View Name</param>
        /// <param name="formKey">Form Key</param>
        protected void RegisterPdfView<TViewModel>(string viewName, string formKey) where TViewModel : class
        {
            PdfDicts.Add(formKey, id => DefaultPdfView<TViewModel>(id, $"pdf/{viewName}"));
        }

        /// <summary>
        /// Register view by form key and custom handler
        /// </summary>
        /// <param name="formKey">Form Key</param>
        /// <param name="funcs">Custom Handler</param>
        protected void RegisterView(string formKey, Func<string, Guid, IActionResult> funcs)
        {
            Dicts.Add(formKey, id => funcs(formKey, id));
        }

        /// <summary>
        /// Show form view by key and document approval id
        /// </summary>
        /// <typeparam name="TViewModel">Form View Model</typeparam>
        /// <param name="formKey">Form Key</param>
        /// <param name="id">Document Approval Id</param>
        /// <returns>Form View</returns>
        protected IActionResult DefaultView<TViewModel>(string formKey, Guid id) where TViewModel : class
        {
            this.FormKey = formKey;
            var viewName = formKey.Replace("-", "");
            var backUrl = $"~/core/form/index?formKey={formKey}";

            //temporary di comment
            if (!AclHelper.HasTemporaryPermission)
            {
                return CommonView($"You dont have permission to create or view or update this form", "Permission denied!", backUrl: backUrl);
            }

            var orgObj = ServiceProxy.GetService<MdmService>().GetEmployeeOrganizationObjects(ServiceProxy.UserClaim.NoReg, ServiceProxy.UserClaim.PostCode);
            ViewData["OrganizationInfo"] = orgObj;
            ViewData["Data"] = SetViewData();
            ViewData["FormKey"] = formKey;

            var validationMessage = string.Empty;
            var validate = ValidateOnCreate(formKey, ref validationMessage);


           

            if (id == Guid.Empty && !validate)
            {
                return CommonView(validationMessage, backUrl: backUrl);
            }
            else
            {
                var model = ApprovalService.GetDocumentRequestDetailViewModel<TViewModel>(id, ServiceProxy.UserClaim.NoReg);
                model.FormKey = formKey;

                return View(viewName, model);
            }
            

        }

        /// <summary>
        /// Set additional view data event in form view
        /// </summary>
        /// <returns>View Data Object</returns>
        protected virtual object SetViewData()
        {
            return null;
        }

        /// <summary>
        /// Set additional view data event in pdf view
        /// </summary>
        /// <returns>View Data Object</returns>
        protected virtual object SetPdfViewData()
        {
            return null;
        }

        /// <summary>
        /// Show pdf view by document approval id and view name
        /// </summary>
        /// <typeparam name="TViewModel">Form View Model</typeparam>
        /// <param name="id">Document Approval Id</param>
        /// <param name="viewName">View Name</param>
        /// <returns>Pdf View</returns>
        protected IActionResult DefaultPdfView<TViewModel>(Guid id, string viewName) where TViewModel : class
        {
            var model = ApprovalService.GetDocumentApprovalById(id);
            var obj = ApprovalService.GetDocumentRequestDetailViewModel<TViewModel>(id, ServiceProxy.UserClaim.NoReg);
            obj.FormKey = model.FormKey;

            var org = ServiceProxy.GetService<MdmService>().GetActualOrganizationStructure(model.CreatedBy);
            var orgObj = ServiceProxy.GetService<MdmService>().GetEmployeeOrganizationObjects(model.CreatedBy);
            var printoutMatrix = CoreService.GetPrintOut(id);

            try
            {
                string Name = "";
                if (org != null)
                {
                    Name = org.Name;
                }
                else if (model.FormKey == "termination")
                {
                    var getDataTermination = ServiceProxy.GetService<TerminationService>().GetTermination(id);
                    if (getDataTermination != null)
                    {
                        Name = getDataTermination.Name;
                    }
                }

                var title = $"{model.DocumentNumber.Replace(@"/", "-").Replace(@"\", "-")} for {Name}";

                Rotativa.AspNetCore.Options.Orientation orientation = Rotativa.AspNetCore.Options.Orientation.Portrait;
                if (
                    viewName.EndsWith("FormAnnualBDJKPlanningPdf")
                    || viewName.EndsWith("FormAnnualLeavePlanningPdf")
                    || viewName.EndsWith("FormAnnualWFHPlanningPdf")
                )
                {
                    orientation = Rotativa.AspNetCore.Options.Orientation.Landscape;
                }

                var customSwitches = string.Format(
                    "--title \"{0}\" " +
                    "--header-html \"{1}\" " +
                    "--header-spacing \"0\" " +
                    "--header-font-size \"9\" " +
                    "--load-error-handling ignore",
                    title, GetHeaderPdfUrl("/core/default/header") + "?docId=" + id.ToString()
                );

                var dataModel = new ViewModels.PdfViewModel()
                {
                    DocumentApproval = model,
                    OrgInfo = org,
                    OrgObjects = orgObj,
                    Object = obj.Object,
                    PrintoutMatrix = printoutMatrix

                    // ViewData = SetPdfViewData() // ❌ INI YANG SEBELUMNYA ERROR, JANGAN DIPAKAI
                };

                ViewData["PdfViewData"] = SetPdfViewData(); // ✅ kalau masih mau pakai di View

                var data = new ViewAsPdf(viewName, dataModel)
                {
                    ContentDisposition = Rotativa.AspNetCore.Options.ContentDisposition.Inline,
                    FileName = title + ".pdf",
                    CustomSwitches = customSwitches,
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                    PageMargins = new Rotativa.AspNetCore.Options.Margins(30, 15, 10, 15),
                    PageOrientation = orientation
                };

                return data;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }



        ///// <summary>
        ///// Log action to database
        ///// </summary>
        ///// <param name="message">Action Message</param>
        //public void LogAction(string message)
        //{
        //    try
        //    {
        //        var username = ServiceProxy.UserClaim.Username;
        //        var isWebApi = !this.RouteData.DataTokens.Keys.Contains("area");
        //        var ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();
        //        var browser = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
        //        var area = RouteData.Values["area"];
        //        var controller = RouteData.Values["controller"];
        //        var action = RouteData.Values["action"];

        //        LogService.LogSuccess(username, ipAddress, browser, string.Format($"<b>Area: {area}</b><br/><b>Controller: {controller}</b><br/><b>Action: {action}</b>"), message);
        //    }
        //    catch (Exception)
        //    {
        //    }

        //}

        /// <summary>
        /// Validate form on create event by key
        /// </summary>
        /// <param name="formKey">Form Key</param>
        /// <param name="validationMessage">Validation Message</param>
        /// <returns>True if valid, false otherwise</returns>
        private bool ValidateOnCreate(string formKey, ref string validationMessage)
        {
            var antiForgery = ServiceProxy.GetAntiForgery();
            var hostingEnvironment = ServiceProxy.GetHostingEnvironment();
            var baseUrl = string.Empty;
            var contextRequest = HttpContext.Request;

            var validateFormMessage = FormService.ValidateLoadForm(formKey);
            //var validateDocumentMessage = ValidateOnCreate(formKey);

            if (!string.IsNullOrEmpty(validateFormMessage))
            {
                validationMessage = validateFormMessage;
                return false;
            }           

            //if (!string.IsNullOrEmpty(validateDocumentMessage))
            //{
            //    validationMessage = validateDocumentMessage;
            //    return false;
            //}
            //if (hostingEnvironment.IsDevelopment())
            //{
            //    baseUrl = $"{contextRequest.Scheme}://{contextRequest.Host}{contextRequest.PathBase}";
            //}
            //else
            //{
            //    var baseUrlConfigValue = ConfigService.GetConfig("Application.LocalUrl")?.ConfigValue;

            //    baseUrl = string.IsNullOrEmpty(baseUrlConfigValue) ? $"http://localhost{contextRequest.PathBase}" : baseUrlConfigValue;
            //}

            //var client = new RestClient(baseUrl);
            //var request = new RestRequest($"api/{formKey}/validate", Method.POST);

            //request.AddHeader("Content-Type", "application/json; charset=utf-8");
            //request.AddHeader("X-Requested-With", "XMLHttpRequest");
            //request.AddHeader("RequestVerificationToken", antiForgery.GetAndStoreTokens(HttpContext).RequestToken);
            //var sbCookie = new StringBuilder();
            //foreach (var cookie in HttpContext.Request.Cookies)
            //{
            //    request.AddCookie(cookie.Key, cookie.Value);
            //    sbCookie.Append(string.Format("cookie Key:{0};cookie Value:{1}", cookie.Key, cookie.Value));
            //    sbCookie.Append("|");
            //}
            //// Kirim body kosong JSON sebagai parameter untuk menghindari kosong total
            //request.AddParameter("application/json", "{}", ParameterType.RequestBody);

            //var username = ServiceProxy.UserClaim.Username;
            //var isWebApi = !this.RouteData.DataTokens.Keys.Contains("area");
            //var ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();
            //var browser = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
            //var area = RouteData.Values["area"];
            //var controller = RouteData.Values["controller"];
            //var action = RouteData.Values["action"];
            //var checkStep = false;
            //var checkStepError = false;
            //var reponsestring = "";
            //var responseData = "";
            //var sb = new StringBuilder();
            //try
            //{
            //    var response = client.Post(request);
            //    throw new Exception("response=" + response.StatusCode + "||" + response.ResponseStatus);
            //    //var response = client
            //    //reponsestring = response.StatusDescription.ToString();
            //    sb.Append(baseUrl);
            //    sb.Append(";");
            //    sb.Append(request.Resource);
            //    sb.Append(";");
            //    sb.Append(antiForgery.GetAndStoreTokens(HttpContext).RequestToken);
            //    sb.Append(";");
            //    sb.Append(sbCookie.ToString());
            //    sb.Append(response.IsSuccessful);
            //    sb.Append(";");
            //    var isNull = string.IsNullOrEmpty(response.Content);
            //    sb.Append(isNull.ToString());
            //    //responseData = JsonConvert.SerializeObject(response);
            //    checkStep = true;

            //    if (response.StatusCode == HttpStatusCode.BadRequest)
            //    {
            //        checkStepError = true;
            //        var exceptionInfo = JsonConvert.DeserializeObject<ExceptionInfo>(response.Content);

            //        validationMessage = exceptionInfo.Message;

            //        return false;
            //    }
            //}
            //catch (Exception e)
            //{
            //    reponsestring = e.ToString();
            //}
            //sb.Append(";");
            //sb.Append(checkStep.ToString());
            //sb.Append(";");
            //sb.Append(checkStepError.ToString());
            //sb.Append(";");
            //sb.Append(responseData);





            //LogService.LogSuccess(username, ipAddress, browser, string.Format($"<b>Area: {area}</b><br/><b>Controller: {controller}</b><br/><b>Action: {action}</b>"), sb.ToString());

            //var client = new RestClient(baseUrl);
            //var request = new RestRequest($"api/{formKey}/validate", Method.Post);

            //var cookieDomain = contextRequest.Host.Host;

            //request.AddHeader("Content-Type", "application/json; charset=utf-8");
            //request.AddHeader("X-Requested-With", "XMLHttpRequest");
            //request.AddHeader("RequestVerificationToken", antiForgery.GetAndStoreTokens(HttpContext).RequestToken);

            //foreach (var cookie in HttpContext.Request.Cookies)
            //{
            //    request.AddCookie(cookie.Key, cookie.Value, "/", cookieDomain);
            //}

            //var response = client.Post(request);
            ////throw new Exception("response="+ response.StatusCode+"||"+response.ResponseStatus);
            //if (response.StatusCode == HttpStatusCode.BadRequest)
            //{
            //    var exceptionInfo = JsonConvert.DeserializeObject<ExceptionInfo>(response.Content);

            //    validationMessage = exceptionInfo.Message;

            //    return false;
            //}

            return true;
        }


        private string ValidateOnCreate(string formKey)
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

            if (excludes.Contains(formKey)) return "";

            return ApprovalService.ValidateLoadDocument(formKey, ServiceProxy.UserClaim.NoReg);
        }
    }
}

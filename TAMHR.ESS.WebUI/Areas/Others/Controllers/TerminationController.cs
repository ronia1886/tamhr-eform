using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Requests;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Agit.Common.Extensions;
using Kendo.Mvc;
using Kendo.Mvc.UI;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Newtonsoft.Json;
using Kendo.Mvc.Extensions;
using Newtonsoft.Json.Linq;
using System.Globalization;
using Rotativa.AspNetCore;
using System.Data;

namespace TAMHR.ESS.WebUI.Areas.Others.Controllers
{
    [Route("api/"+ ApplicationForm.Termination)]
    //[Permission(PermissionKey.ViewHealthDeclarationReport)]
    public class TerminationApiController : FormApiControllerBase<TerminationViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Termination service object.
        /// </summary>
        protected TerminationService TerminationService => ServiceProxy.GetService<TerminationService>();
        protected new ApprovalService ApprovalService => ServiceProxy.GetService<ApprovalService>();
        protected new UserService UserService => ServiceProxy.GetService<UserService>();

        #endregion

        protected override void ValidateOnCreate(string formKey)
        {
            base.ValidateOnCreate(formKey);
        }

        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<TerminationViewModel> terminationViewModel)
        {
            base.ValidateOnPostCreate(terminationViewModel);
        }

        [HttpPost("get-active-employees-exclude-noreg/{noreg}")]
        public async Task<DataSourceResult> GetActiveUsers([DataSourceRequest] DataSourceRequest request, string noReg)
        {
            var data = UserService.GetActiveUsers();
            var rejectData = data.Where(x => x.NoReg == noReg);
            var result = await data.Except(rejectData).ToDataSourceResultAsync(request);

            return result;
        }

        [HttpPost("get-exit-clearance-employees")]
        public async Task<DataSourceResult> GetExitClearanceUsers([DataSourceRequest] DataSourceRequest request)
        {
            var data = TerminationService.GetExitClearanceUsers();
            //var data = UserService.GetActiveUsers();
            var result = await data.ToDataSourceResultAsync(request);

            return result;
        }

        [HttpPost("get-termination-types")]
        public async Task<DataSourceResult> GetTerminationTypes()
        {
            string type = Request.Form["type"];
            return await TerminationService.GetTerminationTypes(type).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("custom-document-approval-title/{id}/{type}/{noreg}")]
        public string CustomDocumentApprovalTitle(string id, string type, string noreg)
        {
            string result = "success";
            TerminationService.CustomApprovalTitle(id, type, noreg);
            return result;
        }

        [HttpPost("get-attachment-detail/{id}")]
        public string GetAttachmentDetail(string id)
        {
            string result = "success";
            var Data = ApprovalService.GetCommonFileById(new Guid(id));
            if (Data.FileType == "application/pdf")
            {
                if (Data.FileSize >= 5120)
                {
                    result = "Attachment size more than 5 Mb, maximum size is 5 Mb";
                }
            }
            else
            {
                result = "Attachment is invalid, allowed extensions are: .pdf";
            }

            return result;
        }

        [HttpPost("get-employee-detail/{id}")]
        public string GetEmployeeDetail(Guid id)
        {
            string ids = id.ToString();
            var data = TerminationService.GetTerminationEmployeeInfo(ids);
            var img = ConfigService.ResolveAvatar(ids);
            var employeeDetail = data.First();
            var json = JsonConvert.SerializeObject(employeeDetail);
            var imgjson = JsonConvert.SerializeObject(img);
            var newjson = json.Replace("}", ",\"ImgUrl\":" + imgjson + "}");
            return newjson;
        }

        [HttpPost("validate-end-date/{date}/{noreg}")]
        public string ValidateEndDate(string date, string noreg)
        {
            string newDate = date.Replace("-", "/");
            DateTime dt = DateTime.ParseExact(newDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            bool offDate = TerminationService.GetListWorkSchEmp(noreg, dt, dt);
            if(offDate == true)
            {
                return "End date cannot be on weekends or holidays";
            }
            else
            {
                return "success";
            }
        }

        [HttpPost("validate-exitclearance-role")]
        public string ValidateExitClearanceRole()
        {
            var resultString = "success";
            var roles = this.Request.Form["roles"].ToString();
            var userid = this.Request.Form["userid"].ToString();
            var rolesArray = (roles.Replace("{" , string.Empty).Replace("}", string.Empty)).Split(",");
            var result = TerminationService.ExitClearanceRoleValidate(rolesArray, userid);
            if(result != "")
            {
                resultString = "User with "+ result + " role is already exists, only 1 user is allowed in this role";
            }
            return resultString;
        }

        [HttpPost("update-termination-exitclearance-task")]
        public string UpdateExitClearanceTask()
        {
            var result = "";
            try
            {
                var roles = this.Request.Form["roles"].ToString();
                var noreg = this.Request.Form["noreg"].ToString();
                var rolesArray = (roles.Replace("{", string.Empty).Replace("}", string.Empty)).Split(",");
                TerminationService.UpdateTerminationExitClearanceTask(rolesArray, noreg);
                result = "success";
            }
            catch(Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        [HttpPost("validate-termination-form/{noreg}/{type}/{savetype}/{documentapprovalid}")]
        public string ValidateTerminationForm(string noreg, string type, string savetype, string documentapprovalid)
        {
            if(new Guid(documentapprovalid) == new Guid())
            {
                savetype = "Create";
            }

            bool validate = TerminationService.ResignationCreateValidate(noreg, type, savetype);

            if (validate == false)
            {
                return "Cannot create termination form with same employee, termination form for this employee is already exists";
            }
            else
            {
                if (type == "Contract Ended")
                {
                    bool validatecontract = TerminationService.ContractValidate(noreg);
                    if(validatecontract == true)
                    {
                        return "success";
                    }
                    else
                    {
                        return "Cannot create termination form with contract ended type because this employee is permanent employee";
                    }
                }
                else
                {
                    return "success";
                } 
            }
        }

        [HttpPost("approve/{id}/{remark}")]
        public async Task<string> Approve(string id, string remark)
        {
            string result = "";
            var username = ServiceProxy.UserClaim.Username;
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;

            try
            {
                var actualOrganizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);
                var data = await TerminationService.PostAsync(username, actualOrganizationStructure, "approve", new Guid(id), remark);
                result = "success";
            }
            catch(Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        [HttpPost("update-enddate")]
        public IActionResult UpdateEndDate(Termination data)
        {
            TerminationService.updateEndDate(data.DocumentApprovalId, data.EndDate, data.NoReg, null, null);
            return NoContent();
        }

        [HttpPost("update-enddate-generate-file")]
        public string UpdateEndDateAndGenerateFile()
        {
            string result = "";
            try
            {
                string documentApprovalId = Request.Form["DocumentApprovalId"].ToString();
                string noReg = Request.Form["NoReg"].ToString();
                string endDate = Request.Form["EndDate"].ToString();
                DateTime newEndDate = DateTime.ParseExact(endDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                //DateTime newEndDate = DateTime.Parse(endDate, null, System.Globalization.DateTimeStyles.RoundtripKind);
                string buildingName = Request.Form["BuildingName"].ToString();
                string PICExitInterview = Request.Form["PICExit"].ToString();

                TerminationService.updateEndDate(new Guid(documentApprovalId), newEndDate, noReg, buildingName, PICExitInterview);

                var data = new DocumentApproval();
                data.Id = new Guid(documentApprovalId);

                var generateDocument = GenerateEmployeeDocument(data);

                result = "success";
            }
            catch (Exception e)
            {
                result = e.Message;
            }

            return result;
        }

        [HttpPost("generate-employee-document")]
        public async Task<IActionResult> GenerateEmployeeDocument(DocumentApproval data)
        {
            DataTable table = new DataTable();
            table.Columns.Add("pdfpath", typeof(string));
            table.Columns.Add("headername", typeof(string));
            table.Columns.Add("type", typeof(string));

            table.Rows.Add("~/Areas/Others/Views/Form/Pdf/TerminationInvitationLetterPdf.cshtml", "Surat Undangan ", "invitation");
            table.Rows.Add("~/Areas/Others/Views/Form/Pdf/TerminationVerklaringLetterPdf.cshtml", "Surat Verklaring ", "verklaring");

            foreach(DataRow row in table.Rows)
            {
                var noreg = ServiceProxy.UserClaim.NoReg;
                var terminationData = TerminationService.GetTerminationViewModel(data.Id);
                var userData = UserService.GetByNoReg(terminationData.NoReg);
                var documentApproval = ApprovalService.GetDocumentApprovalById(data.Id);
                ViewAsPdf invitationPdf = new ViewAsPdf(row["pdfpath"].ToString(), documentApproval)
                {
                    FileName = row["headername"].ToString() + userData.Name + ".pdf",
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                };

                byte[] invitationPdfData = await invitationPdf.BuildFile(ControllerContext);

                var config = TerminationService.GetConfigByConfigKey("Download.Path");

                var configPath = config?.ConfigValue;

                if (!System.IO.Directory.Exists(configPath))
                {
                    System.IO.Directory.CreateDirectory(configPath);
                }

                string fullPath = configPath + "\\" + Path.GetFileName(invitationPdf.FileName);

                using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                {
                    fileStream.Write(invitationPdfData, 0, invitationPdfData.Length);
                }

                CommonFile commonfileData = new CommonFile();
                commonfileData.FileName = invitationPdf.FileName;
                commonfileData.FileSize = invitationPdfData.Length;
                commonfileData.FileType = "application/pdf";
                commonfileData.FileUrl = fullPath;
                commonfileData.CreatedBy = noreg;
                commonfileData.CreatedOn = DateTime.Now;

                TerminationService.addTerminationCommonFile(commonfileData, row["type"].ToString(), data.Id, noreg);
            }

            return NoContent();
        }

        [HttpGet]
        [Route("download")]
        public async Task<IActionResult> DownloadFiles(Guid id)
        {
            var file = ApprovalService.GetCommonFileById(id);

            if (file != null)
            {
                string fullPath = file.FileUrl;
                if (System.IO.File.Exists(fullPath))
                {
                    using (var memory = new MemoryStream())
                    {
                        using (var stream = new FileStream(fullPath, FileMode.Open))
                        {
                            await stream.CopyToAsync(memory);
                        }

                        memory.Position = 0;

                        return File(memory.ToArray(), GetContentType(fullPath), Path.GetFileName(fullPath));
                    }
                }

            }
            return NotFound("File Not Found");
        }
    }

    #region MVC Controller
    /// <summary>
    /// Health declaration MVC controller.
    /// </summary>
    [Area(ApplicationModule.Others)]
    //[Area("Others")]
    [Permission(PermissionKey.ViewTerminationMenuForm)]
    public class TerminationController : MvcControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Approval service object.
        /// </summary>
        protected ApprovalService ApprovalService => ServiceProxy.GetService<ApprovalService>();
        protected TerminationService TerminationService => ServiceProxy.GetService<TerminationService>();
        public CoreService CoreService => ServiceProxy.GetService<CoreService>();
        #endregion

        /// <summary>
        /// Health declaration report page.
        /// </summary>
        public IActionResult Index()
        {
            ViewBag.TerminationTypes = TerminationService.GetTerminationTypes();
            // Return the respected view.
            RequestHistoryViewModel model = new RequestHistoryViewModel();
            model.FormKey = "termination";
            model.Path = string.Empty;
            model.ShowFormTitle = false;
            return View(model);
        }

        public IActionResult ViewForm(string formKey, string type, string mode)
        {
            ViewBag.Mode = mode;
            ViewBag.Type = type;
            DocumentRequestDetailViewModel<TerminationViewModel> model = new DocumentRequestDetailViewModel<TerminationViewModel>();
            model.FormKey = formKey;

            if (type == "resignation")
            {
                model.Object.TerminationTypeId = TerminationService.GetTerminationTypeByTypeKey(type).Id;
            }

            var viewName = formKey.Replace("-", "");
            var permissions = AclHelper.HasPermission($"Form.{viewName}.Create")
                       ? new string[] { "Core.Approval.Edit", "Core.Approval.Submit", "Core.Approval.Cancel", "Core.Approval.ViewAction" }
                       : new string[] { };
            AclHelper.SetTemporaryPermissions(permissions);
            //AclHelper.SetTemporaryPermissions(new[] { "Core.Approval.Edit", "Core.Approval.Submit", "Core.Approval.Cancel", "Core.Approval.ViewAction" });
            return View("~/Areas/Others/Views/Form/Termination.cshtml", model);
        }

        public IActionResult ViewFormMonitoringExitClearance(string formKey, string id)
        {
            //DocumentRequestDetailViewModel<TerminationViewModel> model = new DocumentRequestDetailViewModel<TerminationViewModel>();
            //model.FormKey = formKey;
            DocumentRequestDetailViewModel<List<TerminationExitClearanceStoredEntity>> model = new DocumentRequestDetailViewModel<List<TerminationExitClearanceStoredEntity>>();
            model.FormKey = formKey;
            model.DocumentApprovalId = new Guid(id);
            //model.Id= new Guid(id);
            //model.CanDownload= true;
            model.Object = TerminationService.GetTerminationExitClearancePdfData(new Guid(id)).ToList();
            return View("~/Areas/Others/Views/Form/MonitoringExitClearance.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> View(string formKey, Guid id)
        {
            var menu = await CoreService.GetMenuByGroupAsync("main", "view?formKey=" + formKey);

            return Redirect($"{menu.Url}&docid=" + id + "&typeview=submissionlist");
        }

        [HttpPost]
        public IActionResult Load()
        {
            return PartialView("~/Areas/Others/Views/Form/_UpdateExitClearanceForm.cshtml");
        }

    }
    #endregion
}
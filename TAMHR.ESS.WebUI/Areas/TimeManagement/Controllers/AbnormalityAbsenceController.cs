using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using TAMHR.ESS.Infrastructure.Helpers;
using TAMHR.ESS.Infrastructure.ViewModels;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;
using System;
using System.IO;
using Kendo.Mvc;
using System.Collections.Generic;
using Kendo.Mvc.Extensions;
using OfficeOpenXml;
using Newtonsoft.Json;
using System.Drawing;
using OfficeOpenXml.Drawing;
using System.Data;
using System.Globalization;
using Agit.Common.Utility;
using Agit.Common.Attributes;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web.Runtime.Providers;
using System.Net.Http.Headers;
using TAMHR.ESS.Infrastructure.Web.Extensions;
using System.Text.RegularExpressions;
using System.Net;
using System.Diagnostics;

namespace TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers
{
    [Route("api/abnormality-absence")]
    [Permission(PermissionKey.ManageAbnormalityAbsence)]
    public class AbnormalityAbsenceApiController : FormApiControllerBase<AbnormalityAbsenceViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Proxy time service object
        /// </summary>
        public AbnormalityAbsenceService abnormalityAbsenceService => ServiceProxy.GetService<AbnormalityAbsenceService>();

        protected AbnormalityOverTimeService abnormalityOverTimeService => ServiceProxy.GetService<AbnormalityOverTimeService>();

        protected UserService userService => ServiceProxy.GetService<UserService>();
        #endregion

        /// <summary>
        /// Get list of proxy time
        /// </summary>
        /// <remarks>
        /// Get list of proxy time
        /// </remarks>
        /// <returns>List of Proxy Time</returns>
        [HttpGet]
        public IEnumerable<Domain.AbnormalityAbsenceView> Gets() => abnormalityAbsenceService.Gets();

        /// <summary>
        /// Get list of proxy time
        /// </summary>
        /// <remarks>
        /// Get list of proxy time
        /// </remarks>
        /// <returns>List of Proxy Time</returns>
        [HttpPost("gets")]
        public DataSourceResult GetFromPosts([FromForm] string noReg, [DataSourceRequest] DataSourceRequest request)
        {
            var roles = ServiceProxy.UserClaim.Roles.ToList();
            var obj = new { NoReg = noReg };
            //return ServiceProxy.GetDataSourceResult<AbnormalityAbsenceView>(request, obj);
            return abnormalityAbsenceService.GetsByCurrentUser(noReg).ToDataSourceResult(request);
        }

        /// <summary>
        /// Create new proxy time
        /// </summary>
        /// <remarks>
        /// Create new proxy time
        /// </remarks>
        /// <param name="entity">Proxy Time Object</param>
        /// <returns>Created Proxy Time Object</returns>
        [HttpPost("new-request")]
        public IActionResult Createdata([FromBody] Domain.AbnormalityAbsenceView entity)
        {
            var rs = new AbnormalityAbsence();
            rs.Reason = entity.Reason;
            rs.Id = entity.AbnormalityAbsenceId.HasValue ? entity.AbnormalityAbsenceId.Value : Guid.Empty;
            rs.WorkingDate = entity.AbnormalityWorkingDate;
            var wk = entity.AbnormalityWorkingDate.ToShortDateString();
            if (entity.AbnormaityProxyIn.HasValue && entity.AbnormalityProxyOut.HasValue)
            {
                var clkIn = entity.AbnormaityProxyIn.Value.ToString("hh:mm tt");
                DateTime wrkIn = DateTime.Parse(wk + " " + clkIn);
                rs.WorkingTimeIn = wrkIn;
                var clkout = entity.AbnormalityProxyOut.Value.ToString("hh:mm tt");
                DateTime wrkOut = DateTime.Parse(wk + " " + clkout);
                rs.WorkingTimeOut = wrkOut;
            }
            rs.ShiftCode = entity.ShiftCode;
            rs.TimeManagementId = entity.Id;
            rs.AbnormalityStatus = "Draft";
            rs.AbsentStatus = entity.AbnormalityAbsenStatus;
            rs.Description = entity.Description;
            rs.NormalTimeIn = entity.NormalTimeIn;
            rs.NormalTimeOut = entity.NormalTimeOut;
            rs.Reason = entity.Reason;
            rs.NoReg = entity.NoReg;
            rs.CommonFileId = entity.CommonFileId;
            rs.DocumentApprovalId = entity.DocumentApprovalId;
            //rs.CreatedBy = ServiceProxy.UserClaim.NoReg;
            //rs.CreatedOn = DateTime.Now;
            abnormalityAbsenceService.Upsert(ServiceProxy.UserClaim.NoReg, rs);
            return NoContent();
        }

        

        /// <summary>
        /// Download template
        /// </summary>
        /// <remarks>
        /// Download template
        /// </remarks>
        /// <returns>Data in Excel Format</returns>
        [HttpGet("download-template")]
        public IActionResult DownloadTemplate()
        {
            var controllerName = RouteData.Values["controller"].ToString();
            var cleanControllerName = controllerName.Substring(0, controllerName.Length - 3);

            return GenerateTemplate<Domain.TimeManagement>($"{cleanControllerName}", new[] { "Id", "NormalTimeIn", "NormalTimeOut", "CreatedBy", "CreatedOn", "ModifiedBy", "ModifiedOn", "RowStatus" });
        }

        [HttpPost("insert-file")]
        public IActionResult InsertFile([FromBody] Domain.AbnormalityFile entity)
        {
            AbnormalityFile datafile = new AbnormalityFile();
            datafile.CommonFileId = entity.CommonFileId;
            datafile.TransactionId = entity.TransactionId;
            datafile.RowStatus = true;
            datafile.CreatedBy = ServiceProxy.UserClaim.NoReg;
            datafile.CreatedOn = DateTime.Now;
            abnormalityOverTimeService.AddFile(ServiceProxy.UserClaim.NoReg, datafile);
            return NoContent();
        }

        [HttpPost("get-abnormality-file")]
        public async Task<DataSourceResult> GetHistoriesFromPosts([FromForm] string noreg, [FromForm] AbnormalityFileView entity, [DataSourceRequest] DataSourceRequest request)
        {
            return await abnormalityOverTimeService.GetsAbnormalityFile(entity).ToDataSourceResultAsync(request);
        }

        [HttpPost("delete-file")]
        public IActionResult DeleteFile([FromBody] Domain.AbnormalityFile entity)
        {
            abnormalityOverTimeService.DeleteFile(entity.Id);
            return NoContent();
        }

        [HttpPost("approve")]
        public async Task<IActionResult> Approve([FromBody] DocumentRequestDetailViewModel<AbnormalityAbsenceViewModel> documentRequestDetail)
        {
            //update
            var username = ServiceProxy.UserClaim.Username;
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var docApp = ApprovalService.GetDocumentDetailApprovalById(documentRequestDetail.DocumentApprovalId);

            //ClaimBenefitService.InsertAllowanceSeq36(docApp.CreatedBy, documentRequestDetail.Object);
            documentRequestDetail.ObjectValue = JsonConvert.SerializeObject(documentRequestDetail.Object);

            documentRequestDetail.Id = docApp.Id;

            ApprovalService.DocumentUpdated += ApprovalService_DocumentUpdated;
            ApprovalService.UpdateDocumentRequestDetail(documentRequestDetail);

            var actualOrganizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);

            List<User> userList = userService.GetUsersByRole("HR_PROXY_ABNORMALITY").ToList();

            if(userList.Where(x => x.NoReg == noreg).Count() > 0) {
                await ApprovalService.PostAsync(username, actualOrganizationStructure, "complete", documentRequestDetail.DocumentApprovalId);
            } else
            {
                await ApprovalService.PostAsync(username, actualOrganizationStructure, "approve", documentRequestDetail.DocumentApprovalId);
            }
            
            return NoContent();
        }
    }

    public enum RolesUser
    {
        [StringValue("HR Admin")]
        HrAdmin
    }
    #region MVC Controller
    /// <summary>
    /// Bank page controller
    [Area("TimeManagement")]
    public class AbnormalityAbsenceController : MvcControllerBase
    {
        /// <summary>
        /// Proxy time management service object
        /// </summary>
        protected AbnormalityAbsenceService abnormalityAbsenceService { get { return ServiceProxy.GetService<AbnormalityAbsenceService>(); } }

        /// <summary>
        /// Time management main page
        /// </summary>
        [Permission(PermissionKey.ViewAbnormalityAbsence)]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Load time management form by id
        /// </summary>
        /// <param name="id">Time Management Id</param>
        /// <returns>Time Management Form</returns>
        [HttpPost]
        public IActionResult Load(Guid id, int progress)
        {
            ViewBag.request = progress == 0;
            var faskes = abnormalityAbsenceService.Get(id);
            return PartialView("_AbnormalityAbsenceForm", faskes);
        }


        /// <summary>
        /// Load time management form by id
        /// </summary>
        /// <param name="id">Abnormality Absence</param>
        /// <returns>Time Management Form</returns>
        [HttpPost]
        public IActionResult LoadFile(Guid id, int progress)
        {
            ViewBag.request = progress == 0;
            var faskes = abnormalityAbsenceService.Get(id);
            return PartialView("_AbnormalityAbsenceFile", faskes);
        }
    }
    #endregion
}
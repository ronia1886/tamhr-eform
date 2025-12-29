using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc;
using Kendo.Mvc.UI;
using OfficeOpenXml;
using Rotativa.AspNetCore;
using Agit.Common;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using TAMHR.ESS.Infrastructure.Helpers;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.Infrastructure.Implementation;
using static System.Net.WebRequestMethods;
using Agit.Common.Extensions;
using TAMHR.ESS.Infrastructure.Web.Runtime.Providers;
using System.Security.Claims;
using TAMHR.ESS.Infrastructure.Web.ContextPrincipal;

namespace TAMHR.ESS.WebUI.Areas.OHS.Controllers
{
    #region API Controller
    
    [Route("api/ohs/req-download-medical")]
    //[Permission(PermissionKey.ViewAyoSekolahReport)]
    public class ReqDownloadMedicalApiController : ApiControllerBase
    {
        #region Domain Services
        
        public MedicalRecordService MedicalRecordService => ServiceProxy.GetService<MedicalRecordService>();


        #endregion


        [HttpPost("gets-req-download-medical")]
        public DataSourceResult GetGrid([DataSourceRequest] DataSourceRequest request)
        {

            // string requestor = User.Claims.ElementAtOrDefault(0).Value; //default
            string requestor1 = User.Claims.ElementAtOrDefault(1).Value; // Role
            string requestor2 = User.Claims.ElementAtOrDefault(2).Value; // user id
            string requestor3 = User.Claims.ElementAtOrDefault(3).Value;
            string requestor4 = User.Claims.ElementAtOrDefault(4).Value;
            string requestor5 = User.Claims.ElementAtOrDefault(5).Value;
            string requestor6 = User.Claims.ElementAtOrDefault(6).Value;
            string requestor7 = User.Claims.ElementAtOrDefault(7).Value;
            string requestor8 = User.Claims.ElementAtOrDefault(8).Value;
            string requestor9 = User.Claims.ElementAtOrDefault(9).Value;
            string requestor10 = User.Claims.ElementAtOrDefault(10).Value;
            string requestor11 = User.Claims.ElementAtOrDefault(11).Value;
            string requestor12 = User.Claims.ElementAtOrDefault(12).Value;


            string requestor = ServiceProxy.UserClaim.Name;
            string[] role = ServiceProxy.UserClaim.Roles;
            string targetRole = role.FirstOrDefault(r => r == "OHS_ADMIN");


            var getData = MedicalRecordService.GetReqDownloadMedical(requestor, targetRole).ToList();


            return getData.ToDataSourceResult(request);
        }


        [HttpPut("ApproveRequest")]
        public IActionResult ApproveRequest([FromBody] ReqDownloadMedical param)
        {
            string Approver = ServiceProxy.UserClaim.Name;
            param.Approver = Approver;
            param.StatusRequest = "Approved";
            MedicalRecordService.UpsertPut(param);

            return NoContent();
        }


    }


    #endregion

    #region MVC Controller
    [Area(ApplicationModule.OHS)]
    [Permission(PermissionKey.OHSApproverDownloadView)]
    public class ReqDownloadMedicalController : MvcControllerBase
    {
        #region Domain Services
        protected MdmService MdmService => ServiceProxy.GetService<MdmService>();
        protected McuService McuService => ServiceProxy.GetService<McuService>();
        protected UpkkService UpkkService => ServiceProxy.GetService<UpkkService>();
        protected PatientService PatientService => ServiceProxy.GetService<PatientService>();

        #endregion

        public IActionResult Index()
        {
            
            return View();
        }

    }
    #endregion
}

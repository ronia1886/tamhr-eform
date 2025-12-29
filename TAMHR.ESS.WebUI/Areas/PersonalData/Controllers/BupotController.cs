using System.Net.Mime;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Requests;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Agit.Common;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System;

namespace TAMHR.ESS.WebUI.Areas.PersonalData.Controllers
{
    [Route("api/bupot")]
    [Permission(PermissionKey.ViewBupot)]
    public class BupotApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// BUPOT service object.
        /// </summary>
        protected BupotService BupotService => ServiceProxy.GetService<BupotService>();
        #endregion

        [HttpPost("check-password")]
        public IActionResult CheckPassword([FromBody] GenericRequest<string> genericRequest)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            var isValid = CoreService.ValidHash(noreg, HashType.Payslip, genericRequest.Value, Configurations.BupotDefaultPassword);

            return Ok(new { IsValid = isValid });
        }

        [HttpPost("change-password")]
        public IActionResult ChangeBupotPassword([FromBody] BupotChangePasswordRequest changePasswordRequest)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            CoreService.UpdateHash(noreg, HashType.Payslip, changePasswordRequest.NewPassword);

            CoreService.CreateUserActivityLog(noreg, ActivityLog.Bupot, "Change Bupot document password");

            return NoContent();
        }

        [HttpPost("get-activity-logs")]
        public async Task<DataSourceResult> GetUserActivityLogs([DataSourceRequest] DataSourceRequest request)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            return await CoreService.GetUserActivityLogs(x => x.NoReg == noreg && x.LogTypeCode == ActivityLog.Bupot)
                .ToDataSourceResultAsync(request);
        }

        [HttpGet("download")]
        public async Task<IActionResult> Download(
            string password,
            int month,
            int period,
            string token,
            bool inline = true,
            bool isThr = false)
        {
             
                
            var isValid = ValidateAntiForgery(token);

            Assert.ThrowIf(!isValid, "Invalid token");

            var noreg = ServiceProxy.UserClaim.NoReg;
            var isValidHash = CoreService.ValidHash(noreg, HashType.Payslip, password, Configurations.BupotDefaultPassword);

            Assert.ThrowIf(!isValidHash, "You input the wrong password");

            var output = await BupotService.Download(noreg, month, period, isThr, inline);

            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);

            var action = inline ? "View" : "Download";

            CoreService.CreateUserActivityLog(noreg, ActivityLog.Bupot, string.Format("{0} eBupot document {1} {2}", action + (isThr ? " offcycle" : string.Empty), monthName, period));

            var fileName = string.Format("eBupot-{0}-{1}-{2}.pdf", noreg, monthName.ToUpper(), period);

            var cd = new ContentDisposition
            {
                FileName = fileName,
                Inline = inline
            };

            Response.Headers.Add("Content-Disposition", cd.ToString());
            Response.Headers.Add("X-Content-Type-Options", "nosniff");

            return File(output, "application/pdf");
        }
     
    }

    [Area(ApplicationModule.PersonalData)]
    [Permission(PermissionKey.ViewBupot)]
    public class BupotController : MvcControllerBase
    {
        #region Pages
        /// <summary>
        /// BUPOT default page.
        /// </summary>
        public IActionResult Index()
        {
        
            return View();
        }
        #endregion
    }
}
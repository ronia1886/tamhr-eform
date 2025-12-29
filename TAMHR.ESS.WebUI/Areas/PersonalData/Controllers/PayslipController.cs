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
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace TAMHR.ESS.WebUI.Areas.PersonalData.Controllers
{
    [Route("api/payslip")]
    [Permission(PermissionKey.ViewPayslip)]
    public class PayslipApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Payslip service object.
        /// </summary>
        protected PayslipService PayslipService => ServiceProxy.GetService<PayslipService>();

        #endregion

        [HttpPost("check-password")]
        public IActionResult CheckPassword([FromBody] GenericRequest<string> genericRequest)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            var isValid = CoreService.ValidHash(noreg, HashType.Payslip, genericRequest.Value, Configurations.PayslipDefaultPassword);

            return Ok(new { IsValid = isValid });
        }

        [HttpPost("change-password")]
        public IActionResult ChangePayslipPassword([FromBody] PayslipChangePasswordRequest changePasswordRequest)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            CoreService.UpdateHash(noreg, HashType.Payslip, changePasswordRequest.NewPassword);

            CoreService.CreateUserActivityLog(noreg, ActivityLog.Payslip, "Change payslip document password");

            return NoContent();
        }

        [HttpPost("get-activity-logs")]
        public async Task<DataSourceResult> GetUserActivityLogs([DataSourceRequest] DataSourceRequest request)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            return await CoreService.GetUserActivityLogs(x => x.NoReg == noreg && x.LogTypeCode == ActivityLog.Payslip)
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


            var username = ServiceProxy.UserClaim.Username;
            var ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();
            var browser = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();


            LogService.LogSuccess(username, ipAddress, browser, string.Format($"Api Payslip Download"), "Log Success Valid Get Token Payslip");

            Assert.ThrowIf(!isValid, "Invalid token");

            var noreg = ServiceProxy.UserClaim.NoReg;
            var isValidHash = CoreService.ValidHash(noreg, HashType.Payslip, password, Configurations.PayslipDefaultPassword);

            Assert.ThrowIf(!isValidHash, "You input the wrong password");

            var output = await PayslipService.Download(noreg, month, period, isThr, inline);

            if (output.Length > 0)
            {
                LogService.LogSuccess(username, ipAddress, browser, string.Format($"Api Payslip Download"), string.Format("Log Output Payslip {0} ", output.Length));
            }
            else
            {
                LogService.LogError(username, ipAddress, browser, string.Format($"Api Payslip Download"), string.Format("Log Error Output Payslip {0} ", output.Length));
            }
            

            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);

            var action = inline ? "View" : "Download";

            CoreService.CreateUserActivityLog(noreg, ActivityLog.Payslip, string.Format("{0} payslip document {1} {2}", action + (isThr ? " offcycle" : string.Empty), monthName, period));

            var fileName = string.Format("PAYSLIP-{0}-{1}-{2}.pdf", noreg, monthName.ToUpper(), period);

            var cd = new ContentDisposition
            {
                FileName = fileName,
                Inline = inline
            };

            Response.Headers.Append("Content-Disposition", cd.ToString());
            Response.Headers.Append("X-Content-Type-Options", "nosniff");

            return File(output, "application/pdf");
        }
    }

    [Area(ApplicationModule.PersonalData)]
    [Permission(PermissionKey.ViewPayslip)]
    public class PayslipController : MvcControllerBase
    {
        #region Pages
        /// <summary>
        /// Payslip default page.
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }
        #endregion
    }
}
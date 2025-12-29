using System.Net.Mime;
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

namespace TAMHR.ESS.WebUI.Areas.PersonalData.Controllers
{
    /// <summary>
    /// SPT API controller.
    /// </summary>
    [Route("api/spt")]
    [Permission(PermissionKey.ViewSpt)]
    public class SptApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// SPT service object.
        /// </summary>
        protected SptService SptService => ServiceProxy.GetService<SptService>();
        #endregion

        /// <summary>
        /// Check SPT user password.
        /// </summary>
        /// <param name="genericRequest">This <see cref="GenericRequest{T}"/> of string.</param>
        /// <returns></returns>
        [HttpPost("check-password")]
        public IActionResult CheckPassword([FromBody] GenericRequest<string> genericRequest)
        {
            // Get and set noreg from current user session.
            var noreg = ServiceProxy.UserClaim.NoReg;

            // Check whether password from given request is valid SPT hash or not.
            var isValid = CoreService.ValidHash(noreg, HashType.Payslip, genericRequest.Value, Configurations.SptDefaultPassword);

            // Return Ok result.
            return Ok(new { IsValid = isValid });
        }

        /// <summary>
        /// Get list of SPT user activity logs.
        /// </summary>
        /// <param name="request">This <see cref="DataSourceRequest"/> object.</param>
        /// <returns>This <see cref="DataSourceResult"/> object.</returns>
        [HttpPost("get-activity-logs")]
        public async Task<DataSourceResult> GetUserActivityLogs([DataSourceRequest] DataSourceRequest request)
        {
            // Get and set noreg from current user session.
            var noreg = ServiceProxy.UserClaim.NoReg;

            // Get list of SPT user activity logs by noreg.
            return await CoreService.GetUserActivityLogs(x => x.NoReg == noreg && x.LogTypeCode == ActivityLog.Spt)
                .ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Download SPT document.
        /// </summary>
        /// <param name="password">This user password.</param>
        /// <param name="period">This period.</param>
        /// <param name="token">This token.</param>
        /// <param name="inline">Determine whether the action type is view or download (True if view false if download).</param>
        /// <returns>This PDF binary document.</returns>
        [HttpGet("download")]
        public async Task<IActionResult> Download(string password, int period, string token, bool inline = true)
        {
            // Validate anti forgery token.
            var isValid = ValidateAntiForgery(token);

            // Throw an exception if token is not valid.
            Assert.ThrowIf(!isValid, "Invalid token");

            // Get and set noreg from current user session.
            var noreg = ServiceProxy.UserClaim.NoReg;

            // Check whether current user password is valid hash or not.
            var isValidHash = CoreService.ValidHash(noreg, HashType.Payslip, password, Configurations.SptDefaultPassword);

            // Throw an exception if the given hash is not valid.
            Assert.ThrowIf(!isValidHash, "You input the wrong password");

            // Get the PDF document as array of bytes.
            var output = await SptService.Download(noreg, period, inline);

            // Determine the action type to log and HTTP response inline type.
            var action = inline ? "View" : "Download";

            // Log the user activity.
            CoreService.CreateUserActivityLog(noreg, ActivityLog.Spt, string.Format("{0} SPT document {1}", action, period));

            // Get and set the filename of the PDF document.
            var fileName = string.Format("SPT-{0}-{1}.pdf", noreg, period);

            // Create new ContentDisposition object.
            var cd = new ContentDisposition
            {
                // Get and set the filename.
                FileName = fileName,
                // Get and set the inline flag.
                Inline = inline
            };

            // Add Content-Disposition header.
            Response.Headers.Add("Content-Disposition", cd.ToString());

            // Add X-Content-Type-Options header.
            Response.Headers.Add("X-Content-Type-Options", "nosniff");

            // Return the PDF document file.
            return File(output, "application/pdf");
        }
    }

    /// <summary>
    /// SPT MVC controller.
    /// </summary>
    [Area(ApplicationModule.PersonalData)]
    [Permission(PermissionKey.ViewSpt)]
    public class SptController : MvcControllerBase
    {
        #region Pages
        /// <summary>
        /// SPT default page.
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }
        #endregion
    }
}
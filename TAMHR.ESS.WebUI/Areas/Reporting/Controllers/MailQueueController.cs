using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace TAMHR.ESS.WebUI.Areas.Reporting.Controllers
{
    #region API Controller
    /// <summary>
    /// Api controller for mail queue
    /// </summary>
    [Route("api/mail-queue")]
    [Permission(PermissionKey.ViewMailQueue)]
    public class MailQueueApiController : ApiControllerBase
    {
        [HttpGet("summary")]
        public MailQueueSummaryViewModel GetMailQueueSummary()
        {
            return CoreService.GetMailQueueSummary();
        }

        /// <summary>
        /// Get list of mail queue
        /// </summary>
        /// <remarks>
        /// Get list of mail queue
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        public DataSourceResult GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return CoreService.GetMailQueues().ToDataSourceResult(request);
        }
    }
    #endregion

    #region MVC Controller
    [Area("Reporting")]
    [Permission(PermissionKey.ViewMailQueue)]
    public class MailQueueController : MvcControllerBase
    {
        public IActionResult Index()
        {
            return View();
        }
    }
    #endregion
}
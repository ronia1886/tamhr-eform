using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    /// <summary>
    /// Api controller for user tasks
    /// </summary>
    [Route("api/log")]
    //[Permission(PermissionKey.ViewApplicationLog)]
    public class LogApiController : ApiControllerBase
    {
        /// <summary>
        /// Get list of application log by username
        /// </summary>
        /// <remarks>
        /// Get list of application log by username
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("application")]
        [Permission(PermissionKey.ViewApplicationLog)]
        public async Task<DataSourceResult> GetApplicatoinLogs([DataSourceRequest] DataSourceRequest request)
        {
            var userName = ServiceProxy.UserClaim.Username;

            return await LogService.GetApplicationLogs(userName).ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get list of sync log by noreg
        /// </summary>
        /// <remarks>
        /// Get list of sync log by noreg
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("sync")]
        [Permission(PermissionKey.ViewSyncLog)]
        public async Task<DataSourceResult> GetSyncLogs([DataSourceRequest] DataSourceRequest request)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            return await LogService.GetSyncLogs(noreg).ToDataSourceResultAsync(request);
        }
    }
    #endregion

    #region MVC Controller
    [Area("Core")]
    [Permission(PermissionKey.ViewProxyLog)]
    public class LogController : MvcControllerBase
    {
        public IActionResult Index()
        {
            return View();
        }

        //[Permission(PermissionKey.ViewProxyLog)]
        public IActionResult ProxyLog()
        {
            return View();
        }
    }
    #endregion
}
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using TAMHR.ESS.Infrastructure.Web.Extensions;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    /// <summary>
    /// Api controller for user notifications
    /// </summary>
    [Route("api/notification")]
    //[Permission(PermissionKey.ViewNotifications)]
    
    public class NotifiactionApiController : ApiControllerBase
    {
        [HttpPost("read")]
        public async Task<IActionResult> Read()
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            if (User.GetClaim("Type") == "OHS")
                noreg = User.GetClaim("Username");
            await CoreService.ReadNotifications(noreg);

            return NoContent();
        }

        [HttpPost("clear")]
        public async Task<IActionResult> Clear()
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            await CoreService.ClearNotifications(noreg);

            return NoContent();
        }

        /// <summary>
        /// Get latest notifications by noreg
        /// </summary>
        /// <remarks>
        /// Get latest notifications by noreg
        /// </remarks>
        /// <returns>Notification View Model Object</returns>
        [HttpGet]
        public NotificationViewModel GetLatestNotifications()
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            if (User.GetClaim("Type") == "OHS")
                noreg = User.GetClaim("Username");

            return CoreService.GetLatestNotifications(noreg, 10);
        }

        /// <summary>
        /// Get list of notifications by noreg
        /// </summary>
        /// <remarks>
        /// Get list of notifications by noreg
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            return await CoreService.GetNotifications(noreg).ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get list of notices by noreg
        /// </summary>
        /// <remarks>
        /// Get list of notices by noreg
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("notices")]
        public async Task<DataSourceResult> GetNotices([DataSourceRequest] DataSourceRequest request)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            return await CoreService.GetNotifications(noreg, "notice").ToDataSourceResultAsync(request);
        }
    }
    #endregion
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Requests;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    /// <summary>
    /// Api controller for application configuration
    /// </summary>
    [Route("api/user-hash")]
    [Permission(PermissionKey.ListHash)]
    public class UserHashController : ApiControllerBase
    {
        /// <summary>
        /// Get list of configs
        /// </summary>
        /// <remarks>
        /// Get list of configs
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await CoreService.GetUserHashes()
                .ToDataSourceResultAsync(request);
        }

        [HttpDelete]
        public IActionResult ResetHash([FromForm] Guid id)
        {
            CoreService.DeleteHash(id);

            return NoContent();
        }
    }
    #endregion
}
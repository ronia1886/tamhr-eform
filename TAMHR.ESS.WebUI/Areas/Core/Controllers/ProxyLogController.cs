using System;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
		/// <summary>
    /// Proxy Log API Manager
    /// </summary>
    [Route("api/proxy-log")]
    [Permission(PermissionKey.ManageProxyLog)]
    public class ProxyLogApiController : ApiControllerBase
    {
        /// <summary>
        /// Get proxy log by id
        /// </summary>
        /// <remarks>
        /// Get proxy log by id
        /// </remarks>
        /// <param name="id">Proxy Log Id</param>
        /// <returns>Proxy Log Object</returns>
        [HttpGet("{id}")]
        public ProxyLog Get(Guid id) => CoreService.GetProxyLog(id);

        /// <summary>
        /// Get list of proxy logs
        /// </summary>
        /// <remarks>
        /// Get list of proxy logs
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        public DataSourceResult GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return ServiceProxy.GetDataSourceResult<ProxyLog>(request);
        }

        /// <summary>
        /// Delete proxy log by id
        /// </summary>
        /// <remarks>
        /// Delete proxy log by id
        /// </remarks>
        /// <param name="id">Proxy Log Id</param>
        /// <returns>No Content</returns>
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            CoreService.DeleteProxyLog(id);

            return NoContent();
        }
    }
	#endregion
}
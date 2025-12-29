using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    /// <summary>
    /// Api controller for user tasks
    /// </summary>
    [Route("api/approval-history")]
    //[Permission(PermissionKey.ViewTasks)]
    public class DocumentApprovalHistoryApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Approval service
        /// </summary>
        protected ApprovalService ApprovalService => ServiceProxy.GetService<ApprovalService>();
        #endregion

        /// <summary>
        /// Get list of document approval histories by document approval id
        /// </summary>
        /// <remarks>
        /// Get list of document approval histories by document approval id
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        public async Task<DataSourceResult> GetFromPosts([FromForm]Guid id, [DataSourceRequest] DataSourceRequest request)
        {
            return await ApprovalService.GetDocumentApprovalHistories(id).ToDataSourceResultAsync(request);
        }
    }
    #endregion
}
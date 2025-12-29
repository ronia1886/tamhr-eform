using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
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
    [Route("api/task")]
    //[Permission(PermissionKey.ViewTasks)]
    public class TaskApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Approval service
        /// </summary>
        protected ApprovalService ApprovalService => ServiceProxy.GetService<ApprovalService>();
        #endregion

        /// <summary>
        /// Get list of tasks by username
        /// </summary>
        /// <remarks>
        /// Get list of tasks by username
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            var username = ServiceProxy.UserClaim.Username;

            return await ApprovalService.GetTasks(username).ToDataSourceResultAsync(request);
        }

        [HttpPost("getstermination")]
        public async Task<DataSourceResult> GetFromHistoriesTermination([FromForm] string formKey, [DataSourceRequest] DataSourceRequest request)
        {
            var username = ServiceProxy.UserClaim.Username;

            return await ApprovalService.GetTasksTermination(username, formKey).ToDataSourceResultAsync(request);
        }
        /// <summary>
        /// Get list of documents created by noreg and form
        /// </summary>
        /// <remarks>
        /// Get list of documents created by noreg and form
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("getforms")]
        public async Task<DataSourceResult> GetFromPostsWithFormkey([FromForm]string formKey, [FromForm]bool showAll, [DataSourceRequest] DataSourceRequest request)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            return string.IsNullOrEmpty(formKey) && showAll && AclHelper.HasPermission("Core.ViewAllDocumentApprovals")
                ? await ApprovalService.GetTasks().ToDataSourceResultAsync(request)
                : await ApprovalService.GetTasks(noreg, formKey).ToDataSourceResultAsync(request);
        }
        /// <summary>
        /// Get list of documents created by noreg and form
        /// </summary>
        /// <remarks>
        /// Get list of documents created by noreg and form
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("getotherforms")]
        public async Task<DataSourceResult> GetOtherFromPostsWithFormkey([FromForm] string formKey, [FromForm] bool showAll, [DataSourceRequest] DataSourceRequest request)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            return string.IsNullOrEmpty(formKey) && showAll && AclHelper.HasPermission("Core.ViewAllDocumentApprovals")
                ? await ApprovalService.GetOtherTasks().ToDataSourceResultAsync(request)
                : await ApprovalService.GetOtherTasks(noreg, formKey).ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get list of documents created by noreg and form
        /// </summary>
        /// <remarks>
        /// Get list of documents created by noreg and form
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("getformstermination")]
        public async Task<DataSourceResult> GetFromPostsWithFormkeyTermination([FromForm] string formKey,[FromForm] bool canTerminate, [FromForm] bool showAll, [DataSourceRequest] DataSourceRequest request)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            return string.IsNullOrEmpty(formKey) && showAll && AclHelper.HasPermission("Core.ViewAllDocumentApprovals")
                ? await ApprovalService.GetTasks().ToDataSourceResultAsync(request)
                : await ApprovalService.GetTasksListTermintion(noreg, canTerminate, formKey).ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get list of tasks by parent
        /// </summary>
        /// <remarks>
        /// Get list of tasks by parent
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("childs")]
        public async Task<DataSourceResult> GetChilds([FromForm]Guid id, [DataSourceRequest] DataSourceRequest request)
        {
            return await ApprovalService.GetChilds(id).ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get completed tasks
        /// </summary>
        /// <remarks>
        /// Get completed tasks
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("completed")]
        public DataSourceResult GetCompletedTasks([DataSourceRequest] DataSourceRequest request)
        {
            return ServiceProxy.GetDataSourceResult<DocumentApprovalView>(request, new { RowStatus = true, VisibleInHistory = true, IntegrationDownload = true, DocumentStatusCode = DocumentStatus.Completed });
        }

        /// <summary>
        /// Get list of documents histories
        /// </summary>
        /// <remarks>
        /// Get list of documents histories
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gethistories")]
        public async Task<DataSourceResult> GetFromHistories([DataSourceRequest] DataSourceRequest request)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            return await ApprovalService.GetTaskHistories(noreg).ToDataSourceResultAsync(request);
        }

        [HttpPost("notiflongleave")]
        public IActionResult NotifLongLeave([FromBody] EmployeeLeaveViewModel model)
        {
            if (model == null)
                return BadRequest("Invalid data.");

            model.noreg = ServiceProxy.UserClaim.NoReg;

            bool isUpdated = ApprovalService.NotifLongLeave(model);

            return Ok();
        }


        /// <summary>
        /// Get list of documents historiesvtermination
        /// </summary>
        /// <remarks>
        /// Get list of documents histories termination
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        /// 
        //[HttpPost("gethistoriestermination")]
        //public async Task<DataSourceResult> GetFromHistoriesTermination([FromForm] string formKey, [DataSourceRequest] DataSourceRequest request)
        //{
        //    var noreg = ServiceProxy.UserClaim.NoReg;

        //    return await ApprovalService.GetTaskHistoriesTermination(noreg, formKey).ToDataSourceResultAsync(request);
        //}

        /// <summary>
        /// Get top tasks by username
        /// </summary>
        /// <remarks>
        /// Get top tasks by username
        /// </remarks>
        /// <returns>Notification View Model Object</returns>
        [HttpGet]
        public NotificationViewModel GetTopTasks()
        {
            var username = ServiceProxy.UserClaim.Username;

            return ApprovalService.GetTopTasks(username, 10);
        }

        /// <summary>
        /// Delete task by id
        /// </summary>
        /// <remarks>
        /// Delete task by id
        /// </remarks>
        /// <param name="id">Task Id</param>
        /// <returns>No Content</returns>
        [HttpDelete]
        public IActionResult Delete([FromForm]Guid id)
        {
            ApprovalService.DeleteDocumentApproval(id);

            return NoContent();
        }
    }
    #endregion
}
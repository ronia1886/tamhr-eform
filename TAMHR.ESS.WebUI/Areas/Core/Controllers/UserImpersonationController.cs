using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
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
    /// Language API Manager
    /// </summary>
    [Route("api/user-impersonation")]
    [Permission(PermissionKey.ManageUserImpersonation)]
    public class UserImpersonationApiController : ApiControllerBase
    {
        /// <summary>
        /// Get list of user impersonations
        /// </summary>
        /// <remarks>
        /// Get list of user impersonations
        /// </remarks>
        /// <returns>List of User Impersonations</returns>
        [HttpGet]
        public IEnumerable<IUserImpersonation> Gets() => CoreService.GetUserImpersonations();

        /// <summary>
        /// Get user impersonation by id
        /// </summary>
        /// <remarks>
        /// Get user impersonation by id
        /// </remarks>
        /// <param name="id">User Impersonation Id</param>
        /// <returns>User Impersonation Object</returns>
        [HttpGet("{id}")]
        public UserImpersonationView Get(Guid id) => CoreService.GetUserImpersonation(id);

        /// <summary>
        /// Get list of user impersonations
        /// </summary>
        /// <remarks>
        /// Get list of user impersonations
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await CoreService.GetUserImpersonations().ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Create new user impersonation
        /// </summary>
        /// <remarks>
        /// Create new user impersonation
        /// </remarks>
        /// <param name="userImpersonation">User Impersonation Object</param>
        /// <returns>Created User Impersonation Object</returns>
        [HttpPost]
        public IActionResult Create([FromBody] UserImpersonation userImpersonation)
        {
            CoreService.UpsertUserImpersonation(userImpersonation);

            return CreatedAtAction("Get", new { id = userImpersonation.Id });
        }

        /// <summary>
        /// Update user impersonation
        /// </summary>
        /// <remarks>
        /// Update user impersonation
        /// </remarks>
        /// <param name="userImpersonation">User Impersonation Object</param>
        /// <returns>No Content</returns>
        [HttpPut]
        public IActionResult Update([FromBody]UserImpersonation userImpersonation)
        {
            CoreService.UpsertUserImpersonation(userImpersonation);

            return NoContent();
        }

        /// <summary>
        /// Delete user impersonation by id
        /// </summary>
        /// <remarks>
        /// Delete user impersonation by id
        /// </remarks>
        /// <param name="id">User Impersonation Id</param>
        /// <returns>No Content</returns>
        [HttpDelete]
        public IActionResult Delete([FromForm]Guid id)
        {
            CoreService.DeleteUserImpersonation(id);

            return NoContent();
        }

        /// <summary>
        /// Download data
        /// </summary>
        /// <remarks>
        /// Download data
        /// </remarks>
        /// <returns>Data in Excel Format</returns>
        [HttpGet("download")]
        public IActionResult Download()
        {
            var data = Gets();

            return ExportToXlsx(data, $"UserImpersonation_{DateTime.Now.ToString("dd_MM_yyyy")}.xlsx");
        }

        /// <summary>
        /// Download data template
        /// </summary>
        /// <remarks>
        /// Download data template
        /// </remarks>
        /// <returns>Data Template in Excel Format</returns>
        [HttpGet("download-template")]
        public virtual IActionResult DownloadTemplate()
        {
            var controllerName = RouteData.Values["controller"].ToString();
            var cleanControllerName = controllerName.Substring(0, controllerName.Length - 3);

            return GenerateTemplate<UserImpersonation>($"{cleanControllerName}");
        }

        /// <summary>
        /// Upload and merge data
        /// </summary>
        /// <remarks>
        /// Upload and merge data
        /// </remarks>
        /// <returns>No Content</returns>
        [HttpPost("merge")]
        public async Task<IActionResult> Merge()
        {
            await UploadAndMergeAsync<UserImpersonation>(Request.Form.Files.FirstOrDefault(), new[] { "NoReg", "PostCode", "StartDate" });

            return NoContent();
        }
    }
    #endregion
}
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
    /// Permission API Manager
    /// </summary>
    [Route("api/permission")]
    [Permission(PermissionKey.ManagePermission)]
    public class PermissionApiController : ApiControllerBase
    {
        /// <summary>
        /// Get list of permissions
        /// </summary>
        /// <remarks>
        /// Get list of permissions
        /// </remarks>
        /// <returns>List of Permissions</returns>
        [HttpGet]
        public IEnumerable<Permission> Gets() => CoreService.GetPermissions();

        /// <summary>
        /// Get permission by id
        /// </summary>
        /// <remarks>
        /// Get permission by id
        /// </remarks>
        /// <param name="id">Permission Id</param>
        /// <returns>Permission Object</returns>
        [HttpGet("{id}")]
        public Permission Get(Guid id) => CoreService.GetPermission(id);

        /// <summary>
        /// Get list of permissions
        /// </summary>
        /// <remarks>
        /// Get list of permissions
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await CoreService.GetPermissions().ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Create new permission
        /// </summary>
        /// <remarks>
        /// Create new permission
        /// </remarks>
        /// <param name="permission">Permission Object</param>
        /// <returns>Created Permission Object</returns>
        [HttpPost]
        public IActionResult Create([FromBody] Permission permission)
        {
            CoreService.UpsertPermission(permission);

            return CreatedAtAction("Get", new { id = permission.Id });
        }

        /// <summary>
        /// Update permission
        /// </summary>
        /// <remarks>
        /// Update permission
        /// </remarks>
        /// <param name="permission">Permission Object</param>
        /// <returns>No Content</returns>
        [HttpPut]
        public IActionResult Update(Permission permission)
        {
            CoreService.UpsertPermission(permission);

            return NoContent();
        }

        /// <summary>
        /// Delete permission by id
        /// </summary>
        /// <remarks>
        /// Delete permission by id
        /// </remarks>
        /// <param name="id">Permission Id</param>
        /// <returns>No Content</returns>
        [HttpDelete]
        public IActionResult Delete([FromForm]Guid id)
        {
            CoreService.DeletePermission(id);

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

            return ExportToXlsx(data, $"Permission_{DateTime.Now.ToString("dd_MM_yyyy")}.xlsx");
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

            return GenerateTemplate<Permission>($"{cleanControllerName}");
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
            await UploadAndMergeAsync<Permission>(Request.Form.Files.FirstOrDefault(), new[] { "PermissionKey" });

            return NoContent();
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Permission page controller
    /// </summary>
    [Area("Core")]
    [Permission(PermissionKey.ViewPermission)]
    public class PermissionController : MvcControllerBase
    {
        /// <summary>
        /// Core service object
        /// </summary>
        protected CoreService CoreService { get { return ServiceProxy.GetService<CoreService>(); } }

        /// <summary>
        /// Permission main page
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Load permission form by id
        /// </summary>
        /// <param name="id">Permission Id</param>
        /// <returns>Permission Form</returns>
        [HttpPost]
        public IActionResult Load(Guid id)
        {
            var permission = CoreService.GetPermission(id);

            return PartialView("_PermissionForm", permission);
        }
    }
    #endregion
}
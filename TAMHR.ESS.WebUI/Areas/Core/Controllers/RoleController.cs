using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    /// <summary>
    /// Role API Manager
    /// </summary>
    [Route("api/role")]
    [Permission(PermissionKey.ManageRole)]
    public class RoleApiController : ApiControllerBase
    {
        /// <summary>
        /// Get list of roles
        /// </summary>
        /// <remarks>
        /// Get list of roles
        /// </remarks>
        /// <returns>List of Roles</returns>
        [HttpGet]
        public IEnumerable<Role> Gets() => CoreService.GetRoles();

        /// <summary>
        /// Get role by id
        /// </summary>
        /// <remarks>
        /// Get role by id
        /// </remarks>
        /// <param name="id">Role Id</param>
        /// <returns>Role Object</returns>
        [HttpGet("{id}")]
        public Role Get(Guid id) => CoreService.GetRole(id);

        /// <summary>
        /// Get list of roles
        /// </summary>
        /// <remarks>
        /// Get list of roles
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await CoreService.GetRoles().ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get list of permissions by role id
        /// </summary>
        /// <remarks>
        /// Get list of permissions by role id
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("getpermissions")]
        public async Task<DataSourceResult> GetPermissions([DataSourceRequest] DataSourceRequest request)
        {
            var roleId = Guid.Parse(Request.Form["roleId"]);

            return await CoreService.GetPermissions(roleId).ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Create new role
        /// </summary>
        /// <remarks>
        /// Create new role
        /// </remarks>
        /// <param name="viewModel">Role View Model</param>
        /// <returns>Created Role Object</returns>
        [HttpPost]
        public IActionResult Create([FromBody] RoleViewModel viewModel)
        {
            CoreService.UpsertRole(viewModel);

            return CreatedAtAction("Get", new { id = viewModel.Id });
        }

        /// <summary>
        /// Update role
        /// </summary>
        /// <remarks>
        /// Update role
        /// </remarks>
        /// <param name="viewModel">Role View Model</param>
        /// <returns>No Content</returns>
        [HttpPut]
        public IActionResult Update([FromBody]RoleViewModel viewModel)
        {
            CoreService.UpsertRole(viewModel);

            return NoContent();
        }

        /// <summary>
        /// Delete role by id
        /// </summary>
        /// <remarks>
        /// Delete role by id
        /// </remarks>
        /// <param name="id">Role Id</param>
        /// <returns>No Content</returns>
        [HttpDelete]
        public IActionResult Delete([FromForm]Guid id)
        {
            CoreService.DeleteRole(id);

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

            return ExportToXlsx(data, $"Role_{DateTime.Now.ToString("dd_MM_yyyy")}.xlsx");
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

            return GenerateTemplate<Role>($"{cleanControllerName}");
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
            await UploadAndMergeAsync<Role>(Request.Form.Files.FirstOrDefault(), new[] { "RoleKey" });

            return NoContent();
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Role page controller
    /// </summary>
    [Area("Core")]
    [Permission(PermissionKey.ViewRole)]
    public class RoleController : MvcControllerBase
    {
        /// <summary>
        /// Core service object
        /// </summary>
        protected CoreService CoreService { get { return ServiceProxy.GetService<CoreService>(); } }

        /// <summary>
        /// Role main page
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Load role form by id
        /// </summary>
        /// <param name="id">Role Id</param>
        /// <returns>Role Form</returns>
        [HttpPost]
        public IActionResult Load(Guid id)
        {
            var role = CoreService.GetRole(id);

            return PartialView("_RoleForm", role);
        }
    }
    #endregion
}
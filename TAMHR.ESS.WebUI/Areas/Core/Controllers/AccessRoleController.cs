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
    /// Access Role API Manager
    /// </summary>
    [Route("api/access-role")]
    [Permission(PermissionKey.ManageAccessRole)]
    public class AccessRoleApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Access role service object
        /// </summary>
        public AccessRoleService AccessRoleService => ServiceProxy.GetService<AccessRoleService>();
        #endregion

        /// <summary>
        /// Get list of access roles
        /// </summary>
        /// <remarks>
        /// Get list of access roles
        /// </remarks>
        /// <returns>List of AccessRoles</returns>
        [HttpGet]
        public IEnumerable<AccessRoleView> Gets() => AccessRoleService.Gets();

        /// <summary>
        /// Get access role by id
        /// </summary>
        /// <remarks>
        /// Get access role by id
        /// </remarks>
        /// <param name="id">Access Role Id</param>
        /// <returns>Access Role Object</returns>
        [HttpGet("{id}")]
        public AccessRoleView Get(Guid id) => AccessRoleService.Get(id);

        /// <summary>
        /// Get list of access roles
        /// </summary>
        /// <remarks>
        /// Get list of access roles
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await AccessRoleService.Gets().ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get access group mapping query categories
        /// </summary>
        /// <remarks>
        /// Get access group mapping query categories
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("get-mapping-query")]
        public async Task<DataSourceResult> GetByMappingQuery([DataSourceRequest] DataSourceRequest request)
        {
            string code = Request.Form["code"].ToString();

            return await AccessRoleService.GetMappingQuery(code).ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Create new access role
        /// </summary>
        /// <remarks>
        /// Create new access role
        /// </remarks>
        /// <param name="accessRole">Access Role Object</param>
        /// <returns>Created Access Role Object</returns>
        [HttpPost]
        public IActionResult Create([FromBody] AccessRole accessRole)
        {
            AccessRoleService.Upsert(accessRole);

            return CreatedAtAction("Get", new { id = accessRole.Id });
        }

        /// <summary>
        /// Update access role
        /// </summary>
        /// <remarks>
        /// Update access role
        /// </remarks>
        /// <param name="accessRole">Access Role Object</param>
        /// <returns>No Content</returns>
        [HttpPut]
        public IActionResult Update([FromBody]AccessRole accessRole)
        {
            AccessRoleService.Upsert(accessRole);

            return NoContent();
        }

        /// <summary>
        /// Delete access role by id
        /// </summary>
        /// <remarks>
        /// Delete access role by id
        /// </remarks>
        /// <param name="id">Access Role Id</param>
        /// <returns>No Content</returns>
        [HttpDelete]
        public IActionResult Delete([FromForm]Guid id)
        {
            AccessRoleService.Delete(id);

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

            return ExportToXlsx(data, $"AccessRole_{DateTime.Now.ToString("dd_MM_yyyy")}.xlsx");
        }

        /// <summary>
        /// Download data template
        /// </summary>
        /// <remarks>
        /// Download data template
        /// </remarks>
        /// <returns>Data Template in Excel Format</returns>
        [HttpGet("download-template")]
        public IActionResult DownloadTemplate()
        {
            var controllerName = RouteData.Values["controller"].ToString();
            var cleanControllerName = controllerName.Substring(0, controllerName.Length - 3);

            return GenerateTemplate<AccessRole>($"{cleanControllerName}");
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
            await UploadAndMergeAsync<AccessRole>(Request.Form.Files.FirstOrDefault(), new[] { "AccessCode" });

            return NoContent();
        }
    }
    #endregion
}
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Helpers;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    /// <summary>
    /// Guideline API manager.
    /// </summary>
    [Route("api/guideline")]
    [Permission(PermissionKey.ManageGuideline)]
    public class GuidelineApiController : ApiControllerBase
    {
        /// <summary>
        /// Get list of guidelines.
        /// </summary>
        /// <remarks>
        /// Get list of guidelines.
        /// </remarks>
        /// <param name="request">This <see cref="DataSourceRequest"/> object.</param>
        /// <returns>This <see cref="DataSourceResult"/> object.</returns>
        [HttpPost("gets")]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await CoreService.GetGuidelinesQuery()
                .ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Create new guideline.
        /// </summary>
        /// <remarks>
        /// Create new guideline.
        /// </remarks>
        /// <param name="guideline">This <see cref="Guideline"/> object.</param>
        /// <returns>This created guideline object.</returns>
        [HttpPost]
        public IActionResult Create([FromBody] Guideline guideline)
        {
            guideline.FileUrl = MoveFile("guidelines", guideline.FileUrl.SanitizeFileName(string.Empty), guideline.Title.SanitizeFileName(string.Empty));

            CoreService.UpsertGuideline(guideline);

            return CreatedAtAction("Get", new { id = guideline.Id });
        }

        /// <summary>
        /// Update guideline.
        /// </summary>
        /// <remarks>
        /// Update guideline.
        /// </remarks>
        /// <param name="guideline">This <see cref="Guideline"/> object.</param>
        /// <returns>No content result.</returns>
        [HttpPut]
        public IActionResult Update([FromBody]Guideline guideline)
        {
            guideline.FileUrl = MoveFile("guidelines", guideline.FileUrl, guideline.Title.SanitizeFileName(string.Empty));

            CoreService.UpsertGuideline(guideline);

            return NoContent();
        }

        [HttpGet("download")]
        public IActionResult Download()
        {
            var data = CoreService.GetGuidelinesQuery();

            return ExportToXlsx(data, $"Guidelines_{DateTime.Now.ToString("dd_MM_yyyy")}.xlsx");
        }

        /// <summary>
        /// Download data template.
        /// </summary>
        /// <remarks>
        /// Download data template.
        /// </remarks>
        /// <returns>This template in excel format.</returns>
        [HttpGet("download-template")]
        public virtual IActionResult DownloadTemplate()
        {
            var controllerName = RouteData.Values["controller"].ToString();
            var cleanControllerName = controllerName.Substring(0, controllerName.Length - 3);

            return GenerateTemplate<Guideline>($"{cleanControllerName}");
        }

        /// <summary>
        /// Delete guideline by id.
        /// </summary>
        /// <remarks>
        /// Delete guideline by id.
        /// </remarks>
        /// <param name="id">This guideline id.</param>
        /// <returns>No content result.</returns>
        [HttpDelete]
        public IActionResult Delete([FromForm]Guid id)
        {
            CoreService.DeleteGuideline(id);

            return NoContent();
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Guideline page controller.
    /// </summary>
    [Area("Core")]
    public class GuidelineController : MvcControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Core service object.
        /// </summary>
        protected CoreService CoreService => ServiceProxy.GetService<CoreService>();
        #endregion

        #region Pages
        /// <summary>
        /// Guideline master data page.
        /// </summary>
        /// <returns>Guideline master data view page.</returns>
        [Permission(PermissionKey.ViewGuideline)]
        public IActionResult Index()
        {
            return View();
        }
        #endregion

        #region Partial Views
        /// <summary>
        /// Load guideline form by id.
        /// </summary>
        /// <param name="id">This guideline id.</param>
        /// <returns>Guideline partial view.</returns>
        [HttpPost]
        [Permission(PermissionKey.ManageGuideline)]
        public IActionResult Load(Guid id)
        {
            var guideline = CoreService.GetGuideline(id);

            return PartialView("_GuidelineForm", guideline);
        }
        #endregion
    }
    #endregion
}
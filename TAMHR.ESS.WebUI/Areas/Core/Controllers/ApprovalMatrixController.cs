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
    /// News API Manager
    /// </summary>
    [Route("api/approvalmatrix")]
    [Permission(PermissionKey.ManageApprovalMatrix)]
    public class ApprovalMatrixApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Approval matrix service object
        /// </summary>
        public ApprovalMatrixService ApprovalMatrixService => ServiceProxy.GetService<ApprovalMatrixService>();
        public FormService FormService => ServiceProxy.GetService<FormService>();
        #endregion

        /// <summary>
        /// Get list of approval matrices
        /// </summary>
        /// <remarks>
        /// Get list of approval matrices
        /// </remarks>
        /// <returns>List of Approval Matrices</returns>
        [HttpGet]
        public IEnumerable<ApprovalMatrixView> Gets() => ApprovalMatrixService.Gets();

        /// <summary>
        /// Get approval matrix by id
        /// </summary>
        /// <remarks>
        /// Get approval matrix by id
        /// </remarks>
        /// <param name="id">Approval Matrix Id</param>
        /// <returns>Approval Matrix Object</returns>
        [HttpGet("{id}")]
        public ApprovalMatrix Get(Guid id) => ApprovalMatrixService.Get(id);

        /// <summary>
        /// Get list of approval matrices
        /// </summary>
        /// <remarks>
        /// Get list of approval matrices
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await ApprovalMatrixService.Gets().ToList().ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get approval group mapping query categories
        /// </summary>
        /// <remarks>
        /// Get approval group mapping query categories
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>

        [HttpPost("get-mapping-query")]
        public async Task<DataSourceResult> GetByMappingQuery([DataSourceRequest] DataSourceRequest request)
        {
            string code = Request.Form["code"].ToString();
            return await ApprovalMatrixService.GetMappingQuery(code).ToDataSourceResultAsync(request);
        }
        /// <summary>
        /// Create new approval matrix
        /// </summary>
        /// <remarks>
        /// Create new approval matrix
        /// </remarks>
        /// <param name="approvalMatrix">Approval Matrix Object</param>
        /// <returns>Created Approval Matrix Object</returns>
        [HttpPost]
        public IActionResult Create([FromBody] ApprovalMatrix approvalMatrix)
        {
            ApprovalMatrixService.Upsert(approvalMatrix);

            return CreatedAtAction("Get", new { id = approvalMatrix.Id });
        }

        /// <summary>
        /// Update approval matrix
        /// </summary>
        /// <remarks>
        /// Update approval matrix
        /// </remarks>
        /// <param name="approvalMatrix">Approval Matrix Object</param>
        /// <returns>No Content</returns>
        [HttpPut]
        public IActionResult Update([FromBody]ApprovalMatrix approvalMatrix)
        {
            ApprovalMatrixService.Upsert(approvalMatrix);

            return NoContent();
        }

        /// <summary>
        /// Delete approval matrix by id
        /// </summary>
        /// <remarks>
        /// Delete approval matrix by id
        /// </remarks>
        /// <param name="id">Approval Matrix Id</param>
        /// <returns>No Content</returns>
        [HttpDelete]
        public IActionResult Delete([FromForm]Guid id)
        {
            ApprovalMatrixService.Delete(id);

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

            return ExportToXlsx(data, $"ApprovalMatrix_{DateTime.Now.ToString("dd_MM_yyyy")}.xlsx");
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

            return GenerateTemplate<ApprovalMatrix>($"{cleanControllerName}");
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
            await UploadAndMergeAsync<ApprovalMatrix>(Request.Form.Files.FirstOrDefault(), new[] { "FormId", "Approver", "ApproverLevel" });

            return NoContent();
        }

        [HttpPost("get-initiator-pattern")]
        public async Task<DataSourceResult> GetInitiatorPattern([DataSourceRequest] DataSourceRequest request)
        {
            var formId = Guid.Parse(Request.Form["formId"]);

            var output = ApprovalMatrixService.GetInitiatorPatterns().Where(wh => wh.FormId == formId).ToList();
            return await ApprovalMatrixService.GetInitiatorPatterns().Where(wh => wh.FormId == formId).ToDataSourceResultAsync(request);
        }

        [HttpPost("get-initiator-type")]
        public async Task<DataSourceResult> GetInitiatorType([DataSourceRequest] DataSourceRequest request)
        {
            string code = Request.Form["code"].ToString();

            return await ApprovalMatrixService.GetInitiatorType().Where(wh => wh.ParentGeneralCategoryCode==code).ToDataSourceResultAsync(request);
        }
    }
    #endregion
}
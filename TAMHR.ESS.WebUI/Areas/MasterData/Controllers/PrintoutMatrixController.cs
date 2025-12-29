using System;
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
using System.Linq;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    /// <summary>
    /// Printout Matrix API Manager
    /// </summary>
    [Route("api/master-data/printout-matrix")]
    public class PrintoutMatrixApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Printout matrix service object
        /// </summary>
        public PrintoutMatrixService PrintoutMatrixService => ServiceProxy.GetService<PrintoutMatrixService>();
        #endregion

        /// <summary>
        /// Get list of printout matrices
        /// </summary>
        /// <remarks>
        /// Get list of printout matrices
        /// </remarks>
        /// <returns>List of Printout Matrices</returns>
        [HttpGet]
        public IEnumerable<PrintoutMatrixView> Gets() => PrintoutMatrixService.Gets();

        /// <summary>
        /// Get list of printout matrices
        /// </summary>
        /// <remarks>
        /// Get list of printout matrices
        /// </remarks>
        /// <returns>List of PrintoutMatrixs</returns>
        [HttpPost("gets")]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            //return await PrintoutMatrixService.GetQuery().ToDataSourceResultAsync(request);
            var data = PrintoutMatrixService.GetQuery().ToList();

            return await Task.FromResult(data.ToDataSourceResult(request));
        }

        /// <summary>
        /// Create new printout matrix
        /// </summary>
        /// <remarks>
        /// Create new printout matrix
        /// </remarks>
        /// <param name="printoutMatrix">Printout Matrix Object</param>
        /// <returns>Created Printout Matrix Object</returns>
        [HttpPost]
        public IActionResult Create([FromBody] PrintoutMatrix printoutMatrix)
        {
            PrintoutMatrixService.Upsert(printoutMatrix);

            return CreatedAtAction("Get", new { id = printoutMatrix.Id });
        }

        /// <summary>
        /// Update printout matrix
        /// </summary>
        /// <remarks>
        /// Update printout matrix
        /// </remarks>
        /// <param name="printout matrix">Printout Matrix Object</param>
        /// <returns>No Content</returns>
        [HttpPut]
        public IActionResult Update([FromBody] PrintoutMatrix printoutMatrix)
        {
            PrintoutMatrixService.Upsert(printoutMatrix);

            return NoContent();
        }

        /// <summary>
        /// Delete printout matrix by id
        /// </summary>
        /// <remarks>
        /// Delete printout matrix by id
        /// </remarks>
        /// <param name="id">Printout Matrix Id</param>
        /// <returns>No Content</returns>
        [HttpDelete]
        public IActionResult Delete([FromForm]Guid id)
        {
            PrintoutMatrixService.Delete(id);

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

            return ExportToXlsx(data, $"PrintoutMatrix_{DateTime.Now.ToString("dd_MM_yyyy")}.xlsx");
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Printout matrix page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewPrintoutMatrix)]
    public class PrintoutMatrixController : MvcControllerBase
    {
        /// <summary>
        /// Printout matrix service object
        /// </summary>
        protected PrintoutMatrixService PrintoutMatrixService { get { return ServiceProxy.GetService<PrintoutMatrixService>(); } }

        /// <summary>
        /// Printout matrix main page
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Load printout matrix form by id
        /// </summary>
        /// <param name="id">Printout Matrix Id</param>
        /// <returns>Printout Matrix Form</returns>
        [HttpPost]
        public IActionResult Load(Guid id)
        {
            var printoutMatrix = PrintoutMatrixService.Get(id);

            return PartialView("_PrintoutMatrixForm", printoutMatrix);
        }
    }
    #endregion
}
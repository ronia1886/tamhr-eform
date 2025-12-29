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
using System.Data.Entity;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    /// <summary>
    /// Api controller for general category
    /// </summary>
    [Route("api/category")]
    [Permission(PermissionKey.ManageGeneralCategory)]
    public class GeneralCategoryApiController : ApiControllerBase
    {
        #region Domain Services
        protected MdmService MdmService => ServiceProxy.GetService<MdmService>();
        #endregion

        /// <summary>
        /// Get list of general categories
        /// </summary>
        /// <remarks>
        /// Get list of general categories
        /// </remarks>
        /// <returns>List of General Categories</returns>
        [HttpGet]
        public IEnumerable<GeneralCategory> Gets() => ConfigService.GetGeneralCategories();

        /// <summary>
        /// Get general category by id
        /// </summary>
        /// <remarks>
        /// Get general category by id
        /// </remarks>
        /// <param name="id">General Category Id</param>
        /// <returns>General Category Object</returns>
        [HttpGet("{id}")]
        public GeneralCategory Get(Guid id) => ConfigService.GetGeneralCategory(id);

        /// <summary>
        /// Get list of general categories
        /// </summary>
        /// <remarks>
        /// Get list of general categories
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            //return await ConfigService.GetGeneralCategoriesQuery().ToDataSourceResultAsync(request);
            var data = ConfigService.GetGeneralCategoriesQuery()
                            .AsNoTracking()
                            .ToList();

            return await data.ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get general categories query by category
        /// </summary>
        /// <remarks>
        /// Get general categories query by category
        /// </remarks>
        /// <param name="category">Category</param>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("get-query/{category}")]
        public async Task<DataSourceResult> GetByCategoryQuery(string category, [DataSourceRequest] DataSourceRequest request)
        {
            return await ConfigService.GetGeneralCategoriesQuery(category).ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get mapping categories query by category
        /// </summary>
        /// <remarks>
        /// Get mapping categories query by category
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("get-mapping-query")]
        public async Task<DataSourceResult> GetByMappingCategoryQuery([DataSourceRequest] DataSourceRequest request)
        {
            string code = Request.Form["code"].ToString();
            return await ConfigService.GetGeneralCategoryMappingQuery(code).ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get list of general categories by category
        /// </summary>
        /// <remarks>
        /// Get list of general categories by category
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("getsbycategoryfilter")]
        public async Task<DataSourceResult> GetByCategory(string category, [DataSourceRequest] DataSourceRequest request)
        {
            return await ConfigService.GetGeneralCategories(category, true).ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get list of general categories by category
        /// </summary>
        /// <remarks>
        /// Get list of general categories by category
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("getsbycategory")]
        public async Task<DataSourceResult> GetByCategory(string category, bool all = false)
        {
            var data = await ConfigService.GetGeneralCategories(category, false).ToDataSourceResultAsync(new DataSourceRequest());

            //data.Add();

            return data;
        }

        /// <summary>
        /// Get list of general categories by category
        /// </summary>
        /// <remarks>
        /// Get list of general categories by category
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("getsbycategorycustom")]
        public async Task<DataSourceResult> GetByCategoryCustom(string category, string departmentCode)
        {
            return await ConfigService.GetCustomGeneralCategories(category, true).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getsbycategorycustomshift")]
        public async Task<DataSourceResult> GetByCategoryCustomShift(string category)
        {
            var orgCode = MdmService.GetActualOrganizationStructure(ServiceProxy.UserClaim.NoReg, ServiceProxy.UserClaim.PostCode).OrgCode;

            return await ConfigService.GetCustomShiftGeneralCategories(category, orgCode, true)
                .ToDataSourceResultAsync(new DataSourceRequest());
        }

        /// <summary>
        /// Get list of general categories by category
        /// </summary>
        /// <remarks>
        /// Get list of general categories by category
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("getsbycategorymapping")]
        public async Task<DataSourceResult> GetByMappingCategory()
        {
            string category = this.Request.Form["category"].ToString();
            return await ConfigService.GetGeneralCategoryMapping(category, false).ToDataSourceResultAsync(new DataSourceRequest());
        }

        /// <summary>
        /// Get list of general categories by category
        /// </summary>
        /// <remarks>
        /// Get list of general categories by category
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpGet("getmappings")]
        public IEnumerable<GeneralCategoryMappingView> GetMappings(string category)
        {
            return ConfigService.GetGeneralCategoryMapping(category, false);
        }

        /// <summary>
        /// Create new general category
        /// </summary>
        /// <remarks>
        /// Create new general category
        /// </remarks>
        /// <param name="generalCategory">General Category Object</param>
        /// <returns>Created General Category Object</returns>
        [HttpPost]
        public IActionResult Create([FromBody] GeneralCategory generalCategory)
        {
            ConfigService.UpsertGeneralCategory(generalCategory);

            return CreatedAtAction("Get", new { id = generalCategory.Id });
        }

        /// <summary>
        /// Create new mapping general category
        /// </summary>
        /// <remarks>
        /// Create new mapping general category
        /// </remarks>
        /// <param name="generalCategoryMapping">General Category Object</param>
        /// <returns>Created General Category Object</returns>
        [HttpPost("map")]
        public IActionResult CreateMapping([FromBody] GeneralCategoryMapping generalCategoryMapping)
        {
            ConfigService.UpsertGeneralCategoryMapping(generalCategoryMapping);

            return CreatedAtAction("Get", new { id = generalCategoryMapping.Id });
        }

        /// <summary>
        /// Update general category
        /// </summary>
        /// <remarks>
        /// Update general category
        /// </remarks>
        /// <param name="generalCategory">General Category Object</param>
        /// <returns>No Content</returns>
        [HttpPut]
        public IActionResult Update([FromBody]GeneralCategory generalCategory)
        {
            ConfigService.UpsertGeneralCategory(generalCategory);

            return NoContent();
        }

        /// <summary>
        /// Delete general category by id
        /// </summary>
        /// <remarks>
        /// Delete general category by id
        /// </remarks>
        /// <param name="id">General Category Id</param>
        /// <returns>No Content</returns>
        [HttpDelete]
        public IActionResult Delete([FromForm]Guid id)
        {
            ConfigService.DeleteGeneralCategory(id);

            return NoContent();
        }

        /// <summary>
        /// Delete general category mapping by id
        /// </summary>
        /// <remarks>
        /// Delete general category mapping by id
        /// </remarks>
        /// <param name="id">General Category Mapping Id</param>
        /// <returns>No Content</returns>
        [HttpDelete("map")]
        public IActionResult DeleteMapping([FromForm]Guid id)
        {
            ConfigService.DeleteGeneralCategoryMapping(id);

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

            return ExportToXlsx(data, $"GenralCategory_{DateTime.Now.ToString("dd_MM_yyyy")}.xlsx");
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

            return GenerateTemplate<GeneralCategory>($"{cleanControllerName}");
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
            await UploadAndMergeAsync<GeneralCategory>(Request.Form.Files.FirstOrDefault(), new[] { "Code" });

            return NoContent();
        }
    }
    #endregion
}
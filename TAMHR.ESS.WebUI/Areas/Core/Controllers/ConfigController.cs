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
    /// Api controller for application configuration
    /// </summary>
    [Route("api/config")]
    [Permission(PermissionKey.ManageConfig)]
    public class ConfigApiController : ApiControllerBase
    {
        /// <summary>
        /// Get list of configs
        /// </summary>
        /// <remarks>
        /// Get list of configs
        /// </remarks>
        /// <returns>List of Configs</returns>
        [HttpGet]
        public IEnumerable<Config> Gets() => ConfigService.GetConfigs();

        /// <summary>
        /// Get config by id
        /// </summary>
        /// <remarks>
        /// Get config by id
        /// </remarks>
        /// <param name="id">Config Id</param>
        /// <returns>Config Object</returns>
        [HttpGet("{id}")]
        public Config Get(Guid id) => ConfigService.GetConfig(id);

        /// <summary>
        /// Get list of configs
        /// </summary>
        /// <remarks>
        /// Get list of configs
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await ConfigService.GetConfigs().ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Create new config
        /// </summary>
        /// <remarks>
        /// Create new config
        /// </remarks>
        /// <param name="config">Config Object</param>
        /// <returns>Created Config Object</returns>
        [HttpPost]
        public IActionResult Create([FromBody] Config config)
        {
            ConfigService.UpsertConfig(config);

            return CreatedAtAction("Get", new { id = config.Id });
        }

        /// <summary>
        /// Update config
        /// </summary>
        /// <remarks>
        /// Update config
        /// </remarks>
        /// <param name="config">Config Object</param>
        /// <returns>No Content</returns>
        [HttpPut]
        public IActionResult Update([FromBody]Config config)
        {
            ConfigService.UpsertConfig(config);

            if(config.ConfigKey == "Abnormality.EndDate" || config.ConfigKey == "Abnormality.StartDate")
            {
                ConfigService.ChangedAbnormalityDate();
            }

            return NoContent();
        }

        /// <summary>
        /// Delete config by id
        /// </summary>
        /// <remarks>
        /// Delete config by id
        /// </remarks>
        /// <param name="id">Config Id</param>
        /// <returns>No Content</returns>
        [HttpDelete]
        public IActionResult Delete([FromForm]Guid id)
        {
            ConfigService.DeleteConfig(id);

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

            return ExportToXlsx(data, $"Config_{DateTime.Now.ToString("dd_MM_yyyy")}.xlsx");
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

            return GenerateTemplate<Config>($"{cleanControllerName}");
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
            await UploadAndMergeAsync<Config>(Request.Form.Files.FirstOrDefault(), new[] { "ConfigKey" });

            return NoContent();
        }
    }
    #endregion
}
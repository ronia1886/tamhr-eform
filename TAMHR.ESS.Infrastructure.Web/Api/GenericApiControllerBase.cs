using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Agit.Common;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace TAMHR.ESS.Infrastructure.Web
{
    public abstract class GenericApiControllerBase<T, D> : ApiControllerBase
        where D : class, IEntityBase<Guid>
        where T : GenericDomainServiceBase<D>
    {
        protected abstract string[] ComparerKeys { get; }

        #region Domain Services
        /// <summary>
        /// Common service object
        /// </summary>
        public T CommonService => ServiceProxy.GetService<T>();
        #endregion

        [HttpGet]
        public IEnumerable<D> Gets() => CommonService.Gets();

        [HttpPost("gets")]
        public virtual async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await CommonService.GetQuery().ToDataSourceResultAsync(request);
        }

        [HttpPost]
        public virtual IActionResult Create([FromBody] D input)
        {
            CommonService.Upsert(input);

            return CreatedAtAction("Get", new { id = input.Id });
        }

        [HttpPut]
        public virtual IActionResult Update([FromBody] D input)
        {
            CommonService.Upsert(input);

            return NoContent();
        }

        [HttpDelete]
        public virtual IActionResult Delete([FromForm]Guid id)
        {
            CommonService.DeleteById(id);

            return NoContent();
        }

        [HttpDelete("soft-delete")]
        public virtual IActionResult SoftDelete([FromForm]Guid id)
        {
            CommonService.SoftDeleteById(id);

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
        public virtual IActionResult Download()
        {
            var data = Gets();
            var controllerName = RouteData.Values["controller"].ToString();
            var cleanControllerName = controllerName.Substring(0, controllerName.Length - 3);

            return ExportToXlsx(data, $"{cleanControllerName}");
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

            return GenerateTemplate<D>($"{cleanControllerName}");
        }

        /// <summary>
        /// Upload and merge data
        /// </summary>
        /// <remarks>
        /// Upload and merge data
        /// </remarks>
        /// <returns>No Content</returns>
        [HttpPost("merge")]
        public virtual async Task<IActionResult> Merge()
        {
            await UploadAndMergeAsync<D>(Request.Form.Files.FirstOrDefault(), ComparerKeys);

            return NoContent();
        }
    }
}

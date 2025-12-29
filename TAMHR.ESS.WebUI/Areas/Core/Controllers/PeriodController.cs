using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    /// <summary>
    /// Period API Manager
    /// </summary>
    [Route("api/period")]
    public class PeriodApiController : ApiControllerBase
    {
        /// <summary>
        /// Get list of Period
        /// </summary>
        /// <remarks>
        /// Get list of Period
        /// </remarks>
        /// <returns>List of Period</returns>
        [HttpPost("years")]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await ConfigService.GetPeriodYears().ToDataSourceResultAsync(request);
        }

        [HttpPost("years_shift")]
        public async Task<DataSourceResult> GetFromPostsShift([DataSourceRequest] DataSourceRequest request)
        {
            return await ConfigService.GetPeriodYearsShift().ToDataSourceResultAsync(request);
        }

        [HttpPost("month")]
        public async Task<DataSourceResult> GetFromPosts()
        {
            return await ConfigService.GetPeriodMonth().ToDataSourceResultAsync(new DataSourceRequest());
        }
    }
    #endregion
}
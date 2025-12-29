using System;
using System.Linq;
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
    [Route("api/categorypta")]
    public class CategoryPtaApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Core service object
        /// </summary>
        public ClaimBenefitService ClaimBenefitService => ServiceProxy.GetService<ClaimBenefitService>();
        #endregion

        /// <summary>
        /// Get list of Bank
        /// </summary>
        /// <remarks>
        /// Get list of Bank
        /// </remarks>
        /// <returns>List of Bank</returns>
        [HttpPost("ammount")]
        public DataSourceResult GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {

            var type = this.Request.Form["Type"].ToString() ?? "";

            var amounts = ClaimBenefitService.GetAmmountPta(type);

            var min = amounts?.FirstOrDefault(x => string.IsNullOrEmpty(x.SubType)).Ammount ?? 0;
            var max = amounts?.FirstOrDefault(x => x.SubType != null && x.SubType.ToString().ToLower() == "max").Ammount ?? 0;
            var current = min;

            List<Domain.AllowanceDetail> results = new List<Domain.AllowanceDetail>();

            do
            {
                results.Add(new Domain.AllowanceDetail()
                {
                    ClassFrom = amounts?.ToArray()[0]?.ClassFrom ?? 0,
                    ClassTo = amounts?.ToArray()[0]?.ClassTo ?? 0,
                    Ammount = current
                });

                current += min;

            } while (current <= max);

            DataSourceResult res = new DataSourceResult();
            res.Data = results.ToArray();
            res.Total = results.Count();

            return res;
        }

        [HttpPost("type")]
        public async Task<DataSourceResult> GetFromPosts()
        {
            return await ClaimBenefitService.GetTypePta().ToDataSourceResultAsync(new DataSourceRequest());
        }
    }
    #endregion
}
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    /// <summary>
    /// Bank API Manager
    /// </summary>
    [Route("api/bank")]
    public class BankApiController : GenericApiControllerBase<BankService, Bank>
    {
        /// <summary>
        /// Comparer for upload
        /// </summary>
        protected override string[] ComparerKeys => new[] { "BankKey" };

        /// <summary>
        /// Get list of distinct banks
        /// </summary>
        /// <returns>List of Bank</returns>
        [HttpPost("distinct")]
        public async Task<DataSourceResult> GetDistinct()
        {
            return await CoreService.GetBankDistict().ToDataSourceResultAsync(new DataSourceRequest());
        }

        /// <summary>
        /// Get list of banks
        /// </summary>
        /// <param name="request">Data Source Request Object</param>
        /// <returns>List of Banks</returns>
        [HttpPost("gets")]
        public override async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await CommonService.GetQuery().OrderBy(x => x.BankKey).ToDataSourceResultAsync(request);
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Bank page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewBank)]
    public class BankController : GenericMvcControllerBase<BankService, Bank>
    {
        [HttpPost]
        public override IActionResult Load(Guid id)
        {
            object commonData;

            if (id == Guid.Empty)
            {
                commonData = new Bank();
            }
            else
            {
                commonData = CommonService.GetById(id);
            }

            return GetViewData(commonData);
        }
    }
    #endregion
}
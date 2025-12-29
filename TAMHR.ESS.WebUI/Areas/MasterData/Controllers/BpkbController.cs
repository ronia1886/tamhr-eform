using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    /// <summary>
    /// BPKB API Manager
    /// </summary>
    [Route("api/bpkb")]
    public class BpkbApiController : GenericApiControllerBase<BpkbService, Bpkb>
    {
        public BpkbService BpkbService => ServiceProxy.GetService<BpkbService>();

        protected override string[] ComparerKeys => new[] { "NoReg", "NoBPKB" };

        [HttpPost("gets")]
        public override async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await BpkbService.GetViews().ToDataSourceResultAsync(request);
        }

        [HttpPost("getsbynoreg")]
        public async Task<DataSourceResult> GetByNoreg([DataSourceRequest] DataSourceRequest request)
        {
            return await BpkbService.GetByNoreg(ServiceProxy.UserClaim.NoReg).ToDataSourceResultAsync(request);
        }

        [HttpPost("details")]
        public async Task<DataSourceResult> GetDetails([FromForm] Guid parentId, [DataSourceRequest] DataSourceRequest request)
        {
            return await BpkbService.GetDetails(parentId).ToDataSourceResultAsync(request);
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// BPKB page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewBpkb)]
    public class BpkbController : GenericMvcControllerBase<BpkbService, Bpkb>
    {
        public override IActionResult Load(Guid id)
        {
            var data = CommonService.GetView(id);

            return GetViewData(data);
        }
    }
    #endregion
}
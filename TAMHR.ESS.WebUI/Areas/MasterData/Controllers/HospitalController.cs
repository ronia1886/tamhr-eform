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
    /// Hospital API Manager
    /// </summary>
    [Route("api/hospital")]
    public class HospitalApiController : GenericApiControllerBase<HospitalService, Hospital>
    {
        protected override string[] ComparerKeys => new[] { "Name" };

        [HttpPost("gets")]
        public override async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await CommonService.GetQuery().OrderBy(x => x.Name.ToString()).ToDataSourceResultAsync(request);
        }

        [HttpPost("get-cities")]
        public async Task<DataSourceResult> GetCities([DataSourceRequest] DataSourceRequest request)
        {
            return await CommonService.GetQuery().Select(x => new { Name = x.City }).Distinct().ToDataSourceResultAsync(request);
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Hospital page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewHospital)]
    public class HospitalController : GenericMvcControllerBase<HospitalService, Hospital>
    {
        [HttpPost]
        public override IActionResult Load(Guid id)
        {
            object commonData;

            if (id == Guid.Empty)
            {
                commonData = new Hospital();
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
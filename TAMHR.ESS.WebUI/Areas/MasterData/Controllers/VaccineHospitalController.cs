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
    /// VaccineHospital API Manager
    /// </summary>
    [Route("api/vaccinehospital")]
    public class VaccineHospitalApiController : GenericApiControllerBase<VaccineHospitalService, VaccineHospital>
    {

        public VaccineHospitalRepoService VaccineHospitalRepoService => ServiceProxy.GetService<VaccineHospitalRepoService>();


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

        [HttpPost("get-hospital")]
        public async Task<DataSourceResult> Hospital([DataSourceRequest] DataSourceRequest request)
        {
            return await VaccineHospitalRepoService.GetHospital().ToDataSourceResultAsync(request);
        }

        [HttpPost("get-hospital-byid")]
        public async  Task<DataSourceResult> HospitalById([FromBody]VaccineHospitalId entity, [DataSourceRequest] DataSourceRequest request)
        {
            return await VaccineHospitalRepoService.GetHospital().Where(x => x.Id == entity.Id).ToDataSourceResultAsync(request);
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// VaccineHospital page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewVaccineHospital)]
    public class VaccineHospitalController : GenericMvcControllerBase<VaccineHospitalService, VaccineHospital>
    {
        [HttpPost]
        public override IActionResult Load(Guid id)
        {
            object commonData;

            if (id == Guid.Empty)
            {
                commonData = new VaccineHospital();
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
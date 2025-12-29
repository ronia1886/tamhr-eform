using System.Linq;
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
    /// Pengkinian Data API Manager
    /// </summary>
    [Route("api/pengkinian-data")]
    public class PengkinianDataApiController : GenericApiControllerBase<PengkinianDataService, PersonalDataFamilyMember>
    {
        protected override string[] ComparerKeys => new[] { "Name" };
        protected PengkinianDataService personalDataService => ServiceProxy.GetService<PengkinianDataService>();
        [HttpPost("get-data")]
        public override async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            return await personalDataService.GetPersonalDataByUser(noreg).ToDataSourceResultAsync(request);
        }

        
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Pengkinian Data page controller
    /// </summary>
    [Area("MasterData")]
    //[Permission(PermissionKey.ViewPengkinianData)]
    public class PengkinianDataController : GenericMvcControllerBase<PengkinianDataService, PersonalDataFamilyMember>
    {
    }
    #endregion
}
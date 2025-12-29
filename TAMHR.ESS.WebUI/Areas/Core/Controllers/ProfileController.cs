using System;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using System.Threading.Tasks;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    [Route("api/core")]
    [Permission(PermissionKey.ViewProfile)]
    public class ProfileApiController : MvcControllerBase
    {
        protected ProfileService ProfileService { get { return ServiceProxy.GetService<ProfileService>(); } }
        protected MdmService MdmService { get { return ServiceProxy.GetService<MdmService>(); } }

        [HttpPost("getinfoprofile")]
        public async Task<DataSourceResult> GetInfoProfile()
        {
            string Noreg = ServiceProxy.UserClaim.NoReg;
            return await MdmService.GetEmployeeOrganizationObjects(Noreg).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getinfoeducation")]
        public async Task<DataSourceResult> GetInfoProfileEducation()
        {
            string Noreg = ServiceProxy.UserClaim.NoReg;
            return await ProfileService.GetInfoEducation(Noreg).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getinfofamilymember")]
        public async Task<DataSourceResult> GetInfoProfileFamilyMember()
        {
            string Noreg = ServiceProxy.UserClaim.NoReg;
            return await ProfileService.GetInfoFamilyMember(Noreg).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("get-performance-developments")]
        public async Task<DataSourceResult> GetPerformanceDevelopments([DataSourceRequest]DataSourceRequest request)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;

            return await MdmService.GetPerformanceDevelopments(noreg).ToDataSourceResultAsync(request);
        }
    }
    #endregion
}
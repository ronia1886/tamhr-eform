using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers
{
    #region API Controller
    [Route("api/shift-report")]
    public class ShiftPlanningReportApiController : ApiControllerBase
    {
        #region Domain Services
        protected TimeManagementService TimeManagementService => ServiceProxy.GetService<TimeManagementService>();
        #endregion

        [HttpPost("getlist")]
        public async Task<DataSourceResult> GetList([DataSourceRequest] DataSourceRequest request)
        {
            if (ServiceProxy.UserClaim.Chief)
            {
                var dataPlanning = TimeManagementService.GetListShiftPlanningkReport(ServiceProxy.UserClaim.NoReg, ServiceProxy.UserClaim.Username).ToList();

                return await dataPlanning.ToDataSourceResultAsync(request);
            }
            else
            {
                var dataPlanning = TimeManagementService.GetListShiftPlanningkNonChiefReport(ServiceProxy.UserClaim.NoReg).ToList();

                return await dataPlanning.ToDataSourceResultAsync(request);
            }
        }
    }
    #endregion

    #region MVC Controller
    [Area(ApplicationModule.TimeManagement)]
    public class ShiftPlanningReportController : MvcControllerBase
    {
        public IActionResult Index()
        {
            return View();
        }
    }
    #endregion
}
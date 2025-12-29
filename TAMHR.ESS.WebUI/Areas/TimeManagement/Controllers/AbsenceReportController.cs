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
    /// <summary>
    /// Absence Report API Manager
    /// </summary>
    [Route("api/absence-report")]
    public class AbsenceReportApiController : ApiControllerBase
    {
        #region Domain Services
        protected TimeManagementService TimeManagementService => ServiceProxy.GetService<TimeManagementService>();
        #endregion

        [HttpPost("getlistabsence")]
        public async Task<DataSourceResult> GetList([DataSourceRequest] DataSourceRequest request)
        {
            var dataPlanning = TimeManagementService.GetListAbsenceReport(ServiceProxy.UserClaim.NoReg, ServiceProxy.UserClaim.Username).ToList();

            return await dataPlanning.ToDataSourceResultAsync(request);
        }
    }
    #endregion

    #region MVC Controller
    [Area(ApplicationModule.TimeManagement)]
    public class AbsenceReportController : MvcControllerBase
    {
        public IActionResult index()
        {
            return View();
        }
    }
    #endregion
}
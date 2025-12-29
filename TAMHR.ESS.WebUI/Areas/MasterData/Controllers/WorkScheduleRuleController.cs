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
    /// Work Schedule API Manager
    /// </summary>
    [Route("api/master-data/work-schedule")]
    [Permission(PermissionKey.ManageDailyWorkSchedule)]
    public class WorkScheduleApiController : GenericApiControllerBase<WorkScheduleService, WorkSchedule>
    {
        protected override string[] ComparerKeys => new[] { "Date", "WorkScheduleRule" };

        [HttpPost("get-rules")]
        public async Task<DataSourceResult> GetWorkScheduleRules([DataSourceRequest] DataSourceRequest request)
        {
            var workScheduleService = CommonService as WorkScheduleService;

            return await workScheduleService.GetWorkingScheduleRules().ToDataSourceResultAsync(request);
        }
    }

    /// <summary>
    /// Work Schedule Rule API Manager
    /// </summary>
    [Route("api/master-data/work-schedule-rule")]
    [Permission(PermissionKey.ManageDailyWorkSchedule)]
    public class WorkScheduleRuleApiController : GenericApiControllerBase<WorkScheduleRuleService, WorkScheduleRule>
    {
        protected override string[] ComparerKeys => new[] { "Code" };
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Work schedule rule page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewDailyWorkSchedule)]
    public class WorkScheduleRuleController : GenericMvcControllerBase<WorkScheduleRuleService, WorkScheduleRule>
    {
    }
    #endregion
}
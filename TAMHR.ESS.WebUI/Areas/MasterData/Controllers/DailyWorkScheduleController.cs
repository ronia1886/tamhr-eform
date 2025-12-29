using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    /// <summary>
    /// Daily Work Schedule API Manager
    /// </summary>
    [Route("api/master-data/daily-work-schedule")]
    [Permission(PermissionKey.ManageDailyWorkSchedule)]
    public class DailyWorkScheduleApiController : GenericApiControllerBase<DailyWorkScheduleService, DailyWorkSchedule>
    {
        protected override string[] ComparerKeys => new[] { "ShiftCode" };
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Daily work schedule page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewDailyWorkSchedule)]
    public class DailyWorkScheduleController : GenericMvcControllerBase<DailyWorkScheduleService, DailyWorkSchedule>
    {
    }
    #endregion
}
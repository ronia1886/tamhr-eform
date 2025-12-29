using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers
{
    #region API Controller
    /// <summary>
    /// Absence API Manager
    /// </summary>
    [Route("api/time-management/dashboard")]
    public class DashboardApiController : ApiControllerBase
    {
        public TimeManagementService TimeManagementService => ServiceProxy.GetService<TimeManagementService>();

        [HttpGet]
        public IEnumerable<TimeManagementDashboardStoredEntity> Get()
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var keyDate = DateTime.Now.Date;

            return TimeManagementService.GetDashboard(noreg, keyDate);
        }
    }
    #endregion
}
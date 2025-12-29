using System;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using System.Collections.Generic;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    
    #region API Controller
    [Route("api/vaccine-schedule")]
    public class VaccineScheduleAPIController : ApiControllerBase
    {
        #region Domain Services
        protected VaccineScheduleService vaccineScheduleService => ServiceProxy.GetService<VaccineScheduleService>();
        #endregion

        [HttpGet("get-by-date")]
        public List<VaccineHospital> GetBydate(DateTime vaccineDate)
        {
            var data = vaccineScheduleService.GetVaccineScheduleByDate(vaccineDate);
            return data;
        }
    }
    #endregion

    [Area("Core")]
    public class VaccineScheduleController : MvcControllerBase
    {
        protected CoreService CoreService { get { return ServiceProxy.GetService<CoreService>(); } }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LoadGrid()
        {
            return PartialView("_GridFormVaccineSchedule");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    [Area("Core")]
    public class LookupController : MvcControllerBase
    {
        protected MdmService MdmService { get { return ServiceProxy.GetService<MdmService>(); } }

        [HttpPost]
        public IActionResult EmployeeByDepartment([FromQuery] string orgCode, bool withSelect = false)
        {
            var data = MdmService.GetEmployeeByDepartment(orgCode);
            data = data.Where(x => x.Classification < 9).ToList();
            ViewData["WithSelect"] = withSelect;
            ViewData["OrgCode"] = System.Web.HttpUtility.HtmlEncode(orgCode);
            return PartialView("_EmployeeList", data);
        }

        [HttpPost]
        public IActionResult EmployeeByDepartmentVacation([FromQuery] string orgCode, DateTime keyDate, bool withSelect = false)
        {
            var data = MdmService.GetEmployeeByDepartmentVacation(orgCode, keyDate);
            ViewData["WithSelect"] = withSelect;
            ViewData["OrgCode"] = System.Web.HttpUtility.HtmlEncode(orgCode);
            return PartialView("_EmployeeListDeptVacation", data);
        }

        [HttpPost]
        public IActionResult EmployeeByDivisionVacation([FromQuery] string orgCode, DateTime keyDate, bool withSelect = false)
        {
            var data = MdmService.GetEmployeeByDivisionVacation(orgCode, keyDate);
            ViewData["WithSelect"] = withSelect;
            ViewData["OrgCode"] = System.Web.HttpUtility.HtmlEncode(orgCode);
            return PartialView("_EmployeeListDivVacation", data);
        }
    }
}
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Extensions;

namespace TAMHR.ESS.Infrastructure.Web.ViewComponents
{
    public class Leave : ViewComponent
    {
        private readonly EmployeeLeaveService _employeeLeaveService;

        public Leave(EmployeeLeaveService employeeLeaveService)
        {
            _employeeLeaveService = employeeLeaveService;
        }

        public IViewComponentResult Invoke(string noreg, string type)
        {
            var employeeLeave = _employeeLeaveService.GetByNoregAll(noreg);

            ViewBag.Type = type;

            return View(employeeLeave);
        }
    }
}

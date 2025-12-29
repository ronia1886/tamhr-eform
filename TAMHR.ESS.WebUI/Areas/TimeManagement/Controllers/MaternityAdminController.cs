using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers
{
    [Area(ApplicationModule.TimeManagement)]
    [Permission(PermissionKey.ViewMaternityLeaveAdmin)]
    public class MaternityAdminController : MvcControllerBase
    {
        public IActionResult Index()
        {
            var model = new DocumentRequestDetailViewModel<MaternityLeaveViewModel>
            {
                FormKey = "maternity-leave"
            };

            return View(model);
        }

        public IActionResult Report()
        {
            return View();
        }
    }
}
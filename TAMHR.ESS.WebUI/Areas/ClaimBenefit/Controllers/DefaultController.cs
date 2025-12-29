using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;

namespace TAMHR.ESS.WebUI.Areas.ClaimBenefit.Controllers
{
    [Area(ApplicationModule.ClaimBenefit)]
    public class DefaultController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
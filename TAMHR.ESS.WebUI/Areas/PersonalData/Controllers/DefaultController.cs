using System;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;

namespace TAMHR.ESS.WebUI.Areas.PersonalData.Controllers
{
    [Area(ApplicationModule.PersonalData)]
    public class DefaultController : MvcControllerBase
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Kematian()
        {
            return View();
        }

    }
}
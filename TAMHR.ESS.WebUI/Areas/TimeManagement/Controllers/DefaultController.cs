using System;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;

namespace TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers
{
    [Area(ApplicationModule.TimeManagement)]
    public class DefaultController : MvcControllerBase
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
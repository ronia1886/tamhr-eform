using System;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.Web;

namespace TAMHR.ESS.WebUI.Areas.Reporting.Controllers
{
    [Area("Reporting")]
    public class DefaultController : MvcControllerBase
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult PartialReport(string formKey)
        {
            return PartialView("_" + formKey);
        }
    }
}
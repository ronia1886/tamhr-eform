using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics;

namespace TAMHR.ESS.WebUI.Controllers
{
    public class ErrorController : Controller
    {
        protected IExceptionHandlerFeature Exception { get { return HttpContext.Features.Get<IExceptionHandlerFeature>(); } }

        public IActionResult Index()
        {
            return View(Exception);
        }

        public IActionResult Ajax()
        {
            return BadRequest(new { Exception.Error.Message });
        }
    }
}
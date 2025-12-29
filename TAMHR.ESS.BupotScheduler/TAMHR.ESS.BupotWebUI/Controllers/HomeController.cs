// HomeController.cs
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using TAMHR.ESS.RenamePDFService.Helpers;

namespace TAMHR.ESS.BupotWebUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRenamePdfService _renamePdfService;

        public HomeController(ILogger<HomeController> logger, IRenamePdfService renamePdfService)
        {
            _logger = logger;
            _renamePdfService = renamePdfService;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET: /home/setupjob
        public IActionResult SetupJob()
        {
            return View();
        }

       
     
    }
}

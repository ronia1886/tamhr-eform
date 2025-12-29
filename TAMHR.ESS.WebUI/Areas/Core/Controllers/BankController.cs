using System;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using System.Threading.Tasks;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    [Area("Core")]
    public class BankController : MvcControllerBase
    {
        protected CoreService CoreService { get { return ServiceProxy.GetService<CoreService>(); } }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult LoadGrid()
        {
            return PartialView("_GridFormBank");
        }
    }
}
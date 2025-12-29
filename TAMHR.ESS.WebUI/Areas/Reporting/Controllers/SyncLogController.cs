using System;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.WebUI.Areas.Reporting.Controllers
{
    [Area("Reporting")]
    [Permission(PermissionKey.ViewSyncLogReport)]
    public class SyncLogController : MvcControllerBase
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
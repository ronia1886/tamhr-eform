using System;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    /// <summary>
    /// ARS Matrix API Manager
    /// </summary>
    [Route("api/ars-matrix")]
    //[Permission(PermissionKey.ViewArsMatrix)]
    public class ArsMatrixApiController : ApiControllerBase
    {
    }
    #endregion

    #region MVC Controller
    [Area(ApplicationModule.Core)]
    [Permission(PermissionKey.ViewArsMatrix)]
    public class ArsMatrixController : MvcControllerBase
    {
        protected CoreService CoreService => ServiceProxy.GetService<CoreService>();

        /// <summary>
        /// ARS matrix page
        /// </summary>
        public IActionResult Index()
        {
            var arxMatrices = CoreService.GetArsMatrices();

            return View(arxMatrices);
        }

        public IActionResult Load(string key)
        {
            var details = CoreService.GetArsMatrix(key);

            return PartialView("_Details", details);
        }
    }
    #endregion
}
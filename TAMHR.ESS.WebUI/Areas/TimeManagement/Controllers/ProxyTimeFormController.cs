using System;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.WebUI.Areas.Others.Controllers
{
    #region API Controller
    /// <summary>
    /// Proxy time form API controller.
    /// </summary>
    [Route("api/proxy-time-form")]
    [Permission(PermissionKey.CreateProxyTimeForm)]
    public class ProxyTimeFormApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Proxy time form service object.
        /// </summary>
        public ProxyTimeFormService ProxyTimeFormService => ServiceProxy.GetService<ProxyTimeFormService>();
        #endregion

        #region Public Methods
        /// <summary>
        /// Proxy in current user.
        /// </summary>
        /// <param name="proxyTime">This <see cref="ProxyTime"/> object.</param>
        [HttpPost("proxy-in")]
        public IActionResult ProxyIn([FromBody] ProxyTime proxyTime)
        {
            // Get and set current date & time.
            var now = DateTime.Now;

            // Get and set current user session noreg.
            var noreg = ServiceProxy.UserClaim.NoReg;

            // Proxy in current user.
            ProxyTimeFormService.ProxyIn(noreg, now, proxyTime.Latitude, proxyTime.Longitude);

            // Return NoContent result.
            return NoContent();
        }

        /// <summary>
        /// Proxy out current user.
        /// </summary>
        [HttpPost("proxy-out")]
        public IActionResult ProxyOut()
        {
            // Get and set current date & time.
            var now = DateTime.Now;

            // Get and set current user session noreg.
            var noreg = ServiceProxy.UserClaim.NoReg;

            // Proxy out current user.
            ProxyTimeFormService.ProxyOut(noreg, now);

            // Return NoContent result.
            return NoContent();
        }
        #endregion


    }
    #endregion

    /// <summary>
    /// Proxy time form controller.
    /// </summary>
    [Area(ApplicationModule.TimeManagement)]
    public class ProxyTimeFormController : MvcControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Proxy time form service object.
        /// </summary>
        public ProxyTimeFormService ProxyTimeFormService => ServiceProxy.GetService<ProxyTimeFormService>();
        #endregion

        #region Pages
        /// <summary>
        /// Proxy time main page.
        /// </summary>
        public IActionResult Index()
        {
            // Redirect to common view if exception occurs.
            return RedirectIfException(() =>
            {
                // Get and set current date & time.
                var now = DateTime.Now;

                // Get and set current date.
                var currentDate = now.Date;

                // Get and set current user session noreg.
                var noreg = ServiceProxy.UserClaim.NoReg;

                // Validate proxy, throw an exception if not meet the criteria.
                ProxyTimeFormService.ValidateProxy(noreg, now);

                // Get and set proxy time data from given noreg and current date.
                var proxyTime = ProxyTimeFormService.GetProxyTime(noreg, currentDate);

                // Return the view.
                return View(proxyTime);
            });
        }
        #endregion
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using TAMHR.ESS.Infrastructure.Web.ViewComponents;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    /// <summary>
    /// Form log API controller.
    /// </summary>
    [Route("api/form-log")]
    [Permission(PermissionKey.ManageFormLog)]
    public class FormLogApiController : ApiControllerBase
    {
        #region Domain Services
        protected FormService FormService => ServiceProxy.GetService<FormService>();
        #endregion

        /// <summary>
        /// Get list of form logs.
        /// </summary>
        /// <param name="request">This <see cref="DataSourceRequest"/> object.</param>
        /// <returns>This <see cref="DataSourceResult"/> object.</returns>
        [HttpPost("gets")]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            // Get current user session noreg.
            var actor = ServiceProxy.UserClaim.NoReg;

            return await FormService.GetFormLogs(actor)
                .ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Get list of closed form logs.
        /// </summary>
        /// <param name="noreg">This target noreg.</param>
        /// <param name="request">This <see cref="DataSourceRequest"/> object.</param>
        /// <returns>This <see cref="DataSourceResult"/> object.</returns>
        [HttpPost("get-closed")]
        public async Task<DataSourceResult> GetClosed([FromForm] string noreg, [DataSourceRequest] DataSourceRequest request)
        {
            // Get current user session noreg.
            var actor = ServiceProxy.UserClaim.NoReg;

            return await FormService.GetFormLogs(actor, noreg)
                .Where(x => x.Closed)
                .ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Update or insert form log.
        /// </summary>
        /// <param name="formLog">This <see cref="FormLog"/> object.</param>
        [HttpPost]
        public IActionResult Upsert([FromBody] FormLog formLog)
        {
            // Get current user session noreg.
            var actor = ServiceProxy.UserClaim.NoReg;

            var output = FormService.UpsertFormLog(actor, formLog);

            return Ok(output);
        }
    }
    #endregion

    #region MVC Controller
    [Area("Core")]
    [Permission(PermissionKey.ViewFormLog)]
    public class FormLogController : MvcControllerBase
    {
        #region Domain Services
        protected FormService FormService => ServiceProxy.GetService<FormService>();
        #endregion

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Load(string formKey, string noreg)
        {
            // Get current user session noreg.
            var actor = ServiceProxy.UserClaim.NoReg;

            var form = FormService.Get(formKey);

            var formLog = FormService.GetOpenedFormLog(actor, noreg);

            formLog.NoReg = noreg;
            formLog.FormId = form.Id;

            return PartialView("_Form", formLog);
        }
    }
    #endregion
}
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    /// <summary>
    /// Online Letter API Manager
    /// </summary>
    [Route("api/online-letter")]
    //[Permission(PermissionKey.ViewOnlineLetter)]
    public class OnlineLetterApiController : GenericApiControllerBase<OnlineLetterService, OnlineLetter>
    {
        protected override string[] ComparerKeys => new[] { "LetterNumber" };

        #region Domain Services
        /// <summary>
        /// Online letter service object
        /// </summary>
        public OnlineLetterService OnlineLetterService => ServiceProxy.GetService<OnlineLetterService>();
        #endregion

        public override async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await CommonService.GetView().ToDataSourceResultAsync(request);
        }
    }
    #endregion

    #region MVC Controller
    [Area("Core")]
    [Permission(PermissionKey.ViewOnlineLetter)]
    public class OnlineLetterController : GenericMvcControllerBase<OnlineLetterService, OnlineLetter>
    {
        public override IActionResult Load(Guid id)
        {
            var commonData = CommonService.GetViewById(id);

            return PartialView("_OnlineLetterForm", commonData);
        }
    }
    #endregion
}
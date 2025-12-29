using System;
using System.IO;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    /// <summary>
    /// BDJK Configuration API Manager
    /// </summary>
    [Route("api/bdjk-detail")]
    [ApiController]
    public class BdjkDetailApiController : GenericApiControllerBase<BdjkDetailService, BdjkDetail>
    {
        /// <summary>
        /// Comparer for upload
        /// </summary>
        protected override string[] ComparerKeys => new[] { "BdjkCode" };
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// BDJK detail page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewBdjkDetail)]
    public class BdjkDetailController : GenericMvcControllerBase<BdjkDetailService, BdjkDetail>
    {
        [HttpPost]
        public override IActionResult Load(Guid id)
        {
            object commonData;

            if (id == Guid.Empty)
            {
                commonData = new BdjkDetail();
            }
            else
            {
                commonData = CommonService.GetById(id);
            }

            return GetViewData(commonData);
        }
    }
    #endregion
}
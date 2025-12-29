using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using TAMHR.ESS.Domain;
using System;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    /// <summary>
    /// Faskes API Manager
    /// </summary>
    [Route("api/faskes")]
    public class FaskesApiController : GenericApiControllerBase<FaskesService, Faskes>
    {
        protected override string[] ComparerKeys => new[] { "FaskesCode" };
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Faskes page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewFaskes)]
    public class FaskesController : GenericMvcControllerBase<FaskesService, Faskes>
    {
        [HttpPost]
        public override IActionResult Load(Guid id)
        {
            object commonData;

            if (id == Guid.Empty)
            {
                commonData = new Faskes();
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
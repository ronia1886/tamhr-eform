using Microsoft.AspNetCore.Mvc;
using System;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    /// <summary>
    /// SAP General Category Map API Manager
    /// </summary>
    [Route("api/master-data/sap-category-map")]
    [Permission(PermissionKey.ManageSapGeneralCategoryMap)]
    public class SapGeneralCategoryMapApiController : GenericApiControllerBase<SapGeneralCategoryMapService, SapGeneralCategoryMap>
    {
        protected override string[] ComparerKeys => new[] { "GeneralCategoryCode" };
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Sap general category map page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewSapGeneralCategoryMap)]
    public class SapGeneralCategoryMapController : GenericMvcControllerBase<SapGeneralCategoryMapService, SapGeneralCategoryMap>
    {
        [HttpPost]
        public override IActionResult Load(Guid id)
        {
            object commonData;

            if (id == Guid.Empty)
            {
                commonData = new SapGeneralCategoryMap();
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
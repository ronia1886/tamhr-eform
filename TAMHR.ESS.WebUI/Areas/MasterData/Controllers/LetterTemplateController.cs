using System;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    /// <summary>
    /// Letter Template API Manager
    /// </summary>
    [Route("api/master-data/letter-template")]
    [Permission(PermissionKey.ManageLetterTemplate)]
    public class LetterTemplateApiController : GenericApiControllerBase<LetterTemplateService, LetterTemplate>
    {
        protected override string[] ComparerKeys => throw new NotImplementedException();
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Letter template page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewLetterTemplate)]
    public class LetterTemplateController : GenericMvcControllerBase<LetterTemplateService, LetterTemplate>
    {
        [HttpPost]
        public override IActionResult Load(Guid id)
        {
            object commonData;

            if (id == Guid.Empty)
            {
                commonData = new LetterTemplate();
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
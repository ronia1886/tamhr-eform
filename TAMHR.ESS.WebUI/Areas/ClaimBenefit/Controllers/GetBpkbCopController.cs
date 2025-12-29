using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.WebUI.Areas.ClaimBenefit.Controllers
{
    #region API Controller
    /// <summary>
    /// Get BPKB COP API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.GetBpkbCop)]
    public class GetBpkbCopApiController : FormApiControllerBase<GetBpkbCopViewModel>
    {
    }
    #endregion
}
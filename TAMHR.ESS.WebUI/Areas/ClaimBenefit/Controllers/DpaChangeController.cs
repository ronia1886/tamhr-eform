using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.WebUI.Areas.ClaimBenefit.Controllers
{
    #region API Controller
    /// <summary>
    /// DPA change API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.DpaChange)]
    [ApiController]
    public class DpaChangeApiController : FormApiControllerBase<DpaChangeViewModel>
    {
    }
    #endregion
}
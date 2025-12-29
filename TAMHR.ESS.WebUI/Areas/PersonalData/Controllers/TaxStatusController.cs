using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.WebUI.Areas.PersonalData.Controllers
{
    #region API Controller
	/// <summary>
    /// Tax Status API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.TaxStatus)]
    public class TaxStatusApiController : FormApiControllerBase<TaxStatusViewModel>
    {
    }
	#endregion
}
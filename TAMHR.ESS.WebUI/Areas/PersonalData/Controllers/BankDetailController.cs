using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.WebUI.Areas.PersonalData.Controllers
{
    #region API Controller
    /// <summary>
    /// Bank account API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.BankAccount)]
    public class BankDetailApiController : FormApiControllerBase<BankDetailViewModel>
    {
    } 
    #endregion
}
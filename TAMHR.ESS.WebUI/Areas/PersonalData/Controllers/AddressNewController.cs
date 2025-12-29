using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.WebUI.Areas.PersonalData.Controllers
{
    #region API Controller
    /// <summary>
    /// Others personal data API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.AddressNew)]
    [Permission(PermissionKey.CreateAddressNewDataRequest)]
    public class AddressNewController : FormApiControllerBase<OthersPersonalDataViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Property that hold personal data service object.
        /// </summary>
        protected PersonalDataService PersonalDataService => ServiceProxy.GetService<PersonalDataService>();
        #endregion
    }
    #endregion
}
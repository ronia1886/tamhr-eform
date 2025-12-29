using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using System.Linq;
using System;

namespace TAMHR.ESS.WebUI.Areas.PersonalData.Controllers
{
    #region API Controller
    /// <summary>
    /// Address API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.Address)]
    public class AddressApiController : FormApiControllerBase<AddressViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Property that hold personal data service object.
        /// </summary>
        protected PersonalDataService PersonalDataService => ServiceProxy.GetService<PersonalDataService>();
        #endregion

        #region Override Methods
        /// <summary>
        /// Validation before the creation of document approval.
        /// </summary>
        /// <param name="addressViewModel">This <see cref="AddressViewModel"/> object.</param>
        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<AddressViewModel> addressViewModel)
        {
            // Call base validation method.
            base.ValidateOnPostCreate(addressViewModel);

        }
        #endregion
    }
    #endregion
}
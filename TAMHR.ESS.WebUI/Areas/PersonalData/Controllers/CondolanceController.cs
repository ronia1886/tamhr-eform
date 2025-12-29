using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.WebUI.Areas.PersonalData.Controllers
{
    #region API Controller
    /// <summary>
    /// Condolance API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.Condolance)]
    public class CondolanceApiController : FormApiControllerBase<DismembermentViewModel>
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
        /// <param name="formKey">This form key.</param>
        protected override void ValidateOnCreate(string formKey)
        {
            // Call base validate on create.
            base.ValidateOnCreate(formKey);

            // Validate for tax status form request completion if any.
            PersonalDataService.PreValidateTaxstatusComplete(ServiceProxy.UserClaim.NoReg);

            // Validate for marriage form request completion if any.
            PersonalDataService.PreValidateMarriageComplete(ServiceProxy.UserClaim.NoReg);

            // Validate for divorce form request completion if any.
            PersonalDataService.PreValidateDivorceComplete(ServiceProxy.UserClaim.NoReg);

            // Validate for family registration form request completion if any.
            PersonalDataService.PreValidateBirthComplete(ServiceProxy.UserClaim.NoReg);
        }
        #endregion
    }
    #endregion
}
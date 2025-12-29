using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.WebUI.Areas.ClaimBenefit.Controllers
{
    #region API Controller
    /// <summary>
    /// CPP API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.Cpp)]
    public class CppApiController : FormApiControllerBase<CppViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Property that hold claim benefit service object.
        /// </summary>
        protected ClaimBenefitService ClaimBenefitService => ServiceProxy.GetService<ClaimBenefitService>();
        #endregion

        protected override void ValidateOnCreate(string formKey)
        {
            base.ValidateOnCreate(formKey);

            ClaimBenefitService.PreValidateCpp(ServiceProxy.UserClaim.NoReg, ServiceProxy.UserClaim.Name);
            ClaimBenefitService.PreValidateCpp(ServiceProxy.UserClaim.NoReg);

        }

        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<CppViewModel> requestDetailViewModel)
        {
            base.ValidateOnPostCreate(requestDetailViewModel);

            var noreg = ServiceProxy.UserClaim.NoReg != requestDetailViewModel.Requester
                ? ServiceProxy.UserClaim.NoReg
                : requestDetailViewModel.Requester;

            ClaimBenefitService.PreValidatePostCpp(noreg, requestDetailViewModel);
        }
    }
    #endregion
}
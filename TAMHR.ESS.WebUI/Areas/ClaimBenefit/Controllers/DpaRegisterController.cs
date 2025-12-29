using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using Agit.Common;

namespace TAMHR.ESS.WebUI.Areas.ClaimBenefit.Controllers
{
    #region API Controller
    /// <summary>
    /// DPA register API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.DpaRegister)]
    [ApiController]
    public class DpaRegisterApiController : FormApiControllerBase<DpaRegisterViewModel>
    {
        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<DpaRegisterViewModel> dpaViewModel)
        {
            base.ValidateOnPostCreate(dpaViewModel);

            Validate(dpaViewModel);
        }

        protected override void ValidateOnPostUpdate(DocumentRequestDetailViewModel<DpaRegisterViewModel> dpaViewModel)
        {
            base.ValidateOnPostUpdate(dpaViewModel);

            Validate(dpaViewModel);
        }

        private void Validate(DocumentRequestDetailViewModel<DpaRegisterViewModel> dpaViewModel)
        {
            Assert.ThrowIf(dpaViewModel.Object.AhliWaris == null, "Data Ahli Waris Not Found");
        }
    }
    #endregion
}
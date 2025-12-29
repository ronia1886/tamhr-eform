using System;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.WebUI.Areas.ClaimBenefit.Controllers
{
    #region API Controller
    /// <summary>
    /// Shift meal allowance API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.ShiftMealAllowance)]
    public class ShiftMealAllowanceApiController : FormApiControllerBase<ShiftMealAllowanceViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Property that hold claim benefit service object.
        /// </summary>
        public ClaimBenefitService ClaimBenefitService => ServiceProxy.GetService<ClaimBenefitService>();
        #endregion

        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<ShiftMealAllowanceViewModel> shiftMealAllowanceViewModel)
        {
            base.ValidateOnPostCreate(shiftMealAllowanceViewModel);

            if (shiftMealAllowanceViewModel.Object == null)
            {
                throw new Exception("Cannot create request because request is empty");
            }

            if (shiftMealAllowanceViewModel.Object.Summaries == null)
            {
                throw new Exception("Belum ada data employee yang dipilih");
            }
        }

        protected override void ValidateOnPostUpdate(DocumentRequestDetailViewModel<ShiftMealAllowanceViewModel> shiftMealAllowanceViewModel)
        {
            base.ValidateOnPostUpdate(shiftMealAllowanceViewModel);

            if (shiftMealAllowanceViewModel.Object == null)
            {
                throw new Exception("Cannot update request because request is empty");
            }
        }
    }
    #endregion
}
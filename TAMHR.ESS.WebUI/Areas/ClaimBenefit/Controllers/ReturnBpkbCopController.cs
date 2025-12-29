using System;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using System.Linq;

namespace TAMHR.ESS.WebUI.Areas.ClaimBenefit.Controllers
{
    #region API Controller
    /// <summary>
    /// Return BPKB COP API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.ReturnBpkbCop)]
    public class ReturnBpkbCopApiController : FormApiControllerBase<ReturnBpkbCOPViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Property that hold claim benefit service object.
        /// </summary>
        public ClaimBenefitService ClaimBenefitService => ServiceProxy.GetService<ClaimBenefitService>();
        #endregion

        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<ReturnBpkbCOPViewModel> returnBpkbCOPViewModel)
        {
            base.ValidateOnPostCreate(returnBpkbCOPViewModel);

            if (returnBpkbCOPViewModel.Object == null) throw new Exception("Cannot create request because request is empty");

            if (returnBpkbCOPViewModel.Object.LoanDate > returnBpkbCOPViewModel.Object.ReturnDate)
            {
                throw new Exception("Tanggal peminjaman tidak boleh lebih besar dari tanggal pengembalian");
            }
            string noreg = ServiceProxy.UserClaim.NoReg;
            var duplikat = ApprovalService.FindDuplicateBpkbRequest(noreg, returnBpkbCOPViewModel.FormKey, returnBpkbCOPViewModel.Object.LoanDate.Value, returnBpkbCOPViewModel.Object.ReturnDate.Value);
            if (duplikat.Any())
                throw new Exception($"Duplicate BPKB request \n{string.Join('\n', duplikat.Select(x => $"Document Number: {x.Item1} Borrow Date: {x.Item2:dd/MM/yyyy} Return Date: {x.Item3:dd/MM/yyyy}"))}");
        }

        protected override void ValidateOnPostUpdate(DocumentRequestDetailViewModel<ReturnBpkbCOPViewModel> returnBpkbCOPViewModel)
        {
            base.ValidateOnPostUpdate(returnBpkbCOPViewModel);

            if (returnBpkbCOPViewModel.Object == null) throw new Exception("Cannot update request because request is empty");
        }
    }
    #endregion
}
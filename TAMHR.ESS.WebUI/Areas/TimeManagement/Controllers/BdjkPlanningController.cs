using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers
{
    #region API Controller
    /// <summary>
    /// BDJK planning API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.BdjkPlanning)]
    public class BdjkPlanningApiController : FormApiControllerBase<BdjkPlanningViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Time management service
        /// </summary>
        protected TimeManagementService TimeManagementService => ServiceProxy.GetService<TimeManagementService>();

        /// <summary>
        /// BDJK management service
        /// </summary>
        protected BdjkService BdjkService => ServiceProxy.GetService<BdjkService>();
        #endregion

        protected override void ValidateOnCreate(string formKey)
        {
        }

        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<BdjkPlanningViewModel> requestDetailViewModel)
        {
            base.ValidateOnPostCreate(requestDetailViewModel);

            if (requestDetailViewModel.Object == null || requestDetailViewModel.Object.Details.Length == 0) throw new Exception("Cannot create request because request is empty");
        }

        protected override void ValidateOnPostUpdate(DocumentRequestDetailViewModel<BdjkPlanningViewModel> requestDetailViewModel)
        {
            base.ValidateOnPostUpdate(requestDetailViewModel);

            if (requestDetailViewModel.Object == null || requestDetailViewModel.Object.Details.Length == 0) throw new Exception("Cannot update request because request is empty");
        }

        protected override void ApprovalService_DocumentCreated(object sender, DocumentRequestDetailViewModel e)
        {
            Upsert(e);
        }

        protected override void ApprovalService_DocumentUpdated(object sender, DocumentRequestDetailViewModel e)
        {
            Upsert(e);
        }

        private void Upsert(DocumentRequestDetailViewModel e)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var documentRequestDetail = e as DocumentRequestDetailViewModel<BdjkPlanningViewModel>;
            var period = documentRequestDetail.Object.Period;

            BdjkService.UpsertBdjkRequest(e.DocumentApprovalId, noreg, period, documentRequestDetail.Object.Details);
        }

        [HttpGet("gets")]
        public IEnumerable<BdjkRequestStoredEntity> Gets(string noreg, Guid id, int year, int month)
        {
            return BdjkService.GetBdjkList(year, month, noreg, id);
        }
    }
    #endregion
}
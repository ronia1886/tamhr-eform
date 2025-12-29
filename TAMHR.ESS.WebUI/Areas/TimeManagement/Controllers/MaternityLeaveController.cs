using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.WebUI.Areas.TimeManagement.Controllers
{
    #region API Controller
    /// <summary>
    /// Maternity leave API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.MaternityLeave)]
    public class MaternityLeaveApiController : FormApiControllerBase<MaternityLeaveViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Time management service object
        /// </summary>
        public TimeManagementService TimeManagementService => ServiceProxy.GetService<TimeManagementService>();
        #endregion

        protected override void ApprovalService_DocumentUpdated(object sender, DocumentRequestDetailViewModel e)
        {
            var value = e as DocumentRequestDetailViewModel<MaternityLeaveViewModel>;

            ApprovalService.CreateChangeTracking(ServiceProxy.UserClaim.NoReg, e.DocumentApprovalId, "EstimatedDayOfBirth", value.Object.EstimatedDayOfBirth.Value.ToString("dd-MM-yyyy"));
        }

        protected override void ApprovalService_DocumentCreated(object sender, DocumentRequestDetailViewModel e)
        {
            ApprovalService_DocumentUpdated(sender, e);
        }

        /// <summary>
        /// Validate before create
        /// </summary>
        /// <param name="maternityLeaveViewModel">Maternity Leave View Model Object</param>
        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<MaternityLeaveViewModel> maternityLeaveViewModel)
        {
            base.ValidateOnPostCreate(maternityLeaveViewModel);

            if (maternityLeaveViewModel.Object == null) throw new Exception("Cannot create request because request is empty");

            if (decimal.Parse(maternityLeaveViewModel.Object.GestationalAge.ToString()) > 40)
            {
                throw new Exception("Umur Kehamilan tidak boleh lebih dari 40 Minggu");
            }
        }

        /// <summary>
        /// Validate before update
        /// </summary>
        /// <param name="maternityLeaveViewModel">Maternity Leave View Model Object</param>
        protected override void ValidateOnPostUpdate(DocumentRequestDetailViewModel<MaternityLeaveViewModel> maternityLeaveViewModel)
        {
            if (AclHelper.HasPermission("Form.MaternityLeave.EditHpl"))
            {
                return;
            }

            base.ValidateOnPostUpdate(maternityLeaveViewModel);

            if (maternityLeaveViewModel.Object == null) throw new Exception("Cannot update request because request is empty");
        }

        /// <summary>
        /// Validate before create
        /// </summary>
        /// <param name="formKey">Form Key</param>
        protected override void ValidateOnCreate(string formKey)
        {
            base.ValidateOnCreate(formKey);

            TimeManagementService.PreValidateGenderMan(ServiceProxy.UserClaim.NoReg);
            TimeManagementService.PreValidateMaternityLeave(ServiceProxy.UserClaim.NoReg);
            //TimeManagementService.PreValidateMarried(ServiceProxy.UserClaim.NoReg);
        }

        [HttpPost("update-dob")]
        public IActionResult UpdateDayOfBirth([FromBody] MaternityLeaveDobViewModel input)
        {
            TimeManagementService.UpdateDayOfBirth(ServiceProxy.UserClaim.NoReg, input);

            return NoContent();
        }

        [HttpPost("create-maternity/{encryptedData}")]
        [Permission(PermissionKey.ManageMaternityLeaveAdmin)]
        public async Task<IActionResult> CreateMaternity(string encryptedData, [FromBody]DocumentRequestDetailViewModel<MaternityLeaveViewModel> maternityLeaveViewModel)
        {
            var datas = encryptedData.Split('|');
            var username = datas[0];
            var noreg = datas[1];
            var postCode = datas[2];

            maternityLeaveViewModel.Object.DayOfBirth = maternityLeaveViewModel.Object.EstimatedDayOfBirth;

            TimeManagementService.PreValidateGenderMan(noreg);
            TimeManagementService.PreValidateMaternityLeave(noreg);
           // TimeManagementService.PreValidateMarried(noreg);
/*            var link = "~/Views/Shared/Commons/WarningML.cshtml";
            bool stats  = TimeManagementService.PreValidateMarried(noreg);
            if (!stats)
            {

                return (link);

            }*/
            if (decimal.Parse(maternityLeaveViewModel.Object.GestationalAge.ToString()) > 40)
            {
                throw new Exception("Umur Kehamilan tidak boleh lebih dari 40 Mingg");
            }

            await SubmitEvent(username, noreg, postCode, ApprovalAction.Initiate, maternityLeaveViewModel);
            await TimeManagementService.UpdateMaternityTrackingApproval(noreg, maternityLeaveViewModel.DocumentApprovalId);

            return NoContent();
        }
    }
    #endregion
}
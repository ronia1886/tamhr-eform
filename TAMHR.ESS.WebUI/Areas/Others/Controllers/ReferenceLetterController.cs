using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.WebUI.Areas.Others.Controllers
{
    #region API Controller
    /// <summary>
    /// Reference letter API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.ReferenceLetter)]
    [Permission(PermissionKey.CreateReferenceLetter)]
    public class ReferenceLetterApiController : FormApiControllerBase<ReferenceLetterViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Property that hold others service object.
        /// </summary>
        protected OthersService OthersService => ServiceProxy.GetService<OthersService>();
        #endregion

        #region Override Methods
        /// <summary>
        /// Ignore validation before the creation of document approval by overriding with empty body.
        /// </summary>
        /// <param name="formKey">This form key.</param>
        protected override void ValidateOnCreate(string formKey) { }

        /// <summary>
        /// Method that triggered right after the creation of approval document.
        /// </summary>
        /// <param name="sender">This sender (usually from approval service object).</param>
        /// <param name="e">This <see cref="DocumentRequestDetailViewModel"/> object.</param>
        protected override void ApprovalService_DocumentCreated(object sender, DocumentRequestDetailViewModel e)
        {
            // Create new common sequence from given key.
            var sequence = ApprovalService.CreateCommonSequence(ApplicationForm.ReferenceLetter);

            // Update the reference letter sequence number.
            OthersService.UpdateReferenceLetterSequenceNumber(e.DocumentApprovalId, sequence);
        }
        #endregion
    }
    #endregion
}
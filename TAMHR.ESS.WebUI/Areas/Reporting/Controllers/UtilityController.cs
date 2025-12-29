using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.WebUI.Areas.Reporting.Controllers
{
    #region API Controller
    /// <summary>
    /// Utility API controller.
    /// </summary>
    [Route("api/reporting/utility")]
    [Permission(PermissionKey.ManageProxyTime)]
    public class UtilityApiController : ApiControllerBase
    {
        [HttpPost("generate-range-proxy")]
        public async Task<IActionResult> GenerateRangeProxy([FromBody]RangeProxyViewModel rangeProxyViewModel)
        {
            var noregs = rangeProxyViewModel.NoRegs != null
                ? rangeProxyViewModel.NoRegs
                : new string[] { null };

            await CoreService.GenerateRangeProxy(rangeProxyViewModel.StartDate, rangeProxyViewModel.EndDate, noregs);

            return NoContent();
        }

        [HttpPost("update-document-approval")]
        public async Task<IActionResult> UpdateDocumentApproval()
        {
            await CoreService.UpdateDocumentApproval();

            return NoContent();
        }

        [HttpPost("update-spkl-document-approval")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateSpklDocumentApproval([FromBody] GenericIdViewModel<string> genericIdViewModel)
        {
            var noregs = genericIdViewModel.Ids != null
                ? genericIdViewModel.Ids
                : new string[] { null };

            await CoreService.UpdateSpklDocumentApproval(noregs);

            return NoContent();
        }
    }
    #endregion
}
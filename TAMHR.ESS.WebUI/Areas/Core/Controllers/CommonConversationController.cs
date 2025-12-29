using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    /// <summary>
    /// Api controller for common conversation.
    /// </summary>
    [Route("api/common-conversation")]
    public class CommonConversationController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Common conversation service.
        /// </summary>
        protected CommonConversationService CommonConversationService => ServiceProxy.GetService<CommonConversationService>();
        #endregion

        /// <summary>
        /// Create new common conversation.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CommonConversationRequest commonConversationRequest)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var name = ServiceProxy.UserClaim.Name;

            commonConversationRequest.NoReg = noreg;
            commonConversationRequest.Name = name;
            commonConversationRequest.Message = WebUtility.HtmlEncode(commonConversationRequest.Message);

            var output = await CommonConversationService.SaveConversationAsync(commonConversationRequest);

            return Ok(output);
        }
    }
    #endregion
}
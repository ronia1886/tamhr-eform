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
    /// Api controller for conversation.
    /// </summary>
    [Route("api/conversation")]
    public class ConversationApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Conversation service
        /// </summary>
        protected ConversationService ConversationService => ServiceProxy.GetService<ConversationService>();
        #endregion

        /// <summary>
        /// Create new conversation
        /// </summary>
        /// <remarks>
        /// Create new conversation
        /// </remarks>
        /// <returns>Created Conversation Object</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ConversationViewModel conversationViewModel)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var name = ServiceProxy.UserClaim.Name;
            conversationViewModel.Message = WebUtility.HtmlEncode(conversationViewModel.Message);

            var output = await ConversationService.SaveConversationAsync(noreg, name, conversationViewModel);

            return CreatedAtAction("Get", output);
        }
    }
    #endregion
}
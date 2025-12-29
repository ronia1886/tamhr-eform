using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.Infrastructure.Web.ViewComponents
{
    public class CommonConversation : ViewComponent
    {
        private CommonConversationService _commonConversationService;

        public CommonConversation(CommonConversationService commonConversationService)
        {
            _commonConversationService = commonConversationService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string id)
        {
            var conversations = await _commonConversationService.GetConversations(id);

            return View(conversations);
        }
    }
}

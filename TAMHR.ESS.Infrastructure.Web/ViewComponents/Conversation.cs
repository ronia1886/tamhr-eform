using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.Infrastructure.Web.ViewComponents
{
    public class Conversation : ViewComponent
    {
        private ConversationService _conversationService;

        public Conversation(ConversationService conversationService)
        {
            _conversationService = conversationService;
        }

        public async Task<IViewComponentResult> InvokeAsync(Guid id)
        {
            var conversations = await _conversationService.GetConversations(id);

            return View(conversations);
        }
    }
}

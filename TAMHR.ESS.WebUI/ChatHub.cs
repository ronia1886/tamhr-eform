using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using TAMHR.ESS.Infrastructure.Web.UserAgentManager;

namespace TAMHR.ESS.WebUI
{
    public class ChatHub : Hub
    {
        public async Task ReceiveMessage(string channelId, string noreg, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", channelId, noreg, message);
        }

        public async Task AssociateJob(string jobId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, jobId);
        }
    }

}

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain.Models.OHS;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Modules.OHS.DomainServices;
using TAMHR.ESS.Infrastructure.Web;
using Agit.Domain.UnitOfWork;
using Agit.Domain;
using TAMHR.ESS.WebUI.State;
using Dapper;
using System;

namespace TAMHR.ESS.WebUI.Areas.OHS.ChatHub
{
    public class TanyaOhsChatHub : Hub
    {
        public async Task SendMessage(string TanyaOhsId, string Username, string Message)
        {
            var sql = "INSERT INTO TB_R_TANYAOHS_CHAT (Id, TanyaOhsId, Username, Message, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, RowStatus) VALUES(@Id, @TanyaOhsId, @Username, @Message, @CreatedBy, GETDATE(), null, null, 1);";
            try
            {
                Db.CreateConnection().Execute(sql, new
                {
                    Id = Guid.NewGuid(),
                    TanyaOhsId,
                    Username,
                    Message,
                    CreatedBy = Username
                });
                if (TanyaOhsHelper.Is.Pic(Username))
                {
                    if (TanyaOhsHelper.Is.NotWorkingTime())
                    {
                        TanyaOhsHelper.Notif.WorkingReply(TanyaOhsId, Message);
                        TanyaOhsHelper.Email.WorkingReply(TanyaOhsId, Message);
                    }
                }
            }
            catch (Exception)
            {
                var a = "";
            }
            await Clients.All.SendAsync("ReceiveMessage", TanyaOhsId, Username, Message);
        }
    }
}

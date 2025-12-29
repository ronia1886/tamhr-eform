using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Requests;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Agit.Common;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TAMHR.ESS.WebUI.Areas.OHS.State;
using Dapper;
using TAMHR.ESS.Domain;
using System.Linq;
using System.Collections.Generic;
using System;
using TAMHR.ESS.Infrastructure.Modules.OHS.DomainServices;
using TAMHR.ESS.WebUI.State;
using TAMHR.ESS.Domain.Models.OHS;
using TAMHR.ESS.Infrastructure.Web.Extensions;
using System.Collections.Specialized;
using Notification = TAMHR.ESS.Domain.Notification;
using Microsoft.AspNetCore.SignalR;
using TAMHR.ESS.WebUI.Areas.OHS.ChatHub;

namespace TAMHR.ESS.WebUI.Areas.OHS.Controllers
{
    [Route("api/tanyaOHS")]
    public class TanyaOHSApiController : ApiControllerBase
    {
        private readonly CoreService _coreService;
        private readonly EmailService _emailService;
        protected TanyaOhsService tanyaOhsService => ServiceProxy.GetService<TanyaOhsService>();
        private readonly IHubContext<TanyaOhsChatHub> _hubContext;

        public TanyaOHSApiController(IHubContext<TanyaOhsChatHub> hubContext)
        {
            _hubContext = hubContext;
        }
        [HttpPost("getDashboard")]
        public dynamic GetDashboard([FromBody] dynamic body)
        {
            using (var conn = Db.CreateConnection())
            {
                const string sql = @"
                    SELECT * 
                    FROM VW_TANYAOHS 
                    WHERE CreatedOn >= @Start 
                    AND CreatedOn <= @End
                    AND KategoriLayanan LIKE '%' + @KategoriLayanan + '%' 
                    AND Solve LIKE '%' + @Solve + '%'";

                var result = conn.Query<TanyaOhs>(sql, new
                {
                    Start = (DateTime?)body.start,
                    End = (DateTime?)body.end,
                    KategoriLayanan = (string?)body.KategoriLayanan ?? string.Empty,
                    Solve = (string?)body.Solve ?? string.Empty
                });

                return new
                {
                    status = "ok",
                    result
                };
            }
        }
        [HttpPost("gets")]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            var SqlAutoCLose = "UPDATE TB_R_TANYAOHS SET Status = 'Closed', ModifiedBy = 'Auto Close', ModifiedOn = CONCAT(CONVERT(date, DATEADD(day, -1, GETDATE())),' 23:59:59.000') WHERE CreatedOn < CAST(GETDATE() as DATE) AND Status = 'On-Going' AND KategoriLayanan <> 'Konsultasi Asuransi';";
            Db.CreateConnection().Execute(SqlAutoCLose);
            return await tanyaOhsService.GetQuery().OrderBy((i) => i.RowNum).ToDataSourceResultAsync(request);
        }

        [HttpGet("get-category")]
        public async Task<List<GeneralCategory>> GetCategory()
        {
            string connString = Global.StaticConfig.GetSection("ConnectionString").Value;
            var connection = new SqlConnection(connString);
            var query = "SELECT x.* FROM TAMHR_ESS.dbo.TB_M_GENERAL_CATEGORY x\r\nWHERE Category IN (N'TanyaOHSCode')";
            var result = connection.Query<GeneralCategory>(query).ToList();
            return result;
        }

        [HttpPost("NewConsult")]
        public object NewConsult([FromBody] TanyaOhs body)
        {
            var Id = TanyaOhsHelper.Orm.CreateTanyaOhs(body, User.GetClaim("Id"), User.GetClaim("Username"));
            if (body.KategoriLayanan == "Konsultasi Kesehatan" || body.KategoriLayanan == "Konsultasi Asuransi")
                TanyaOhsHelper.Chat.SendNewConsult(_hubContext, Id.ToString());
            TanyaOhsHelper.Chat.Event(_hubContext, "NewConsult", Id.ToString());
            if (body.KategoriLayanan.Contains("Asuransi"))
            {
                TanyaOhsHelper.Notif.WorkingCreate(body.KategoriLayanan, User.GetClaim("NoReg"), body.DoctorId);
                TanyaOhsHelper.Email.WorkingCreate(body.KategoriLayanan, body.Keluhan, User.GetClaim("Id"), body.DoctorId);
            }
            else
            {
                if (TanyaOhsHelper.Is.NotWorkingTime())
                {
                    try
                    {
                        TanyaOhsHelper.Notif.WorkingCreate(body.KategoriLayanan, User.GetClaim("NoReg"), body.DoctorId);
                        TanyaOhsHelper.Email.WorkingCreate(body.KategoriLayanan, body.Keluhan, User.GetClaim("Id"), body.DoctorId);
                    }
                    catch (Exception e)
                    {
                        var a = e.Message;
                    }
                }
            }
            return new
            {
                Status = "ok",
                Result = new
                {
                    Id
                }
            };
        }

        [HttpPost("EndService")]
        public object EndService([FromBody] TanyaOhs body)
        {
            var sql = $"UPDATE TB_R_TANYAOHS SET Status=@Status, Solve=@Solve, Feedback=@Feedback, Rating=@Rating, ModifiedBy=@ModifiedBy, ModifiedOn=GETDATE() WHERE Id=@Id;";
            Db.CreateConnection().Execute(sql, new
            {
                body.Id,
                Status = "Closed",
                body.Solve,
                body.Feedback,
                body.Rating,
                ModifiedBy = User.GetClaim("Username")
            });
            var EventName = User.GetClaim("Type") == "OHS" ? "EndServicePic" : "EndServiceUser";
            TanyaOhsHelper.Chat.SendEndChat(_hubContext, body.Id.ToString());
            TanyaOhsHelper.Chat.Event(_hubContext, EventName, body.Id.ToString());
            return new
            {
                Status = "ok"
            };
        }

        [HttpPost("ReplyFeedback")]
        public object ReplyFeedback([FromBody] dynamic body)
        {
            var tanyaOhsId = (string)body.Id;
            var sql = $"UPDATE TB_R_TANYAOHS SET ReplyFeedback=@ReplyFeedback, ModifiedBy=@ModifiedBy, ModifiedOn=GETDATE() WHERE Id=@Id;";
            Db.CreateConnection().Execute(sql, new
            {
                Id = tanyaOhsId,
                ReplyFeedback = (string)body.ReplyFeedback,
                ModifiedBy = User.GetClaim("Username")
            });
            TanyaOhsHelper.Email.ReplyFeedback(tanyaOhsId, User.GetClaim("Name"));
            return new
            {
                Status = "ok"
            };
        }

        [HttpGet("GetTanyaOhs")]
        public dynamic GetTanyaOhs([FromQuery] string Id)
        {
            return TanyaOhsHelper.Orm.GetTanyaOhs(Id);
        }

        [HttpGet("GetChat")]
        public async Task<dynamic> GetChat([FromQuery] string TanyaOhsId)
        {
            await TanyaOhsHelper.Service.ChatBeenRead(TanyaOhsId, User.GetClaim("Username"));
            return await TanyaOhsHelper.Service.GetChat(TanyaOhsId);
        }

        [HttpGet("GetDoctor")]
        public async Task<dynamic> GetDoctor([FromQuery] string Role)
        {
            using (var conn = Db.CreateConnection())
            {
                const string sql = "SELECT * FROM VW_USER WHERE [Role] = @Role";
                var result = (await conn.QueryAsync(sql, new { Role })).ToList();
                return result;
            }
        }

        [HttpGet("GetPicStatus")]
        public object GetPicStatus()
        {
            var sql = $"SELECT Status FROM TB_M_USER_OHS WHERE Id=@Id;";
            var Result = Db.CreateConnection().Query(sql, new
            {
                Id = User.GetClaim("Id")
            });
            return new
            {
                Status = "ok",
                Result = Result.First()
            };
        }

        [HttpPost("ChangePicStatus")]
        public object ChangePicStatus([FromBody] dynamic body)
        {
            var sql = $"UPDATE TB_M_USER_OHS SET Status=@Status, ModifiedBy=@ModifiedBy, ModifiedOn=GETDATE() WHERE Id=@Id;";
            Db.CreateConnection().Execute(sql, new
            {
                Id = User.GetClaim("Id"),
                Status = (string)body.Status,
                ModifiedBy = User.GetClaim("Username")
            });
            TanyaOhsHelper.Chat.Event(_hubContext, "PicStatusChanged", "");
            return new
            {
                Status = "ok"
            };
        }

        [HttpGet]
        [Route("Test")]
        public object Test()
        {
            return new { Message = "hello" };
        }
    }

    [Area(ApplicationModule.OHS)]
    public class TanyaOHSController : MvcControllerBase
    {
        [Permission(PermissionKey.TanyaOhsKaryawan)]
        public IActionResult Index()
        {
            return View();
        }
        [Permission(PermissionKey.TanyaOhsPicAsuransi)]
        public IActionResult IndexPicAsuransi()
        {
            return View("IndexPic");
        }
        [Permission(PermissionKey.TanyaOhsPicKesehatan)]
        public IActionResult IndexPicKesehatan()
        {
            return View("IndexPic");
        }
        [Permission(PermissionKey.TanyaOhsAdmin)]
        public IActionResult IndexAdmin()
        {
            return View();
        }
        public IActionResult KebijakanKesehatan()
        {
            return View("KebijakanKesehatan");
        }
        public IActionResult KebijakanKeselamatan()
        {
            return View("KebijakanKeselamatan");
        }
    }
}
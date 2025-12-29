using Agit.Common;
using Agit.Common.Email;
using Agit.Common.Extensions;
using Dapper;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using Scriban;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Domain.Models.Core.StoredEntities;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.UserAgentManager;
using TAMHR.ESS.WebUI.Areas.OHS.ChatHub;
using TAMHR.ESS.WebUI.Areas.OHS.State;
using TAMHR.ESS.WebUI.State;

namespace TAMHR.ESS.WebUI.Areas.OHS
{
    public static class TanyaOhsHelper
    {
        public static class AppConfig
        {
            public static bool GetBoolean(string key)
            {
                return Db.AppConfig.GetValue<bool>(key);
            }
        }

        public static class Chat
        {
            public static void Event(IHubContext<TanyaOhsChatHub> hubContext, string EventName, string TanyaOhsId)
            {
                hubContext.Clients.All.SendAsync("ReceiveEvent", EventName, TanyaOhsId);
            }
            public static void Send(IHubContext<TanyaOhsChatHub> hubContext, string TanyaOhsId, string Username, string Message)
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
                    if (Is.Pic(Username))
                    {
                        if (Is.NotWorkingTime())
                        {
                            Notif.WorkingReply(TanyaOhsId, Message);
                            Email.WorkingReply(TanyaOhsId, Message);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + e.StackTrace);
                }
                hubContext.Clients.All.SendAsync("ReceiveMessage", TanyaOhsId, Username, Message);
            }
            public static void SendEndChat(IHubContext<TanyaOhsChatHub> hubContext, string TanyaOhsId)
            {
                var Data = Orm.GetTanyaOhs(TanyaOhsId);
                var MessagePic = "Terima kasih, sudah menanggunakan Layanan TanyaOHS untuk bertanya dan konsultasi, semoga jawaban kami dapat membantu!";
                var MessageUser = "Terima kasih, atas jawabannya, Saya sangat terbantu!";
                var UsernameUser = Orm.GetUser(Data.UserId).Username;
                //var UsernamePic = Orm.GetUser(Data.DoctorId).Username;
                //handle doctor id kosong
                var userPic = Orm.GetUser(Data.DoctorId);
                var UsernamePic = userPic?.Username ?? "Unknown";
                Send(hubContext, TanyaOhsId, UsernamePic, MessagePic);
                Send(hubContext, TanyaOhsId, UsernameUser, MessageUser);
            }
            public static void SendNewConsult(IHubContext<TanyaOhsChatHub> hubContext, string TanyaOhsId)
            {
                var Data = Orm.GetTanyaOhs(TanyaOhsId);
                var MessagePic = "Halo Selamat Datang di Layanan TanyaOHS, Ada yang bisa dibantu?";
                var MessageUser = $"Halo, Saya ada keluhan terkait : {Data.Keluhan}";
                var UsernameUser = Orm.GetUser(Data.UserId).Username;
                var UsernamePic = Orm.GetUser(Data.DoctorId).Username;
                Send(hubContext, TanyaOhsId, UsernamePic, MessagePic);
                Send(hubContext, TanyaOhsId, UsernameUser, MessageUser);
            }
        }

        public static class CryptoHelper
        {
            // Encrypt plaintext (simpan hasil ini di DB)
            public static string Encrypt(string plainText)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(plainText);
                byte[] encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
                return Convert.ToBase64String(encrypted);
            }

            // Decrypt ciphertext (ambil dari DB, gunakan untuk SMTP)
            public static string Decrypt(string cipherText)
            {
                byte[] bytes = Convert.FromBase64String(cipherText);
                byte[] decrypted = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(decrypted);
            }
        }

        public static class Email
        {
            //public static EmailTemplate TemplateAsuransi = Orm.GetEmailTemplate("ohs-tanyaohs-email-asuransi");
            public static EmailTemplate TemplateAsuransi = Orm.GetEmailTemplate("ohs-tanyaohs-email-asuransi") ?? new EmailTemplate
            {
                MailFrom = "noreply.eform@toyota.astra.co.id",
                Subject = "[Template Missing] ohs-tanyaohs-email-feedback",
                MailContent = "Template email tidak ditemukan di database."
            };
            //public static EmailTemplate TemplateFeedback = Orm.GetEmailTemplate("ohs-tanyaohs-email-feedback");
            public static EmailTemplate TemplateFeedback = Orm.GetEmailTemplate("ohs-tanyaohs-email-feedback") ?? new EmailTemplate
            {
                MailFrom = "noreply.eform@toyota.astra.co.id",
                Subject = "[Template Missing] ohs-tanyaohs-email-feedback",
                MailContent = "Template email tidak ditemukan di database."
            };
            //public static EmailTemplate TemplateKesehatan = Orm.GetEmailTemplate("ohs-tanyaohs-email-kesehatan");
            public static EmailTemplate TemplateKesehatan = Orm.GetEmailTemplate("ohs-tanyaohs-email-kesehatan") ?? new EmailTemplate
            {
                MailFrom = "noreply.eform@toyota.astra.co.id",
                Subject = "[Template Missing] ohs-tanyaohs-email-feedback",
                MailContent = "Template email tidak ditemukan di database."
            };
            public static EmailManager CreateEmailManager()
            {
                
                var host = Orm.GetConfigValue("Email.Host");
                var port = int.Parse(Orm.GetConfigValue("Email.Port"));
                var enableSsl = bool.Parse(Orm.GetConfigValue("Email.EnableSsl"));
                var useDefaultCredentials = bool.Parse(Orm.GetConfigValue("Email.UseDefaultCredentials"));
                var user = Orm.GetConfigValue("Email.User");
                //var password = Orm.GetConfigValue("Email.Password");
                var rahasia = Orm.GetConfigValue(Configurations.EmailPassword);
                var mailFrom = Orm.GetConfigValue("Email.MailFrom");
                var configuration = new EmailConfiguration
                {
                    Host = host,
                    Port = port,
                    EnableSsl = enableSsl,
                    UseDefaultCredentials = useDefaultCredentials,
                    IsBodyHtml = true,
                    User = user,
                    Password = rahasia,
                    SmtpDeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network
                };

                var emailManager = new EmailManager(configuration);

                return emailManager;
            }
            public static void Send(string From, string Subject, string Content, string To)
            {
                var emailManager = CreateEmailManager();


                emailManager.SendAsync(From, Subject, Content, To);


            }
            public static void ReplyFeedback(string tanyaOhsId, string AdminName)
            {
                var template = TemplateFeedback;
                var tanyaOhs = Orm.GetTanyaOhs(tanyaOhsId);
                var user = Orm.GetUser(tanyaOhs.UserId);
                Dictionary<string, string> replacements = new Dictionary<string, string>(){
                    {"{{name}}", user.Name},
                    {"{{message}}", tanyaOhs.ReplyFeedback},
                    {"{{from}}", AdminName},
                    {"{{year}}", DateTime.Now.Year.ToString()},
                };
                var data = template.MailContent;
                foreach (string key in replacements.Keys)
                {
                    data = data.Replace(key, replacements[key]);
                }
                Send(template.MailFrom, template.Subject, data, user.Email);
            }
            public static void Working(EmailTemplate Template, string Message, string FromId, string ToId)
            {
                var From = Orm.GetUser(FromId);
                var To = Orm.GetUser(ToId);
                Dictionary<string, string> replacements = new Dictionary<string, string>(){
                    {"{{name}}", To.Name},
                    {"{{message}}", Message},
                    {"{{from}}", From.Name},
                    {"{{year}}", DateTime.Now.Year.ToString()},
                };
                var data = Template.MailContent;
                foreach (string key in replacements.Keys)
                {
                    data = data.Replace(key, replacements[key]);
                }
                Send(Template.MailFrom, Template.Subject, data, To.Email);
            }
            public static void WorkingCreate(string KategoriLayanan, string Message, string FromId, string ToId)
            {
                EmailTemplate template = null;
                if (KategoriLayanan == "Konsultasi Asuransi")
                    template = TemplateAsuransi;
                if (KategoriLayanan == "Konsultasi Kesehatan")
                    template = TemplateKesehatan;
                Working(template, Message, FromId, ToId);
            }
            public static void WorkingReply(string TanyaOhsId, string Message)
            {
                var tanyaOhs = Orm.GetTanyaOhs(TanyaOhsId);
                var pic = Orm.GetUser(tanyaOhs.DoctorId);
                var KategoriLayanan = tanyaOhs.KategoriLayanan;
                EmailTemplate template = null;
                if (KategoriLayanan == "Konsultasi Asuransi")
                    template = TemplateAsuransi;
                if (KategoriLayanan == "Konsultasi Kesehatan")
                    template = TemplateKesehatan;
                var FromId = pic.Id.ToString();
                var ToId = tanyaOhs.UserId;
                Working(template, Message, FromId, ToId);
            }
        }

        public static class Is
        {
            public static bool Pic(string Username)
            {
                var user = Orm.GetUserByUsername(Username);
                return user.Type == "OHS";
            }
            public static bool WorkingTime()
            {
                var date = DateTime.Now;
                var IsWorkingTime = false;
                if (date.Hour >= 8 && date.Hour < 17)
                {
                    IsWorkingTime = true;
                }
                return IsWorkingTime;
            }
            public static bool NotWorkingTime()
            {
                var WorkingTimeReverse = false;
                try
                {
                    WorkingTimeReverse = AppConfig.GetBoolean("Ohs:TanyaOhs:WorkingTimeReverse");
                }
                catch { }
                if (WorkingTimeReverse)
                    return WorkingTime();
                else
                    return !WorkingTime();
            }
        }

        public static class Orm
        {
            //public static string GetConfigValue(string ConfigKey)
            //{
            //    return Db.CreateConnection().Query<Config>(
            //        $"SELECT * FROM TB_M_CONFIG WHERE ConfigKey = '{ConfigKey}'"
            //        ).First().ConfigValue;
            //}
            public static string GetConfigValue(string configKey)
            {
                using (var connection = Db.CreateConnection())
                {
                    const string sql = "SELECT ConfigValue FROM TB_M_CONFIG WHERE ConfigKey = @ConfigKey";

                    var result = connection.QueryFirstOrDefault<string>(sql, new { ConfigKey = configKey });

                    return result ?? string.Empty;
                }
            }
            public static EmailTemplate GetEmailTemplate(string MailKey)
            {
                return Db.CreateConnection().Query<EmailTemplate>(
                    $"SELECT * FROM TB_M_EMAIL_TEMPLATE WHERE MailKey = '{MailKey}'").FirstOrDefault();
            }
            public static string CreateTanyaOhs(TanyaOhs body, string userId, string username)
            {
                var sql = "INSERT INTO TB_R_TANYAOHS (Id, UserId, Keluhan, KategoriLayanan, Solve, Feedback, Rating, ReplyFeedback, Status, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, RowStatus, DoctorId) VALUES(@Id, @UserId, @Keluhan, @KategoriLayanan, @Solve, @Feedback, @Rating, @ReplyFeedback, @Status, @CreatedBy, GETDATE(), null, null, 1, @DoctorId);";
                var id = Guid.NewGuid();
                Db.CreateConnection().Execute(sql, new
                {
                    id,
                    UserId = userId,
                    body.Keluhan,
                    body.KategoriLayanan,
                    Solve = "",
                    Feedback = "",
                    Rating = "",
                    ReplyFeedback = "",
                    Status = "On-Going",
                    CreatedBy = username,
                    body.DoctorId
                });
                return id.ToString();
            }
            public static TanyaOhs GetTanyaOhs(string TanyaOhsId)
            {
                if (string.IsNullOrWhiteSpace(TanyaOhsId))
                    return null;

                const string sql = @"SELECT TOP 1 * FROM VW_TANYAOHS WHERE Id = @Id";

                using (var db = Db.CreateConnection())
                {
                    var result = db.QueryFirstOrDefault<TanyaOhs>(sql, new { Id = TanyaOhsId });
                    return result;
                }
            }
            public static bool DeleteTanyaOhs(string TanyaOhsId)
            {
                try
                {
                    var sql = $"DELETE FROM TB_R_TANYAOHS WHERE Id = '{TanyaOhsId}'";
                    Db.CreateConnection().Execute(sql);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            public static UserView GetUser(string Id)
            {
                if (string.IsNullOrWhiteSpace(Id))
                    return null;

                const string query = "SELECT * FROM VW_USER WHERE Id = @Id";

                using (var conn = Db.CreateConnection())
                {
                    return conn.Query<UserView>(query, new { Id }).FirstOrDefault();
                }
            }
            public static UserView GetUserByUsername(string Username)
            {
                const string query = @"SELECT * FROM VW_USER WHERE Username = @Username";

                using (var conn = Db.CreateConnection())
                {
                    return conn.Query<UserView>(query, new { Username }).FirstOrDefault();
                }
            }
            public static void UpdatePicStatus(string username, string status, IHubContext<TanyaOhsChatHub> hubContext)
            {
                var sql = $"UPDATE TB_M_USER_OHS SET Status=@Status, ModifiedBy=@ModifiedBy, ModifiedOn=GETDATE() WHERE Id=@Id;";
                var user = GetUserByUsername(username);
                if (user.Type == "OHS")
                {
                    Db.CreateConnection().Execute(sql, new
                    {
                        user.Id,
                        Status = status,
                        ModifiedBy = user.Username
                    });
                    Chat.Event(hubContext, "PicStatusChanged", "");
                }
            }
        }
        public class Service
        {
            public async static Task<dynamic> GetChat(string TanyaOhsId)
            {
                const string query = @"SELECT * 
                           FROM TB_R_TANYAOHS_CHAT 
                           WHERE TanyaOhsId = @TanyaOhsId 
                           ORDER BY CreatedOn";

                using (var conn = Db.CreateConnection())
                {
                    var result = await conn.QueryAsync(query, new { TanyaOhsId });
                    return result;
                }
            }
            public async static Task<dynamic> ChatBeenRead(string TanyaOhsId, string username)
            {
                const string query = @"
                    UPDATE TB_R_TANYAOHS_CHAT 
                    SET IsRead = 1 
                    WHERE TanyaOhsId = @TanyaOhsId 
                    AND Username <> @Username";

                using (var conn = Db.CreateConnection())
                {
                    var result = await conn.ExecuteAsync(query, new { TanyaOhsId, Username = username });
                    return result;
                }
            }
        }

        public static class Notif
        {
            public static string NotifMessage = "Receive Message {KategoriLayanan} from {Username} at {Date}";
            public static void Send(string From, string To, string Message)
            {
                var sql = "INSERT INTO TB_R_NOTIFICATION (Id, FromNoReg, ToNoReg, Message, NotificationTypeCode, TriggerDate, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, RowStatus) VALUES(@Id, @FromNoReg, @ToNoReg, @Message, @NotificationTypeCode, GETDATE(), @CreatedBy, GETDATE(), null, null, 1);";
                Db.CreateConnection().Execute(sql, new
                {
                    Id = Guid.NewGuid(),
                    FromNoreg = From,
                    ToNoreg = To,
                    Message,
                    NotificationTypeCode = "notice",
                    CreatedBy = From,
                });
            }
            public static void Working(string KategoriLayanan, string From, string To)
            {
                Dictionary<string, string> replacements = new Dictionary<string, string>(){
                    {"{KategoriLayanan}", KategoriLayanan},
                    {"{Username}", From},
                    {"{Date}", DateTime.Now.ToString("dd-MM-yyyy - HH:mm")},
                };
                var data = NotifMessage;
                foreach (string key in replacements.Keys)
                {
                    data = data.Replace(key, replacements[key]);
                }
                Send(From, To, data);
            }

            public static void WorkingCreate(string KategoriLayanan, string From, string PicId)
            {
                var To = Orm.GetUser(PicId).Username;
                Working(KategoriLayanan, From, To);
            }
            public static void WorkingReply(string TanyaOhsId, string Message)
            {
                var tanyaOhs = Orm.GetTanyaOhs(TanyaOhsId);
                var pic = Orm.GetUser(tanyaOhs.DoctorId);
                Working(tanyaOhs.KategoriLayanan, pic.NoReg, tanyaOhs.Noreg);
            }
        }
    }
}

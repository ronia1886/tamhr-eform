using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Common;
using Agit.Domain;
using Agit.Common.Email;
using Agit.Domain.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Scriban;
using System.Collections.Generic;
using Newtonsoft.Json;
using TAMHR.ESS.Infrastructure.ViewModels;
using System.IO;
using OfficeOpenXml;
using System.Drawing;
using System.Net.Mail;
using System.Net;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle email operation
    /// </summary>
    public class EmailService : DomainServiceBase
    {

        #region Repositories
        /// <summary>
        /// Core service object
        /// </summary>
        protected readonly CoreService CoreService;
        /// <summary>
        /// Mail queue repository
        /// </summary>
        protected IRepository<MailQueue> MailQueueRepository => UnitOfWork.GetRepository<MailQueue>();

        /// <summary>
        /// Email template repository
        /// </summary>
        protected IRepository<EmailTemplate> EmailTemplateRepository => UnitOfWork.GetRepository<EmailTemplate>();

        /// <summary>
        /// Sync log repository
        /// </summary>
        protected IRepository<SyncLog> SyncLogRepository => UnitOfWork.GetRepository<SyncLog>();

        protected IReadonlyRepository<ReminderEmailAreaActivitytViewModel> ReminderEmailAreaActivityViewReadOnlyRepository => UnitOfWork.GetRepository<ReminderEmailAreaActivitytViewModel>();
        #endregion

        #region Constructor
        public EmailService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
            
        }
        #endregion

        /// <summary>
        /// Create email manager from configuration
        /// </summary>
        /// <returns>Email Manager Object</returns>
        public EmailManager CreateEmailManager()
        {
            var configService = new ConfigService(UnitOfWork);
            var configRepository = configService.GetConfigs(true);
            var configs = configRepository.Where(x => x.ConfigKey.StartsWith("Email."));
            var host = configs.FirstOrDefault(x => x.ConfigKey == "Email.Host")?.ConfigValue;
            var port = int.Parse(configs.FirstOrDefault(x => x.ConfigKey == "Email.Port")?.ConfigValue);
            var enableSsl = bool.Parse(configs.FirstOrDefault(x => x.ConfigKey == "Email.EnableSsl")?.ConfigValue);
            var useDefaultCredentials = bool.Parse(configs.FirstOrDefault(x => x.ConfigKey == "Email.UseDefaultCredentials")?.ConfigValue);
            var user = configs.FirstOrDefault(x => x.ConfigKey == "Email.User")?.ConfigValue;
            var password = configs.FirstOrDefault(x => x.ConfigKey == "Email.Password")?.ConfigValue;
            var mailFrom = configs.FirstOrDefault(x => x.ConfigKey == "Email.MailFrom")?.ConfigValue;


            var configuration = new EmailConfiguration
            {
                Host = host,
                Port = port,
                EnableSsl = enableSsl,
                UseDefaultCredentials = useDefaultCredentials,
                IsBodyHtml = true,
                User = user,
                Password = password,
                SmtpDeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network
            };

            var emailManager = new EmailManager(configuration);

            return emailManager;
        }

        /// <summary>
        /// Send email.
        /// </summary>
        /// <param name="mailKey">This email key.</param>
        /// <param name="recipients">This email recipients.</param>
        /// <param name="data">This object data.</param>
        /// <param name="cc">This email cc (default is null).</param>
        /// <param name="bcc">This email bcc (default is null).</param>
        /// <returns>True if success sending email, false otherwise.</returns>
        public bool SendEmail(string mailKey, string recipients, object data, string cc = null, string bcc = null)
        {
            var coreService = new CoreService(UnitOfWork);
            var userService = new UserService(UnitOfWork);
            // Set default sending email result (default is false).
            var output = false;

            // Create SMTP email manager.
            var emailManager = CreateEmailManager();

            // Get email template from given key.
            var emailTemplate = EmailTemplateRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.MailKey == mailKey);

            // If email template was not found then return output;
            if (emailTemplate == null) return output;

            // Set mail from.
            var from = emailTemplate.MailFrom;

            // Compile email content template.
            var template = Template.Parse(emailTemplate.MailContent);

            // Compile title template.
            var titleTemplate = Template.Parse(emailTemplate.Subject);

            // Set subject with compiled template data.
            var subject = titleTemplate.Render(data);

            // Set email content with compiled template data.
            var content = template.Render(data);

            var listRecipient = recipients.Split(',');

            var mailQueue = MailQueue.Create(mailKey, subject, content, listRecipient, from);

            MailQueueRepository.Add(mailQueue);
            mailQueue.ModifiedOn = DateTime.Now;

            // Try sending email.
            try
            {
                // Sending email.
                
                emailManager.Send(from, subject, content, recipients, cc: cc, bcc: bcc);
                mailQueue.ModifiedBy = "system";

                // Set result to true.
                output = true;
                mailQueue.MailStatusCode = "sent";
            }
            catch (Exception e)
            {              
                mailQueue.ModifiedBy = "system";
                var msg = e.Message;
                mailQueue.ExceptionMessage = e.ToString();
            }

            UnitOfWork.SaveChanges();
            
            // Return the email result.
            return output;
        }

        public bool SendEmailAsync(string mailKey, string recipients, object data, string cc = null, string bcc = null)
        {
            
            var coreService = new CoreService(UnitOfWork);
            var userService = new UserService(UnitOfWork);
            // Set default sending email result (default is false).
            var output = false;

            // Create SMTP email manager.
            var emailManager = CreateEmailManager();

            // Get email template from given key.
            var emailTemplate = EmailTemplateRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.MailKey == mailKey);

            // If email template was not found then return output;
            if (emailTemplate == null) return output;

            // Set mail from.
            var from = emailTemplate.MailFrom;

            // Compile email content template.
            var template = Template.Parse(emailTemplate.MailContent);

            // Compile title template.
            var titleTemplate = Template.Parse(emailTemplate.Subject);

            // Set subject with compiled template data.
            var subject = titleTemplate.Render(data);

            // Set email content with compiled template data.
            var content = template.Render(data);

            var listRecipient = recipients.Split(',');

            var mailQueue = MailQueue.Create(mailKey, subject, content, listRecipient, from);

            MailQueueRepository.Add(mailQueue);
            mailQueue.ModifiedOn = DateTime.Now;

            // Try sending email.
            try
            {
                // Sending email.

                emailManager.SendAsync(from, subject, content, recipients, cc: cc, bcc: bcc);
                mailQueue.ModifiedBy = "system";

                // Set result to true.
                output = true;
                mailQueue.MailStatusCode = "sent";
            }
            catch (Exception e)
            {
                mailQueue.ModifiedBy = "system";
                var msg = e.Message;
                mailQueue.ExceptionMessage = e.ToString();
            }

            UnitOfWork.SaveChanges();

            // Return the email result.
            return output;
        }

        /// <summary>
        /// Send reminder asynchronously
        /// </summary>
        /// <returns>Task Object</returns>
        public async Task SendReminderAsync()
        {
            var now = DateTime.Now;
            var configService = new ConfigService(UnitOfWork);
            var emailManager = CreateEmailManager();
            var appUrl = configService.GetConfig("Application.Url")?.ConfigValue;
            var url = $"{appUrl}/core/home/tasks";
            var pendingTasks = UnitOfWork.SqlQuery<PendingTaskView>();
            var emailTemplate = EmailTemplateRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.MailKey == MailTemplate.PendingTasks);

            Assert.IsNotNull(emailTemplate, nameof(emailTemplate));

            var template = Template.Parse(emailTemplate.MailContent);
            var subjectTemplate = Template.Parse(emailTemplate.Subject);

            foreach (var pendingTask in pendingTasks)
            {
                var content = template.Render(new {
                    pendingTask.Name,
                    pendingTask.NoReg,
                    pendingTask.TotalPending,
                    Url = url,
                    now.Year
                });

                await emailManager.SendAsync(emailTemplate.MailFrom, emailTemplate.Subject, content, pendingTask.Email).ConfigureAwait(false);
            }
        }

        public async Task SendBpkbDateReminderAsync()
        {
            var now = DateTime.Now;
            var configService = new ConfigService(UnitOfWork);
            var userService = new UserService(UnitOfWork);
            var approvalService = new ApprovalService(UnitOfWork, null, null);
            var BpkbService = new BpkbService(UnitOfWork);
            var emailManager = CreateEmailManager();
            var appUrl = configService.GetConfig("Application.Url")?.ConfigValue;
            var url = $"{appUrl}/core/home/tasks";
            var dataBpkbCop = approvalService.GetCompleteRequestDetails("cop");
            var dataUser = userService.GetActiveUsers();
            var dataUserHR = userService.GetUsersByRole(BuiltInRoles.Administrator);
            var exp = Convert.ToInt16(configService.GetConfig("Bpkb.Expired")?.ConfigValue);
            
            var emailTemplate = EmailTemplateRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.MailKey == MailTemplate.BpkbExpired);

            Assert.IsNotNull(emailTemplate, nameof(emailTemplate));

            var template = Template.Parse(emailTemplate.MailContent);
            var subjectTemplate = Template.Parse(emailTemplate.Subject);

            foreach (var item in dataBpkbCop)
            {
                var obj = JsonConvert.DeserializeObject<CopViewModel>(item.ObjectValue);

                if (obj.StnkDate.HasValue)
                {
                    if (DateTime.Now.Date == obj.StnkDate.Value.AddYears(1).AddMonths(-exp).Date)
                    {
                        var currUser = dataUser.FirstOrDefault(x => x.NoReg == item.CreatedBy);
                        var content = template.Render(new
                        {
                            Names = currUser.Name,
                            Year =  now.Year,
                            DateStnk = obj.StnkDate?.ToString("dd MMMM yyyy"),
                            StnkNumber = obj.LisencePlat
                        });
                        var recipients = currUser.Email;
                        await emailManager.SendAsync(emailTemplate.MailFrom, emailTemplate.Subject, content, recipients, cc: dataUserHR.FirstOrDefault()?.Email).ConfigureAwait(false);
                    }
                }
            }
        }

        public static DateTime GetReminderDate(DateTime currentDate)
        {
            int[] daysOfWeekend = new int[] { (int)DayOfWeek.Saturday, (int)DayOfWeek.Sunday };

            return Enumerable.Range(1, DateTime.DaysInMonth(currentDate.Year, currentDate.Month))
                             .Select(day => new DateTime(currentDate.Year, currentDate.Month, day))
                             .Where(date => !daysOfWeekend.Contains((int)date.DayOfWeek))
                             .OrderByDescending(date => date)
                             .Skip(2)
                             .First();
        }

        public async Task sendEmailReminderWeeklyWFHPlanningAsync()
        {
            //Get User Service
            var userService = new UserService(UnitOfWork);
            //Get Config Service
            var configService = new ConfigService(UnitOfWork);
            var configRepository = configService.GetConfigs(true);
            var configs = configRepository.Where(x => x.ConfigKey.StartsWith("Email."));
            var host = configs.FirstOrDefault(x => x.ConfigKey == "Email.Host")?.ConfigValue;
            var port = int.Parse(configs.FirstOrDefault(x => x.ConfigKey == "Email.Port")?.ConfigValue);
            var enableSsl = bool.Parse(configs.FirstOrDefault(x => x.ConfigKey == "Email.EnableSsl")?.ConfigValue);
            var useDefaultCredentials = bool.Parse(configs.FirstOrDefault(x => x.ConfigKey == "Email.UseDefaultCredentials")?.ConfigValue);
            var userName = configs.FirstOrDefault(x => x.ConfigKey == "Email.User")?.ConfigValue;
            var password = configs.FirstOrDefault(x => x.ConfigKey == "Email.Password")?.ConfigValue;
            var mailFrom = configs.FirstOrDefault(x => x.ConfigKey == "Email.MailFrom")?.ConfigValue;

            var now = DateTime.Now;
            var today = DateTime.Now;
            var activeUser = userService.GetActiveUsers();

            //Get user with Role Administration
            var dataUserHR = userService.GetUsersByRole(BuiltInRoles.Administrator);
            //config to get day start of reminder
            int startGap = Convert.ToInt16(configService.GetConfig("WeeklyWFHPlanning.ReminderWeeklyWFH")?.ConfigValue);

            int gap = 14;
            //Get config gap weekly wfh planning 
            Config gapWeeklyWFHCon = configService.GetConfig("WeeklyWFHPlanning.GapWeeklyWFH");

            if (gapWeeklyWFHCon != null)
            {
                gap = Convert.ToInt32(gapWeeklyWFHCon.ConfigValue);
            }


            //Inject Weekly WFH Planning Service
            var weeklyWfhPlanningService = new WeeklyWFHPlanningService(UnitOfWork, null);

            Config testerStartDate = configService.GetConfig("Others.TesterDate");

            if (testerStartDate != null && testerStartDate.ConfigText == "True")
            {
                now = DateTime.Parse(testerStartDate.ConfigValue);
            }
            //Generate date with Store Procedure Weekly WFH Planning 
            WFHGeneratePlanningDateWeekly pdw = weeklyWfhPlanningService.GetWeeklyWFHPlanningByDate(now, gap * 7, false);

            //Start date Weekly WFH Planning
            DateTime startDate = pdw.StartDate;
            //End date Weekly WFH Planning
            DateTime endDate = pdw.EndDate;

            var testerDate = configService.GetConfig("WeeklyWFHPlanning.TesterDate");

            if (testerDate != null && testerDate.ConfigText == "True")
            {
                today = DateTime.Parse(testerDate.ConfigValue);
            }

            //For testing
            //DateTime currentDate = new DateTime(2024, 8, 29);

            DateTime currentDate = DateTime.Now;
            DateTime reminderDate = GetReminderDate(currentDate);


            // Check if today is the reminder date
            if (currentDate.Date == reminderDate.Date)
            {
                Console.WriteLine($"Today is the reminder date: {reminderDate:dd/MM/yyyy}");
                Console.WriteLine("Send email reminders");


                var getData = weeklyWfhPlanningService.GetUserReminderSummaryWeeklyWFHPlanning(startDate, endDate);
                //Get All Active User That have to reminder
                if (getData == null)
                {
                    return;
                }

                var parentNoReg = getData.Select(x => x.ParentNoReg).Distinct();
                var getParentUser = activeUser.Where(a => parentNoReg.Contains(a.NoReg)).ToList();

                IEnumerable<UserPositionView> dataUser = weeklyWfhPlanningService.GetUserReminderWeeklyWFHPlanning(startDate, endDate);

                if (dataUser == null)
                {
                    return;
                }


                var emailManager = CreateEmailManager();

                var emailTemplate = EmailTemplateRepository.Fetch()
                    .AsNoTracking()
                    .Where(x => x.MailKey.Contains(MailTemplate.WeeklyWfhPlanning))
                    .ToDictionary(a => a.MailKey);

                var templateReminder = emailTemplate[MailTemplate.WeeklyWfhPlanning];
                var templateSummary = emailTemplate[MailTemplate.WeeklyWfhPlanningSummary];

                var generalCategory = configService.GetGeneralCategory(templateReminder.ModuleCode);

                Assert.IsNotNull(emailTemplate, nameof(emailTemplate));

                var fileName = string.Format("Hybrid Work Schedule Planning Reminder Summary {0:ddMMyyyy}-{1:ddMMyyyy}.xlsx", pdw.StartDate, pdw.EndDate);


                foreach (var parent in getParentUser)
                {

                    var childUser = getData.Where(a => a.ParentNoReg == parent.NoReg).ToList();
                    if (childUser == null)
                    {
                        continue;
                    }

                    var template = Template.Parse(templateSummary.MailContent);
                    var subjectTemplate = Template.Parse(templateSummary.Subject);

                    MemoryStream ms;
                    using (var package = new ExcelPackage())
                    {
                        int rowIndex = 2;
                        var detailSheet = package.Workbook.Worksheets.Add("Detail");
                        var detailCols = new List<string> { "ParentNoReg", "Noreg", "Name", "PostCode", "PostName", "JobName", "Directorate", "Division", "Department", "Section", "Line", "Group", "Class" };

                        for (var i = 1; i <= detailCols.Count; i++)
                        {
                            detailSheet.Cells[1, i].Value = detailCols[i - 1];
                            detailSheet.Cells[1, i].Style.Font.Bold = true;
                            detailSheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                            detailSheet.Cells[1, i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                            detailSheet.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            detailSheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(Color.Black);
                            detailSheet.Cells[1, i].Style.Font.Color.SetColor(Color.White);
                        }

                        int idxCol = 1;

                        foreach (var data in childUser)
                        {
                            idxCol = 1;
                            detailSheet.Cells[rowIndex, idxCol++].Value = data.ParentNoReg;
                            detailSheet.Cells[rowIndex, idxCol++].Value = data.NoReg;
                            detailSheet.Cells[rowIndex, idxCol++].Value = data.Name;
                            detailSheet.Cells[rowIndex, idxCol++].Value = data.PostCode;
                            detailSheet.Cells[rowIndex, idxCol++].Value = data.PostName;
                            detailSheet.Cells[rowIndex, idxCol++].Value = data.JobName;
                            detailSheet.Cells[rowIndex, idxCol++].Value = data.Directorate;
                            detailSheet.Cells[rowIndex, idxCol++].Value = data.Division;
                            detailSheet.Cells[rowIndex, idxCol++].Value = data.Department;
                            detailSheet.Cells[rowIndex, idxCol++].Value = data.Section;
                            detailSheet.Cells[rowIndex, idxCol++].Value = data.Line;
                            detailSheet.Cells[rowIndex, idxCol++].Value = data.Group;
                            detailSheet.Cells[rowIndex, idxCol++].Value = data.Class;
                            rowIndex++;
                        }
                        ms = new MemoryStream(package.GetAsByteArray());

                    }

                    // EmailMessage mail;
                    if (parent.NoReg != null)
                    {

                        var content = template.Render(new
                        {
                            Names = parent.Name,
                            Module = generalCategory.Name,
                            FormTitle = templateSummary.Title,
                            Year = now.Year
                        });
                        var toAddress = new List<string>();
                        toAddress.Add(parent.Email);

                        EmailMessage mail = new EmailMessage
                        {
                            From = templateSummary.MailFrom,
                            Subject = templateSummary.Subject,
                            Body = content,
                            IsBodyHtml = true,

                        };
                        mail.ToAddresses.Add(parent.Email);

                        //attach the excel file to the message
                        mail.Attachments.Add(new Attachment(ms, fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
                        await emailManager.SendAsync(mail).ConfigureAwait(false);
                    }
                    //cleanup the memorystream
                    ms.Dispose();
                }

                foreach (var user in dataUser)
                {
                    var template = Template.Parse(templateReminder.MailContent);
                    var subjectTemplate = Template.Parse(templateReminder.Subject);

                    var content = template.Render(new
                    {
                        Names = user.Name,
                        Module = generalCategory.Name,
                        FormTitle = templateReminder.Title,
                        Year = now.Year
                    });
                    var recipients = user.Email;
                    await emailManager.SendAsync(templateReminder.MailFrom, templateReminder.Subject, content, recipients).ConfigureAwait(false);
                }
            }
            else
            {
                Console.WriteLine($"Today is not the reminder date. The reminder date is: {reminderDate:dd/MM/yyyy}");
                Console.WriteLine("No emails to send yet");
            }
        }

            public async Task SendGetBpkbReminderAsync()
            {
            var now = DateTime.Now;
            var configService = new ConfigService(UnitOfWork);
            var userService = new UserService(UnitOfWork);
            var approvalService = new ApprovalService(UnitOfWork, null, null);
            var emailManager = CreateEmailManager();
            var appUrl = configService.GetConfig("Application.Url")?.ConfigValue;
            var url = $"{appUrl}/core/home/tasks";
            var dataBpkbCop = approvalService.GetCompleteRequestDetails("return-bpkb-cop");
            var dataInprogressBpkbCop = approvalService.GetInprogressRequestDetails("return-bpkb-cop");
            var dataUser = userService.GetActiveUsers();
            var dataUserHR = userService.GetUsersByRole(BuiltInRoles.Administrator);
            int exp = Convert.ToInt16(configService.GetConfig("Get.Bpkb")?.ConfigValue); 
            
            var emailTemplate = EmailTemplateRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.MailKey == MailTemplate.GetBpkb);

            Assert.IsNotNull(emailTemplate, nameof(emailTemplate));

            var template = Template.Parse(emailTemplate.MailContent);
            var subjectTemplate = Template.Parse(emailTemplate.Subject);

            foreach (var item in dataBpkbCop)
            {
                var obj = JsonConvert.DeserializeObject<ReturnBpkbCOPViewModel>(item.ObjectValue);

                if (obj.ReturnDate.HasValue && DateTime.Now.Date == obj.ReturnDate.Value.AddDays(-exp).Date)
                {
                    var currUser = dataUser.FirstOrDefault(x => x.NoReg == item.CreatedBy);
                    var content = template.Render(new
                    {
                        names = currUser.Name,
                        year = now.Year,
                        BpkbNumber = obj.BPKBNo,
                        DateReturn = obj.ReturnDate?.ToString("dd MMM yyyy")
                    });
                    var recipients = currUser.Email;
                    await emailManager.SendAsync(emailTemplate.MailFrom, emailTemplate.Subject, content, recipients, cc: dataUserHR.FirstOrDefault()?.Email).ConfigureAwait(false);
                }

               
            }

            foreach (var item in dataInprogressBpkbCop)
            {
                var obj = JsonConvert.DeserializeObject<ReturnBpkbCOPViewModel>(item.ObjectValue);
                if (obj.ReturnDate > DateTime.Now)
                {
                    var currUser = dataUser.FirstOrDefault(x => x.NoReg == item.CreatedBy);
                    var content = template.Render(new
                    {
                        names = currUser.Name,
                        year = now.Year,
                        BpkbNumber = obj.BPKBNo,
                        DateReturn = obj.ReturnDate?.ToString("dd MMM yyyy")
                    });
                    var recipients = currUser.Email;
                    await emailManager.SendAsync(emailTemplate.MailFrom, emailTemplate.Subject, content, recipients, cc: dataUserHR.FirstOrDefault()?.Email).ConfigureAwait(false);
                }
            }


        }
        /*
        public async Task SendWeeklyWfhPlanningApproval()
        {
            var now = DateTime.Now;
            var configService = new ConfigService(UnitOfWork);
            var userService = new UserService(UnitOfWork);
            var approvalService = new ApprovalService(UnitOfWork, null, null);
            var emailManager = CreateEmailManager();
            var appUrl = configService.GetConfig("Application.Url")?.ConfigValue;
            var url = $"{appUrl}/core/home/tasks";
            var dataBpkbCop = approvalService.GetCompleteRequestDetails("weekly-wfh-planning");
            var dataInprogressBpkbCop = approvalService.GetInprogressRequestDetails("weekly-wfh-planning");
            var dataUser = userService.GetActiveUsers();
            var dataUserHR = userService.GetUsersByRole(BuiltInRoles.Administrator);
            int exp = Convert.ToInt16(configService.GetConfig("Get.Bpkb")?.ConfigValue);

            var emailTemplate = EmailTemplateRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.MailKey == MailTemplate.GetBpkb);

            Assert.IsNotNull(emailTemplate, nameof(emailTemplate));

            var template = Template.Parse(emailTemplate.MailContent);
            var subjectTemplate = Template.Parse(emailTemplate.Subject);

            foreach (var item in dataBpkbCop)
            {
                var obj = JsonConvert.DeserializeObject<ReturnBpkbCOPViewModel>(item.ObjectValue);

                if (obj.ReturnDate.HasValue && DateTime.Now.Date == obj.ReturnDate.Value.AddDays(-exp).Date)
                {
                    var currUser = dataUser.FirstOrDefault(x => x.NoReg == item.CreatedBy);
                    var content = template.Render(new
                    {
                        names = currUser.Name,
                        year = now.Year,
                        BpkbNumber = obj.BPKBNo,
                        DateReturn = obj.ReturnDate?.ToString("dd MMM yyyy")
                    });
                    var recipients = currUser.Email;
                    await emailManager.SendAsync(emailTemplate.MailFrom, emailTemplate.Subject, content, recipients, cc: dataUserHR.FirstOrDefault()?.Email).ConfigureAwait(false);
                }


            }

            foreach (var item in dataInprogressBpkbCop)
            {
                var obj = JsonConvert.DeserializeObject<ReturnBpkbCOPViewModel>(item.ObjectValue);
                if (obj.ReturnDate > DateTime.Now)
                {
                    var currUser = dataUser.FirstOrDefault(x => x.NoReg == item.CreatedBy);
                    var content = template.Render(new
                    {
                        names = currUser.Name,
                        year = now.Year,
                        BpkbNumber = obj.BPKBNo,
                        DateReturn = obj.ReturnDate?.ToString("dd MMM yyyy")
                    });
                    var recipients = currUser.Email;
                    await emailManager.SendAsync(emailTemplate.MailFrom, emailTemplate.Subject, content, recipients, cc: dataUserHR.FirstOrDefault()?.Email).ConfigureAwait(false);
                }
            }


        }*/

        public async Task SendSheNotifAsync(List<PersonalDataBpjs> bpjs, List<PersonalDataInsurance> insurance)
        {
            var now = DateTime.Now;
            var personalDataService = new PersonalDataService(UnitOfWork, null);
            var userService = new UserService(UnitOfWork);
            var configService = new ConfigService(UnitOfWork);

            var emailManager = CreateEmailManager();

            var emailTemplate = EmailTemplateRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.MailKey == MailTemplate.SheUpdateMember);

            Assert.IsNotNull(emailTemplate, nameof(emailTemplate));

            var template = Template.Parse(emailTemplate.MailContent);
            var subjectTemplate = Template.Parse(emailTemplate.Subject);

            var dataUser = userService.GetActiveUsers();
            var dataUserSHE = userService.GetUsersByRole(BuiltInRoles.She);
            var configFamilyType = configService.GetGeneralCategories("familyTypeCode");

            foreach (var item in bpjs)
            {
                var currUser = dataUser.FirstOrDefault(x => x.NoReg == item.NoReg);
                if (currUser == null)
                    continue;
                //var dataFamily = personalDataService.GetFamilyMember(item.FamilyMemberId);
                //var dataFamilyDetail = personalDataService.GetFamilyMemberDetail(item.FamilyMemberId);
                var dataFamily = item.FamilyMemberId != null
                    ? personalDataService.GetFamilyMember(item.FamilyMemberId.Value)
                    : null;

                var dataFamilyDetail = item.FamilyMemberId != null
                    ? personalDataService.GetFamilyMemberDetail(item.FamilyMemberId.Value)
                    : null;

                string name = dataFamilyDetail.Name;
                char karakterPembagi = '-';
                string[] value =  name.Split(karakterPembagi);
                string familyname = value[0];


                var content = template.Render(new
                {
                    names = currUser.Name,
                    document = "BPJS",
                    familyName = familyname,
                    familyType = configFamilyType.FirstOrDefault(x => x.Code == dataFamily.FamilyTypeCode).Name,
                    karyawanName = currUser.Name,
                    noreg = currUser.NoReg,
                    year = now.Year
                });
                var recipients = dataUserSHE.FirstOrDefault().Email;
                //await emailManager.SendAsync(emailTemplate.MailFrom, emailTemplate.Subject, content, "yudha.satria.0890@gmail.com");
                await emailManager.SendAsync(emailTemplate.MailFrom, emailTemplate.Subject, content, recipients).ConfigureAwait(false);
                //emailManager.SendAsync(emailTemplate.MailFrom, emailTemplate.Subject, content, recipients);
            }

            foreach (var item in insurance)
            {
                var currUser = dataUser.FirstOrDefault(x => x.NoReg == item.NoReg);
                var dataFamily = personalDataService.GetFamilyMember(item.FamilyMemberId.Value);
                var dataFamilyDetail = personalDataService.GetFamilyMemberDetail(item.FamilyMemberId.Value);

                string name = dataFamilyDetail.Name;
                char karakterPembagi = '-';
                string[] value = name.Split(karakterPembagi);
                string familyname = value[0];

                var content = template.Render(new
                {
                    names = currUser.Name,
                    document = "ASURANSI",
                    familyName = familyname,
                    familyType = configFamilyType.FirstOrDefault(x => x.Code == dataFamily.FamilyTypeCode).Name,
                    karyawanName = currUser.Name,
                    noreg = currUser.NoReg,
                    year = now.Year
                });
                var recipients = dataUserSHE.FirstOrDefault().Email;
                await emailManager.SendAsync(emailTemplate.MailFrom, emailTemplate.Subject, content, recipients).ConfigureAwait(false);
                //emailManager.SendAsync(emailTemplate.MailFrom, emailTemplate.Subject, content, recipients);
            }
        }

        /// <summary>
        /// Sending email queue asynchronously
        /// </summary>
        /// <returns>Task Object</returns>
        public async Task SendQueueAsync()
        {
            var now = DateTime.Now.Date;
            var emailManager = CreateEmailManager();
            var queues = MailQueueRepository.Fetch().Where(x => x.MailStatusCode == MailStatus.Pending && x.CreatedOn >= now).ToList();
            var logService = new LogService(UnitOfWork);
            var messages = new List<string>();

            foreach (var queue in queues)
            {
                try
                {
                    if (queue.ScheduleTime.HasValue && queue.ScheduleTime > now) continue;

                    await emailManager.SendAsync(queue.MailFrom, queue.MailSubject, queue.MailContent, queue.Recipients, queue.CC, queue.Bcc).ConfigureAwait(false);

                    queue.MailStatusCode = MailStatus.Sent;
                }
                catch(Exception ex)
                {
                    messages.Add(ex.InnerException != null ? ex.InnerException.Message : ex.Message);

                    queue.RetryCount++;
                }
            }

            logService.Logs(ApplicationConstants.LogType.Error, "system", "localhost", string.Empty, "EmailService", messages);

            UnitOfWork.SaveChanges();
        }

        public int UpdateOutstandingRoutingApprovalByOC()
        {
            int totalFailure = 0;
            var routingApprovalDatas = UnitOfWork.UspQuery<UpdateRoutingApprovalStoredEntity>()
                .ToList();

            var _configService = new ConfigService(UnitOfWork);
            var _userService = new UserService(UnitOfWork);
            // Get and set application url from configuration.
            var applicationUrl = _configService.GetConfigValue<string>(Configurations.ApplicationUrl);

            foreach (var routingApprovalData in routingApprovalDatas)
            {
                // Get list of HR training administrator emails.
                //var hrTrainingAdministratorEmails = GetEmailFromRoles(BuiltInRoles.TrainingAdministrator);

                var recipients = _userService.GetByUsername(routingApprovalData.UserName).Email;
                // Create new email data.
                var data = new
                {

                    doc_number = routingApprovalData.DocumentNumber,
                    //routingApprovalData.Id,
                    //routingApprovalData.Noreg,
                    name = routingApprovalData.Name,
                    //routingApprovalData.JobCode,
                    //routingApprovalData.JobName,
                    //routingApprovalData.PostCode,
                    //routingApprovalData.PostName,
                    //routingApprovalData.ApprovalLevel,
                    //routingApprovalData.DiffNoreg,
                    diff_name = routingApprovalData.DiffName,
                    //routingApprovalData.DiffJobCode,
                    //routingApprovalData.DiffJobName,
                    //routingApprovalData.DiffPostCode,
                    //routingApprovalData.DiffPostName,
                    //routingApprovalData.UserName,
                    //routingApprovalData.MovementType,
                    url = applicationUrl+routingApprovalData.url

                };

                // If failed when sending email then count the total failure.
                if (!SendEmail("my-task-replacement", recipients, data))
                //if (!SendEmail("pending-tasks", recipients, data))
                {
                    // Count the total failure.
                    totalFailure++;
                }
            }


            // Return asynchronous operation.
            //return Task.FromResult(totalFailure);
            return totalFailure;
        }

        public async Task sendEmailReminderOHSAsync()
        {
            var AreaServices = new SafetyIncidentService(UnitOfWork);
            var userService = new UserService(UnitOfWork);
            var coreService = new CoreService(UnitOfWork);
            var configService = new ConfigService(UnitOfWork);

            var activeUser = userService.GetActiveUsers();

            // Get email template
            var emailTemplate = coreService.GetEmailTemplate("ohs-reminder-email");
            Assert.IsNotNull(emailTemplate, "emailTemplate");

            var mailSubject = emailTemplate.Subject;
            var mailFrom = emailTemplate.MailFrom;
            var template = Template.Parse(emailTemplate.MailContent);
            var mailManager = CreateEmailManager();

            // Ambil data email dari repository
            var DataEmail = ReminderEmailAreaActivityViewReadOnlyRepository.Fetch()
                .AsNoTracking()
                .Select(x => x.Username)  // Pilih hanya Username terlebih dahulu
                .Distinct() // Sekarang DISTINCT bekerja hanya pada username
                .ToList();

            foreach (var username in DataEmail)
            {
                Console.WriteLine($"Processing user: {username}");

                // Dapatkan daftar semua pengguna dengan username ini
                var users = userService.GetByUserNames(new List<string> { username });

                if (!users.Any())
                {
                    Console.WriteLine($"Skipping email for {username}: No users found.");
                    continue;
                }

                // Ambil semua email yang valid dari pengguna ini
                var recipientEmails = users
                    .Where(u => !string.IsNullOrEmpty(u.Email)) // Hanya ambil email yang tidak kosong
                    .Select(u => u.Email)
                    .Distinct() // Pastikan email tidak duplikat
                    .ToList();

                if (!recipientEmails.Any())
                {
                    Console.WriteLine($"Skipping email for {username}: No valid email found.");
                    continue;
                }

                var Names = username;
                var baseUrlConfigValue = configService.GetConfig("Application.LocalUrl")?.ConfigValue ?? "https://default-url.com";
                var UrlLink = $"{baseUrlConfigValue}/ohs/areaactivity/index";

                // Parse email body
                var year = DateTime.Now.Year;
                var mailContent = template.Render(new
                {
                    Module = "core",
                    From = "OHS-Admin",
                    Names = Names,
                    FormTitle = "Reminder Email Input Area Activity",
                    Year = year,
                    url = UrlLink
                });

                var mailQueue = MailQueue.Create("OHSMailReminder", mailSubject, mailContent, recipientEmails, mailFrom);
                
                try
                {
                    Console.WriteLine($"EMAIL PENERIMA: {string.Join(",", recipientEmails)}");
                    Console.WriteLine($"EMAIL DARI: {mailFrom}");
                    Console.WriteLine($"EMAIL SUBJECT: {mailSubject}");

                    // Kirim email ke semua pengguna dalam satu batch
                    await mailManager.SendAsync(mailFrom, mailSubject, mailContent, string.Join(",", recipientEmails));
                    mailQueue.MailStatusCode = MailStatus.Sent;

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send email: {ex.Message} \nStackTrace: {ex.StackTrace}");
                    mailQueue.ExceptionMessage = $"Failed to send email: {ex.ToString()} \nStackTrace: {ex.StackTrace}";
                    mailQueue.MailStatusCode = MailStatus.Failed;
                    //_logger.LogError(ex, $"Error sending OHS reminder email for {username}");
                }

                coreService.CreateMailQueue(mailQueue);
            }
        }

    }
}

using TAMHR.ESS.Domain;
using Agit.Domain.UnitOfWork;
using Agit.Domain;
using Agit.Domain.Repository;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using Agit.Common.Extensions;
using Newtonsoft.Json;
using Agit.Common.Email;
using Scriban;
using TAMHR.ESS.Infrastructure.ViewModels;
using Agit.Common;
using System.Threading.Tasks;
using System.Text;
using System.Net.Mail;


namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class SafetyIncidentService : DomainServiceBase
    {

        #region Domain Repository
        /// <summary>
        /// Field that hold email service object.
        /// </summary>
        private readonly EmailService _emailService;
        /// <summary>
        /// Field that hold core service object.
        /// </summary>
        private readonly CoreService _coreService;
        /// <summary>
        /// Field that hold config service object.
        /// </summary>
        private readonly ConfigService _configService;
        /// <summary>
        /// Field that hold user service object.
        /// </summary>
        private readonly UserService _userService;
        protected IRepository<SafetyIncidentModel> SafetyIncidentTBRepository => UnitOfWork.GetRepository<SafetyIncidentModel>();
        protected IReadonlyRepository<SafetyIncidentModel> SafetyIncidentReadOnlyTBRepository => UnitOfWork.GetRepository<SafetyIncidentModel>();
        protected IRepository<SafetyIncidentViewModel> SafetyIncidentViewRepository => UnitOfWork.GetRepository<SafetyIncidentViewModel>();
        protected IReadonlyRepository<SafetyIncidentViewModel> SafetyIncidentReadOnlyViewRepository => UnitOfWork.GetRepository<SafetyIncidentViewModel>();
        
        // Repository untuk Role Akses berdasarkan NoReg
        protected IRepository<RoleAreaActivitytModel> RoleAreaActivityRepository => UnitOfWork.GetRepository<RoleAreaActivitytModel>();
        protected IReadonlyRepository<RoleAreaActivitytModel> RoleAreaActivityReadOnlyRepository => UnitOfWork.GetRepository<RoleAreaActivitytModel>();
        protected IRepository<RoleAreaActivitytViewModel> RoleAreaActivityViewRepository => UnitOfWork.GetRepository<RoleAreaActivitytViewModel>();
        protected IReadonlyRepository<RoleAreaActivitytViewModel> RoleAreaActivityViewReadOnlyRepository => UnitOfWork.GetRepository<RoleAreaActivitytViewModel>();
        protected IReadonlyRepository<ReminderEmailAreaActivitytViewModel> ReminderEmailAreaActivityViewReadOnlyRepository => UnitOfWork.GetRepository<ReminderEmailAreaActivitytViewModel>();
        protected IReadonlyRepository<DashboardListAreaActivityViewModel> DashboardListAreaActivityReadOnlyViewRepository => UnitOfWork.GetRepository<DashboardListAreaActivityViewModel>();

        protected IRepository<User> UserRepository => UnitOfWork.GetRepository<User>();
        #endregion

        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for bank entity.
        /// </summary>
        private readonly string[] Properties = new[] {
            "IncidentTypeCode",
            "IncidentDescription",
            "IncidentDate",
            "DivisionCode",
            "DivisionName",
            "AreaId",
            "AreaName",
            "Subject",
            "Remark",
            "PropertyType",
            "TotalLoss",
            "AccidentType",
            "TotalVictim",
            "LossTime",
            "Attachment",
            "RowStatus",
            "DeletedOn"
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>

        public SafetyIncidentService(
            CoreService coreService,
            ConfigService configService,
            UserService userService,
            EmailService emailService,
            IUnitOfWork unitOfWork) : base(unitOfWork) {

            // Get and set core service object from DI container.
            _coreService = coreService;
            // Get and set user service object from DI container.
            _userService = userService;
            // Get and set email service object from DI container.
            _emailService = emailService;
            // Get and set config service object from DI container.
            _configService = configService;

        }

        public SafetyIncidentService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
        #endregion

        public IQueryable<SafetyIncidentViewModel> GetQuery()
        {
            return SafetyIncidentReadOnlyViewRepository.Fetch()
                .Where(x => x.RowStatus == true)
                .AsNoTracking();
        }

        public IQueryable<SafetyIncidentViewModel> GetSafetyAccident(string incidentTypeCode, string periode = null, string divisionCode = null, string areaId = null, string NoReg = null)
        {
            if (periode == null)
            {
                periode = string.Empty;
            }
            if (divisionCode == null)
            {
                divisionCode = string.Empty;
            }
            if (areaId == null)
            {
                areaId = string.Empty;
            }

            // Mulai dari query dasar
            var query = GetQuery().Where(x => x.IncidentTypeCode == incidentTypeCode);

            // Tambahkan filter dinamis berdasarkan parameter yang diberikan
            if (!string.IsNullOrEmpty(periode))
            {
                // Parse periode menjadi bulan dan tahun
                var periodeDate = DateTime.ParseExact(periode, "yyyy-MM", null);
                var startDate = new DateTime(periodeDate.Year, periodeDate.Month, 1); // Awal bulan
                var endDate = startDate.AddMonths(1).AddDays(-1); // Akhir bulan

                // Filter IncidentDate dalam rentang bulan dan tahun yang diberikan
                query = query.Where(x => x.IncidentDate >= startDate && x.IncidentDate <= endDate);
            }

            if (!string.IsNullOrEmpty(divisionCode))
            {
                query = query.Where(x => x.DivisionCode == divisionCode);
            }

            if (!string.IsNullOrEmpty(areaId))
            {
                query = query.Where(x => x.AreaId == areaId);
            }
            
            // **Filter berdasarkan hak akses NoReg**
            if (!string.IsNullOrEmpty(NoReg))
            {
                Console.WriteLine($"Filtering by NoReg: {NoReg}");

                // Ambil daftar role akses dari tabel RoleAreaActivity berdasarkan NoReg
                var userAccess = RoleAreaActivityReadOnlyRepository.Fetch()
                    .Where(a => a.NoReg == NoReg && a.IsActive == true && a.RowStatus == true)
                    .Select(a => new { a.DivisionCode, a.AreaId })
                    .ToList(); // Eksekusi query ke database

                var allowedAreaIds = userAccess
                    .Select(a => a.AreaId.ToString()) // Konversi Guid ke string untuk perbandingan
                    .Distinct()
                    .ToList();

                // Terapkan filter jika ada akses yang ditemukan
                if (allowedAreaIds.Any())
                {
                    //query = query.Where(e =>
                    //    allowedAreaIds.Contains(e.AreaId)
                    //);
                    var allowedAreaQueryable = allowedAreaIds.AsQueryable();

                    query = query.Join(
                        allowedAreaQueryable,
                        a => a.DivisionCode,
                        id => id,
                        (a, id) => a
                    );
                }
            }

            return query;
        }


        public IEnumerable<SafetyIncidentViewModel> Gets(string divisionCode = null, string areaId = null, string periode = null, string NoReg = null)
        {
            var query = GetQuery();

            // Jika parameter tidak null atau tidak kosong, lakukan filter
            if (!string.IsNullOrEmpty(divisionCode))
            {
                Console.WriteLine($"Division: {divisionCode}");
                query = query.Where(e => e.DivisionCode == divisionCode);
            }

            if (!string.IsNullOrEmpty(areaId))
            {
                Console.WriteLine($"areaId: {areaId}");
                query = query.Where(e => e.AreaId == areaId);
            }

            if (!string.IsNullOrEmpty(periode))
            {
                Console.WriteLine($"periode: {periode}");
                query = query.Where(e => e.CreatedOnSearch == periode);
            }

            // **Filter berdasarkan hak akses NoReg**
            if (!string.IsNullOrEmpty(NoReg))
            {
                Console.WriteLine($"Filtering by NoReg: {NoReg}");

                // Ambil daftar role akses dari tabel RoleAreaActivity berdasarkan NoReg
                var userAccess = RoleAreaActivityReadOnlyRepository.Fetch()
                    .Where(a => a.NoReg == NoReg && a.IsActive == true && a.RowStatus == true)
                    .Select(a => new { a.DivisionCode, a.AreaId })
                    .ToList(); // Eksekusi query ke database

                var allowedAreaIds = userAccess
                    .Select(a => a.AreaId.ToString()) // Konversi Guid ke string untuk perbandingan
                    .Distinct()
                    .ToList();

                // Terapkan filter jika ada akses yang ditemukan
                if (allowedAreaIds.Any())
                {
                    //query = query.Where(e =>
                    //    allowedAreaIds.Contains(e.AreaId)
                    //);
                    var allowedAreaQueryable = allowedAreaIds.AsQueryable();

                    query = query.Join(
                        allowedAreaQueryable,
                        a => a.DivisionCode,
                        id => id,
                        (a, id) => a
                    );
                }
            }

            return query;
        }

        public SafetyIncidentViewModel GetById(Guid id)
        {
            return SafetyIncidentReadOnlyViewRepository.Fetch().AsNoTracking().Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }
        public void InsertSafetyIncident(SafetyIncidentModel model, string Noreg = null, string Username = null)
        {
            try
            {
                // Hanya masukkan properti yang valid dan diperlukan
                var validProperties = Properties.Where(p => model.GetType().GetProperty(p)?.GetValue(model, null) != null).ToArray();
                // Validasi setiap properti
                foreach (var property in validProperties)
                {
                    var value = model.GetType().GetProperty(property)?.GetValue(model, null);
                    if (value == null)
                    {
                        Console.WriteLine($"Property '{property}' is null.");
                    }
                    else {
                        Console.WriteLine($"Property amannnn");
                    }
                }
                SafetyIncidentTBRepository.Add(model);
                UnitOfWork.SaveChanges();
                Console.WriteLine("Data berhasil disimpan.");

                SafetyIncidentReadOnlyViewRepository.Fetch()
                .Where(x => x.RowStatus == true)
                .AsNoTracking();

                SendPushNotification(model, Noreg, Username);
                SendEmail(model, Noreg, Username);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public void UpdateSafetyIncident(SafetyIncidentModel model)
        {
            try
            {
                // Pastikan model tidak null
                if (model == null)
                {
                    Console.WriteLine("Model yang diterima adalah null.");
                    throw new ArgumentNullException(nameof(model), "Model tidak boleh null.");
                }

                // Ambil entitas yang ada di database
                var existingEntity = SafetyIncidentTBRepository.Fetch().AsNoTracking().Where(x => x.Id == model.Id).FirstOrDefaultIfEmpty();
                if (existingEntity == null)
                {
                    Console.WriteLine($"Entitas dengan ID {model.Id} tidak ditemukan.");
                    throw new KeyNotFoundException("Data tidak ditemukan.");
                }

                // Update properti yang diizinkan
                foreach (var property in Properties)
                {
                    var propertyInfo = model.GetType().GetProperty(property);
                    if (propertyInfo == null)
                    {
                        Console.WriteLine($"Property {property} tidak ditemukan dalam model.");
                        continue; // Abaikan properti yang tidak ditemukan
                    }

                    var newValue = propertyInfo.GetValue(model, null);
                    if (newValue == null)
                    {
                        Console.WriteLine($"Nilai baru untuk properti {property} adalah null, dilewati.");
                        continue;
                    }

                    var existingPropertyInfo = existingEntity.GetType().GetProperty(property);
                    if (existingPropertyInfo == null)
                    {
                        Console.WriteLine($"Property {property} tidak ditemukan dalam existingEntity.");
                        continue; // Abaikan properti yang tidak ditemukan
                    }

                    // Set properti pada entitas
                    existingPropertyInfo.SetValue(existingEntity, newValue);
                }

                Console.WriteLine($"Model setelah update properti: {JsonConvert.SerializeObject(existingEntity)}");
                Console.WriteLine($"Properties: {JsonConvert.SerializeObject(Properties)}");

                // Upsert entitas
                if (existingEntity == null)
                {
                    Console.WriteLine("Entitas existingEntity adalah null setelah update properti.");
                    throw new Exception("Entitas existingEntity tidak valid.");
                }

                SafetyIncidentTBRepository.Upsert<Guid>(model, Properties);
                Console.WriteLine("Upsert berhasil dipanggil.");

                // Save changes
                var result = UnitOfWork.SaveChanges();
                Console.WriteLine($"SaveChanges Result: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public void Delete(Guid id)
        {
            var item = SafetyIncidentTBRepository.Fetch()
                .Where(x => x.Id == id)
                .FirstOrDefault();

            if (item != null)
            {
                item.RowStatus = false;
                item.DeletedOn = DateTime.Now;
                SafetyIncidentTBRepository.Upsert<Guid>(item, Properties);
                UnitOfWork.SaveChanges();
            }
        }

        public void SendPushNotification(SafetyIncidentModel model, string Noreg = null, string Username = null)
        {
            if (model == null)
            {
                Console.WriteLine("Error: Model is null!");
                return;
            }

            var UsernameFor = RoleAreaActivityViewReadOnlyRepository.Fetch()
                .Where(x => x.AccessKey == "ohs-admin")
                .AsNoTracking()
                .Select(x => x.Username)
                .Distinct()
                .ToList();

            Console.WriteLine($"UsernameFor >> {(UsernameFor.Any() ? string.Join(", ", UsernameFor) : "No usernames found")}");

            if (UsernameFor == null || !UsernameFor.Any())
            {
                Console.WriteLine("Error: UsernameFor is empty. Skipping user retrieval.");
                return;
            }

            // Get list of users by list of usernames.
            var users = _userService.GetByUserNames(UsernameFor);

            if (users == null || !users.Any())
            {
                Console.WriteLine("Error: No users found for the given usernames.");
                return;
            }

            Console.WriteLine($"Users found >> {string.Join(", ", users.Select(u => u?.Username ?? "NULL USER"))}");

            var notifications = new List<Notification>();

            foreach (var user in users)
            {
                if (user == null)
                {
                    Console.WriteLine("Error: User object is null in foreach loop!");
                    continue;
                }

                if (string.IsNullOrEmpty(user.NoReg))
                {
                    Console.WriteLine($"Warning: user.NoReg is null or empty for user {user.Username}");
                    continue;
                }

                if (string.IsNullOrEmpty(Noreg))
                {
                    Console.WriteLine($"Warning: Noreg is null or empty. Skipping user {user.Username}.");
                    continue;
                }

                Dictionary<string, string> replacements = new Dictionary<string, string>
                {
                    { "near_miss", "Near Miss" },
                    { "property_damage", "Property Damage" },
                    { "working_accident", "Working Accident" },
                    { "traffic_accident", "Traffic Accident" }
                };

                string incidentType = model.IncidentTypeCode;
                if (replacements.ContainsKey(incidentType))
                {
                    incidentType = replacements[incidentType];
                }

                var notificationMessage = $"Ada laporan Incident/Accident baru dari PIC Area {model.AreaName} dengan Kategori <b>{incidentType}</b> dan berikut link nya <a class='' data-trigger='handler' data-handler='redirectHandler' data-url='~/ohs/areaactivity/listAreaActivity?Periode=2025-02&DivisionCode={model.DivisionCode}&DivisionName={model.DivisionName}&AreaId={model.AreaId}'><b>Link Form Accident/Incident</b></a>";

                Console.WriteLine($"NoregFrom >> {Noreg}");
                Console.WriteLine($"NoregFor >> {user.NoReg}");
                Console.WriteLine($"notificationMessage >> {notificationMessage}");

                try
                {
                    var notification = Notification.Create(Noreg, user.NoReg, notificationMessage, "notice");

                    if (notification == null)
                    {
                        Console.WriteLine("Error: Notification.Create returned null.");
                        continue;
                    }

                    notifications.Add(notification);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error saat membuat notifikasi: {ex.Message}");
                    continue;
                }
            }

            Console.WriteLine($"Total notifications created: {notifications.Count}");

            if (!notifications.Any())
            {
                Console.WriteLine("Warning: No notifications created. Skipping CreateNotifications.");
                return;
            }

            if (_coreService == null)
            {
                Console.WriteLine("Error: _coreService is null. Skipping notification sending.");
                return;
            }

            try
            {
                _coreService.CreateNotifications(notifications);
                Console.WriteLine("Notifications successfully sent.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saat mengirim notifikasi: {ex.Message}");
            }
        }

        public async Task<SafetyIncidentModel> SendEmail(SafetyIncidentModel model, string Noreg = null, string Username = null)
        {
            // Get notice email template.
            var emailTemplate = _coreService.GetEmailTemplate("ohs-area-activity");

            // Throw an exception if email template was not found.
            Assert.IsNotNull(emailTemplate, "emailTemplate");

            // Get mail subject and sender.
            var mailSubject = emailTemplate.Subject;
            var mailFrom = emailTemplate.MailFrom;

            // Parse email template.
            var template = Template.Parse(emailTemplate.MailContent);
            var mailManager = _emailService.CreateEmailManager();

            // Get list of admin users for "ohs-admin" role.
            var UsernameFor = RoleAreaActivityViewReadOnlyRepository.Fetch()
                .Where(x => x.AccessKey == "ohs-admin")
                .AsNoTracking()
                .Select(x => x.Username)
                .Distinct()
                .ToList();

            Console.WriteLine($"UsernameFor >> {(UsernameFor.Any() ? string.Join(", ", UsernameFor) : "No usernames found")}");

            var users = _userService.GetByUserNames(UsernameFor);

            string Area = model.AreaName.Split('-')[0].Trim();  // Hasil: "Ngoro"

            var fromMessage = $"SHE AREA {model.DivisionName} - {Area}";

            // Mapping incident type
            Dictionary<string, string> replacements = new Dictionary<string, string>
                {
                    { "near_miss", "Near Miss" },
                    { "property_damage", "Property Damage" },
                    { "working_accident", "Working Accident" },
                    { "traffic_accident", "Traffic Accident" }
                };

            string incidentType = model.IncidentTypeCode;
            if (replacements.ContainsKey(incidentType))
            {
                incidentType = replacements[incidentType];
            }

            var baseUrlConfigValue = _configService.GetConfig("Application.LocalUrl")?.ConfigValue;
            // Generate URL link
            var UrlLink = $"{baseUrlConfigValue}/ohs/areaactivity/listAreaActivity?Periode={model.IncidentDate:yyyy-MM}&DivisionCode={model.DivisionCode}&DivisionName={model.DivisionName}&AreaId={model.AreaId}";

            // Build HTML Table
            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine("<table border=\"1\" cellspacing=\"0\" cellpadding=\"5\" width=\"100%\" style=\"border-collapse: collapse; font-family: Arial, sans-serif; font-size: 12px;\">");
            messageBuilder.AppendLine("<thead><tr style=\"background-color: #f2f2f2; text-align: center;\">");

            // Header dengan lebar yang sudah diatur
            messageBuilder.AppendLine("<th style=\"width: 15%; min-width: 100px;\">Type Accident/Incident</th>");
            messageBuilder.AppendLine("<th style=\"width: 25%; min-width: 150px;\">Incident Description</th>");
            messageBuilder.AppendLine("<th style=\"width: 10%; min-width: 80px;\">Division</th>");
            messageBuilder.AppendLine("<th style=\"width: 10%; min-width: 80px;\">Area</th>");
            messageBuilder.AppendLine("<th style=\"width: 8%; min-width: 70px;\">Date</th>");
            messageBuilder.AppendLine("<th style=\"width: 7%; min-width: 60px;\">Time</th>");

            if (model.IncidentTypeCode == "property_damage")
            {
                messageBuilder.AppendLine("<th style=\"width: 12%; min-width: 120px;\">Property Type</th>");
            }

            if (model.IncidentTypeCode != "property_damage")
            {
                messageBuilder.AppendLine("<th style=\"width: 15%; min-width: 100px;\">Subject</th>");
            }

            if (model.IncidentTypeCode == "traffic_accident" || model.IncidentTypeCode == "working_accident")
            {
                messageBuilder.AppendLine("<th style=\"width: 10%; min-width: 80px;\">Loss Time (Day)</th>");
                messageBuilder.AppendLine("<th style=\"width: 10%; min-width: 80px;\">Total Victim (Person)</th>");
                messageBuilder.AppendLine("<th style=\"width: 10%; min-width: 120px;\">Accident Type</th>");
            }

            if (model.IncidentTypeCode == "property_damage" || model.IncidentTypeCode == "working_accident")
            {
                messageBuilder.AppendLine("<th style=\"width: 12%; min-width: 120px;\">Total Loss (Rupiah)</th>");
            }

            messageBuilder.AppendLine("<th style=\"width: 10%; min-width: 90px;\">Attachment</th>");
            messageBuilder.AppendLine("</tr></thead><tbody>");

            // Tambahkan data dari model
            messageBuilder.AppendLine("<tr>");
            messageBuilder.AppendLine($"<td style=\"text-align: center; word-break: break-word;\">{incidentType}</td>");
            messageBuilder.AppendLine($"<td style=\"text-align: center; word-break: break-word;\">{model.IncidentDescription}</td>");
            messageBuilder.AppendLine($"<td style=\"text-align: center; word-break: break-word;\">{model.DivisionName}</td>");
            messageBuilder.AppendLine($"<td style=\"text-align: center; word-break: break-word;\">{model.AreaName}</td>");
            messageBuilder.AppendLine($"<td style=\"text-align: center;\">{model.IncidentDate:dd/MM/yyyy}</td>");
            messageBuilder.AppendLine($"<td style=\"text-align: center;\">{model.IncidentDate:HH:mm:ss}</td>");

            if (model.IncidentTypeCode == "property_damage")
            {
                messageBuilder.AppendLine($"<td style=\"text-align: center; word-break: break-word;\">{model.PropertyType}</td>");
            }

            if (model.IncidentTypeCode != "property_damage")
            {
                messageBuilder.AppendLine($"<td style=\"text-align: center; word-break: break-word;\">{model.Subject}</td>");
            }

            if (model.IncidentTypeCode == "traffic_accident" || model.IncidentTypeCode == "working_accident")
            {
                messageBuilder.AppendLine($"<td style=\"text-align: center;\">{model.LossTime}</td>");
                messageBuilder.AppendLine($"<td style=\"text-align: center;\">{model.TotalVictim}</td>");
                messageBuilder.AppendLine($"<td style=\"text-align: center; word-break: break-word;\">{model.AccidentType}</td>");
            }

            if (model.IncidentTypeCode == "property_damage" || model.IncidentTypeCode == "working_accident")
            {
                messageBuilder.AppendLine($"<td style=\"text-align: center;\">{model.TotalLoss}</td>");
            }

            var urlDownload = $"{baseUrlConfigValue}/api/files/download?id={model.Attachment}";
            messageBuilder.AppendLine($"<td style=\"text-align: center;\"><a href=\"{urlDownload}\">link file</a></td>");

            messageBuilder.AppendLine("</tr>");
            messageBuilder.AppendLine("</tbody></table>");

            string message = messageBuilder.ToString();

            // Parse email body
            var year = DateTime.Now.Year;
            var mailContent = template.Render(new
            {
                Module = "core",
                From = fromMessage,
                Names = "Admin OHS",
                FormTitle = "Report Incident/Accident Area Activity",
                Message = message,
                Year = year,
                Url = UrlLink
            });

            try
            {
                Console.WriteLine($"EMAIL PENERIMA: {string.Join(",", users.Select(x => x.Email))}");
                Console.WriteLine($"EMAIL DARI: {mailFrom}");
                Console.WriteLine($"EMAIL SUBJECT: {mailSubject}");

                // Send email asynchronously.
                await mailManager.SendAsync(mailFrom, mailSubject, mailContent, string.Join(",", users.Select(x => x.Email)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }

            return model;
        }

        public async Task<SafetyIncidentModel> OhsEmailReminder(SafetyIncidentModel model, string Noreg = null, string Username = null)
        {
            var userService = new UserService(UnitOfWork);
            var activeUser = userService.GetActiveUsers();
            // Get notice email template.
            var emailTemplate = _coreService.GetEmailTemplate("ohs-reminder-email");

            // Throw an exception if email template was not found.
            Assert.IsNotNull(emailTemplate, "emailTemplate");

            // Get mail subject and sender.
            var mailSubject = emailTemplate.Subject;
            var mailFrom = emailTemplate.MailFrom;

            // Parse email template.
            var template = Template.Parse(emailTemplate.MailContent);
            var mailManager = _emailService.CreateEmailManager();

            // Get list of admin users for "ohs-admin" role.
            var UsernameFor = ReminderEmailAreaActivityViewReadOnlyRepository.Fetch()
                .Where(x => x.TotalRecords == "0")
                .AsNoTracking()
                .Select(x => x.Username)
                .Distinct()
                .ToList();

            Console.WriteLine($"UsernameFor >> {(UsernameFor.Any() ? string.Join(", ", UsernameFor) : "No usernames found")}");

            var users = _userService.GetByUserNames(UsernameFor);

            string Area = model.AreaName.Split('-')[0].Trim();  // Hasil: "Ngoro"

            var Names = $"SHE AREA {model.DivisionName} - {Area}";

            var baseUrlConfigValue = _configService.GetConfig("Application.LocalUrl")?.ConfigValue;
            // Generate URL link
            var UrlLink = $"{baseUrlConfigValue}/ohs/areaactivity/listAreaActivity?Periode={model.IncidentDate:yyyy-MM}&DivisionCode={model.DivisionCode}&DivisionName={model.DivisionName}&AreaId={model.AreaId}";

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

            try
            {
                Console.WriteLine($"EMAIL PENERIMA: {string.Join(",", users.Select(x => x.Email))}");
                Console.WriteLine($"EMAIL DARI: {mailFrom}");
                Console.WriteLine($"EMAIL SUBJECT: {mailSubject}");

                // Send email asynchronously.
                await mailManager.SendAsync(mailFrom, mailSubject, mailContent, string.Join(",", users.Select(x => x.Email)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }

            return model;
        }
    }
}
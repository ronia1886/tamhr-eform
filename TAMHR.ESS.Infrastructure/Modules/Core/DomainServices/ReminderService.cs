using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Helpers;
using Agit.Domain;
using Agit.Common.Extensions;
using Agit.Domain.Extensions;
using Agit.Domain.UnitOfWork;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure;
using Z.EntityFramework.Plus;
using Newtonsoft.Json;
using TAMHR.ESS.Infrastructure.ViewModels;
using Agit.Domain.Repository;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle reminder.
    /// </summary>
    public class ReminderService : DomainServiceBase
    {
        protected IRepository<DocumentRequestDetail> DocumentRequestDetailRepository => UnitOfWork.GetRepository<DocumentRequestDetail>();
        protected IRepository<PersonalData> PersonalDataRepository => UnitOfWork.GetRepository<PersonalData>();
        protected IRepository<PersonalDataCommonAttribute> PersonalDataCommonAttributeRepository => UnitOfWork.GetRepository<PersonalDataCommonAttribute>();
        protected IRepository<PersonalDataEvent> PersonalDataEventRepository => UnitOfWork.GetRepository<PersonalDataEvent>();
        protected IRepository<PersonalDataBpjs> PersonalDataBpjsRepository => UnitOfWork.GetRepository<PersonalDataBpjs>();
        protected IRepository<PersonalDataFamilyMember> PersonalDataFamilyMemberRepository => UnitOfWork.GetRepository<PersonalDataFamilyMember>();
        protected IRepository<PersonalDataInsurance> PersonalDataInsuranceRepository => UnitOfWork.GetRepository<PersonalDataInsurance>();
        

        #region Variables & Properties
        /// <summary>
        /// Config service object.
        /// </summary>
        private ConfigService _configService;

        /// <summary>
        /// Email service object.
        /// </summary>
        private EmailService _emailService;
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public ReminderService(ConfigService configService, EmailService emailService, IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
            // Get and set config service object from DI container.
            _configService = configService;

            // Get and set email service object from DI container.
            _emailService = emailService;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Send task email reminder.
        /// </summary>
        /// <returns>This asynchronous task.</returns>
        public Task<int> SendReminderSheduleVaccine()
        {
            // Create my task view repository object.
            var set = UnitOfWork.GetRepository<VaccinReportEmployeeView>();
            //cofig repositoy.
            var cf = UnitOfWork.GetRepository<Config>().Fetch();
            // Set default total failure to 0.
            var totalFailure = 0;

            var config = cf.Where(x => x.ConfigKey == "DayVaccineSchedule").AsNoTracking().FirstOrDefault();
            int h = 0;
            if (config != null)
            {
                h = Convert.ToInt32(config.ConfigValue.Trim());
            }

            for (int i = h; i >= 0; i--)
            {
                // Get and set application url from configuration.
                //var applicationUrl = _configService.GetConfigValue<string>(Configurations.ApplicationUrl);

                // Get list of task objects without object tracking.
                var p2 = set.Fetch()
                        .Where(x => x.VaccineDate1.HasValue && (DateTime.Now.Date == x.VaccineDate1.Value.AddDays(-i).Date)
                                 || x.VaccineDate2.HasValue && (DateTime.Now.Date == x.VaccineDate2.Value.AddDays(-i).Date)
                               ).AsNoTracking().ToList();
                var employee = p2.Select(x => x.EmployeeName).Distinct().ToList();

                // Loop over task objects.
                foreach (var item in p2)
                {
                    var hdr = set.Fetch()
                              .Where(x => x.EmployeeName == item.EmployeeName && x.Email != null).AsNoTracking().FirstOrDefault();
                    var user = p2.Where(x => x.EmployeeName == item.EmployeeName && x.Name == item.Name).ToList();
                   
                    var dataTable = GenerateDataTableEmailVaccineReminder(user, hdr);
                    // Create new email data.
                    var data = new
                    {
                        number = i,
                        employee_name = hdr.Name,
                        vaccine_to = hdr.VaccineTo,
                        table_data = dataTable,
                        year = DateTime.Now.Year

                        // Get and set task link.
                        //url = applicationUrl + "/login"
                    };

                    // Send email notification.
                    if (!_emailService.SendEmail("vaksin-reminder-jadwal", hdr.Email, data))
                    {
                        // Count the total failure.
                        totalFailure++;
                    }
                }
            }

            // Return asynchronous operation.
            return Task.FromResult(totalFailure);
        }

        private string GenerateDataTableEmailVaccineReminder(List<VaccinReportEmployeeView> data, VaccinReportEmployeeView entity)
        {
            //var list = UnitOfWork.GetRepository<VaccinReportEmployeeView>().Fetch().Where(x => x.EmployeeName == entity.EmployeeName)
            //    .AsNoTracking()
            //    .ToList();

            string[] colum = { "Nama", "Hubungan","Status","Vaccine Ke", "Jadwal Vaccine" ,"Nama Rumah Sakit"};
            //Table start.
            string html = "<table cellpadding='5' cellspacing='0' style='border: 1px solid #ccc;font-size: 9pt;font-family:arial'>";

            //Adding HeaderRow.
            html += "<tr>";
            foreach (var column in colum)
            {
                html += "<th style='background-color: #B8DBFD;border: 1px solid #ccc'>" + column + "</th>";
            }
            html += "</tr>";
            //Adding DataRow.
            foreach (var row in data.OrderByDescending(x => x.Class))
            {
                html += "<tr>";
                html += "<td style='width: 28.853 %; height: 18px;border: 1px solid #ccc'> <span style='font-size:10.0pt; font-family: sans-serif;'>" + row.Name + "</span></td>";
                html += "<td style='width: 28.853 %; height: 18px;border: 1px solid #ccc'> <span style='font-size:10.0pt; font-family: sans-serif;'>" + row.FamilyStatus + "</span></td>";
                html += "<td style='width: 28.853 %; height: 18px;border: 1px solid #ccc'> <span style='font-size:10.0pt; font-family: sans-serif;'>" + row.Status + "</span></td>";
                html += "<td style='width: 28.853 %; height: 18px;border: 1px solid #ccc'> <span style='font-size:10.0pt; font-family: sans-serif;'>" + row.VaccineTo + "</span></td>";
                html += "<td style='width: 28.853 %; height: 18px;border: 1px solid #ccc'> <span style='font-size:10.0pt; font-family: sans-serif;'>"; html +=  row.VaccineTo.ToString() == "2" ? (row.VaccineDate2.HasValue ? row.VaccineDate2.Value.ToString("dd MMM yyyy") : "") : (row.VaccineDate1.HasValue?  row.VaccineDate1.Value.ToString("dd MMM yyyy") : "") + "</span></td>";
                html += "<td style='width: 28.853 %; height: 18px;border: 1px solid #ccc'> <span style='font-size:10.0pt; font-family: sans-serif;'>"; html += row.VaccineTo.ToString() == "2" ? row.VaccineHospital2 : row.VaccineHospital1 + "</span></td>";
                html += "</tr>";
            }
            //Table end.
            html += "</table>";
            return html;
        }

        public Task<int> ScheduleUpdateSHAStatusVaccine() {
            // Set default total failure to 0.
            var totalFailure = 0;
            UnitOfWork.UspQuery("dbo.SP_VACCINE_UPDATE_SHASTATUS");
            var result = UnitOfWork.SaveChanges() > 1;

            // Return asynchronous operation.
            if (result)
            {
                totalFailure = 1;
            }
            return Task.FromResult(totalFailure);
        }
        public Task<int> SheduleUpdateStatusVaccine()
        {
            // Set default total failure to 0.
            var totalFailure = 0;
            var set = UnitOfWork.GetRepository<Vaccine>();
            var set2 = UnitOfWork.GetRepository<Vaccine>();
            set.Fetch().Where(x => x.Status.Trim() == "3" && DateTime.Now.Date > x.VaccineDate1.GetValueOrDefault())
                       .Update(x => new Vaccine
                       {
                           Status = "4"
                       });
            set2.Fetch().Where(x => x.Status.Trim() == "6" && DateTime.Now.Date > x.VaccineDate2.GetValueOrDefault())
                       .Update(x => new Vaccine
                       {
                           Status = "7"
                       });

            var resukt = UnitOfWork.SaveChanges() > 1;

            // Return asynchronous operation.
            if (resukt)
            {
                totalFailure = 1;
            }
            return Task.FromResult(totalFailure);
        }

        public Task<int> SendReminderTermination()
        {
            // Create my task view repository object.
            var set = UnitOfWork.GetRepository<TerminationReminderView>();
            //cofig repositoy.
            var cf = UnitOfWork.GetRepository<Config>().Fetch();
            // Set default total failure to 0.
            var totalFailure = 0;

            // Get and set application url from configuration.
            var applicationUrl = _configService.GetConfigValue<string>(Configurations.ApplicationUrl);

            // Get list of task objects without object tracking.
            var listData = set.Fetch().OrderByDescending(ob => ob.DocumentNumber)
                    .AsNoTracking().ToList();
            string ccEmail = null;
            
            var ccTermMailStatus = _configService.GetConfigValue<bool>("Email.TerminationCcStatus");
            if(ccTermMailStatus)
            {
                ccEmail = _configService.GetConfigValue<string>("Email.Cc");
            }
            
            // Loop over task objects.
            foreach (var item in listData)
            {
                // Create new email data.
                var data = new
                {
                    names = item.Name,
                    document_type = item.TerminationType,
                    document_number = item.DocumentNumber,
                    employee_name = item.EmployeeName,
                    // Get and set task link.
                    url = applicationUrl
                };

                // Send email notification.
                if (!_emailService.SendEmail("termination-reminder", item.Email, data, ccEmail))
                {
                    // Count the total failure.
                    totalFailure++;
                }
            }

            // Return asynchronous operation.
            return Task.FromResult(totalFailure);
        }

        //public Task<int> CheckBPJSInsurance()
        //{
        //    IEnumerable<PersonalDataCheckBpjsInsuranceStoredEntity> DataCheckBpjsInsurance = UnitOfWork.UspQuery<PersonalDataCheckBpjsInsuranceStoredEntity>();
        //    foreach (var data in DataCheckBpjsInsurance.ToList())
        //    {
        //        string noregCurentApprover = data.LastApprovedBy;
        //        DocumentApproval documentApproval = new DocumentApproval();
        //        documentApproval.CreatedBy = data.CreatedBy;
        //        documentApproval.CreatedOn = data.CreatedOn.Value;
        //        documentApproval.DocumentNumber = data.DocumentNumber;

        //        PersonalDataFamilyMember personalDataFamilyMember = new PersonalDataFamilyMember();
        //        personalDataFamilyMember.Id = data.PersonalDataFamilyMemberId;
        //        //xxx
        //        var actualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Fetch()
        //            .AsNoTracking()
        //            .Where(x => x.NoReg == documentApproval.CreatedBy && x.Staffing == 100)
        //            .FirstOrDefaultIfEmpty();

        //        var approverActualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Fetch()
        //            .AsNoTracking()
        //            .Where(x => x.NoReg == noregCurentApprover && x.Staffing == 100)
        //            .FirstOrDefaultIfEmpty();

        //        var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
        //        var objBirthRegistration = JsonConvert.DeserializeObject<BirthRegistrationViewModel>(documentRequestDetail.ObjectValue);
        //        var personalData = PersonalDataRepository.Fetch().Where(x => x.NoReg == documentApproval.CreatedBy && x.RowStatus).FirstOrDefault();
        //        var personalDataCommonAttributeParent = PersonalDataCommonAttributeRepository.FindById(personalData.CommonAttributeId);

        //        //asuransi
        //        var Insurance = UnitOfWork.GetRepository<PersonalDataInsurance>().Fetch().Where(x => x.NoReg == documentApproval.CreatedBy && x.FamilyMemberId == null).FirstOrDefault();
        //        string NewMemberNumber = "";
        //        if (Insurance != null)
        //        {
        //            if (!string.IsNullOrEmpty(Insurance.MemberNumber))
        //            {
        //                //NewMemberNumber = Insurance.MemberNumber.Substring(5, 2);
        //                string MemberNumber = Insurance.MemberNumber;
        //                string split = "-";
        //                string[] val = MemberNumber.Split(split);
        //                string value = val[1];
        //                int aa = Convert.ToInt32(objBirthRegistration.AnakKe) + 1;
        //                if (aa <= 9)
        //                {
        //                    NewMemberNumber = val[0] + "-0" + aa.ToString();
        //                }
        //                else
        //                {
        //                    NewMemberNumber = val[0] + "-" + aa.ToString();
        //                }

        //            }
        //            else
        //            {
        //                NewMemberNumber = "";
        //            }
        //        }

        //        var personalDataInsurance = new PersonalDataInsurance()
        //        {
        //            NoReg = documentApproval.CreatedBy,
        //            FamilyMemberId = personalDataFamilyMember.Id,
        //            MemberNumber = NewMemberNumber,
        //            BenefitClassification = Insurance.BenefitClassification == null ? "" : Insurance.BenefitClassification,
        //            StartDate = objBirthRegistration.DateOfBirth.Value,
        //            EndDate = DateTime.Parse("9999-12-31"),
        //            ActionType = "pendaftaran",
        //            CompleteStatus = false
        //        };

        //        PersonalDataInsuranceRepository.Add(personalDataInsurance);

        //        var personalDataEvent = new PersonalDataEvent()
        //        {
        //            NoReg = documentApproval.CreatedBy,
        //            FamilyMemberId = personalDataFamilyMember.Id,
        //            EventType = "family-registration",
        //            EventDate = objBirthRegistration.DateOfBirth.Value
        //        };

        //        PersonalDataEventRepository.Add(personalDataEvent);

        //        var dataBpjs = (from bpjs in PersonalDataBpjsRepository.Fetch()
        //                        join fm in PersonalDataFamilyMemberRepository.Fetch()
        //                        on bpjs.FamilyMemberId equals fm.Id
        //                        where bpjs.NoReg == documentApproval.CreatedBy && (fm.FamilyTypeCode == "anakkandung" || fm.FamilyTypeCode == "anakangkat") && (fm.StartDate <= DateTime.Now && fm.EndDate >= DateTime.Now)
        //                        select bpjs).Count();

        //        if (dataBpjs < 3)
        //        {
        //            var personalDataBpjs = new PersonalDataBpjs()
        //            {
        //                NoReg = documentApproval.CreatedBy,
        //                FamilyMemberId = personalDataFamilyMember.Id,
        //                BpjsNumber = "",
        //                FaskesCode = "",
        //                Telephone = "",
        //                Email = "",
        //                PassportNumber = "",
        //                StartDate = objBirthRegistration.DateOfBirth.Value,
        //                EndDate = DateTime.Parse("9999-12-31"),
        //                ActionType = "pendaftaran",
        //                CompleteStatus = false
        //            };
        //            PersonalDataBpjsRepository.Add(personalDataBpjs);

        //            var dataApprover = new MdmService(UnitOfWork).GetActualOrganizationStructure(documentApproval.CreatedBy);
        //            var users = new UserService(this.UnitOfWork).GetUsersByRole("SHE");
        //            var member = GetFamilyMemberDetail(personalDataFamilyMember.Id);

        //            users?.ForEach(x =>
        //                NotificationRepository.Add(new Domain.Notification
        //                {
        //                    FromNoReg = dataApprover.NoReg,
        //                    ToNoReg = x.NoReg,
        //                    Message = $"Document Category BPJS for {GetFamilyMemberDetail(personalDataFamilyMember.Id).Name} has been completed {(personalDataBpjs.ActionType == "pendaftaran" ? "register" : "deactived")} by {dataApprover.Name}",
        //                    NotificationTypeCode = "notice",
        //                })
        //            );

        //            //create request tax status    
        //            var documentApprovalFileRequest = DocumentApprovalFileRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).ToList();
        //            var documentApprovalFile = new List<DocumentApprovalFile>();

        //            foreach (var item in documentApprovalFileRequest)
        //            {
        //                documentApprovalFile.Add(new DocumentApprovalFile()
        //                {
        //                    CommonFileId = item.CommonFileId,
        //                    FieldCategory = "Object.SupportingAttachmentPath"
        //                });
        //            }

        //            var personalDataTaxStatus = PersonalDataTaxStatusRepository.Fetch().Where(x => x.NoReg == documentApproval.CreatedBy && x.RowStatus && (DateTime.Now >= x.StartDate && DateTime.Now <= x.EndDate)).FirstOrDefault();
        //            var taxStatus = personalDataTaxStatus.TaxStatus.Substring(0, personalDataTaxStatus.TaxStatus.Length - 1)
        //                + (int.Parse(personalDataTaxStatus.TaxStatus.Substring(personalDataTaxStatus.TaxStatus.Length - 1)) + 1).ToString();

        //            if (personalDataCommonAttributeParent.GenderCode == "perempuan")
        //            {
        //                //update delimit
        //                personalDataTaxStatus.EndDate = DateTime.Now.Date.AddDays(-1);
        //                PersonalDataTaxStatusRepository.Upsert<Guid>(personalDataTaxStatus);

        //                var personalDataTaxStatusGender = new PersonalDataTaxStatus()
        //                {
        //                    NoReg = documentApproval.CreatedBy,
        //                    Npwp = personalDataTaxStatus.Npwp,
        //                    TaxStatus = taxStatus,
        //                    TaxPtkp = "TK0",
        //                    StartDate = DateTime.Now.Date,
        //                    EndDate = DateTime.Parse("9999-12-31")
        //                };
        //                PersonalDataTaxStatusRepository.Upsert<Guid>(personalDataTaxStatusGender);
        //            }
        //            else
        //            {
        //                var taxstatusRequestDetail = new DocumentRequestDetailViewModel<TaxStatusViewModel>()
        //                {
        //                    FormKey = "tax-status",
        //                    Object = new TaxStatusViewModel()
        //                    {
        //                        NPWPNumber = personalDataTaxStatus.Npwp,
        //                        StatusTax = taxStatus
        //                    },
        //                    Attachments = documentApprovalFile,
        //                };
        //                new ApprovalService(this.UnitOfWork, null, _localizer).CreateAutoRequestApprovalDocumentAsync(noregCurentApprover, documentApproval, actualOrganizationStructure, approverActualOrganizationStructure, taxstatusRequestDetail);
        //            }

        //            if (!users.IsEmpty())
        //            {
        //                var emailService = new EmailService(UnitOfWork);
        //                var coreService = new CoreService(UnitOfWork);

        //                var emailTemplate = coreService.GetEmailTemplate(MailTemplate.SheUpdateMember);
        //                var instanceKey = $"app-notice";
        //                var mailSubject = emailTemplate.Subject;
        //                var mailFrom = emailTemplate.MailFrom;
        //                var template = Scriban.Template.Parse(emailTemplate.MailContent);

        //                var mailManager = emailService.CreateEmailManager();
        //                var mailContent = template.Render(new
        //                {
        //                    names = personalDataCommonAttributeParent.Name,
        //                    document = "BPJS/Asuransi",
        //                    familyName = member.Name,
        //                    familyType = "",
        //                    karyawanName = personalDataCommonAttributeParent.Name,
        //                    noreg = documentApproval.CreatedBy,
        //                    year = DateTime.Now.Year
        //                });

        //                mailManager.SendAsync(mailFrom, mailSubject, mailContent, string.Join(",", users.Select(x => x.Email))).ConfigureAwait(false);
        //            }
        //        }

        //        UnitOfWork.SaveChanges();
        //    }

        //    int totalFailure = 0;
        //    return Task.FromResult(totalFailure);
        //}
        #endregion
    }

    
}

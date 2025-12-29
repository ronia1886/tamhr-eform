using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Helpers;
using TAMHR.ESS.Infrastructure.ViewModels;
using Agit.Common;
using Agit.Domain;
using Agit.Common.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Agit.Domain.Extensions;
using Z.EntityFramework.Plus;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Agit.Common.Email;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class PersonalDataService : DomainServiceBase
    {
        #region Repositories
        protected IReadonlyRepository<PersonalDataMedicalHistory> MedicalHistoryReadonlyRepository => UnitOfWork.GetRepository<PersonalDataMedicalHistory>();
        protected IRepository<DocumentApproval> DocumentApprovalRepository => UnitOfWork.GetRepository<DocumentApproval>();
        protected IRepository<DocumentApprovalFile> DocumentApprovalFileRepository => UnitOfWork.GetRepository<DocumentApprovalFile>();
        protected IRepository<DocumentRequestDetail> DocumentRequestDetailRepository => UnitOfWork.GetRepository<DocumentRequestDetail>();
        protected IRepository<PersonalData> PersonalDataRepository => UnitOfWork.GetRepository<PersonalData>();
        protected IRepository<PersonalDataBankAccount> PersonalDataBankAccountRepository => UnitOfWork.GetRepository<PersonalDataBankAccount>();
        protected IRepository<PersonalDataBpjs> PersonalDataBpjsRepository => UnitOfWork.GetRepository<PersonalDataBpjs>();
        protected IRepository<PersonalDataCommonAttribute> PersonalDataCommonAttributeRepository => UnitOfWork.GetRepository<PersonalDataCommonAttribute>();
        protected IRepository<PersonalDataEducation> PersonalDataEducationRepository => UnitOfWork.GetRepository<PersonalDataEducation>();
        protected IRepository<PersonalDataFamilyMember> PersonalDataFamilyMemberRepository => UnitOfWork.GetRepository<PersonalDataFamilyMember>();
        protected IRepository<PersonalDataInsurance> PersonalDataInsuranceRepository => UnitOfWork.GetRepository<PersonalDataInsurance>();
        protected IRepository<PersonalDataTaxStatus> PersonalDataTaxStatusRepository => UnitOfWork.GetRepository<PersonalDataTaxStatus>();
        protected IRepository<PersonalDataEvent> PersonalDataEventRepository => UnitOfWork.GetRepository<PersonalDataEvent>();
        protected IRepository<ActualOrganizationStructure> ActualOrganizationStructureRepository => UnitOfWork.GetRepository<ActualOrganizationStructure>();
        protected IRepository<AllowanceDetail> AllowanceDetailRepository => UnitOfWork.GetRepository<AllowanceDetail>();
        protected IRepository<Form> FormRepository => UnitOfWork.GetRepository<Form>();
        protected IRepository<EmployeeSubgroupNP> EmployeeSubgroupNPRepository => UnitOfWork.GetRepository<EmployeeSubgroupNP>();
        protected IRepository<GeneralCategory> GeneralCategoryRepository => UnitOfWork.GetRepository<GeneralCategory>();
        protected IRepository<User> UserRepository => UnitOfWork.GetRepository<User>();
        protected IRepository<Hospital> HospitalRepository => UnitOfWork.GetRepository<Hospital>();
        protected IRepository<Notification> NotificationRepository => UnitOfWork.GetRepository<Notification>();
        protected IRepository<Vaccine> VaccineRepository => UnitOfWork.GetRepository<Vaccine>();
        protected IRepository<PersonalDataDriverLicense> PersonalDataDriversLicense => UnitOfWork.GetRepository<PersonalDataDriverLicense>();
        #endregion

        #region Variables & Properties
        private IStringLocalizer<IUnitOfWork> _localizer;
        #endregion

        #region Constructor
        private EmailService _emailService;
        public PersonalDataService(IUnitOfWork unitOfWork, IStringLocalizer<IUnitOfWork> localizer = null,EmailService emailService = null)
            : base(unitOfWork)
        {
            _localizer = localizer;
            _emailService = emailService;
        }
        //public PersonalDataService(ConfigService configService, EmailService emailService, IUnitOfWork unitOfWork, IStringLocalizer<IUnitOfWork> localizer)
        //    : base(unitOfWork)
        //{
        //    _localizer = localizer;
        //    // Get and set config service object from DI container.
        //    //_configService = configService;

        //    //// Get and set email service object from DI container.
        //    //_emailService = emailService;
        //}
        #endregion
        public IQueryable<PersonalDataEvent> GetPersonalDataEventQuery()
        {
            return UnitOfWork.GetRepository<PersonalDataEvent>()
                .Fetch()
                .AsNoTracking();
        }
        public void CompleteOthersPersonalData(string actor, DocumentApproval documentApproval)
        {
            var now = DateTime.Now.Date;

            var othersPersonalDataRepository = UnitOfWork.GetRepository<PersonalDataOtherInformation>();

            var documentRequestDetail = DocumentRequestDetailRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.DocumentApprovalId == documentApproval.Id);

            var othersPersonalDataViewModel = JsonConvert.DeserializeObject<OthersPersonalDataViewModel>(documentRequestDetail.ObjectValue);

            var othersPersonalData = othersPersonalDataRepository.Fetch()
                .FirstOrDefault(x => x.NoReg == documentApproval.CreatedBy);

            if (othersPersonalData != null)
            {
                othersPersonalData.PhoneNumber1 = othersPersonalDataViewModel.PhoneNumber1;
                othersPersonalData.PhoneNumber2 = othersPersonalDataViewModel.PhoneNumber2;
                othersPersonalData.PhoneNumber3 = othersPersonalDataViewModel.PhoneNumber3;
                othersPersonalData.HomePhoneNumber = othersPersonalDataViewModel.HomePhoneNumber;
                othersPersonalData.PersonalEmail = othersPersonalDataViewModel.PersonalEmail;
                othersPersonalData.TransportationCodes = string.Join(",", othersPersonalDataViewModel.PublicTransportations);
                othersPersonalData.Hobbies = othersPersonalDataViewModel.Hobbies;
                othersPersonalData.EmergencyCallRelationshipCode = othersPersonalDataViewModel.EmergencyCallRelationshipCode;
                othersPersonalData.EmergencyCallName = othersPersonalDataViewModel.EmergencyCallRelationshipName;
                othersPersonalData.EmergencyCallPhoneNumber = othersPersonalDataViewModel.EmergencyCallRelationshipPhoneNumber;
            }
            else
            {
                othersPersonalDataRepository.Add(new PersonalDataOtherInformation
                {
                    NoReg = documentApproval.CreatedBy,
                    PhoneNumber1 = othersPersonalDataViewModel.PhoneNumber1,
                    PhoneNumber2 = othersPersonalDataViewModel.PhoneNumber2,
                    PhoneNumber3 = othersPersonalDataViewModel.PhoneNumber3,
                    HomePhoneNumber = othersPersonalDataViewModel.HomePhoneNumber,
                    PersonalEmail = othersPersonalDataViewModel.PersonalEmail,
                    TransportationCodes = string.Join(",", othersPersonalDataViewModel.PublicTransportations),
                    Hobbies = othersPersonalDataViewModel.Hobbies,
                    EmergencyCallRelationshipCode = othersPersonalDataViewModel.EmergencyCallRelationshipCode,
                    EmergencyCallName = othersPersonalDataViewModel.EmergencyCallRelationshipName,
                    EmergencyCallPhoneNumber = othersPersonalDataViewModel.EmergencyCallRelationshipPhoneNumber,
                    StartDate = DateTime.Now.Date,
                    EndDate = DateTime.MaxValue.Date
                });
            }

            UnitOfWork.SaveChanges();
        }

        public async System.Threading.Tasks.Task CompleteMarriageStatusAsync(string noregCurentApprover, DocumentApproval documentApproval)
        {
            var actualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == documentApproval.CreatedBy && x.Staffing == 100)
                .FirstOrDefaultIfEmpty();

            var approverActualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noregCurentApprover && x.Staffing == 100)
                .FirstOrDefaultIfEmpty();

            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            var objMarriageStatus = JsonConvert.DeserializeObject<MarriageStatusViewModel>(documentRequestDetail.ObjectValue);
            var gender = (from p in PersonalDataRepository.Fetch()
                            join pc in PersonalDataCommonAttributeRepository.Fetch()
                            on p.CommonAttributeId equals pc.Id
                            where p.RowStatus && pc.RowStatus && p.NoReg == documentApproval.CreatedBy && pc.StartDate <= DateTime.Now && pc.EndDate >= DateTime.Now
                            select pc.GenderCode).FirstOrDefault();

            //Personal Data
            var personalData = PersonalDataRepository.Fetch().Where(x => x.NoReg == documentApproval.CreatedBy).FirstOrDefault();
            personalData.MaritalStatusCode = "menikah";
            PersonalDataRepository.Upsert<Guid>(personalData);

            //Common Attribute parent
            var personalDataCommonAttributeParent = PersonalDataCommonAttributeRepository.Find(x => x.Id == personalData.CommonAttributeId).FirstOrDefault();
            personalDataCommonAttributeParent.KKNumber = objMarriageStatus.FamilyCardNo;
            PersonalDataCommonAttributeRepository.Upsert<Guid>(personalDataCommonAttributeParent);
                
            //Common Attribute
            var personalDataCommonAttribute = new PersonalDataCommonAttribute()
            {
                Name = objMarriageStatus.PartnerName,
                Nik = objMarriageStatus.NIK,
                KKNumber = objMarriageStatus.FamilyCardNo,
                BirthDate = objMarriageStatus.DateOfBirth.Value,
                BirthPlace = objMarriageStatus.IsOtherPlaceOfBirthCode ? objMarriageStatus.OtherPlaceOfBirthCode : objMarriageStatus.PlaceOfBirthCode,
                NationalityCode = objMarriageStatus.IsOtherNation ? objMarriageStatus.OtherNation : objMarriageStatus.NationalityCode,
                //NationalityCode = objMarriageStatus.NationalityCode,
                GenderCode = objMarriageStatus.GenderCode,
                CountryOfBirthCode = objMarriageStatus.NationCode,
                Address = objMarriageStatus.Address,
                CityCode = objMarriageStatus.CityCode,
                SubDistrictCode = objMarriageStatus.SubDistrictCode,
                PostalCode = objMarriageStatus.PostalCode,
                CountryCode = objMarriageStatus.NationCode,
                RegionCode = objMarriageStatus.ProvinceCode,
                DistrictCode = objMarriageStatus.DistrictCode,
                Rt = objMarriageStatus.Rt,
                Rw = objMarriageStatus.Rw,
                ReligionCode = objMarriageStatus.ReligionCode,
                BloodTypeCode = objMarriageStatus.BloodTypeCode,
                StartDate = objMarriageStatus.MarriageDate.Value,
                EndDate = DateTime.Parse("9999-12-31")                    
            };
            PersonalDataCommonAttributeRepository.Add(personalDataCommonAttribute);
            UnitOfWork.SaveChanges();
            //Family Member
            var personalDataFamilyMember = new PersonalDataFamilyMember()
            {
                NoReg = documentApproval.CreatedBy,
                CommonAttributeId = personalDataCommonAttribute.Id,
                IsMainFamily = true,
                FamilyTypeCode = "suamiistri",
                StartDate = objMarriageStatus.MarriageDate.Value,
                EndDate = DateTime.Parse("9999-12-31")
            };
            PersonalDataFamilyMemberRepository.Add(personalDataFamilyMember);
            UnitOfWork.SaveChanges();

            var dataApprover = new MdmService(UnitOfWork).GetActualOrganizationStructure(documentApproval.CreatedBy);
            var users = new UserService(this.UnitOfWork).GetUsersByRole("SHE");
            var member = GetFamilyMemberDetail(personalDataFamilyMember.Id);
            //bpjs
            var personalDataBpjs = new PersonalDataBpjs()
            {
                NoReg = documentApproval.CreatedBy,
                FamilyMemberId = personalDataFamilyMember.Id,
                BpjsNumber = objMarriageStatus.PartnerBjpsNo,
                FaskesCode = objMarriageStatus.FaskesCode,
                Telephone = objMarriageStatus.PartnerPhone,
                Email = objMarriageStatus.PartnerEmail,
                PassportNumber = "",
                StartDate = objMarriageStatus.MarriageDate.Value,
                EndDate = DateTime.Parse("9999-12-31"),
                ActionType = "pendaftaran",
                CompleteStatus = false
            };
            PersonalDataBpjsRepository.Add(personalDataBpjs);
                 
            users?.ForEach(x =>
                NotificationRepository.Add(new Domain.Notification
                {
                    FromNoReg = dataApprover.NoReg,
                    ToNoReg = x.NoReg,
                    Message = $"Document Category BPJS for { member.Name } has been completed { (personalDataBpjs.ActionType == "pendaftaran" ? "register" : "deactived") } by { dataApprover.Name }",
                    NotificationTypeCode = "notice",
                })
            );

            //asuransi
            var Insurance = PersonalDataInsuranceRepository.Fetch().Where(x => x.NoReg == documentApproval.CreatedBy && x.FamilyMemberId == null).FirstOrDefault();
            string NewMemberNumber = "";
            string BenefitClassification = "";
            if (Insurance != null)
            {
                if (!string.IsNullOrEmpty(Insurance.MemberNumber))
                {
                    //NewMemberNumber = Insurance.MemberNumber.Substring(5, 2);
                    string MemberNumber = Insurance.MemberNumber;
                    string split = "-";
                    string[] val = MemberNumber.Split(split);
                    string value = val[1];
                    NewMemberNumber = val[0] + "-01";
                    BenefitClassification = Insurance.BenefitClassification;

                }
                else
                {
                    NewMemberNumber = "";
                    BenefitClassification = "";
                }
            }
            else
            {
                NewMemberNumber = "";
                BenefitClassification = "";
            }
               
            var personalDataInsurance = new PersonalDataInsurance()
            {
                NoReg = documentApproval.CreatedBy,
                FamilyMemberId = personalDataFamilyMember.Id,
                MemberNumber = NewMemberNumber,
                BenefitClassification = BenefitClassification,
                StartDate = objMarriageStatus.MarriageDate.Value,
                EndDate = DateTime.Parse("9999-12-31"),
                ActionType = "pendaftaran",
                CompleteStatus = false
            };

            PersonalDataInsuranceRepository.Add(personalDataInsurance);

            users?.ForEach(x =>
                NotificationRepository.Add(new Domain.Notification
                {
                    FromNoReg = dataApprover.NoReg,
                    ToNoReg = x.NoReg,
                    Message = $"Document Category Asuransi for { member.Name } has been completed { (personalDataInsurance.ActionType == "pendaftaran" ? "register" : "deactived") } by { dataApprover.Name }",
                    NotificationTypeCode = "notice",
                })
            );

            //event
            var personalDataEvent = new PersonalDataEvent()
            {
                NoReg = documentApproval.CreatedBy,
                FamilyMemberId = personalDataFamilyMember.Id,
                EventType = "marriage-status",
                EventDate = objMarriageStatus.MarriageDate.Value
            };

            PersonalDataEventRepository.Add(personalDataEvent);

            ////create tunjangan
            ///List File Request
            var documentApprovalFileRequest = DocumentApprovalFileRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).ToList();
            var documentApprovalFile1 = documentApprovalFileRequest.Where(x => x.FieldCategory == "Object.KTPPath");
            var documentApprovalFile2 = documentApprovalFileRequest.Where(x => x.FieldCategory == "Object.MarriageCertificatePath");
            var documentApprovalFile = new List<DocumentApprovalFile>();

            foreach (var item in documentApprovalFile1)
            {
                documentApprovalFile.Add(new DocumentApprovalFile()
                {
                    CommonFileId = item.CommonFileId,
                    FieldCategory = "Object.PopulationPath"
                });
            }
            foreach (var item in documentApprovalFile2)
            {
                documentApprovalFile.Add(new DocumentApprovalFile()
                {
                    CommonFileId = item.CommonFileId,
                    FieldCategory = "Object.MarriageCertificatePath"
                });
            }

            //tunjangan pernikahan
            //jika sudah pernah dapat maka tidak dapat tunjangan lagi
            var dataMarriageAllowance = from da in DocumentApprovalRepository.Fetch()
                                        join f in FormRepository.Fetch()
                                        on da.FormId equals f.Id
                                        where f.FormKey == "marriage-allowance" && da.CreatedBy == documentApproval.CreatedBy && (da.DocumentStatusCode == "completed" || da.DocumentStatusCode == "inprogress")
                                        select da.Id;

            if (dataMarriageAllowance.Count() == 0)
            {
                var marriageAllowanceRequestDetail = new DocumentRequestDetailViewModel<MarriageAllowanceViewModel>()
                {
                    FormKey = "marriage-allowance",
                    Object = new MarriageAllowanceViewModel()
                    {
                        PopulationNumber = personalDataCommonAttribute.Nik,
                        PopulationPath = objMarriageStatus.KTPPath,
                        IsOtherNation = objMarriageStatus.IsOtherNation,
                        OtherNation = objMarriageStatus.OtherNation,
                        PartnerName = personalDataCommonAttribute.Name,
                        CountryCode = objMarriageStatus.NationCode,
                        PlaceOfBirthCode = objMarriageStatus.PlaceOfBirthCode,
                        AmountAllowance = objMarriageStatus.AmountAllowance,
                        IsOtherPlaceOfBirthCode = objMarriageStatus.IsOtherPlaceOfBirthCode,
                        OtherPlaceOfBirthCode = objMarriageStatus.OtherPlaceOfBirthCode,
                        BloodTypeCode = personalDataCommonAttribute.BloodTypeCode,
                        DateOfBirth = personalDataCommonAttribute.BirthDate,
                        Citizenship = objMarriageStatus.NationalityCode,
                        GenderCode = personalDataCommonAttribute.GenderCode,
                        Religion = personalDataCommonAttribute.ReligionCode,
                        WeddingDate = personalDataCommonAttribute.StartDate,
                        MarriageCertificatePath = objMarriageStatus.MarriageCertificatePath
                    },
                    Attachments = documentApprovalFile
                };

                new ApprovalService(this.UnitOfWork, null, _localizer).CreateAutoRequestApprovalDocumentAsync(noregCurentApprover, documentApproval, actualOrganizationStructure, approverActualOrganizationStructure, marriageAllowanceRequestDetail);
            }

            //create request task status
            //jika gender perempuan langsung update tax status tanpa request
            var personalDataTaxStatus = PersonalDataTaxStatusRepository.Fetch().Where(x => x.NoReg == documentApproval.CreatedBy && x.RowStatus && (DateTime.Now >= x.StartDate && DateTime.Now <= x.EndDate)).FirstOrDefault();

            if (gender == "perempuan")
            {
                personalDataTaxStatus.EndDate = DateTime.Now.Date.AddDays(-1);
                PersonalDataTaxStatusRepository.Upsert<Guid>(personalDataTaxStatus);

                var personalDataTaxStatusGender = new PersonalDataTaxStatus()
                {
                    NoReg = documentApproval.CreatedBy,
                    Npwp = personalDataTaxStatus.Npwp,
                    TaxStatus = "TK0",
                    TaxPtkp = "TK0",
                    StartDate = DateTime.Now.Date,
                    EndDate = DateTime.Parse("9999-12-31")
                };

                PersonalDataTaxStatusRepository.Upsert<Guid>(personalDataTaxStatusGender);
            }
            else
            {
                var taxStatus = "K" + personalDataTaxStatus.TaxStatus.Substring(personalDataTaxStatus.TaxStatus.Length - 1);
                var documentApprovalFileTax = new List<DocumentApprovalFile>() { };

                foreach (var item in documentApprovalFileRequest.Where(x => x.FieldCategory != "Object.PartnerBjpsPath"))
                {
                    documentApprovalFileTax.Add(new DocumentApprovalFile()
                    {
                        CommonFileId = item.CommonFileId,
                        FieldCategory = "Object.SupportingAttachmentPath"
                    });
                }

                var taxstatusRequestDetail = new DocumentRequestDetailViewModel<TaxStatusViewModel>()
                {
                    FormKey = "tax-status",
                    Object = new TaxStatusViewModel()
                    {
                        NPWPNumber = personalDataTaxStatus.Npwp,
                        StatusTax = taxStatus
                    },
                    Attachments = documentApprovalFileTax
                };

                new ApprovalService(this.UnitOfWork, null, _localizer).CreateAutoRequestApprovalDocumentAsync(noregCurentApprover, documentApproval, actualOrganizationStructure, approverActualOrganizationStructure, taxstatusRequestDetail);
            }

            if (!users.IsEmpty())
            {
                var emailService = new EmailService(UnitOfWork);
                var coreService = new CoreService(UnitOfWork);

                var emailTemplate = coreService.GetEmailTemplate(MailTemplate.SheUpdateMember);
                var instanceKey = $"app-notice";
                var mailSubject = emailTemplate.Subject;
                var mailFrom = emailTemplate.MailFrom;
                var template = Scriban.Template.Parse(emailTemplate.MailContent);

                var mailManager = emailService.CreateEmailManager();
                var mailContent = template.Render(new
                {
                    names = personalDataCommonAttributeParent.Name,
                    document = "BPJS/Asuransi",
                    familyName = member.Name,
                    familyType = "suamiistri",
                    karyawanName = personalDataCommonAttributeParent.Name,
                    noreg = documentApproval.CreatedBy,
                    year = DateTime.Now.Year
                });

                await mailManager.SendAsync(mailFrom, mailSubject, mailContent, string.Join(",", users.Select(x => x.Email))).ConfigureAwait(false);
            }

            UnitOfWork.SaveChanges();
        }

        public async System.Threading.Tasks.Task CompleteBirthRegistrationAsync(string noregCurentApprover, DocumentApproval documentApproval)
        {
            var actualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == documentApproval.CreatedBy && x.Staffing == 100)
                .FirstOrDefaultIfEmpty();

            var approverActualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noregCurentApprover && x.Staffing == 100)
                .FirstOrDefaultIfEmpty();

            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            var objBirthRegistration = JsonConvert.DeserializeObject<BirthRegistrationViewModel>(documentRequestDetail.ObjectValue);
            var personalData = PersonalDataRepository.Fetch().Where(x => x.NoReg == documentApproval.CreatedBy && x.RowStatus).FirstOrDefault();
            var personalDataCommonAttributeParent = PersonalDataCommonAttributeRepository.FindById(personalData.CommonAttributeId);

            var personalDataCommonAttribute = new PersonalDataCommonAttribute()
            {
                Name = objBirthRegistration.ChildName,
                BirthDate = objBirthRegistration.DateOfBirth.Value,
                BirthPlace = objBirthRegistration.IsOtherPlaceOfBirthCode ? objBirthRegistration.OtherPlaceOfBirthCode : objBirthRegistration.PlaceOfBirthCode,
                NationalityCode = objBirthRegistration.IsOtherNation ? objBirthRegistration.OtherNation : objBirthRegistration.NationalityCode,
                //NationalityCode =  objBirthRegistration.NationalityCode,
                GenderCode = objBirthRegistration.GenderCode,
                CountryOfBirthCode = objBirthRegistration.NationCode,
                Address = personalDataCommonAttributeParent.Address,
                CityCode = personalDataCommonAttributeParent.CityCode,
                PostalCode = personalDataCommonAttributeParent.PostalCode,
                CountryCode = objBirthRegistration.NationCode,
                DistrictCode = personalDataCommonAttributeParent.DistrictCode,
                SubDistrictCode = personalDataCommonAttributeParent.SubDistrictCode,
                RegionCode = personalDataCommonAttributeParent.CountryCode,
                Rt = personalDataCommonAttributeParent.Rt,
                Rw = personalDataCommonAttributeParent.Rw,
                ReligionCode = objBirthRegistration.ReligionCode,
                BloodTypeCode = objBirthRegistration.BloodTypeCode,
                StartDate = objBirthRegistration.DateOfBirth.Value,
                EndDate = DateTime.Parse("9999-12-31")
            };

            PersonalDataCommonAttributeRepository.Add(personalDataCommonAttribute);
            UnitOfWork.SaveChanges();

            //Family Member
            var personalDataFamilyMember = new PersonalDataFamilyMember()
            {
                NoReg = documentApproval.CreatedBy,
                CommonAttributeId = personalDataCommonAttribute.Id,
                IsMainFamily = true,
                FamilyTypeCode = objBirthRegistration.ChildStatus,
                StartDate = objBirthRegistration.DateOfBirth.Value,
                EndDate = DateTime.Parse("9999-12-31")
            };

            PersonalDataFamilyMemberRepository.Add(personalDataFamilyMember);
            UnitOfWork.SaveChanges();

            //asuransi
            var Insurance = PersonalDataInsuranceRepository.Fetch().Where(x => x.NoReg == documentApproval.CreatedBy && (x.FamilyMemberId == null || x.FamilyMemberId == Guid.Empty)).FirstOrDefault();
            string NewMemberNumber = "";
            if (Insurance != null)
            {
                if (!string.IsNullOrEmpty(Insurance.MemberNumber))
                {
                    //NewMemberNumber = Insurance.MemberNumber.Substring(5, 2);
                    string MemberNumber = Insurance.MemberNumber;
                    string split = "-";
                    string[] val = MemberNumber.Split(split);
                    string value = val[1];
                    int aa = Convert.ToInt32(objBirthRegistration.AnakKe) + 1;
                    if (aa <= 9)
                    {
                        NewMemberNumber = val[0] + "-0" + aa.ToString();
                    }
                    else
                    {
                        NewMemberNumber = val[0] + "-" + aa.ToString();
                    }

                }
                else
                {
                    NewMemberNumber = "";
                }
            }

            var personalDataInsurance = new PersonalDataInsurance()
            {
                NoReg = documentApproval.CreatedBy,
                FamilyMemberId = personalDataFamilyMember.Id,
                MemberNumber = NewMemberNumber,
                BenefitClassification = Insurance != null ? Insurance.BenefitClassification == null ? "" : Insurance.BenefitClassification : "",
                StartDate = objBirthRegistration.DateOfBirth.Value,
                EndDate = DateTime.Parse("9999-12-31"),
                ActionType = "pendaftaran",
                CompleteStatus = false
            };

            PersonalDataInsuranceRepository.Add(personalDataInsurance);

            var personalDataEvent = new PersonalDataEvent()
            {
                NoReg = documentApproval.CreatedBy,
                FamilyMemberId = personalDataFamilyMember.Id,
                EventType = "family-registration",
                EventDate = objBirthRegistration.DateOfBirth.Value
            };

            PersonalDataEventRepository.Add(personalDataEvent);

            var dataBpjs = (from bpjs in PersonalDataBpjsRepository.Fetch()
                            join fm in PersonalDataFamilyMemberRepository.Fetch()
                            on bpjs.FamilyMemberId equals fm.Id
                            where bpjs.NoReg == documentApproval.CreatedBy && (fm.FamilyTypeCode == "anakkandung" || fm.FamilyTypeCode == "anakangkat") && (fm.StartDate <= DateTime.Now && fm.EndDate >= DateTime.Now)
                            select bpjs).Count();

            if (dataBpjs < 3)
            {
                var personalDataBpjs = new PersonalDataBpjs()
                {
                    NoReg = documentApproval.CreatedBy,
                    FamilyMemberId = personalDataFamilyMember.Id,
                    BpjsNumber = "",
                    FaskesCode = "",
                    Telephone = "",
                    Email = "",
                    PassportNumber = "",
                    StartDate = objBirthRegistration.DateOfBirth.Value,
                    EndDate = DateTime.Parse("9999-12-31"),
                    ActionType = "pendaftaran",
                    CompleteStatus = false
                };

                PersonalDataBpjsRepository.Add(personalDataBpjs);

                var dataApprover = new MdmService(UnitOfWork).GetActualOrganizationStructure(documentApproval.CreatedBy);
                var users = new UserService(this.UnitOfWork).GetUsersByRole("SHE");
                var member = GetFamilyMemberDetail(personalDataFamilyMember.Id);

                users?.ForEach(x =>
                    NotificationRepository.Add(new Domain.Notification
                    {
                        FromNoReg = dataApprover.NoReg,
                        ToNoReg = x.NoReg,
                        Message = $"Document Category BPJS for { GetFamilyMemberDetail(personalDataFamilyMember.Id).Name } has been completed { (personalDataBpjs.ActionType == "pendaftaran" ? "register" : "deactived") } by { dataApprover.Name }",
                        NotificationTypeCode = "notice",
                    })
                );

                //create request tax status    
                var documentApprovalFileRequest = DocumentApprovalFileRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).ToList();
                var documentApprovalFile = new List<DocumentApprovalFile>();

                foreach (var item in documentApprovalFileRequest)
                {
                    documentApprovalFile.Add(new DocumentApprovalFile()
                    {
                        CommonFileId = item.CommonFileId,
                        FieldCategory = "Object.SupportingAttachmentPath"
                    });
                }

                var personalDataTaxStatus = PersonalDataTaxStatusRepository.Fetch().Where(x => x.NoReg == documentApproval.CreatedBy && x.RowStatus && (DateTime.Now >= x.StartDate && DateTime.Now <= x.EndDate)).FirstOrDefault();

                if (personalDataCommonAttributeParent.GenderCode == "perempuan")
                {
                    //update delimit
                    personalDataTaxStatus.EndDate = DateTime.Now.Date.AddDays(-1);
                    PersonalDataTaxStatusRepository.Upsert<Guid>(personalDataTaxStatus);

                    var personalDataTaxStatusGender = new PersonalDataTaxStatus()
                    {
                        NoReg = documentApproval.CreatedBy,
                        Npwp = personalDataTaxStatus.Npwp,
                        TaxStatus = "TK0",
                        TaxPtkp = "TK0",
                        StartDate = DateTime.Now.Date,
                        EndDate = DateTime.Parse("9999-12-31")
                    };
                    PersonalDataTaxStatusRepository.Upsert<Guid>(personalDataTaxStatusGender);
                }
                else
                {
                    var taxStatus = personalDataTaxStatus.TaxStatus.Substring(0, personalDataTaxStatus.TaxStatus.Length - 1)
                        + (int.Parse(personalDataTaxStatus.TaxStatus.Substring(personalDataTaxStatus.TaxStatus.Length - 1)) + 1).ToString();

                    var taxstatusRequestDetail = new DocumentRequestDetailViewModel<TaxStatusViewModel>()
                    {
                        FormKey = "tax-status",
                        Object = new TaxStatusViewModel()
                        {
                            NPWPNumber = personalDataTaxStatus.Npwp,
                            StatusTax = taxStatus
                        },
                        Attachments = documentApprovalFile,
                    };

                    new ApprovalService(this.UnitOfWork, null, _localizer).CreateAutoRequestApprovalDocumentAsync(noregCurentApprover, documentApproval, actualOrganizationStructure, approverActualOrganizationStructure, taxstatusRequestDetail);
                }

                if (!users.IsEmpty())
                {
                    var emailService = new EmailService(UnitOfWork);
                    var coreService = new CoreService(UnitOfWork);

                    var emailTemplate = coreService.GetEmailTemplate(MailTemplate.SheUpdateMember);
                    var instanceKey = $"app-notice";
                    var mailSubject = emailTemplate.Subject;
                    var mailFrom = emailTemplate.MailFrom;
                    var template = Scriban.Template.Parse(emailTemplate.MailContent);

                    var mailManager = emailService.CreateEmailManager();
                    var mailContent = template.Render(new
                    {
                        names = personalDataCommonAttributeParent.Name,
                        document = "BPJS/Asuransi",
                        familyName = member.Name,
                        familyType = "",
                        karyawanName = personalDataCommonAttributeParent.Name,
                        noreg = documentApproval.CreatedBy,
                        year = DateTime.Now.Year
                    });

                    await mailManager.SendAsync(mailFrom, mailSubject, mailContent, string.Join(",", users.Select(x => x.Email))).ConfigureAwait(false);
                }
            }

            UnitOfWork.SaveChanges();
        }

        public void CompleteDivorce(string noregCurentApprover, DocumentApproval documentApproval)
        {
            var actualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == documentApproval.CreatedBy && x.Staffing == 100)
                .FirstOrDefaultIfEmpty();

            var approverActualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noregCurentApprover && x.Staffing == 100)
                .FirstOrDefaultIfEmpty();

            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            var objDivorece = JsonConvert.DeserializeObject<DivorceViewModel>(documentRequestDetail.ObjectValue);

            var personalData = PersonalDataRepository.Fetch().Where(x => x.NoReg == documentApproval.CreatedBy).FirstOrDefault();
            var personalDataCommonAttributeParent = PersonalDataCommonAttributeRepository.FindById(personalData.CommonAttributeId);

            if (personalDataCommonAttributeParent.GenderCode == "perempuan")
            {
                personalData.MaritalStatusCode = "janda";
                PersonalDataRepository.Upsert<Guid>(personalData);
            }
            else if(personalDataCommonAttributeParent.GenderCode != "perempuan")
            {
                personalData.MaritalStatusCode = "duda";
                PersonalDataRepository.Upsert<Guid>(personalData);
            }

            //family member
            var personalDataFamilyMember = PersonalDataFamilyMemberRepository.Fetch().Where(x => x.Id == Guid.Parse(objDivorece.PartnerId)).FirstOrDefault();
            personalDataFamilyMember.EndDate = objDivorece.DivorceDate.Value;
            PersonalDataFamilyMemberRepository.Upsert<Guid>(personalDataFamilyMember);

            var dataApprover = new MdmService(UnitOfWork).GetActualOrganizationStructure(documentApproval.CreatedBy);
            var users = new UserService(this.UnitOfWork).GetUsersByRole("SHE");
            var member = GetFamilyMemberDetail(personalDataFamilyMember.Id);

            //bpjs
            PersonalDataBpjs personalDataBpjs = PersonalDataBpjsRepository.Fetch().Where(x => x.FamilyMemberId == Guid.Parse(objDivorece.PartnerId)).FirstOrDefault();
            if (personalDataBpjs != null)
            {
                personalDataBpjs.EndDate = objDivorece.DivorceDate.Value;
                personalDataBpjs.CompleteStatus = true;
                PersonalDataBpjsRepository.Upsert<Guid>(personalDataBpjs);

                var personalDataBpjsnew = new PersonalDataBpjs()
                {
                    NoReg = personalDataBpjs.NoReg,
                    FamilyMemberId = personalDataBpjs.FamilyMemberId,
                    BpjsNumber = personalDataBpjs.BpjsNumber,
                    FaskesCode = personalDataBpjs.FaskesCode,
                    Telephone = personalDataBpjs.Telephone,
                    Email = personalDataBpjs.Email,
                    PassportNumber = personalDataBpjs.PassportNumber,
                    StartDate = objDivorece.DivorceDate.Value,
                    EndDate = DateTime.Parse("9999-12-31"),
                    ActionType = "nonactive",
                    CompleteStatus = false
                };

                PersonalDataBpjsRepository.Upsert<Guid>(personalDataBpjsnew);

                users?.ForEach(x =>
                    NotificationRepository.Add(new Domain.Notification
                    {
                        FromNoReg = dataApprover.NoReg,
                        ToNoReg = x.NoReg,
                        Message = $"Document Category BPJS for { member.Name } has been completed { (personalDataBpjs.ActionType == "pendaftaran" ? "register" : "deactived") } by { dataApprover.Name }",
                        NotificationTypeCode = "notice",
                    })
                );
            }

            var personalDataInsurance = PersonalDataInsuranceRepository.Fetch().Where(x => x.FamilyMemberId == Guid.Parse(objDivorece.PartnerId)).FirstOrDefault();

            if (personalDataInsurance != null)
            {
                personalDataInsurance.EndDate = objDivorece.DivorceDate.Value;
                personalDataInsurance.CompleteStatus = true;
                PersonalDataInsuranceRepository.Upsert<Guid>(personalDataInsurance);

                var personalDataInsurancenew = new PersonalDataInsurance()
                {
                    NoReg = personalDataInsurance.NoReg,
                    FamilyMemberId = personalDataInsurance.FamilyMemberId,
                    MemberNumber = personalDataInsurance.MemberNumber,
                    BenefitClassification = personalDataInsurance.BenefitClassification,
                    StartDate = objDivorece.DivorceDate.Value,
                    EndDate = DateTime.Parse("9999-12-31"),
                    ActionType = "nonactive",
                    CompleteStatus = false
                };

                PersonalDataInsuranceRepository.Upsert<Guid>(personalDataInsurancenew);

                users?.ForEach(x =>
                    NotificationRepository.Add(new Domain.Notification
                    {
                        FromNoReg = dataApprover.NoReg,
                        ToNoReg = x.NoReg,
                        Message = $"Document Category Asuransi for { member.Name } has been completed { (personalDataInsurance.ActionType == "pendaftaran" ? "register" : "deactived") } by { dataApprover.Name }",
                        NotificationTypeCode = "notice",
                    })
                );

            }

            var personalDataEvent = new PersonalDataEvent()
            {
                NoReg = documentApproval.CreatedBy,
                FamilyMemberId = personalDataFamilyMember.Id,
                EventType = "divorce",
                EventDate = objDivorece.DivorceDate.Value
            };

            PersonalDataEventRepository.Add(personalDataEvent);

            var documentApprovalFileRequest = DocumentApprovalFileRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).ToList();
            var documentApprovalFile = new List<DocumentApprovalFile>();

            foreach (var item in documentApprovalFileRequest)
            {
                documentApprovalFile.Add(new DocumentApprovalFile()
                {
                    CommonFileId = item.CommonFileId,
                    FieldCategory = "Object.SupportingAttachmentPath"
                });
            }

            var personalDataTaxStatus = PersonalDataTaxStatusRepository.Fetch().Where(x => x.NoReg == documentApproval.CreatedBy && x.RowStatus
                    && (DateTime.Now >= x.StartDate && DateTime.Now <= x.EndDate)).FirstOrDefault();
            

            if (personalDataCommonAttributeParent.GenderCode == "perempuan")
            {
                personalDataTaxStatus.EndDate = DateTime.Now.Date.AddDays(-1);
                PersonalDataTaxStatusRepository.Upsert<Guid>(personalDataTaxStatus);

                PersonalDataTaxStatus personalDataTaxStatusGender = new PersonalDataTaxStatus()
                {
                    NoReg = documentApproval.CreatedBy,
                    Npwp = personalDataTaxStatus.Npwp,
                    TaxStatus = "TK0",
                    TaxPtkp = "TK0",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Parse("9999-12-31")
                };

                PersonalDataTaxStatusRepository.Upsert<Guid>(personalDataTaxStatusGender);
            }
            else
            {
                var taxStatus = "TK" + personalDataTaxStatus.TaxStatus.Substring(personalDataTaxStatus.TaxStatus.Length - 1); 

                DocumentRequestDetailViewModel<TaxStatusViewModel> taxstatusRequestDetail = new DocumentRequestDetailViewModel<TaxStatusViewModel>()
                {
                    FormKey = "tax-status",
                    Object = new TaxStatusViewModel()
                    {
                        NPWPNumber = personalDataTaxStatus.Npwp,
                        StatusTax = taxStatus
                    },
                    Attachments = documentApprovalFile,
                };

                new ApprovalService(this.UnitOfWork, null, _localizer).CreateAutoRequestApprovalDocumentAsync(noregCurentApprover, documentApproval, actualOrganizationStructure, approverActualOrganizationStructure, taxstatusRequestDetail);
            }

            UnitOfWork.SaveChanges();
        }

        public void CompleteDismemberment(string noregCurentApprover, DocumentApproval documentApproval)
        {
            var actualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == documentApproval.CreatedBy && x.Staffing == 100)
                .FirstOrDefaultIfEmpty();

            var approverActualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noregCurentApprover && x.Staffing == 100)
                .FirstOrDefaultIfEmpty();

            DocumentRequestDetail documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            DismembermentViewModel objDismemberment = JsonConvert.DeserializeObject<DismembermentViewModel>(documentRequestDetail.ObjectValue);
            PersonalData personalData = PersonalDataRepository.Fetch().Where(x => x.NoReg == documentApproval.CreatedBy && x.RowStatus).FirstOrDefault();
            PersonalDataCommonAttribute personalDataCommonAttributeParent = PersonalDataCommonAttributeRepository.FindById(personalData.CommonAttributeId);

            //jika keluarga inti
            if (objDismemberment.IsMainFamily == "ya")
            {
                //family member
                PersonalDataFamilyMember personalDataFamilyMember = PersonalDataFamilyMemberRepository.Fetch().Where(x => x.Id == Guid.Parse(objDismemberment.OtherFamilyId)).FirstOrDefault();
                personalDataFamilyMember.EndDate = objDismemberment.DismembermentDate.Value;

                PersonalDataFamilyMemberRepository.Upsert<Guid>(personalDataFamilyMember);
                UnitOfWork.SaveChanges();

                //bpjs
                PersonalDataBpjs personalDataBpjs = PersonalDataBpjsRepository.Fetch().Where(x => x.FamilyMemberId == Guid.Parse(objDismemberment.OtherFamilyId)).FirstOrDefault();
                if (personalDataBpjs != null)
                {
                    personalDataBpjs.EndDate = objDismemberment.DismembermentDate.Value;
                    personalDataBpjs.CompleteStatus = true;
                    PersonalDataBpjsRepository.Upsert<Guid>(personalDataBpjs);
                    PersonalDataBpjs personalDataBpjsnew = new PersonalDataBpjs()
                    {
                        NoReg = personalDataBpjs.NoReg,
                        FamilyMemberId = personalDataBpjs.FamilyMemberId,
                        BpjsNumber = personalDataBpjs.BpjsNumber,
                        FaskesCode = personalDataBpjs.FaskesCode,
                        Telephone = personalDataBpjs.Telephone,
                        Email = personalDataBpjs.Email,
                        PassportNumber = personalDataBpjs.PassportNumber,
                        StartDate = objDismemberment.DismembermentDate.Value,
                        EndDate = DateTime.Parse("9999-12-31"),
                        ActionType = "nonactive",
                        CompleteStatus = false
                    };
                    PersonalDataBpjsRepository.Upsert<Guid>(personalDataBpjsnew);
                }
                    
                //asuransi
                PersonalDataInsurance personalDataInsurance = PersonalDataInsuranceRepository.Fetch().Where(x => x.FamilyMemberId == Guid.Parse(objDismemberment.OtherFamilyId)).FirstOrDefault();
                if (personalDataInsurance != null)
                {
                    personalDataInsurance.EndDate = objDismemberment.DismembermentDate.Value;
                    personalDataInsurance.CompleteStatus = true;
                    PersonalDataInsuranceRepository.Upsert<Guid>(personalDataInsurance);
                    PersonalDataInsurance personalDataInsurancenew = new PersonalDataInsurance()
                    {
                        NoReg = personalDataInsurance.NoReg,
                        FamilyMemberId = personalDataInsurance.FamilyMemberId,
                        MemberNumber = personalDataInsurance.MemberNumber,
                        BenefitClassification = personalDataInsurance.BenefitClassification,
                        StartDate = objDismemberment.DismembermentDate.Value,
                        EndDate = DateTime.Parse("9999-12-31"),
                        ActionType = "nonactive",
                        CompleteStatus = false
                    };
                    PersonalDataInsuranceRepository.Upsert<Guid>(personalDataInsurancenew);
                }

                //event
                PersonalDataEvent personalDataEvent = new PersonalDataEvent()
                {
                    NoReg = documentApproval.CreatedBy,
                    FamilyMemberId = personalDataFamilyMember.Id,
                    EventType = "condolance",
                    EventDate = objDismemberment.DismembermentDate.Value
                };
                PersonalDataEventRepository.Add(personalDataEvent);

                //create request tax status
                List<DocumentApprovalFile> documentApprovalFileRequest = DocumentApprovalFileRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).ToList();
                List<DocumentApprovalFile> documentApprovalFile = new List<DocumentApprovalFile>();
                foreach (var item in documentApprovalFileRequest)
                {
                    documentApprovalFile.Add(new DocumentApprovalFile()
                    {
                        CommonFileId = item.CommonFileId,
                        FieldCategory = "Object.SupportingAttachmentPath"
                    });
                }
                PersonalDataTaxStatus personalDataTaxStatus = PersonalDataTaxStatusRepository.Fetch().Where(x => x.NoReg == documentApproval.CreatedBy && x.RowStatus
                        && (DateTime.Now >= x.StartDate && DateTime.Now <= x.EndDate)).FirstOrDefault();
                string taxStatus = "";
                    
                if (personalDataFamilyMember.FamilyTypeCode == "suamiistri" && personalDataCommonAttributeParent.GenderCode == "perempuan")
                {
                    taxStatus = "TK" + personalDataTaxStatus.TaxStatus.Substring(personalDataTaxStatus.TaxStatus.Length - 1);
                    //Personal Data
                    personalData.MaritalStatusCode = "janda";
                    PersonalDataRepository.Upsert<Guid>(personalData);
                }
                else if (personalDataFamilyMember.FamilyTypeCode == "suamiistri" && personalDataCommonAttributeParent.GenderCode == "lakilaki")
                {
                    taxStatus = "TK" + personalDataTaxStatus.TaxStatus.Substring(personalDataTaxStatus.TaxStatus.Length - 1);
                    //Personal Data
                    personalData.MaritalStatusCode = "duda";
                    PersonalDataRepository.Upsert<Guid>(personalData);
                }
                else
                {
                    int jumlahAnak = int.Parse(personalDataTaxStatus.TaxStatus.Substring(personalDataTaxStatus.TaxStatus.Length - 1));
                    jumlahAnak = jumlahAnak == 0 ? 0 : jumlahAnak - 1;
                    taxStatus = personalDataTaxStatus.TaxStatus.Remove(personalDataTaxStatus.TaxStatus.Length - 1) + jumlahAnak.ToString();
                }

                if (personalDataCommonAttributeParent.GenderCode == "perempuan")
                {
                    //update delimit
                    personalDataTaxStatus.EndDate = DateTime.Now.Date.AddDays(-1);
                    PersonalDataTaxStatusRepository.Upsert<Guid>(personalDataTaxStatus);

                    //add
                    PersonalDataTaxStatus personalDataTaxStatusGender = new PersonalDataTaxStatus()
                    {
                        NoReg = documentApproval.CreatedBy,
                        Npwp = personalDataTaxStatus.Npwp,
                        TaxStatus = "TK0",
                        TaxPtkp = "TK0",
                        StartDate = DateTime.Now.Date,
                        EndDate = DateTime.Parse("9999-12-31")
                    };
                    PersonalDataTaxStatusRepository.Upsert<Guid>(personalDataTaxStatusGender);
                }
                else
                {
                    DocumentRequestDetailViewModel<TaxStatusViewModel> taxstatusRequestDetail = new DocumentRequestDetailViewModel<TaxStatusViewModel>()
                    {
                        FormKey = "tax-status",
                        Object = new TaxStatusViewModel()
                        {
                            NPWPNumber = personalDataTaxStatus.Npwp,
                            StatusTax = taxStatus,
                        },
                        Attachments = documentApprovalFile,
                    };
                    //new ApprovalService(this.UnitOfWork, null, _localizer).CreateDocumentApproval(documentApproval.CreatedBy, taxstatusRequestDetail);
                    new ApprovalService(this.UnitOfWork, null, _localizer).CreateAutoRequestApprovalDocumentAsync(noregCurentApprover, documentApproval, actualOrganizationStructure, approverActualOrganizationStructure, taxstatusRequestDetail);
                }
            }
            else
            {
                //Common Attribute
                string FamilyTypeCode = objDismemberment.NonFamilyRelationship;
                string GenderCode = string.Empty;

                if (objDismemberment.NonFamilyRelationship == "ibukandung" || objDismemberment.NonFamilyRelationship == "nenek" || objDismemberment.NonFamilyRelationship == "ibumertua")
                {
                    GenderCode = "perempuan";
                }
                else
                {
                    GenderCode = "lakilaki";
                }
                PersonalDataCommonAttribute personalDataCommonAttribute = new PersonalDataCommonAttribute()
                {
                    Name = objDismemberment.FamilyName,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Parse("9999-12-31"),
                    BirthDate = DateTime.Parse("1900-01-01"),
                    BirthPlace = personalDataCommonAttributeParent.BirthPlace,
                    NationalityCode = personalDataCommonAttributeParent.NationalityCode,
                    GenderCode = GenderCode,
                    CountryOfBirthCode = personalDataCommonAttributeParent.CountryOfBirthCode,
                    Address = personalDataCommonAttributeParent.Address,
                    CityCode = personalDataCommonAttributeParent.CityCode,
                    PostalCode = personalDataCommonAttributeParent.PostalCode,
                    CountryCode = personalDataCommonAttributeParent.CountryCode,
                    SubDistrictCode = personalDataCommonAttributeParent.SubDistrictCode,
                    DistrictCode = personalDataCommonAttributeParent.DistrictCode,
                    RegionCode = personalDataCommonAttributeParent.RegionCode,
                    ReligionCode = personalDataCommonAttributeParent.ReligionCode,
                    BloodTypeCode = personalDataCommonAttributeParent.BloodTypeCode

                };
                PersonalDataCommonAttributeRepository.Add(personalDataCommonAttribute);
                UnitOfWork.SaveChanges();
                //Family Member
                PersonalDataFamilyMember personalDataFamilyMember = new PersonalDataFamilyMember()
                {
                    NoReg = documentApproval.CreatedBy,
                    CommonAttributeId = personalDataCommonAttribute.Id,
                    IsMainFamily = false,
                    FamilyTypeCode = objDismemberment.NonFamilyRelationship,
                    StartDate = objDismemberment.DismembermentDate.Value,
                    EndDate = objDismemberment.DismembermentDate.Value
                };

                PersonalDataFamilyMemberRepository.Add(personalDataFamilyMember);

                //event
                PersonalDataEvent personalDataEvent = new PersonalDataEvent()
                {
                    NoReg = documentApproval.CreatedBy,
                    FamilyMemberId = personalDataFamilyMember.Id,
                    EventType = "condolance",
                    EventDate = objDismemberment.DismembermentDate.Value
                };
                PersonalDataEventRepository.Add(personalDataEvent);
            }

            ////create tunjangan
            ///List File Request
            List<DocumentApprovalFile> documentApprovalFileRequest2 = DocumentApprovalFileRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).ToList();
            var documentApprovalFile1 = documentApprovalFileRequest2.Where(x => x.FieldCategory == "Object.FamilyCardPath").FirstOrDefault();
            List<DocumentApprovalFile> documentApprovalFile2 = new List<DocumentApprovalFile>() {
                new DocumentApprovalFile(){
                    CommonFileId = documentApprovalFile1.CommonFileId,
                    FieldCategory = "Object.FamilyCardPath"
                }
            };
            //tunjangan pernikahan
            DocumentRequestDetailViewModel<MisseryAllowanceViewModel> misseryAllowanceViewModelRequestDetail = new DocumentRequestDetailViewModel<MisseryAllowanceViewModel>()
            {
                FormKey = "condolance-allowance",
                Object = new MisseryAllowanceViewModel()
                {
                    FamilyName = objDismemberment.FamilyName,
                    MisseryDate = objDismemberment.DismembermentDate,
                    AllowancesAmount = objDismemberment.AllowancesAmount,
                    IsMainFamily = objDismemberment.IsMainFamily,
                    OtherFamilyId = objDismemberment.OtherFamilyId,
                    OtherFamilyName = objDismemberment.OtherFamilyName,
                    NonFamilyRelationship = objDismemberment.NonFamilyRelationship,
                    FamilyCardPath = objDismemberment.FamilyCardPath
                },
                Attachments = documentApprovalFile2
            };

            new ApprovalService(this.UnitOfWork, null, null).CreateAutoRequestApprovalDocumentAsync(noregCurentApprover, documentApproval, actualOrganizationStructure, approverActualOrganizationStructure, misseryAllowanceViewModelRequestDetail);

            UnitOfWork.SaveChanges();
        }

        public void CompleteAddress(string noregCurentApprover, DocumentApproval documentApproval)
        {
            var actualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>()
                .Find(x => x.NoReg == documentApproval.CreatedBy)
                .FirstOrDefaultIfEmpty();

            var documentRequestDetail = DocumentRequestDetailRepository.Fetch()
                .Where(x => x.DocumentApprovalId == documentApproval.Id)
                .FirstOrDefault();

            var personalData = PersonalDataRepository.Fetch()
                .Where(x => x.NoReg == documentApproval.CreatedBy && x.RowStatus)
                .FirstOrDefault();

            var objAddress = JsonConvert.DeserializeObject<AddressViewModel>(documentRequestDetail.ObjectValue);

            var personalDataCommonAttributeUpdate = PersonalDataCommonAttributeRepository.Fetch()
                .Where(x =>x.Id == personalData.CommonAttributeId && x.EndDate == DateTime.Parse("9999-12-31") && x.RowStatus)
                .FirstOrDefault();

            if (personalDataCommonAttributeUpdate != null)
            {
                personalDataCommonAttributeUpdate.EndDate = DateTime.Now.Date.AddDays(-1);

                PersonalDataCommonAttributeRepository.Upsert<Guid>(personalDataCommonAttributeUpdate);
            }

            var personalDataCommonAttributeAdd = new PersonalDataCommonAttribute()
            {
                Name = personalDataCommonAttributeUpdate.Name,
                Nik = objAddress.PopulationNumber,
                KKNumber = objAddress.FamilyCardNumber,
                BirthDate = personalDataCommonAttributeUpdate.BirthDate,
                BirthPlace = personalDataCommonAttributeUpdate.BirthPlace,
                NationalityCode = personalDataCommonAttributeUpdate.NationalityCode,
                GenderCode = personalDataCommonAttributeUpdate.GenderCode,
                CountryOfBirthCode = personalDataCommonAttributeUpdate.CountryOfBirthCode,
                Address = objAddress.CompleteAddress,
                SubDistrictCode = objAddress.SubDistrictCode,
                CityCode = objAddress.City,
                PostalCode = objAddress.PostalCode,
                RegionCode = objAddress.Provice,
                CountryCode = personalDataCommonAttributeUpdate.CountryOfBirthCode,
                DistrictCode = objAddress.DistrictCode,
                Rt = objAddress.RT,
                Rw = objAddress.RW,
                ReligionCode = personalDataCommonAttributeUpdate.ReligionCode,
                BloodTypeCode = personalDataCommonAttributeUpdate.BloodTypeCode,
                StartDate = DateTime.Now.Date,
                EndDate = DateTime.Parse("9999-12-31")
            };

            PersonalDataCommonAttributeRepository.Add(personalDataCommonAttributeAdd);

            UnitOfWork.SaveChanges();

            personalData.CommonAttributeId = personalDataCommonAttributeAdd.Id;
            
            PersonalDataRepository.Upsert<Guid>(personalData);
            
            UnitOfWork.SaveChanges();
        }
        public IEnumerable<PersonalDataAllStoredEntity> GetPersonalDataByUser(string noreg)
        {
            return UnitOfWork.UdfQuery<PersonalDataAllStoredEntity>(new { noreg });
        }
        public void CompleteBankAccount(string noregCurentApprover, DocumentApproval documentApproval)
        {
            var actualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Find(x => x.NoReg == documentApproval.CreatedBy).FirstOrDefaultIfEmpty();
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            var personalData = PersonalDataRepository.Fetch().Where(x => x.NoReg == documentApproval.CreatedBy && x.RowStatus).FirstOrDefault();
            var objBankAccount = JsonConvert.DeserializeObject<BankDetailViewModel>(documentRequestDetail.ObjectValue);

            if (personalData != null)
            {
                var personalDataBankAccountUpdate = PersonalDataBankAccountRepository.Fetch()
                    .Where(x => x.NoReg == documentApproval.CreatedBy && x.RowStatus)
                    .FirstOrDefault();

                Assert.ThrowIf(personalDataBankAccountUpdate == null, "Bank account must be defined");

                personalDataBankAccountUpdate.EndDate = DateTime.Now.Date.AddDays(-1);
                personalDataBankAccountUpdate.RowStatus = false;

                PersonalDataBankAccountRepository.Upsert<Guid>(personalDataBankAccountUpdate);
            }

            var personalDataBankAccount = new PersonalDataBankAccount()
            {
                NoReg = documentApproval.CreatedBy,
                AccountName = objBankAccount.AccountName,
                AccountNumber = objBankAccount.AccountNumber,
                BankCode = objBankAccount.KeyBank,
                StartDate = DateTime.Now.Date,
                EndDate = DateTime.Parse("9999-12-31")
            };

            PersonalDataBankAccountRepository.Add(personalDataBankAccount);

            UnitOfWork.SaveChanges();
        }

        public void CompleteTaxStatus(string noregCurentApprover, DocumentApproval documentApproval)
        {
            var actualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Find(x => x.NoReg == documentApproval.CreatedBy).FirstOrDefaultIfEmpty();
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            var objTaxStatus = JsonConvert.DeserializeObject<TaxStatusViewModel>(documentRequestDetail.ObjectValue);
            var dataGender = (
                from p in PersonalDataRepository.Fetch()
                join pc in PersonalDataCommonAttributeRepository.Fetch()
                on p.CommonAttributeId equals pc.Id
                where pc.StartDate <= DateTime.Now && pc.EndDate >= DateTime.Now && pc.RowStatus && p.NoReg == documentApproval.CreatedBy
                select pc.GenderCode
            ).FirstOrDefault();

            var personalDataTaxStatusUpdate = PersonalDataTaxStatusRepository.Fetch()
                .Where(x => x.NoReg == documentApproval.CreatedBy && x.EndDate == DateTime.Parse("9999-12-31") && x.RowStatus)
                .FirstOrDefault();

            if (personalDataTaxStatusUpdate != null)
            {
                personalDataTaxStatusUpdate.EndDate = DateTime.Now.Date.AddDays(-1);

                PersonalDataTaxStatusRepository.Upsert<Guid>(personalDataTaxStatusUpdate);
            }

            var personalDataTaxStatus = new PersonalDataTaxStatus()
            {
                NoReg = documentApproval.CreatedBy,
                Npwp = objTaxStatus.NPWPNumber,
                TaxStatus = dataGender == "lakilaki" ? objTaxStatus.StatusTax : "TK0",
                TaxPtkp = dataGender == "lakilaki" ? objTaxStatus.StatusTax : "TK0",
                StartDate = DateTime.Now.Date,
                EndDate = DateTime.Parse("9999-12-31")
            };

            PersonalDataTaxStatusRepository.Add(personalDataTaxStatus);

            UnitOfWork.SaveChanges();
        }

        public void CompleteEducation(string noregCurentApprover, DocumentApproval documentApproval)
        {
            var actualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Find(x => x.NoReg == documentApproval.CreatedBy).FirstOrDefaultIfEmpty();
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            var objEducation = JsonConvert.DeserializeObject<EducationViewModel>(documentRequestDetail.ObjectValue);

            var personalDataEducation = new PersonalDataEducation()
            {
                NoReg = documentApproval.CreatedBy,
                EducationTypeCode = objEducation.LevelOfEducationCode,
                Institute = objEducation.IsOtherCollegeName ? objEducation.OtherCollegeName : objEducation.CollegeName,
                CountryCode = "",
                Major = objEducation.DepartmentCode,
                FinalGrade = objEducation.GPA.ToString(),
                GraduationDate = objEducation.Period.Value,
                StartDate = DateTime.Now.Date,
                EndDate = DateTime.Parse("9999-12-31")
            };

            PersonalDataEducationRepository.Add(personalDataEducation);

            UnitOfWork.SaveChanges();
        }

        public PersonalDataCommonAttribute GetFamilyMemberDetail(Guid Id)
        {
            var data = (
                from fm in PersonalDataFamilyMemberRepository.Fetch()
                join ca in PersonalDataCommonAttributeRepository.Fetch()
                on fm.CommonAttributeId equals ca.Id
                join gc in GeneralCategoryRepository.Fetch()
                on fm.FamilyTypeCode equals gc.Code
                where fm.Id == Id && (fm.StartDate <= DateTime.Now && fm.EndDate >= DateTime.Now) && fm.RowStatus /*&& gc.Category == "familyTypeCode"*/
                select new PersonalDataCommonAttribute
                {
                    Id = fm.Id,
                    Name = ca.Name + " - " + gc.Name,
                }
            ).FirstOrDefault();

            return data;
        }

        public IQueryable<object> GetFamilyMembers(string noreg)
        {
            var data = from fm in PersonalDataFamilyMemberRepository.Fetch().AsNoTracking()
                       join ca in PersonalDataCommonAttributeRepository.Fetch().AsNoTracking()
                       on fm.CommonAttributeId equals ca.Id
                       join gc in GeneralCategoryRepository.Fetch().AsNoTracking()
                       on fm.FamilyTypeCode equals gc.Code
                       where fm.NoReg == noreg && fm.RowStatus && gc.Category == "familyTypeCode"
                       select new
                       {
                           fm.Id,
                           ca.Name,
                           FullName = ca.Name + " - " + gc.Name
                       };

            return data;
        }

        public PersonalDataFamilyMember GetFamilyMember(Guid Id)
        {
            var data = PersonalDataFamilyMemberRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == Id);

            return data;
        }

        public IQueryable<object> GetObjFamilyMemberBYNoreg(string noreg)
        {
            var data = from fm in PersonalDataFamilyMemberRepository.Fetch().AsNoTracking()
                       join ca in PersonalDataCommonAttributeRepository.Fetch().AsNoTracking()
                       on fm.CommonAttributeId equals ca.Id
                       join gc in GeneralCategoryRepository.Fetch().AsNoTracking()
                       on fm.FamilyTypeCode equals gc.Code
                       where fm.NoReg == noreg && (fm.StartDate <= DateTime.Now && fm.EndDate >= DateTime.Now) && fm.RowStatus && gc.Category == "familyTypeCode" && gc.Code != "orangtua"
                       select new
                       {
                           FamilyMemberId = fm.Id,
                           Name = ca.Name,
                           FullName = ca.Name + " - " + gc.Name
                       };

            //cek di document approval
            var data2 = (from da in DocumentApprovalRepository.Fetch().AsNoTracking()
                         join d in DocumentRequestDetailRepository.Fetch().AsNoTracking()
                         on da.Id equals d.DocumentApprovalId
                         where da.CreatedBy == noreg && da.Title.Contains("Death or Dismemberment") && da.RowStatus && da.DocumentStatusCode == "inprogress"
                         select JsonConvert.DeserializeObject<DismembermentViewModel>(d.ObjectValue)).ToList();

            // ❌ SEBELUMNYA (biarkan sebagai catatan saja)
            // data = data.Where(x => !data2.Any(y => y.OtherFamilyId == x.FamilyMemberId.ToString()));
            data = data.AsEnumerable()
                        .Where(x => !data2.Any(y => y.OtherFamilyId == x.FamilyMemberId.ToString()))
                        .AsQueryable();

            // ✅ SESUDAH — samakan tipe: bandingkan Guid ↔ Guid
            var excludedFamilyIds = data2
                .Select(d => d.OtherFamilyId)
                .Select(s => { Guid g; return Guid.TryParse(s, out g) ? g : Guid.Empty; })
                .Where(g => g != Guid.Empty)
                .ToList();

            data = data.Where(x => !excludedFamilyIds.Contains(x.FamilyMemberId));


            return data;
        }

        public IQueryable<object> GetNonMainFamily(string noreg)
        {
            var data = (from gc in GeneralCategoryRepository.Fetch().AsNoTracking()
                        where gc.Category == "nonmainfamily"
                        && !(
                                from generalCategory in GeneralCategoryRepository.Fetch().AsNoTracking()
                                join fm in PersonalDataFamilyMemberRepository.Fetch().AsNoTracking()
                                on generalCategory.Code equals fm.FamilyTypeCode
                                where generalCategory.Category == "nonmainfamily" 
                                        && fm.EndDate <= DateTime.Now 
                                        &&  fm.NoReg == noreg
                                select generalCategory.Code
                            ).Contains(gc.Code)
                        select new
                        {
                            Code = gc.Code,
                            Name = gc.Name
                        }).Distinct().OrderBy(x => x.Name);

            return data;
        }

        public IQueryable<object> GetObjInfoFamTypeCode(Guid famMemberId)
        {
            var data = from fm in PersonalDataFamilyMemberRepository.Fetch().AsNoTracking()
                       join ca in PersonalDataCommonAttributeRepository.Fetch().AsNoTracking()
                       on fm.CommonAttributeId equals ca.Id
                       where fm.Id == famMemberId
                       select new
                       {
                           FamilyTypeCode = fm.FamilyTypeCode,
                           GenderCode = ca.GenderCode
                       };

            return data;
        }

        public IQueryable<object> GetGender(string Noreg)
        {
            var data = from PD in PersonalDataRepository.Fetch().AsNoTracking()
                       join CA in PersonalDataCommonAttributeRepository.Fetch().AsNoTracking()
                       on PD.CommonAttributeId equals CA.Id
                       where PD.NoReg == Noreg
                       select new
                       {
                           GenderCode = CA.GenderCode
                       };

            return data;
        }
        private static readonly IReadOnlyList<string> _familyMemberFormKey = new List<string>()
        {
            "divorce"
        };
        public Result<object> GetPasanganByNoReg(string noReg)
        {
            var currentUser = UserRepository.Fetch().AsNoTracking().Where(x => x.NoReg == noReg)
                .FirstOrDefault();

            if (currentUser is null)
                return Error.NotFound("Invalid user");

            var spouse = PersonalDataFamilyMemberRepository.Fetch().AsNoTracking()
                .Where(x => x.NoReg == noReg &&
                    x.StartDate <= DateTime.Now &&
                    x.EndDate >= DateTime.Now &&
                    x.RowStatus &&
                    x.FamilyTypeCode == EnumHelper.FamilyTypeCode.suamiistri.ToString())
                .Join(PersonalDataCommonAttributeRepository.Fetch().AsNoTracking(),
                x => x.CommonAttributeId,
                x => x.Id,
                (x, y) => new { FamilyMemberId = x.Id, Name = y.Name, Date = x.StartDate }).FirstOrDefault();



            var divorces = UnitOfWork.GetRepository<Form>().Fetch().AsNoTracking()
                    .Where(x => _familyMemberFormKey.Contains(x.FormKey))
                    .Join(UnitOfWork.GetRepository<DocumentApproval>().Fetch().AsNoTracking()
                        .Where(x => x.CreatedBy == noReg && x.RowStatus && (x.DocumentStatusCode == "inprogress" || x.DocumentStatusCode == "draft")),
                        x => x.Id,
                        x => x.FormId,
                        (x, y) => y)
                    .Join(UnitOfWork.GetRepository<DocumentRequestDetail>().Fetch().AsNoTracking(),
                        x => x.Id,
                        x => x.DocumentApprovalId,
                        (x, y) => new { x.DocumentNumber, y.ObjectValue })
                    .AsEnumerable()
                    .Select(x =>
                    {
                        return new { DocumentNumber = x.DocumentNumber, DivorceMember = JsonConvert.DeserializeObject<DivorceViewMember>(x.ObjectValue) };
                    })
                    .Where(x => x.DivorceMember.PartnerId.HasValue && x.DivorceMember.PartnerId != Guid.Empty).ToList();

            if (spouse is null)
            {
                string _user = _localizer["User"].Value;
                string _text = _localizer["is not married, cannot create request."].Value;
                return Error.NotFound($"{_user}  <b class=\"font-red\">{currentUser.Name}</b> {_text}");
            }

            var divorced = divorces.Where(x => x.DivorceMember.PartnerId == spouse.FamilyMemberId).FirstOrDefault();
            if (divorced != null)
                return DivorceErrors.AlreadyRequested(spouse.Name, divorced.DocumentNumber);

            return spouse;

        }
        public IQueryable<object> GetFamilyMemberPasanganBYNoreg(string Noreg)
        {
            var data = from fm in PersonalDataFamilyMemberRepository.Fetch().AsNoTracking()
                       join ca in PersonalDataCommonAttributeRepository.Fetch().AsNoTracking()
                       on fm.CommonAttributeId equals ca.Id
                       where fm.NoReg == Noreg && (fm.StartDate <= DateTime.Now && fm.EndDate >= DateTime.Now) && fm.RowStatus
                       && fm.FamilyTypeCode == EnumHelper.FamilyTypeCode.suamiistri.ToString()
                       select new
                       {
                           FamilyMemberId = fm.Id,
                           Name = ca.Name,
                           Date = fm.StartDate
                       };

            var data2 = (
                from da in DocumentApprovalRepository.Fetch().AsNoTracking()
                join d in DocumentRequestDetailRepository.Fetch().AsNoTracking()
                on da.Id equals d.DocumentApprovalId
                where da.CreatedBy == Noreg && da.Title.Contains("Death or Dismemberment") && da.RowStatus && da.DocumentStatusCode == "inprogress"
                select JsonConvert.DeserializeObject<DismembermentViewModel>(d.ObjectValue)
            ).ToList();

            //data = data.Where(x => !data2.Any(y => y.OtherFamilyId == x.FamilyMemberId.ToString()));
            data = data.AsEnumerable()
                        .Where(x => !data2.Any(y => y.OtherFamilyId == x.FamilyMemberId.ToString()))
                        .AsQueryable();

            var excludedFamilyIds = data2
                .Select(d => d.OtherFamilyId)
                .Select(s => { Guid g; return Guid.TryParse(s, out g) ? g : Guid.Empty; })
                .Where(g => g != Guid.Empty)
                .ToList();

            data = data.Where(x => !excludedFamilyIds.Contains(x.FamilyMemberId));

            return data;
        }

        public IQueryable<object> GetMaritalStatusBYNoreg(string Noreg)
        {
            var data = from fm in PersonalDataRepository.Fetch().AsNoTracking()
                       where fm.NoReg == Noreg && fm.RowStatus
                       && fm.MaritalStatusCode == EnumHelper.MaritalStatus.menikah.ToString()
                       select new
                       {
                           Id = fm.Id,
                           CommonAttributeId = fm.CommonAttributeId,
                           NoReg = fm.NoReg
                       };

            return data;
        }

        public IQueryable<object> GetHospitalAddress(string HospitalName)
        {
            var data = from fm in HospitalRepository.Fetch().AsNoTracking()
                       where fm.Name == HospitalName && fm.RowStatus
                       select new
                       {
                           HospitalName = fm.Name,
                           HospitalAddress = fm.Address,
                           HospitalCity = fm.City
                       };

            return data;
        }

        public IQueryable<object> GetInfo(string Noreg)
        {
            var now = DateTime.Now;
            var data = from aoe in ActualOrganizationStructureRepository.Fetch().AsNoTracking()
                       join ts in PersonalDataTaxStatusRepository.Fetch().AsNoTracking()
                       on aoe.NoReg equals ts.NoReg  
                       join ad in AllowanceDetailRepository.Fetch().AsNoTracking()
                       on ts.TaxStatus equals ad.SubType
                       join np in EmployeeSubgroupNPRepository.Fetch().AsNoTracking()
                       on aoe.EmployeeSubgroup equals np.EmployeeSubgroup
                       where aoe.NoReg == Noreg && ts.RowStatus && (np.NP <= ad.ClassTo) && ad.Type == "medicalbpk"
                       orderby ts.EndDate descending
                       select new
                       {
                           Name = aoe.Name,
                           Kelas = aoe.EmployeeSubgroupText,
                           NP = np.NP,
                           Ammount = ad.Ammount,
                           TaxStatus = ts.TaxStatus
                       };


            return data;
        }

        public IQueryable<object> GetInfoEmployee(string Noreg)
        {
            var data = from aoe in ActualOrganizationStructureRepository.Fetch().AsNoTracking()
                       join ts in PersonalDataTaxStatusRepository.Fetch().AsNoTracking()
                       on aoe.NoReg equals ts.NoReg
                       join np in EmployeeSubgroupNPRepository.Fetch().AsNoTracking()
                       on aoe.EmployeeSubgroup equals np.EmployeeSubgroup
                       where aoe.NoReg == Noreg && ts.RowStatus && ts.StartDate <= DateTime.Now && ts.EndDate >= DateTime.Now
                       select new
                       {
                           Name = aoe.Name,
                           Kelas = aoe.EmployeeSubgroupText,
                           NP = np.NP,
                           TaxStatus = ts.TaxStatus
                       };

            return data;
        }

        public PersonalDataCommonAttribute GetPersonalDataAttribute(string noReg)
        {
            var data = (
                from p in PersonalDataRepository.Fetch().AsNoTracking()
                join pc in PersonalDataCommonAttributeRepository.Fetch().AsNoTracking()
                on p.CommonAttributeId equals pc.Id
                where p.RowStatus && pc.RowStatus && p.NoReg == noReg && pc.StartDate <= DateTime.Now && pc.EndDate >= DateTime.Now
                select pc
            ).FirstOrDefault();

            return data;
        }

        public IQueryable<object> GetInfoTitle(string formKey)
        {
            var data = from form in FormRepository.Fetch().AsNoTracking()
                       where form.FormKey == formKey
                       select new
                       {
                           Title = form.Title,
                       };

            return data;
        }

        public IQueryable<object> GetInfoMedicalBPK(int NP, string NewTaxStatus)
        {
            var data = from ad in AllowanceDetailRepository.Fetch().AsNoTracking()
                       where (NP >= ad.ClassFrom && NP <= ad.ClassTo) && ad.Type == "medicalbpk" && ad.SubType == NewTaxStatus
                       select new
                       {
                           AmmountTunjangan = ad.Ammount,
                           Description = ad.Description
                       };

            return data;
        }

        public IQueryable<object> GetInfoTunjangan(int NP)
        {
            var data = from ad in AllowanceDetailRepository.Fetch().AsNoTracking()
                       where (NP >= ad.ClassFrom && NP <= ad.ClassTo) && ad.Type == "marriageallowance"
                       select new
                       {
                           AmmountTunjangan = ad.Ammount
                       };

            return data;
        }

        public IQueryable<object> GetInfoBPJS()
        {
            var data = from ad in GeneralCategoryRepository.Fetch().AsNoTracking()
                       where ad.Category == "MedicalBenefitType" && ad.Code == "bpjs"
                       select new
                       {
                           Name = ad.Name
                       };

            return data;
        }

        public IQueryable<object> GetInfoAVIVA()
        {
            var data = from ad in GeneralCategoryRepository.Fetch().AsNoTracking()
                       where ad.Category == "MedicalBenefitType" && ad.Code == "aviva"
                       select new
                       {
                           Name = ad.Name
                       };

            return data;
        }

        public IQueryable<object> GetCountChild(string Noreg)
        {
            var data = from aa in PersonalDataFamilyMemberRepository.Fetch().AsNoTracking()
                       where aa.NoReg == Noreg &&  (aa.FamilyTypeCode == "anakkandung" || aa.FamilyTypeCode == "anakangkat")
                       select aa.NoReg;
            return data;
        }

        public bool IsGetAllowanceMarriage(string Noreg)
        {
            var dataMarriageAllowance = from da in DocumentApprovalRepository.Fetch().AsNoTracking()
                                        join f in FormRepository.Fetch().AsNoTracking()
                                        on da.FormId equals f.Id
                                        where f.FormKey == "marriage-allowance" && da.CreatedBy == Noreg && (da.DocumentStatusCode == "completed" || da.DocumentStatusCode == "inprogress")
                                        select da.Id;

            return dataMarriageAllowance.Count() == 0;
        }

        public void PreValidateMarriageStatus(string noreg)
        {
            var message = string.Empty;

            //cek personal data
            var dataPersonalData = PersonalDataRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.RowStatus)
                .FirstOrDefault();

            //if (dataPersonalData == null) {
            //    message = $"User {noreg} tidak mempunyai personal data.";
            //}
            if (dataPersonalData != null)
            {
                if (dataPersonalData.MaritalStatusCode == "menikah")
                {
                    var dataUser = from x in UserRepository.Fetch().AsNoTracking()
                                   where x.NoReg == noreg
                                   select new
                                   {
                                       Name = x.Name
                                   };

                    //message = $"User  <b class=\"font-red\">{dataUser.FirstOrDefault().Name}</b> sudah menikah, tidak dapat mengajukan perubahan data.";
                    string _user = _localizer["User"].Value;
                    string _text = _localizer["is married, cannot file data changes."].Value;
                    message = $"{_user}  <b class=\"font-red\">{dataUser.FirstOrDefault().Name}</b> {_text}";
                }
            }

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        public void PreValidateDivorceStatus(string noreg)
        {
            var message = string.Empty;

            //cek personal data
            var dataPersonalData = PersonalDataRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.RowStatus)
                .FirstOrDefault();

            if (dataPersonalData != null)
            {
                if (dataPersonalData.MaritalStatusCode != "menikah")
                {
                    var dataUser = from x in UserRepository.Fetch().AsNoTracking()
                                   where x.NoReg == noreg
                                   select new
                                   {
                                       Name = x.Name
                                   };

                    var _user = _localizer["User"].Value;
                    var _text = _localizer["is not married, cannot create request."].Value;

                    message = $"{_user}  <b class=\"font-red\">{dataUser.FirstOrDefault().Name}</b> {_text}";
                }
            }

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        public void PreValidateTaxstatusComplete(string noreg)
        {
            var message = string.Empty;
            //data
            var dataTax = (from da in DocumentApprovalRepository.Fetch().AsNoTracking()
                           join f in FormRepository.Fetch().AsNoTracking()
                           on da.FormId equals f.Id
                           where da.RowStatus && da.CreatedBy == noreg && (da.DocumentStatusCode == "draft" || da.DocumentStatusCode == "inprogress") &&
                           f.FormKey == "tax-status"
                           select da.Id).Count();

            if (dataTax > 0)
                message = _localizer["Tax Status Request must be completed before create request"].Value;

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        public void PreValidateMarriageComplete(string noreg)
        {
            var message = string.Empty;
            //data
            var dataTax = (from da in DocumentApprovalRepository.Fetch().AsNoTracking()
                           join f in FormRepository.Fetch().AsNoTracking()
                           on da.FormId equals f.Id
                           where da.RowStatus && da.CreatedBy == noreg && (da.DocumentStatusCode == "draft" || da.DocumentStatusCode == "inprogress") &&
                           f.FormKey == "marriage-status"
                           select da.Id).Count();

            if (dataTax > 0)
                message = _localizer["Marriage Status Request must be completed before create request"].Value;

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        public void PreValidateDivorceComplete(string noreg)
        {
            var message = string.Empty;
            //data
            var dataTax = (from da in DocumentApprovalRepository.Fetch().AsNoTracking()
                           join f in FormRepository.Fetch().AsNoTracking()
                           on da.FormId equals f.Id
                           where da.RowStatus && da.CreatedBy == noreg && (da.DocumentStatusCode == "draft" || da.DocumentStatusCode == "inprogress") &&
                           f.FormKey == "divorce"
                           select da.Id).Count();

            if (dataTax > 0)
                message = _localizer["Divorce Request must be completed before create request"].Value;

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        public void PreValidateDismembermentComplete(string noreg)
        {
            var message = string.Empty;
            //data
            var dataTax = (from da in DocumentApprovalRepository.Fetch().AsNoTracking()
                           join f in FormRepository.Fetch().AsNoTracking()
                           on da.FormId equals f.Id
                           where da.RowStatus && da.CreatedBy == noreg && (da.DocumentStatusCode == "draft" || da.DocumentStatusCode == "inprogress") &&
                           f.FormKey == "condolance"
                           select da.Id).Count();

            if (dataTax > 0)
                message = _localizer["Condolence Request must be completed before create request"].Value;

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        public void PreValidateBirthComplete(string noreg)
        {
            var message = string.Empty;
            //data
            var dataTax = (from da in DocumentApprovalRepository.Fetch().AsNoTracking()
                           join f in FormRepository.Fetch().AsNoTracking()
                           on da.FormId equals f.Id
                           where da.RowStatus && da.CreatedBy == noreg && (da.DocumentStatusCode == "draft" || da.DocumentStatusCode == "inprogress") &&
                           f.FormKey == "family-registration"
                           select da.Id).Count();

            if (dataTax > 0)
                message = _localizer["Family Registration Request must be completed before create request"].Value;

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        #region PersonalData
        /// <summary>
        /// Update or insert PersonalData
        /// </summary>
        /// <param name="PersonalData">PersonalData Object</param>
        public void UpsertPersonalData(PersonalData dbitem)
        {
            PersonalDataRepository.Upsert<Guid>(dbitem);

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Soft delete PersonalData by id and its dependencies if any
        /// </summary>
        /// <param name="id">PersonalData Id</param>
        public void SoftDeletePersonalData(Guid id)
        {
            var menu = PersonalDataRepository.FindById(id);

            menu.RowStatus = false;

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Delete PersonalData by id and its dependencies if any
        /// </summary>
        /// <param name="id">PersonalData Id</param>
        public void DeletePersonalData(Guid id)
        {
            PersonalDataRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        }
        #endregion

        #region PersonalDataBankAccount
        /// <summary>
        /// Update or insert PersonalDataBankAccount
        /// </summary>
        /// <param name="PersonalDataBankAccount">PersonalDataBankAccount Object</param>
        public void UpsertPersonalDataBankAccount(PersonalDataBankAccount dbitem)
        {
            PersonalDataBankAccountRepository.Upsert<Guid>(dbitem);

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Soft delete PersonalDataBankAccount by id and its dependencies if any
        /// </summary>
        /// <param name="id">PersonalDataBankAccount Id</param>
        public void SoftDeletePersonalDataBankAccount(Guid id)
        {
            var menu = PersonalDataBankAccountRepository.FindById(id);

            menu.RowStatus = false;

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Delete PersonalDataBankAccount by id and its dependencies if any
        /// </summary>
        /// <param name="id">PersonalDataBankAccount Id</param>
        public void DeletePersonalDataBankAccount(Guid id)
        {
            PersonalDataBankAccountRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        }
        #endregion
         
        #region PersonalDataBpjs
        public IQueryable<PersonalDataBpjs> GetPersonalDataBpjsQuery() => PersonalDataBpjsRepository.Fetch().Where(x => x.RowStatus);
        //public IQueryable<PersonalDataBpjs> GetPersonalDataBpjsByListId(List<Guid> listId) => PersonalDataBpjsRepository.Fetch().Where(x => listId.Contains(x.Id));
        public IQueryable<PersonalDataBpjs> GetPersonalDataBpjsByListId(List<Guid> listId)
        {
            if (listId == null || listId.Count == 0)
            {
                // return query kosong, bukan null supaya aman dipanggil
                return Enumerable.Empty<PersonalDataBpjs>().AsQueryable();
            }
            var ids = listId.AsQueryable();

            return from p in PersonalDataBpjsRepository.Fetch()
                   join i in ids on p.Id equals i
                   select p;
        }
        public IEnumerable<PersonalDataBpjsStoredEntity> GetDataBpjs() => UnitOfWork.UspQuery<PersonalDataBpjsStoredEntity>();
        public IEnumerable<PersonalDataInsuranceStoredEntity> GetDataInsurance() => UnitOfWork.UspQuery<PersonalDataInsuranceStoredEntity>();
        public PersonalDataInsurance GetPersonalDataInsurance(string noreg, Guid? id, DateTime keyDate)
        {
            var insurance = PersonalDataInsuranceRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.FamilyMemberId == id && keyDate >= x.StartDate && keyDate <= x.EndDate)
                .FirstOrDefaultIfEmpty();

            return insurance;
        }

        /// <summary>
        /// Update or insert PersonalDataBpjs
        /// </summary>
        /// <param name="PersonalDataBpjs">PersonalDataBpjs Object</param>
        public void UpsertPersonalDataBpjs(PersonalDataBpjs dbitem)
        {
            PersonalDataBpjsRepository.Upsert<Guid>(dbitem);

            UnitOfWork.SaveChanges();
        }
        public void UpsertMultiplePersonalDataBpjs(List<PersonalDataBpjs> dbitem)
        {
            dbitem.ForEach(x => PersonalDataBpjsRepository.Upsert<Guid>(x));

            UnitOfWork.SaveChanges();
        }
        /// <summary>
        /// Soft delete PersonalDataBpjs by id and its dependencies if any
        /// </summary>
        /// <param name="id">PersonalDataBpjs Id</param>
        public void SoftDeletePersonalDataBpjs(Guid id)
        {
            var menu = PersonalDataBpjsRepository.FindById(id);

            menu.RowStatus = false;

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Delete PersonalDataBpjs by id and its dependencies if any
        /// </summary>
        /// <param name="id">PersonalDataBpjs Id</param>
        public void DeletePersonalDataBpjs(Guid id)
        {
            PersonalDataBpjsRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        }
        #endregion

        #region PersonalDataCommonAttribute
        /// <summary>
        /// Update or insert PersonalDataCommonAttribute
        /// </summary>
        /// <param name="PersonalDataCommonAttribute">PersonalDataCommonAttribute Object</param>
        public void UpsertPersonalDataCommonAttribute(PersonalDataCommonAttribute dbitem)
        {
            PersonalDataCommonAttributeRepository.Upsert<Guid>(dbitem);

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Soft delete PersonalDataCommonAttribute by id and its dependencies if any
        /// </summary>
        /// <param name="id">PersonalDataCommonAttribute Id</param>
        public void SoftDeletePersonalDataCommonAttribute(Guid id)
        {
            var menu = PersonalDataCommonAttributeRepository.FindById(id);

            menu.RowStatus = false;

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Delete PersonalDataCommonAttribute by id and its dependencies if any
        /// </summary>
        /// <param name="id">PersonalDataCommonAttribute Id</param>
        public void DeletePersonalDataCommonAttribute(Guid id)
        {
            PersonalDataCommonAttributeRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        }
        #endregion

        #region PersonalDataEducation
        /// <summary>
        /// Update or insert PersonalDataEducation
        /// </summary>
        /// <param name="PersonalDataEducation">PersonalDataEducation Object</param>
        public void UpsertPersonalDataEducation(PersonalDataEducation dbitem)
        {
            PersonalDataEducationRepository.Upsert<Guid>(dbitem);

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Soft delete PersonalDataEducation by id and its dependencies if any
        /// </summary>
        /// <param name="id">PersonalDataEducation Id</param>
        public void SoftDeletePersonalDataEducation(Guid id)
        {
            var menu = PersonalDataEducationRepository.FindById(id);

            menu.RowStatus = false;

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Delete PersonalDataEducation by id and its dependencies if any
        /// </summary>
        /// <param name="id">PersonalDataEducation Id</param>
        public void DeletePersonalDataEducation(Guid id)
        {
            PersonalDataEducationRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        }
        #endregion

        #region PersonalDataFamilyMember
        /// <summary>
        /// Update or insert PersonalDataFamilyMember
        /// </summary>
        /// <param name="PersonalDataFamilyMember">PersonalDataFamilyMember Object</param>
        public void UpsertPersonalDataFamilyMember(PersonalDataFamilyMember dbitem)
        {
            PersonalDataFamilyMemberRepository.Upsert<Guid>(dbitem);

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Soft delete PersonalDataFamilyMember by id and its dependencies if any
        /// </summary>
        /// <param name="id">PersonalDataFamilyMember Id</param>
        public void SoftDeletePersonalDataFamilyMember(Guid id)
        {
            var menu = PersonalDataFamilyMemberRepository.FindById(id);

            menu.RowStatus = false;

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Delete PersonalDataFamilyMember by id and its dependencies if any
        /// </summary>
        /// <param name="id">PersonalDataFamilyMember Id</param>
        public void DeletePersonalDataFamilyMember(Guid id)
        {
            PersonalDataFamilyMemberRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        }
        #endregion

        #region PersonalDataInsurance
        public IQueryable<PersonalDataInsurance> GetPersonalDataInsurancesQuery() => PersonalDataInsuranceRepository.Fetch().Where(x => x.RowStatus);
        public PersonalDataInsurance GetPersonalDataInsurancesByNoreg(string Noreg, Guid? familyMemberId) => PersonalDataInsuranceRepository.Fetch().FirstOrDefault(x => x.RowStatus && x.CompleteStatus && x.NoReg == Noreg && x.FamilyMemberId == familyMemberId);
        //public IQueryable<PersonalDataInsurance> GetPersonalDataInsurancesByListId(List<Guid> listId) => PersonalDataInsuranceRepository.Fetch().Where(x => listId.Contains(x.Id));
        public IQueryable<PersonalDataInsurance> GetPersonalDataInsurancesByListId(List<Guid> listId)
        {
            if (listId == null || listId.Count == 0)
            {
                // return query kosong, bukan null supaya aman dipanggil
                return Enumerable.Empty<PersonalDataInsurance>().AsQueryable();
            }

            var ids = listId.AsQueryable();

            return from p in PersonalDataInsuranceRepository.Fetch()
                   join i in ids on p.Id equals i
                   select p;
        }
        /// <summary>
        /// Update or insert PersonalDataInsurance
        /// </summary>
        /// <param name="PersonalDataInsurance">PersonalDataInsurance Object</param>
        public void UpsertPersonalDataInsurance(PersonalDataInsurance dbitem)
        {
            PersonalDataInsuranceRepository.Upsert<Guid>(dbitem);

            UnitOfWork.SaveChanges();
        }
        public void UpsertMultiplePersonalDataInsurance(List<PersonalDataInsurance> dbitem)
        {
            dbitem.ForEach(x => PersonalDataInsuranceRepository.Upsert<Guid>(x));

            UnitOfWork.SaveChanges();
        }
        /// <summary>
        /// Soft delete PersonalDataInsurance by id and its dependencies if any
        /// </summary>
        /// <param name="id">PersonalDataInsurance Id</param>
        public void SoftDeletePersonalDataInsurance(Guid id)
        {
            var menu = PersonalDataInsuranceRepository.FindById(id);

            menu.RowStatus = false;

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Delete PersonalDataInsurance by id and its dependencies if any
        /// </summary>
        /// <param name="id">PersonalDataInsurance Id</param>
        public void DeletePersonalDataInsurance(Guid id)
        {
            PersonalDataInsuranceRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        }
        #endregion

        #region PersonalDataTaxStatus
        /// <summary>
        /// Update or insert PersonalDataTaxStatus
        /// </summary>
        /// <param name="PersonalDataTaxStatus">PersonalDataTaxStatus Object</param>
        public void UpsertPersonalDataTaxStatus(PersonalDataTaxStatus dbitem)
        {
            PersonalDataTaxStatusRepository.Upsert<Guid>(dbitem);

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Soft delete PersonalDataTaxStatus by id and its dependencies if any
        /// </summary>
        /// <param name="id">PersonalDataTaxStatus Id</param>
        public void SoftDeletePersonalDataTaxStatus(Guid id)
        {
            var menu = PersonalDataTaxStatusRepository.FindById(id);

            menu.RowStatus = false;

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Delete PersonalDataTaxStatus by id and its dependencies if any
        /// </summary>
        /// <param name="id">PersonalDataTaxStatus Id</param>
        public void DeletePersonalDataTaxStatus(Guid id)
        {
            PersonalDataTaxStatusRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        }
        #endregion

        #region Medical History
        public IQueryable<PersonalDataMedicalHistory> GetMedicalHistories(string noreg)
        {
            return MedicalHistoryReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.RowStatus);
        }
        #endregion

        #region Drivers License
        public bool SaveDriversLicense(DriverLicenseViewModel driversLicense)
        {
           
                var list = new PersonalDataDriverLicense
                {
                    SimNumber = driversLicense.SimNumber,
                    SimType = driversLicense.SimType
                };
                PersonalDataDriversLicense.Add(list);
                UnitOfWork.SaveChanges();

           
            return UnitOfWork.SaveChanges() > 0;
        }
        public void UpsertPersonalDataDriversLicense(PersonalDataDriverLicense dbitem)
        {
            PersonalDataDriversLicense.Upsert<Guid>(dbitem);

            UnitOfWork.SaveChanges();
        }
        #endregion

        public Task<int> CheckBPJSInsurance()
        {
            IEnumerable<PersonalDataCheckBpjsInsuranceStoredEntity> DataCheckBpjsInsurance = UnitOfWork.UspQuery<PersonalDataCheckBpjsInsuranceStoredEntity>();
            foreach (var data in DataCheckBpjsInsurance.ToList())
            {
                string noregCurentApprover = data.LastApprovedBy;
                DocumentApproval documentApproval = UnitOfWork.GetRepository<DocumentApproval>().Fetch().Where(wh => wh.DocumentNumber==data.DocumentNumber).FirstOrDefault();

                //documentApproval.Id = data.docum
                //documentApproval.CreatedBy = data.CreatedBy;
                //documentApproval.CreatedOn = data.CreatedOn.Value;
                //documentApproval.DocumentNumber = data.DocumentNumber;

                PersonalDataFamilyMember personalDataFamilyMember = new PersonalDataFamilyMember();
                personalDataFamilyMember.Id = data.PersonalDataFamilyMemberId;
                //xxx
                var actualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Fetch()
                    .AsNoTracking()
                    .Where(x => x.NoReg == documentApproval.CreatedBy && x.Staffing == 100)
                    .FirstOrDefaultIfEmpty();

                var approverActualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Fetch()
                    .AsNoTracking()
                    .Where(x => x.NoReg == noregCurentApprover && x.Staffing == 100)
                    .FirstOrDefaultIfEmpty();

                var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
                var objBirthRegistration = JsonConvert.DeserializeObject<BirthRegistrationViewModel>(documentRequestDetail.ObjectValue);
                var personalData = PersonalDataRepository.Fetch().Where(x => x.NoReg == documentApproval.CreatedBy && x.RowStatus).FirstOrDefault();
                var personalDataCommonAttributeParent = PersonalDataCommonAttributeRepository.FindById(personalData.CommonAttributeId);

                //asuransi
                var Insurance = UnitOfWork.GetRepository<PersonalDataInsurance>().Fetch().Where(x => x.NoReg == documentApproval.CreatedBy && (x.FamilyMemberId == null || x.FamilyMemberId == Guid.Empty)).FirstOrDefault();
                string NewMemberNumber = "";
                var checkMember = UnitOfWork.GetRepository<PersonalDataInsurance>().Fetch().Where(x => x.NoReg == documentApproval.CreatedBy && x.FamilyMemberId == data.PersonalDataFamilyMemberId).FirstOrDefault();
                if (checkMember == null)
                {
                    if (Insurance != null)
                    {
                        if (!string.IsNullOrEmpty(Insurance.MemberNumber))
                        {
                            //NewMemberNumber = Insurance.MemberNumber.Substring(5, 2);
                            string MemberNumber = Insurance.MemberNumber;
                            string split = "-";
                            string[] val = MemberNumber.Split(split);
                            string value = val[1];
                            int aa = Convert.ToInt32(objBirthRegistration.AnakKe) + 1;
                            if (aa <= 9)
                            {
                                NewMemberNumber = val[0] + "-0" + aa.ToString();
                            }
                            else
                            {
                                NewMemberNumber = val[0] + "-" + aa.ToString();
                            }

                        }
                        else
                        {
                            NewMemberNumber = "";
                        }
                    }

                    var personalDataInsurance = new PersonalDataInsurance()
                    {
                        NoReg = documentApproval.CreatedBy,
                        FamilyMemberId = personalDataFamilyMember.Id,
                        MemberNumber = NewMemberNumber,
                        BenefitClassification = Insurance != null ? Insurance.BenefitClassification == null ? "" : Insurance.BenefitClassification : "",
                        StartDate = objBirthRegistration.DateOfBirth.Value,
                        EndDate = DateTime.Parse("9999-12-31"),
                        ActionType = "pendaftaran",
                        CompleteStatus = false
                    };

                    PersonalDataInsuranceRepository.Add(personalDataInsurance);
                }

                //check personal data event
                var checkPersonalDataEvent = UnitOfWork.GetRepository<PersonalDataEvent>().Fetch().Where(x => x.NoReg == documentApproval.CreatedBy && x.FamilyMemberId == data.PersonalDataFamilyMemberId).FirstOrDefault();
                if (checkPersonalDataEvent == null)
                {
                    var personalDataEvent = new PersonalDataEvent()
                    {
                        NoReg = documentApproval.CreatedBy,
                        FamilyMemberId = personalDataFamilyMember.Id,
                        EventType = "family-registration",
                        EventDate = objBirthRegistration.DateOfBirth.Value
                    };

                    PersonalDataEventRepository.Add(personalDataEvent);
                }
                

                var dataBpjs = (from bpjs in PersonalDataBpjsRepository.Fetch()
                                join fm in PersonalDataFamilyMemberRepository.Fetch()
                                on bpjs.FamilyMemberId equals fm.Id
                                where bpjs.NoReg == documentApproval.CreatedBy && (fm.FamilyTypeCode == "anakkandung" || fm.FamilyTypeCode == "anakangkat") && (fm.StartDate <= DateTime.Now && fm.EndDate >= DateTime.Now)
                                select bpjs).Count();

                if (dataBpjs < 3)
                {
                    var checkMemberBpjs = UnitOfWork.GetRepository<PersonalDataBpjs>().Fetch().Where(x => x.NoReg == documentApproval.CreatedBy && x.FamilyMemberId == data.PersonalDataFamilyMemberId).FirstOrDefault();
                    if (checkMemberBpjs == null)
                    {
                        var personalDataBpjs = new PersonalDataBpjs()
                        {
                            NoReg = documentApproval.CreatedBy,
                            FamilyMemberId = personalDataFamilyMember.Id,
                            BpjsNumber = "",
                            FaskesCode = "",
                            Telephone = "",
                            Email = "",
                            PassportNumber = "",
                            StartDate = objBirthRegistration.DateOfBirth.Value,
                            EndDate = DateTime.Parse("9999-12-31"),
                            ActionType = "pendaftaran",
                            CompleteStatus = false
                        };
                        PersonalDataBpjsRepository.Add(personalDataBpjs);

                        var dataApprover = new MdmService(UnitOfWork).GetActualOrganizationStructure(documentApproval.CreatedBy);
                        var users = new UserService(this.UnitOfWork).GetUsersByRole("SHE");
                        var member = GetFamilyMemberDetail(personalDataFamilyMember.Id);

                        users?.ForEach(x =>
                            NotificationRepository.Add(new Domain.Notification
                            {
                                FromNoReg = dataApprover.NoReg,
                                ToNoReg = x.NoReg,
                                Message = $"Document Category BPJS for {GetFamilyMemberDetail(personalDataFamilyMember.Id).Name} has been completed {(personalDataBpjs.ActionType == "pendaftaran" ? "register" : "deactived")} by {dataApprover.Name}",
                                NotificationTypeCode = "notice",
                            })
                        );

                        //create request tax status    
                        var documentApprovalFileRequest = DocumentApprovalFileRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).ToList();
                        var documentApprovalFile = new List<DocumentApprovalFile>();

                        foreach (var item in documentApprovalFileRequest)
                        {
                            documentApprovalFile.Add(new DocumentApprovalFile()
                            {
                                CommonFileId = item.CommonFileId,
                                FieldCategory = "Object.SupportingAttachmentPath"
                            });
                        }

                        var personalDataTaxStatus = PersonalDataTaxStatusRepository.Fetch().Where(x => x.NoReg == documentApproval.CreatedBy && x.RowStatus && (DateTime.Now >= x.StartDate && DateTime.Now <= x.EndDate)).FirstOrDefault();
                        

                        if (personalDataCommonAttributeParent.GenderCode == "perempuan")
                        {
                            //update delimit
                            personalDataTaxStatus.EndDate = DateTime.Now.Date.AddDays(-1);
                            PersonalDataTaxStatusRepository.Upsert<Guid>(personalDataTaxStatus);

                            var personalDataTaxStatusGender = new PersonalDataTaxStatus()
                            {
                                NoReg = documentApproval.CreatedBy,
                                Npwp = personalDataTaxStatus.Npwp,
                                TaxStatus = "TK0",
                                TaxPtkp = "TK0",
                                StartDate = DateTime.Now.Date,
                                EndDate = DateTime.Parse("9999-12-31")
                            };
                            PersonalDataTaxStatusRepository.Upsert<Guid>(personalDataTaxStatusGender);
                        }
                        else
                        {
                            var taxStatus = personalDataTaxStatus.TaxStatus.Substring(0, personalDataTaxStatus.TaxStatus.Length - 1)
                            + (int.Parse(personalDataTaxStatus.TaxStatus.Substring(personalDataTaxStatus.TaxStatus.Length - 1)) + 1).ToString();

                            var taxstatusRequestDetail = new DocumentRequestDetailViewModel<TaxStatusViewModel>()
                            {
                                FormKey = "tax-status",
                                Object = new TaxStatusViewModel()
                                {
                                    NPWPNumber = personalDataTaxStatus.Npwp,
                                    StatusTax = taxStatus
                                },
                                Attachments = documentApprovalFile,
                            };
                            new ApprovalService(this.UnitOfWork, null, _localizer).CreateAutoRequestApprovalDocumentAsync(noregCurentApprover, documentApproval, actualOrganizationStructure, approverActualOrganizationStructure, taxstatusRequestDetail);
                        }

                        if (!users.IsEmpty())
                        {
                            var emailService = new EmailService(UnitOfWork);
                            var coreService = new CoreService(UnitOfWork);

                            var emailTemplate = coreService.GetEmailTemplate(MailTemplate.SheUpdateMember);
                            var instanceKey = $"app-notice";
                            var mailSubject = emailTemplate.Subject;
                            var mailFrom = emailTemplate.MailFrom;
                            var template = Scriban.Template.Parse(emailTemplate.MailContent);

                            var mailManager = emailService.CreateEmailManager();
                            var mailContent = template.Render(new
                            {
                                names = personalDataCommonAttributeParent.Name,
                                document = "BPJS/Asuransi",
                                familyName = member.Name,
                                familyType = "",
                                karyawanName = personalDataCommonAttributeParent.Name,
                                noreg = documentApproval.CreatedBy,
                                year = DateTime.Now.Year
                            });

                            mailManager.SendAsync(mailFrom, mailSubject, mailContent, string.Join(",", users.Select(x => x.Email))).ConfigureAwait(false);
                        }
                    }
                        
                }

                UnitOfWork.SaveChanges();
            }

            int totalFailure = 0;
            return Task.FromResult(totalFailure);
        }

        #region NPWP
        public string GetExistingNPWP(string NoReg)
        {
            var personalDataTaxStatus = PersonalDataTaxStatusRepository.Fetch().Where(x => x.NoReg == NoReg && x.RowStatus
                    && (DateTime.Now >= x.StartDate && DateTime.Now <= x.EndDate)).FirstOrDefault();
            return personalDataTaxStatus.Npwp;
        }
        #endregion
    }
}

using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Common;
using Agit.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Z.EntityFramework.Plus;
using System.Globalization;
using Agit.Common.Extensions;
using TAMHR.ESS.Infrastructure.ViewModels;
using Newtonsoft.Json;
using Agit.Domain.Extensions;
using System.Text.RegularExpressions;
using System.Data;
using Dapper;
using System.Dynamic;
using Agit.Domain.Ado;
using TAMHR.ESS.Infrastructure.Helpers;
using System.IO;
using Agit.Common.Archieve;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle configuration and general category
    /// </summary>
    public class SapIntegrationService : DomainServiceBase
    {
        #region Repositories
        protected IReadonlyRepository<ActualOrganizationStructure> ActualOrganizationStructureReadonlyRepository => UnitOfWork.GetRepository<ActualOrganizationStructure>();
        protected IRepository<DocumentApproval> DocumentApprovalRepository => UnitOfWork.GetRepository<DocumentApproval>();
        protected IRepository<DocumentRequestDetail> DocumentRequestDetailRepository => UnitOfWork.GetRepository<DocumentRequestDetail>();
        protected IRepository<Form> FormRepository => UnitOfWork.GetRepository<Form>();
        protected IRepository<PersonalData> PersonalDataRepository => UnitOfWork.GetRepository<PersonalData>();
        protected IRepository<PersonalDataFamilyMember> PersonalDataFamilyMemberRepository => UnitOfWork.GetRepository<PersonalDataFamilyMember>();
        protected IRepository<PersonalDataCommonAttribute> PersonalDataCommonAttributeRepository => UnitOfWork.GetRepository<PersonalDataCommonAttribute>();
        protected IRepository<PersonalDataBankAccount> PersonalDataBankAccountRepository => UnitOfWork.GetRepository<PersonalDataBankAccount>();
        protected IRepository<SapGeneralCategoryMap> SapGeneralCategoryMapRepository => UnitOfWork.GetRepository<SapGeneralCategoryMap>();
        protected IRepository<GeneralCategory> GeneralCategoryRepository => UnitOfWork.GetRepository<GeneralCategory>();
        protected IRepository<PersonalDataEvent> PersonalDataEventRepository => UnitOfWork.GetRepository<PersonalDataEvent>();
        protected IRepository<AllowanceDetail> AllowanceDetailRepository => UnitOfWork.GetRepository<AllowanceDetail>();
        protected IRepository<ActualOrganizationStructure> ActualOrganizationStructureRepository => UnitOfWork.GetRepository<ActualOrganizationStructure>();
        protected IReadonlyRepository<HitCountDetailView> HitCountDetailReadonlyRepository => UnitOfWork.GetRepository<HitCountDetailView>();

        #endregion

        #region Constructor
        /// <summary>
        /// Constructore
        /// </summary>
        /// <param name="unitOfWork">Concrete UnitOfWork</param>
        public SapIntegrationService(IUnitOfWork unitOfWork) : base(unitOfWork) { }
        #endregion

        private void AddDictionary(Dictionary<string, List<object>> dictionary, string key, object data)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, new List<object>());
            }

            dictionary[key].Add(data);
        }

        public IQueryable<HitCountDetailView> GetHistories(Guid documentApprovalId)
        {
            return HitCountDetailReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.DocumentApprovalId == documentApprovalId);
        }

        public void UpdateHitCount(string actor, Guid[] ids)
        {
            var dt = new DataTable();
            dt.Columns.Add("Id", typeof(Guid));

            ids.ForEach(x => dt.Rows.Add(x));

            UnitOfWork.Transact(trans =>
            {
                UnitOfWork.UspQuery("SP_UPDATE_HIT_COUNT", new { actor, ids = dt.AsTableValuedParameter("TVP_GENERIC_ID") }, trans);

                UnitOfWork.SaveChanges();
            });
        }

        public string[] GetDataTermination(Guid[] ids)
        {
            List<string> list = new List<string>();
            foreach (var id in ids)
            {
                var doc = DocumentApprovalRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.Id == id);
                var form = FormRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.Id == doc.FormId);
                if (form.FormKey == "termination")
                {
                    list.Add(doc.DocumentNumber);
                }


            }

            return list.ToArray();
        }

        public string[] GetDataUpdateContract(Guid[] ids)
        {
            List<string> list = new List<string>();
            foreach (var id in ids)
            {
                var doc = DocumentApprovalRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.Id == id);
                var form = FormRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.Id == doc.FormId);
                if (form.FormKey == "enrollment-contract")
                {
                    list.Add(doc.DocumentNumber);
                }


            }

            return list.ToArray();
        }
        public Dictionary<string, List<object>> GetListData(Guid[] ids)
        {
            var data = new Dictionary<string, List<object>>();

            foreach (var id in ids)
            {
                var doc = DocumentApprovalRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.Id == id);
                var docDetail = DocumentRequestDetailRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.DocumentApprovalId == doc.Id);
                var form = FormRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.Id == doc.FormId);

                var now = DateTime.Now;
                var startDate = DateTime.Now;
                var validityStartDate = new DateTime(now.Year, now.Month, 1);
                var validityEndDate = new DateTime(now.Year, now.Month, 10);

                if (form.FormKey == "tax-status")
                {
                    startDate = new DateTime(doc.LastApprovedOn.Value.Date.Year + 1, 1, 1); 
                }
                else
                {
                    if (doc.LastApprovedOn.Value.Date > validityEndDate)
                    {
                        validityStartDate.AddMonths(1);
                    }

                    startDate = new DateTime(validityStartDate.Year, validityStartDate.Month, 1);
                }

                if (form.FormKey == "marriage-status")
                {
                    AddDictionary(data, "marriage-status", GetMarriageStatus(doc, docDetail));
                }
                else if (form.FormKey == "family-registration")
                {
                    AddDictionary(data, "family-registration", GetFamilyRegistration(doc, docDetail));
                }
                else if (form.FormKey == "divorce")
                {
                    AddDictionary(data, "divorce", GetDivorceStatus(doc, docDetail));
                }
                else if (form.FormKey == "condolance")
                {
                    AddDictionary(data, "condolance", GetDismembermentStatus(doc, docDetail));
                }
                else if (form.FormKey == "address")
                {
                    AddDictionary(data, "address", GetAddressStatus(doc, docDetail));
                }
                else if (form.FormKey == "education")
                {
                    AddDictionary(data, "education", GetEducationStatus(doc, docDetail));
                }
                else if (form.FormKey == "bank-account")
                {
                    AddDictionary(data, "bank-account", GetBankAccountStatus(doc, docDetail, startDate));
                }
                else if (form.FormKey == "tax-status")
                {
                    AddDictionary(data, "tax-status", GetTaxStatus(doc, docDetail, startDate));
                }
                else if (form.FormKey == "eyeglasses-allowance" || form.FormKey == "marriage-allowance" || form.FormKey == "condolance-allowance" || form.FormKey == "distressed-allowance")
                {
                    if (form.FormKey == "eyeglasses-allowance")
                    {
                        var objModel = JsonConvert.DeserializeObject<EyeglassesAllowanceViewModel>(docDetail.ObjectValue);

                        if (objModel.IsFrame == true && objModel.IsLens == true)
                        {
                            AddDictionary(data, "claim-benefit", GetBenefitClaimGeneralFrame(doc, docDetail, startDate));
                            AddDictionary(data, "claim-benefit", GetBenefitClaimGeneralLens(doc, docDetail, startDate));
                        }
                        else if (objModel.IsFrame == true && objModel.IsLens == false)
                        {
                            AddDictionary(data, "claim-benefit", GetBenefitClaimGeneralFrame(doc, docDetail, startDate));
                        }
                        else if (objModel.IsFrame == false && objModel.IsLens == true)
                        {
                            AddDictionary(data, "claim-benefit", GetBenefitClaimGeneralLens(doc, docDetail, startDate));
                        }                        
                    }
                    else if (form.FormKey == "marriage-allowance")
                    {
                        AddDictionary(data, "claim-benefit", GetBenefitClaimGeneralMarriage(doc, docDetail, startDate));
                    }
                    else if (form.FormKey == "condolance-allowance")
                    {
                        AddDictionary(data, "claim-benefit", GetBenefitClaimGeneralCondolance(doc, docDetail, startDate));
                    }
                    else if (form.FormKey == "distressed-allowance")
                    {
                        AddDictionary(data, "claim-benefit", GetBenefitClaimGeneralDistressed(doc, docDetail, startDate));
                    }
                }
                else if (form.FormKey == "vacation-allowance")
                {
                    AddDictionary(data, "vacation-allowance", GetBenefitClaimRecreationVacation(doc, docDetail, startDate));
                }
                else if (form.FormKey == "pta-allowance")
                {
                    AddDictionary(data, "pta-allowance", GetBenefitClaimRecreationPTA(doc, docDetail, startDate));
                }
                else if (form.FormKey == "concept-idea-allowance")
                {
                    AddDictionary(data, "claim-benefit", GetBenefitClaimConcept(doc, docDetail, startDate));
                }
                else if (form.FormKey == "reimbursement")
                {
                    AddDictionary(data, "claim-benefit", GetBenefitClaimReimbursment(doc, docDetail, startDate));
                }
                else if (form.FormKey == "cop-fuel-allowance")
                {
                    AddDictionary(data, "claim-benefit", GetBenefitClaimFuelCOP(doc, docDetail, startDate));
                }
                else if (form.FormKey == "ayo-sekolah")
                {
                    AddDictionary(data, "claim-benefit", GetBenefitClaimAyoSekolah(doc, docDetail, startDate));
                }
                else if (form.FormKey == "kb-allowance")
                {
                    AddDictionary(data, "claim-benefit", GetBenefitClaimKB(doc, docDetail, startDate));
                }
                else if (form.FormKey == "company-loan")
                {
                    AddDictionary(data, "company-loan", GetCompanyLoan(doc, docDetail));
                }
                else if (form.FormKey == "company-loan36-new")
                {
                    AddDictionary(data, "company-loan", GetCompanyLoan(doc, docDetail));
                }
                else if (form.FormKey == "meal-allowance")
                {
                    AddDictionary(data, "meal-allowance", GetMeal(doc, docDetail, startDate));
                }
                else if (form.FormKey == "shift-meal-allowance")
                {
                    AddDictionary(data, "shift-meal-allowance", GetShiftMeal(doc, docDetail, startDate));
                }
                else if (form.FormKey == "hiring-employee")
                {
                    AddDictionary(data, "hiring-employee", GetHiringEmployee(doc, startDate));
                }
                else if (form.FormKey == "bpjs-tk")
                {
                    AddDictionary(data, "bpjs-tk", GetBpjsTk(doc, startDate));
                }

            }

            return data;
        }

        private dynamic GetMarriageStatus(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail)
        {
            var objModel = JsonConvert.DeserializeObject<MarriageStatusViewModel>(documentRequestDetail.ObjectValue);
            var personalData = PersonalDataRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.NoReg == documentApproval.CreatedBy);

            var sapGeneralCategoryMap = SapGeneralCategoryMapRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.SapCategory == "FamilyType" || x.SapCategory == "Nationality" || x.SapCategory == "Gender" || x.SapCategory == "MaritalStatus")
                .ToList();

            var GeneralCategory = GeneralCategoryRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "region" || x.Category == "birthplace")
                .ToList();

            var _BirthPlace = objModel.IsOtherPlaceOfBirthCode ? objModel.OtherPlaceOfBirthCode :
                GeneralCategory.FirstOrDefault(x => x.Code == objModel.PlaceOfBirthCode) != null ? GeneralCategory.FirstOrDefault(x => x.Code == objModel.PlaceOfBirthCode)?.Description :
                objModel.PlaceOfBirthCode;

            return new
            {
                NoReg = documentApproval.CreatedBy,
                FamilyTypeCode = sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == "suamiistri" && x.SapCategory == "FamilyType").SapCode,
                StartDate = (documentApproval.LastApprovedOn ?? DateTime.Now).ToString("dd/MM/yyyy"),
                EndDate = "31/12/9999",
                Name = objModel.PartnerName,
                NationalityCode = string.IsNullOrEmpty(objModel.NationCode) ? "" : sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == objModel.NationCode && x.SapCategory == "Nationality")?.SapCode,
                BirthPlace = _BirthPlace,
                BirthDate = objModel.DateOfBirth.Value.ToString("dd/MM/yyyy"),
                GenderCode = sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == objModel.GenderCode && x.SapCategory == "Gender")?.SapCode,
                Occupation = "", //personalData.JobTitle, //perlu konfirmasi source data
                MartialStatusCode = sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == personalData.MaritalStatusCode && x.SapCategory == "MaritalStatus")?.SapCode
            };
        }

        private dynamic GetFamilyRegistration(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail)
        {
            var objModel = JsonConvert.DeserializeObject<BirthRegistrationViewModel>(documentRequestDetail.ObjectValue);
            var personalData = PersonalDataRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.NoReg == documentApproval.CreatedBy);
            var sapGeneralCategoryMap = SapGeneralCategoryMapRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.SapCategory == "FamilyType" || x.SapCategory == "Nationality" || x.SapCategory == "Gender")
                .ToList();

            var GeneralCategory = GeneralCategoryRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "region" || x.Category == "subDistrict" || x.Category == "birthplace")
                .ToList();

            var _BirthPlace = objModel.IsOtherPlaceOfBirthCode ? objModel.OtherPlaceOfBirthCode :
                GeneralCategory.FirstOrDefault(x => x.Code == objModel.PlaceOfBirthCode ) != null ? GeneralCategory.FirstOrDefault(x => x.Code == objModel.PlaceOfBirthCode )?.Description :
                objModel.PlaceOfBirthCode;

            return new
            {
                NoReg = documentApproval.CreatedBy,
                FamilyTypeCode = sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == objModel.ChildStatus && x.SapCategory == "FamilyType")?.SapCode,
                StartDate = (documentApproval.LastApprovedOn ?? DateTime.Now).ToString("dd/MM/yyyy"),
                EndDate = "31/12/9999",
                Name = objModel.ChildName,
                NationalityCode = string.IsNullOrEmpty(objModel.NationCode) ? "" : sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == objModel.NationCode && x.SapCategory == "Nationality")?.SapCode,
                BirthPlace = _BirthPlace,
                BirthDate = objModel.DateOfBirth.Value.ToString("dd/MM/yyyy"),
                GenderCode = sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == objModel.GenderCode && x.SapCategory == "Gender")?.SapCode
            };
        }

        private dynamic GetDivorceStatus(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail)
        {
            var objModel = JsonConvert.DeserializeObject<DivorceViewModel>(documentRequestDetail.ObjectValue);
            var personalData = PersonalDataRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.NoReg == documentApproval.CreatedBy);
            var personalDataCommonAttributeParent = PersonalDataCommonAttributeRepository.FindById(personalData.CommonAttributeId);
            var PartnerId = Guid.Parse(objModel.PartnerId);
            var FamilyMemberId = PersonalDataFamilyMemberRepository.FindById(PartnerId).CommonAttributeId;
            var BirthPlacePasangan = PersonalDataCommonAttributeRepository.FindById(FamilyMemberId);
            var FamilyCAID = PersonalDataFamilyMemberRepository.FindById(PartnerId).CommonAttributeId;
            var PartnerData = PersonalDataCommonAttributeRepository.FindById(FamilyCAID);
            var EventDate = PersonalDataEventRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.FamilyMemberId == new Guid(objModel.PartnerId)).EventDate;
            var sapGeneralCategoryMap = SapGeneralCategoryMapRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.SapCategory == "FamilyType" || x.SapCategory == "Gender" || x.SapCategory == "MaritalStatus" || x.SapCategory == "Nationality")
                .ToList();

            var GeneralCategory = GeneralCategoryRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "region" || x.Category == "birthplace" || x.Category == "subDistrict")
                .ToList();

            var genderCode = sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == personalDataCommonAttributeParent.GenderCode && x.SapCategory == "Gender")?.SapCode;
            var maritalStatusCode = "";

            if (genderCode == "Male")
            {
                maritalStatusCode = sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == "duda" && x.SapCategory == "MaritalStatus")?.SapCode;
            }
            else
            {
                maritalStatusCode = sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == "janda" && x.SapCategory == "MaritalStatus")?.SapCode;
            }

            var _birthPlace = GeneralCategory.FirstOrDefault(x => x.Code == BirthPlacePasangan.BirthPlace) != null ? GeneralCategory.FirstOrDefault(x => x.Code == BirthPlacePasangan.BirthPlace).Description : BirthPlacePasangan.BirthPlace;

            return new
            {
                NoReg = documentApproval.CreatedBy,
                FamilyTypeCode = sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == "suamiistri" && x.SapCategory == "FamilyType")?.SapCode,
                StartDate = (documentApproval.LastApprovedOn ?? DateTime.Now).ToString("dd/MM/yyyy"),
                EndDate = "31/12/9999",
                Name = objModel.PartnerName,
                NationalityCode = string.IsNullOrEmpty(PartnerData.CountryCode) ? "" : sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == PartnerData.CountryCode && x.SapCategory == "Nationality")?.SapCode,
                BirthPlace = _birthPlace,
                BirthDate = BirthPlacePasangan.BirthDate.ToString("dd/MM/yyyy"),
                GenderCode = genderCode,
                Occupation = "", //sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == personalData.JobTitle && x.SapCategory == "Region").SapCode, //perlu konfirmasi source data
                MartialStatusCode = maritalStatusCode
            };
        }

        private dynamic GetDismembermentStatus(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail)
        {
            var objModel = JsonConvert.DeserializeObject<DismembermentViewModel>(documentRequestDetail.ObjectValue);
            var personalData = PersonalDataRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.NoReg == documentApproval.CreatedBy);
            var personalDataCommonAttributeParent = PersonalDataCommonAttributeRepository.FindById(personalData.CommonAttributeId);

            var FamilyTypeCode = "";
            var StartDate = "";
            var EndDate = "";
            var BirthDate = "";
            var GenderCode = "";
            var Name = "";
            var NationalityCode = "";
            var MartialStatusCode = "";
            var BirthPlace = "";
            string MainFamily = "";

            var sapGeneralCategoryMap = SapGeneralCategoryMapRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.SapCategory == "FamilyType" || x.SapCategory == "Gender" || x.SapCategory == "MaritalStatus" || x.SapCategory == "Nationality")
                .ToList();

            var GeneralCategory = GeneralCategoryRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "region" || x.Category == "birthplace")
                .ToList();


            if (objModel.IsMainFamily.ToLower() == "ya")
            {
                var getFamily = PersonalDataFamilyMemberRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.Id == new Guid(objModel.OtherFamilyId));
                var common = PersonalDataCommonAttributeRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.Id == getFamily.CommonAttributeId);
                var _birthPlace = GeneralCategory.FirstOrDefault(x => x.Code == common.BirthPlace) != null ? GeneralCategory.FirstOrDefault(x => x.Code == common.BirthPlace).Description : common.BirthPlace;

                FamilyTypeCode = sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == getFamily.FamilyTypeCode && x.SapCategory == "FamilyType")?.SapCode;
                StartDate = getFamily.StartDate?.ToString("dd/MM/yyyy");
                EndDate = getFamily.EndDate?.ToString("dd/MM/yyyy");
                BirthDate = common.BirthDate.ToString("dd/MM/yyyy");
                GenderCode = sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == personalDataCommonAttributeParent.GenderCode && x.SapCategory == "Gender")?.SapCode;
                Name = common.Name;
                NationalityCode = sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == personalDataCommonAttributeParent.CountryCode && x.SapCategory == "Nationality")?.SapCode;
                MartialStatusCode = sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == personalData.MaritalStatusCode && x.SapCategory == "MaritalStatus")?.SapCode;
                BirthPlace = _birthPlace;
                MainFamily = "ya";
            }
            else
            {
                var id = personalData.CommonAttributeId;
                var getCommon = PersonalDataCommonAttributeRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.Id == id);
                var _birthPlace = GeneralCategory.FirstOrDefault(x => x.Code == getCommon.BirthPlace) != null ? GeneralCategory.FirstOrDefault(x => x.Code == getCommon.BirthPlace).Description : getCommon.BirthPlace;
                var date = getCommon.BirthDate.ToString("dd/MM/yyyy");
                if (objModel.NonFamilyRelationship == "ayahkandung" || objModel.NonFamilyRelationship == "ibukandung")
                {
                    FamilyTypeCode = "0205";
                }
                else
                {
                    FamilyTypeCode = "0204";
                }

                var Gender = "";
                if (objModel.NonFamilyRelationship == "ayahkandung" || objModel.NonFamilyRelationship == "kakek" || objModel.NonFamilyRelationship == "ayahmertua")
                {
                    Gender = sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == "lakilaki" && x.SapCategory == "Gender")?.SapCode.ToString();
                }
                else
                {
                    Gender = sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == "perempuan" && x.SapCategory == "Gender")?.SapCode.ToString();
                }

                if (objModel.NonFamilyRelationship == "ayahkandung" || objModel.NonFamilyRelationship == "ayahkandung")
                {
                    FamilyTypeCode = "11";
                }
                else if (objModel.NonFamilyRelationship == "ayahmertua" || objModel.NonFamilyRelationship == "ibumertua")
                {
                    FamilyTypeCode = "12";
                }
                else
                {
                    FamilyTypeCode = "91";
                }

                StartDate = objModel.DismembermentDate?.ToString("dd/MM/yyyy");
                EndDate = objModel.DismembermentDate?.ToString("dd/MM/yyyy");
                BirthDate = date;
                GenderCode = Gender; 
                Name = objModel.FamilyName;
                NationalityCode = sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == personalDataCommonAttributeParent.CountryCode && x.SapCategory == "Nationality")?.SapCode;
                MartialStatusCode = "";
                BirthPlace = _birthPlace;
                MainFamily = "tidak";
            }

            return new
            {
                NoReg = documentApproval.CreatedBy,
                FamilyTypeCode = FamilyTypeCode,
                StartDate = (documentApproval.LastApprovedOn ?? DateTime.Now).ToString("dd/MM/yyyy"), 
                EndDate = EndDate, 
                Name = Name, 
                NationalityCode = NationalityCode,
                BirthPlace = BirthPlace, 
                BirthDate = BirthDate,
                GenderCode = GenderCode,
                Occupation = "", 
                MartialStatusCode = MartialStatusCode,
                MainFamily = MainFamily,
            };
        }

        private dynamic GetAddressStatus(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail)
        {
            var objModel = JsonConvert.DeserializeObject<AddressViewModel>(documentRequestDetail.ObjectValue);
            var sapGeneralCategoryMap = SapGeneralCategoryMapRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.SapCategory == "Region")
                .ToList();

            var cityCode = GeneralCategoryRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.Code == objModel.City && x.Category == "district")?.Description;

            var districtCode = GeneralCategoryRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.Code == objModel.DistrictCode && x.Category == "subDistrict")?.Description;

            var postalCode = GeneralCategoryRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.Code == objModel.PostalCode && x.Category == "postCode")?.Description;

            var personalData = PersonalDataRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.NoReg == documentApproval.CreatedBy);
            var personalDataCommonAttributeParent = PersonalDataCommonAttributeRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.Id == personalData.CommonAttributeId);

            return new
            {
                NoReg = documentApproval.CreatedBy,
                //IDCardAddress = personalDataCommonAttributeParent.Address,
                IDCardAddress = "1",
                StartDate = (documentApproval.LastApprovedOn ?? DateTime.Now).ToString("dd/MM/yyyy"),
                EndDate = "31/12/9999",
                RegionCode = sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == personalDataCommonAttributeParent.RegionCode)?.SapCode,
                CityCode = cityCode,
                DistrictCode = districtCode,
                PostalCode = postalCode,
                Address = objModel.CompleteAddress + ". RT:" + objModel.RT + " RW:" + objModel.RW,
                Country = "ID" //SAP code Negara Indonesia
            };
        }

        private dynamic GetEducationStatus(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail)
        {
            var objModel = JsonConvert.DeserializeObject<EducationViewModel>(documentRequestDetail.ObjectValue);
            var sapGeneralCategoryMap = SapGeneralCategoryMapRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.SapCategory == "EducationType" || x.SapCategory == "Major")
                .ToList();

            return new
            {
                NoReg = documentApproval.CreatedBy,
                EducationTypeCode = sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == objModel.LevelOfEducationCode && x.SapCategory == "EducationType")?.SapCode,
                StartDate = (documentApproval.LastApprovedOn ?? DateTime.Now).ToString("dd/MM/yyyy"),
                EndDate = "31/12/9999",
                Education = "", //di eform tidak ada, jadi dikosongkan
                Institute = objModel.IsOtherCollegeName ? objModel.OtherCollegeName : GeneralCategoryRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.Code == objModel.CollegeName && x.Category == "Collage")?.Name,
                CountryCode = "ID", //SAP code Negara Indonesia
                Certificate = "Z1",  //sesuai request Eform
                Major = sapGeneralCategoryMap.FirstOrDefault(x => x.GeneralCategoryCode == objModel.DepartmentCode && x.SapCategory == "Major")?.SapCode,
                GraduationDate = objModel.Period.Value.ToString("dd/MM/yyyy"),
                FinalGrade = objModel.GPA.ToString() 
            };
        }

        private dynamic GetBankAccountStatus(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail, DateTime strDate)
        {
            var objModel = JsonConvert.DeserializeObject<BankDetailViewModel>(documentRequestDetail.ObjectValue);
            var sapGeneralCategoryMap = SapGeneralCategoryMapRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.SapCategory == "Bank")
                .ToList();

            //var personalBankAccount = PersonalDataBankAccountRepository.Fetch()
            //    .AsNoTracking()
            //    .Where(x => x.AccountNumber == objModel.AccountNumber || x.BankCode == objModel.KeyBank)
            //    .FirstOrDefault();
            var personalData = PersonalDataRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == documentApproval.CreatedBy)
                .FirstOrDefault();

            var Name = PersonalDataCommonAttributeRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == personalData.CommonAttributeId)
                .FirstOrDefault().Name;

            return new
            {
                NoReg = documentApproval.CreatedBy,
                StartDate = strDate.ToString("dd/MM/yyyy"),
                EndDate = "31/12/9999",
                BanksDetailType = "0", //kode bank detail 
                //BankName = objModel.BankName,
                //BankBranch = objModel.BranchBank,
                //BankAddress = objModel.LocationBank, 
                BankCode = objModel.KeyBank,
                AccountNumber = objModel.AccountNumber,
                Name = objModel.AccountName ?? Name,
                PaymentMethod = "T",
                PaymentCurrency = "IDR"
            };
        }

        private dynamic GetTaxStatus(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail, DateTime strDate)
        {
            var objModel = JsonConvert.DeserializeObject<TaxStatusViewModel>(documentRequestDetail.ObjectValue);
            var NPWP1 = objModel.NPWPNumber;
            var NPWP2 = objModel.NPWPNumber;
            var NPWP3 = objModel.NPWPNumber;

            if (objModel.NPWPNumber.Length == 20)
            {
                NPWP1 = objModel.NPWPNumber.Substring(0, 3).Replace("-", string.Empty).Replace(".", string.Empty);
                NPWP2 = objModel.NPWPNumber.Substring(3, 9).Replace("-", string.Empty).Replace(".", string.Empty);
                NPWP3 = objModel.NPWPNumber.Substring(12, 8).Replace("-", string.Empty).Replace(".", string.Empty);
            }
            
            string Dependants = objModel.StatusTax.Replace("T", "").Replace("K", "");
            if (int.Parse(Dependants) > 3)
                Dependants = "3";
            //if(Dependants.Contains(""))
            string MarriageStatus = Regex.Replace(objModel.StatusTax, @"[\d-]", string.Empty);
            string Married = "";
            if(MarriageStatus == "K")
            {
                Married = "X";
            }
            return new
            {
                NoReg = documentApproval.CreatedBy,
                StartDate = strDate.ToString("dd/MM/yyyy"),
                EndDate = "31/12/9999",
                NPWP1,
                NPWP2,
                NPWP3,
                Dependants,
                Married,
                Spouse = Married
            };
        }

        private dynamic GetBenefitClaimGeneralFrame(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail, DateTime startDate)
        {
            var objModel = JsonConvert.DeserializeObject<EyeglassesAllowanceViewModel>(documentRequestDetail.ObjectValue);
            var dateOfOrigin = startDate.ToString("dd/MM/yyyy");//documentApproval.DocumentStatusCode == "completed" ? documentApproval.ModifiedOn.Value.ToString("dd/MM/yyyy") : "";
            
            return new
            {
                GeneralNoReg = documentApproval.CreatedBy,
                GeneralInfotype = "0015",
                GeneralWageType = "0207", //hardcode sesuai value yang ada di excel SAP to NET Mapping
                GeneralAmount = objModel.AmountFrame,
                GeneralCurrency = "IDR",
                GeneralDateofOrigin = dateOfOrigin
            };
        }

        private dynamic GetBenefitClaimGeneralLens(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail, DateTime startDate)
        {
            var objModel = JsonConvert.DeserializeObject<EyeglassesAllowanceViewModel>(documentRequestDetail.ObjectValue);
            var dateOfOrigin = startDate.ToString("dd/MM/yyyy");//documentApproval.DocumentStatusCode == "completed" ? documentApproval.ModifiedOn.Value.ToString("dd/MM/yyyy") : "";
            
            return new
            {
                GeneralNoReg = documentApproval.CreatedBy,
                GeneralInfotype = "0015",
                GeneralWageType = "0206", //hardcode sesuai value yang ada di excel SAP to NET Mapping
                GeneralAmount = objModel.AmountLens,
                GeneralCurrency = "IDR",
                GeneralDateofOrigin = dateOfOrigin
            };
        }

        private dynamic GetBenefitClaimGeneralMarriage(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail, DateTime startDate)
        {
            var objModel = JsonConvert.DeserializeObject<MarriageAllowanceViewModel>(documentRequestDetail.ObjectValue);
            var dateOfOrigin = startDate.ToString("dd/MM/yyyy");//documentApproval.DocumentStatusCode == "completed" ? documentApproval.ModifiedOn.Value.ToString("dd/MM/yyyy") : "";
            
            return new
            {
                GeneralNoReg = documentApproval.CreatedBy,
                GeneralInfotype = "0015",
                GeneralWageType = "0203", //hardcode sesuai value yang ada di excel SAP to NET Mapping
                GeneralAmount = objModel.AmountAllowance,
                GeneralCurrency = "IDR",
                GeneralDateofOrigin = dateOfOrigin
            };
        }

        private dynamic GetBenefitClaimGeneralCondolance(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail, DateTime startDate)
        {
            var objModel = JsonConvert.DeserializeObject<MisseryAllowanceViewModel>(documentRequestDetail.ObjectValue);
            var dateOfOrigin = startDate.ToString("dd/MM/yyyy");//documentApproval.DocumentStatusCode == "completed" ? documentApproval.ModifiedOn.Value.ToString("dd/MM/yyyy") : "";
            var GeneralWageType = "";

            if (objModel.IsMainFamily == "ya")
            {
                GeneralWageType = "0204";
            }
            else
            {
                GeneralWageType = "0205";
            }

            return new
            {
                GeneralNoReg = documentApproval.CreatedBy,
                GeneralInfotype = "0015",
                GeneralWageType = GeneralWageType, //hardcode sesuai value yang ada di excel SAP to NET Mapping
                GeneralAmount = Convert.ToInt64(objModel.AllowancesAmount),
                GeneralCurrency = "IDR",
                GeneralDateofOrigin = dateOfOrigin
            };
        }

        private dynamic GetBenefitClaimGeneralDistressed(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail, DateTime startDate)
        {
            var objModel = JsonConvert.DeserializeObject<DistressedAllowanceViewModel>(documentRequestDetail.ObjectValue);
            var dateOfOrigin = startDate.ToString("dd/MM/yyyy");//documentApproval.DocumentStatusCode == "completed" ? documentApproval.ModifiedOn.Value.ToString("dd/MM/yyyy") : "";
            
            return new
            {
                GeneralNoReg = documentApproval.CreatedBy,
                GeneralInfotype = "0015",
                GeneralWageType = "0209",
                GeneralAmount = objModel.AmountAllowance,
                GeneralCurrency = "IDR",
                GeneralDateofOrigin = dateOfOrigin
            };
        }

        private dynamic GetBenefitClaimRecreationVacation(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail, DateTime startDate)
        {
            var objModel = JsonConvert.DeserializeObject<VacationAllowanceViewModel>(documentRequestDetail.ObjectValue);
            var dateOfOrigin = startDate.ToString("dd/MM/yyyy");//documentApproval.DocumentStatusCode == "completed" ? documentApproval.ModifiedOn.Value.ToString("dd/MM/yyyy") : "";
            var list = new List<TempObj>();

            foreach (var aa in objModel.Departments)
            {
                foreach( var bb in aa.Employees)
                {
                    var getAmount = Convert.ToInt32(new CoreService(UnitOfWork).GetAllowanceAmount(bb.NoReg, "recreation")); 
                    list.Add(new TempObj(bb.NoReg, "0015", "0208", getAmount.ToString(), "IDR", dateOfOrigin));
                }
            }

            return list;
        }
        class TempObj
        {
            public string GeneralNoReg { get; set; }
            public string GeneralInfotype { get; set; }
            public string GeneralWageType { get; set; }
            public string GeneralAmount { get; set; }
            public string GeneralCurrency { get; set; }
            public string GeneralDateofOrigin { get; set; }

            public TempObj(string _GeneralNoReg, string _GeneralInfotype, string _GeneralWageType, string _GeneralAmount, string _GeneralCurrency, string _GeneralDateofOrigin)
            {
                GeneralNoReg = _GeneralNoReg;
                GeneralInfotype = _GeneralInfotype;
                GeneralWageType = _GeneralWageType;
                GeneralAmount = _GeneralAmount;
                GeneralCurrency = _GeneralCurrency;
                GeneralDateofOrigin = _GeneralDateofOrigin;
            }
        }

        private dynamic GetBenefitClaimRecreationPTA(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail, DateTime startDate)
        {
            var objModel = JsonConvert.DeserializeObject<PtaAllowanceViewModel>(documentRequestDetail.ObjectValue);
            var dateOfOrigin = startDate.ToString("dd/MM/yyyy");//documentApproval.DocumentStatusCode == "completed" ? documentApproval.ModifiedOn.Value.ToString("dd/MM/yyyy") : "";
            var list = new List<TempObj>();

            foreach (var Departments in objModel.Summaries)
            {
                foreach (var Employees in Departments.Employees)
                {
                    string Reward = "";
                    if (Employees.Reward.ToLower() == "pta")
                        Reward = "0300";
                    else
                        Reward = "0301";
                    list.Add(new TempObj(Employees.NoReg, "0015", Reward, Departments.Amount.ToString(), "IDR", dateOfOrigin));
                }
            }
            return list;

        }

        private dynamic GetBenefitClaimConcept(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail, DateTime startDate)
        {
            var objModel = JsonConvert.DeserializeObject<IdeBerkonsepViewModel>(documentRequestDetail.ObjectValue);
            var dateOfOrigin = startDate.ToString("dd/MM/yyyy");//documentApproval.DocumentStatusCode == "completed" ? documentApproval.ModifiedOn.Value.ToString("dd/MM/yyyy") : "";
            
            return new
            {
                GeneralNoReg = documentApproval.CreatedBy,
                GeneralInfotype = "0015",
                GeneralWageType = "0302", //hardcode sesuai value yang ada di excel SAP to NET Mapping
                GeneralAmount = objModel.Amount,
                GeneralCurrency = "IDR",
                GeneralDateofOrigin = dateOfOrigin
            };
        }

        private dynamic GetBenefitClaimReimbursment(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail, DateTime startDate)
        {
            var objModel = JsonConvert.DeserializeObject<ReimbursementViewModel>(documentRequestDetail.ObjectValue);
            var dateOfOrigin = startDate.ToString("dd/MM/yyyy"); //documentApproval.DocumentStatusCode == "completed" ? documentApproval.ModifiedOn.Value.ToString("dd/MM/yyyy") : "";
            
            return new
            {
                GeneralNoReg = documentApproval.CreatedBy,
                GeneralInfotype = "0015",
                GeneralWageType = "0410", //hardcode sesuai value yang ada di excel SAP to NET Mapping
                GeneralAmount = objModel.TotalCompanyClaim,
                GeneralCurrency = "IDR",
                GeneralDateofOrigin = dateOfOrigin
            };
        }

        private dynamic GetBenefitClaimFuelCOP(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail, DateTime startDate)
        {
            var objModel = JsonConvert.DeserializeObject<CopFuelAllowanceViewModel>(documentRequestDetail.ObjectValue);
            var dateOfOrigin = startDate.ToString("dd/MM/yyyy");//documentApproval.DocumentStatusCode == "completed" ? documentApproval.ModifiedOn.Value.ToString("dd/MM/yyyy") : "";
            var pemakaian = 0;

            foreach (var obj in objModel.data)
            {
                pemakaian += obj.Back - obj.Start;
            }

            string SubType = ActualOrganizationStructureRepository.Fetch().Where(x => x.NoReg == documentApproval.CreatedBy).FirstOrDefault().EmployeeSubgroup.ToString();
            var orgObjects = UnitOfWork.UspQuery<EmployeeeOrganizationObjectStoredEntity>(new { NoReg = documentApproval.CreatedBy }).ToList();
            var NP = orgObjects.FirstOrDefault(x => x.ObjectDescription == "Division")?.NP;

            var Ammount = AllowanceDetailRepository.Fetch().Where(x => x.SubType == SubType && (x.ClassFrom >= Convert.ToInt32(NP) && x.ClassTo <= Convert.ToInt32(NP))).FirstOrDefault().Ammount;
            var GeneralAmount = pemakaian * Convert.ToInt64(Ammount);

            return new
            {
                GeneralNoReg = documentApproval.CreatedBy,
                GeneralInfotype = "0015",
                GeneralWageType = "0455", //hardcode sesuai value yang ada di excel SAP to NET Mapping
                GeneralAmount = GeneralAmount,//objModel., belum ada di viewmodel
                GeneralCurrency = "IDR",
                GeneralDateofOrigin = dateOfOrigin
            };
        }

        private dynamic GetBenefitClaimAyoSekolah(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail, DateTime startDate)
        {
            var objModel = JsonConvert.DeserializeObject<AyoSekolahViewModel>(documentRequestDetail.ObjectValue);
            var dateOfOrigin = startDate.ToString("dd/MM/yyyy");//documentApproval.DocumentStatusCode == "completed" ? documentApproval.ModifiedOn.Value.ToString("dd/MM/yyyy") : "";
            //var Amount = CoreService.GetInfoAllowance.(documentApproval.CreatedBy, "ayosekolah", "").FirstorDefault();
            var getAmmount = new CoreService(UnitOfWork).GetInfoAllowance(documentApproval.CreatedBy, "ayosekolah", "");
            var AmmountAyoSekolah = getAmmount.GetType().GetProperty("Ammount").GetValue(getAmmount);

            return new
            {
                GeneralNoReg = documentApproval.CreatedBy,
                GeneralInfotype = "0015",
                GeneralWageType = "0508", //hardcode sesuai value yang ada di excel SAP to NET Mapping
                GeneralAmount = (Convert.ToInt32(AmmountAyoSekolah)).ToString(),//objModel., belum ada di viewmodel
                GeneralCurrency = "IDR",
                GeneralDateofOrigin = dateOfOrigin
            };
        }

        private dynamic GetBenefitClaimKB(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail, DateTime startDate)
        {
            var objModel = JsonConvert.DeserializeObject<KbAllowanceViewModel>(documentRequestDetail.ObjectValue);
            var dateOfOrigin = startDate.ToString("dd/MM/yyyy");//documentApproval.DocumentStatusCode == "completed" ? documentApproval.ModifiedOn.Value.ToString("dd/MM/yyyy") : "";
            
            return new
            {
                GeneralNoReg = documentApproval.CreatedBy,
                GeneralInfotype = "0015",
                GeneralWageType = "0201", //hardcode sesuai value yang ada di excel SAP to NET Mapping
                GeneralAmount = objModel.Cost,
                GeneralCurrency = "IDR",
                GeneralDateofOrigin = dateOfOrigin
            };
        }

        private dynamic GetCompanyLoan(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail)
        {
            var objModel = JsonConvert.DeserializeObject<LoanViewModel>(documentRequestDetail.ObjectValue);
            var ApprovalDay = documentApproval.LastApprovedOn.Value.Day;
            var ApprovalMonth = documentApproval.LastApprovedOn.Value.Month;

            string StartDate;
            string RepaymentDate;

            if (ApprovalDay > 20)
            {
                if (ApprovalMonth == 12)
                {
                    StartDate = "01/01/" + (documentApproval.LastApprovedOn.Value.Year + 1).ToString();
                    RepaymentDate = "01/02/" + (documentApproval.LastApprovedOn.Value.Year + 1).ToString();
                }
                else if(ApprovalMonth == 11)
                {
                    StartDate = "01/" + (documentApproval.LastApprovedOn.Value.Month + 1).ToString("00") + "/" + documentApproval.LastApprovedOn.Value.Year.ToString();
                    RepaymentDate = "01/01/" + (documentApproval.LastApprovedOn.Value.Year+1).ToString();
                }
                else
                {
                    StartDate = "01/" + (documentApproval.LastApprovedOn.Value.Month + 1).ToString("00") + "/" + documentApproval.LastApprovedOn.Value.Year.ToString();
                    RepaymentDate = "01/" + (documentApproval.LastApprovedOn.Value.Month + 2).ToString("00") + "/" + documentApproval.LastApprovedOn.Value.Year.ToString();
                }
            }
            else
            {
                if (ApprovalMonth == 12)
                {
                    var aa = documentApproval.LastApprovedOn.Value.Year + 1;
                    StartDate = "01/01/" + (documentApproval.LastApprovedOn.Value.Year + 1);
                    RepaymentDate = "01/02/" + (documentApproval.LastApprovedOn.Value.Year + 1);
                }
                else
                {
                    StartDate = "01/" + documentApproval.LastApprovedOn.Value.Month.ToString("00") + "/" + documentApproval.LastApprovedOn.Value.Year.ToString();
                    RepaymentDate = "01/" + (documentApproval.LastApprovedOn.Value.Month + 1).ToString("00") + "/" + documentApproval.LastApprovedOn.Value.Year.ToString();
                }
                
            }

            var orgObjects = UnitOfWork.UspQuery<EmployeeeOrganizationObjectStoredEntity>(new { NoReg = documentApproval.CreatedBy }).ToList();
            var NP = orgObjects.FirstOrDefault(x => x.ObjectDescription == "Division")?.NP;
            var bunga = 0;
            if (Convert.ToInt32(NP) >= 3 && Convert.ToInt32(NP) <= 6)
            {
                var interestCarLoan = new ClaimBenefitService(UnitOfWork).GetLoan("CalculationLoan", "InterestHouseLoan", Convert.ToInt32(NP)).FirstOrDefault();
                var AmmountInterest = interestCarLoan.GetType().GetProperty("Ammount").GetValue(interestCarLoan);
                bunga = Convert.ToInt32(AmmountInterest);
            }
            else
            {
                var interestCarLoan = new ClaimBenefitService(UnitOfWork).GetLoan("CalculationLoan", "InterestHouseLoan", Convert.ToInt32(NP)).FirstOrDefault();
                var AmmountInterest = interestCarLoan.GetType().GetProperty("Ammount").GetValue(interestCarLoan);
                bunga = Convert.ToInt32(AmmountInterest);
            }

            var periodLoan = new ClaimBenefitService(UnitOfWork).GetLoan("CalculationLoan", "PeriodLoan", Convert.ToInt32(NP)).FirstOrDefault();
            var AmmountperiodLoan = periodLoan.GetType().GetProperty("Ammount").GetValue(periodLoan);

            var periodloan = Convert.ToInt16(AmmountperiodLoan);
            var pinjaman = objModel.LoanAmount;
            //PMT
            var calculatebunga = (double)bunga / 100 / 12;
            int perido = (int)periodloan;
            Double pinjam = (double)pinjaman;
            var denominator = Math.Pow((1 + calculatebunga), perido) - 1;
            var sumCicilan = (calculatebunga + (calculatebunga / denominator)) * pinjam;
            var ii = Convert.ToDecimal(sumCicilan);
            var pembulatan = Math.Round(ii, 0);
            var jumlah = pembulatan * periodloan;
            var potongan = pembulatan;

            var loanType = SapGeneralCategoryMapRepository.Fetch().Where(x => x.GeneralCategoryCode == objModel.LoanType).FirstOrDefault().SapCode.ToString();


            return new
            {
                NoReg = documentApproval.CreatedBy,
                InfoType = "0045",
                LoanType = loanType,
                //SequenceNumber = "01",
                SequenceNumber = "",
                StartDate = StartDate,
                ApprovalDate = documentApproval.LastApprovedOn.Value.ToString("dd/MM/yyyy"), //perlu nanya mang yudha
                LoanAmount = objModel.LoanAmount,
                CurrencyKey = "IDR",
                LoanConditions = "01",//hardcode sesuai value yang ada di excel SAP to NET Mapping
                RepaymentStart = RepaymentDate, //+1 bulan setelah start date
                Annuity = Convert.ToInt32(pembulatan),
                PaymentDate = StartDate,
                PaymentType = "0150", 
                Amount = objModel.LoanAmount,
                InterestRate = "",
                RefInterest = "",
            };

        }

        private dynamic GetMeal(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail, DateTime startDate)
        {
            var objModel = JsonConvert.DeserializeObject<MealAllowanceViewModel>(documentRequestDetail.ObjectValue);
            var FinalApproveDate = startDate.ToString("dd/MM/yyyy");
            var wageTypeCode = objModel.WageTypeCode;

            if (string.IsNullOrEmpty(wageTypeCode))
            {
                var vld = "50000136";
                var spld = "50000133";
                var security = "50000323";
                var karawangArea = "1110";
                var cikampekArea = "cikampek";
                var cibitungArea = "1060";

                wageTypeCode = "0710";

                var organization = ActualOrganizationStructureReadonlyRepository.Fetch()
                    .AsNoTracking()
                    .FirstOrDefault(x => x.NoReg == documentApproval.CreatedBy && x.Staffing == 100);

                if (organization != null)
                {
                    if (organization.Structure.Contains("(" + security + ")") || organization.OrgCode == security)
                    {
                        wageTypeCode = "0133";
                    }
                    else if (organization.Structure.Contains("(" + spld + ")") || organization.OrgCode == spld)
                    {
                        wageTypeCode = "0701";
                    }
                    else if (organization.Structure.Contains("(" + vld + ")") || organization.OrgCode == vld)
                    {
                        wageTypeCode = "0135";

                        if (organization.PersonalSubarea == karawangArea)
                        {
                            wageTypeCode = "0703";
                        }
                        else if (organization.PersonalSubarea == cikampekArea)
                        {
                            wageTypeCode = "0704";
                        }
                        else if (organization.PersonalSubarea == cibitungArea)
                        {
                            wageTypeCode = "0707";
                        }
                    }
                }
            }

            dynamic expandoObject = new ExpandoObject();

            expandoObject.NoReg = documentApproval.CreatedBy;
            expandoObject.InfoType = "2010";
            expandoObject.Date = FinalApproveDate;
            expandoObject.WageType = wageTypeCode;
            expandoObject.NumberOfHours = "";
            expandoObject.Number = objModel.data != null ? objModel.data.Count() : 0;
            expandoObject.Unit = "DAYS";

            return expandoObject;
        }

        private dynamic GetShiftMeal(DocumentApproval documentApproval, DocumentRequestDetail documentRequestDetail, DateTime startDate)
        {
            var objModel = JsonConvert.DeserializeObject<ShiftMealAllowanceViewModel>(documentRequestDetail.ObjectValue);
            var FinalApproveDate = startDate.ToString("dd/MM/yyyy");//documentApproval.DocumentStatusCode == "completed" ? documentApproval.ModifiedOn.Value.ToString("dd/MM/yyyy") : "";
            var listClaim = new List<dynamic>();

            foreach (var summarie in objModel.Summaries)
            {
                if (summarie.Employees == null)
                    continue;

                foreach (var emp in summarie.Employees)
                {
                    listClaim.Add(
                    new
                    {
                        NoReg = emp.NoReg,
                        Infotype = "2010",
                        Date = FinalApproveDate,
                        WageType = "0710", //hardcode sesuai value yang ada di excel SAP to NET Mapping
                        NumberOfHours = "",
                        Number = 1,
                        Unit = "DAYS"
                    });
                }
            }

            return listClaim;
        }

        private dynamic GetHiringEmployee(DocumentApproval documentApproval, DateTime startDate)
        {
            var hiringData = UnitOfWork.UdfQuery<HiringEmployeeStoredEntity>(new { documentApproval.Id });

            var listDataEmployee = new List<dynamic>();

            foreach (var data in hiringData)
            {
                if (data.NoReg == null)
                    continue;

                listDataEmployee.Add(
                    new
                    {
                        Id = data.Id,
                        NoReg = data.NoReg,
                        PlacementDate = data.PlacementDate,
                        ActionType = data.ActionType,
                        ReasonForAction = data.ReasonForAction,
                        Position = data.Position,
                        PersonelArea = data.PersonelArea,
                        EmployeeGroup = data.EmployeeGroup,
                        EmployeeSubGroup = data.EmployeeSubGroup,
                        SubArea = data.SubArea,
                        WorkContract = data.WorkContract,
                        EffectiveDate = data.EffectiveDate,
                        SubClass = data.SubClass,
                        LabourType = data.LabourType,
                        Name = data.Name,
                        FullName = data.FullName,
                        BirthPlace = data.BirthPlace,
                        BirthDate = data.BirthDate,
                        Gender = data.Gender,
                        Religion = data.Religion,
                        BloodType = data.BloodType,
                        Nationality = data.Nationality,
                        NIK = data.NIK,
                        Passport = data.Passport,
                        KK = data.KK,
                        BPJSNumber = data.BPJSNumber,
                        PhoneNumber = data.PhoneNumber,
                        email = data.email,
                        NPWP = data.NPWP,
                        TaxStatus = data.TaxStatus,
                        Dependence = data.Dependence,
                        MaritalStatus = data.MaritalStatus,
                        MaritalNumber = data.MaritalNumber,
                        MaritalDate = data.MaritalDate,
                        SimANumber = data.SimANumber,
                        SimCNumber = data.SimCNumber,
                        PrimaryAddress = data.PrimaryAddress,
                        PrimaryRT = data.PrimaryRT,
                        PrimaryRW = data.PrimaryRW,
                        PrimaryUrbanVillage = data.PrimaryUrbanVillage,
                        PrimarySubDistrict = data.PrimarySubDistrict,
                        PrimaryDistrict = data.PrimaryDistrict,
                        PrimaryProvince = data.PrimaryProvince,
                        PrimaryPostalCode = data.PrimaryPostalCode,
                        HomeAddress = data.HomeAddress,
                        HomeRT = data.HomeRT,
                        HomeRW = data.HomeRW,
                        HomeUrbanVillage = data.HomeUrbanVillage,
                        HomeSubDistrict = data.HomeSubDistrict,
                        HomeDistrict = data.HomeDistrict,
                        HomeProvince = data.HomeProvince,
                        HomePostalCode = data.HomePostalCode,
                        BankAccountNumber = data.BankAccountNumber,
                        BankAccountName = data.BankAccountName,
                        BankAccountKey = data.BankAccountKey,
                        PaymentType = data.PaymentType,
                        AyahKandung = data.AyahKandung,
                        AyahKandungBirthPlace = data.AyahKandungBirthPlace,
                        AyahKandungBirthDate = data.AyahKandungBirthDate,
                        AyahKandungGender = data.AyahKandungGender,
                        IbuKandung = data.IbuKandung,
                        IbuKandungBirthPlace = data.IbuKandungBirthPlace,
                        IbuKandungBirthDate = data.IbuKandungBirthDate,
                        IbuKandungGender = data.IbuKandungGender,
                        SaudaraKandung = data.SaudaraKandung,
                        SaudaraKandungBirthPlace = data.SaudaraKandungBirthPlace,
                        SaudaraKandungBirthDate = data.SaudaraKandungBirthDate,
                        SaudaraKandungGender = data.SaudaraKandungGender,
                        Pasangan = data.Pasangan,
                        PasanganBirthPlace = data.PasanganBirthPlace,
                        PasanganBirthDate = data.PasanganBirthDate,
                        PasanganGender = data.PasanganGender,
                        FirstChild = data.FirstChild,
                        FirstChildNumber = data.FirstChildNumber,
                        FirstChildStatus = data.FirstChildStatus,
                        FirstChildBirthPlace = data.FirstChildBirthPlace,
                        FirstChildBirthDate = data.FirstChildBirthDate,
                        FirstChildGender = data.FirstChildGender,
                        SecondChild = data.SecondChild,
                        SecondChildNumber = data.SecondChildNumber,
                        SecondChildStatus = data.SecondChildStatus,
                        SecondChildBirthPlace = data.SecondChildBirthPlace,
                        SecondChildBirthDate = data.SecondChildBirthDate,
                        SecondChildGender = data.SecondChildGender,
                        ThirdChild = data.ThirdChild,
                        ThirdChildNumber = data.ThirdChildNumber,
                        ThirdChildStatus = data.ThirdChildStatus,
                        ThirdChildBirthPlace = data.ThirdChildBirthPlace,
                        ThirdChildBirthDate = data.ThirdChildBirthDate,
                        ThirdChildGender = data.ThirdChildGender,
                        AyahMertua = data.AyahMertua,
                        AyahMertuaBirthPlace = data.AyahMertuaBirthPlace,
                        AyahMertuaBirthDate = data.AyahMertuaBirthDate,
                        AyahMertuaGender = data.AyahMertuaGender,
                        IbuMertua = data.IbuMertua,
                        IbuMertuaBirthPlace = data.IbuMertuaBirthPlace,
                        IbuMertuaBirthDate = data.IbuMertuaBirthDate,
                        IbuMertuaGender = data.IbuMertuaGender,
                        KakekDariAyah = data.KakekDariAyah,
                        KakekDariAyahBirthDate = data.KakekDariAyahBirthDate,
                        KakekDariAyahGender = data.KakekDariAyahGender,
                        KakekDariIbu = data.KakekDariIbu,
                        KakekDariIbuBirthDate = data.KakekDariIbuBirthDate,
                        KakekDariIbuGender = data.KakekDariIbuGender,
                        NenekDariAyah = data.NenekDariAyah,
                        NenekDariAyahBirthDate = data.NenekDariAyahBirthDate,
                        NenekDariAyahGender = data.NenekDariAyahGender,
                        NenekDariIbu = data.NenekDariIbu,
                        NenekDariIbuBirthDate = data.NenekDariIbuBirthDate,
                        NenekDariIbuGender = data.NenekDariIbuGender,
                        EducationType = data.EducationType,
                        Institute = data.Institute,
                        Major = data.Major,
                        NationInstitute = data.NationInstitute,
                        StartDateEducation = data.StartDateEducation,
                        GraduationDate = data.GraduationDate,
                        FinalGrade = data.FinalGrade,
                        Certificate = data.Certificate,
                        JoinAstraDate = data.JoinAstraDate,
                        WorkScheduleRule = data.WorkScheduleRule,
                        TimeManagementStatus = data.TimeManagementStatus,
                        TimeRecord = data.TimeRecord,
                        IdVersion = data.IdVersion

                    }
                );


            }

            return listDataEmployee;
        }

        private dynamic GetBpjsTk(DocumentApproval documentApproval, DateTime startDate)
        {
            var dataBpjsTk = UnitOfWork.UdfQuery<PersonalDataBpjsTkStoredEntity>(new { documentApproval.Id });

            var listDataEmployee = new List<dynamic>();

            foreach (var data in dataBpjsTk)
            {
                if (data.NoReg == null)
                    continue;

                listDataEmployee.Add(
                    new
                    {
                        Id = data.Id,
                        NoReg = data.NoReg,
                        StartDate = data.StartDate,
                        EndDate = data.EndDate,
                        BPJSTK = data.BPJSTK

                    }
                );


            }

            return listDataEmployee;
        }

        public void GenerateFiles(IEnumerable<ContentEntry> contentEntries)
        {
            var sapFolderPath = " ";
            var directoryInfo = new DirectoryInfo(sapFolderPath);

            var files = directoryInfo.GetFiles("*.xlsx");

            foreach (var file in files)
            {
                file.Delete();
            }

            foreach (var contentEntry in contentEntries)
            {
                File.WriteAllBytes(Path.Combine(sapFolderPath, contentEntry.FileName), contentEntry.Contents);
            }
        }

        public IEnumerable<ZipEntry> GenerateExcelTermination(string[] documentNumbers)
        {
            var storedConnection = this.UnitOfWork.GetConnection();
            var adoService = new AdoService(storedConnection, new AdoServiceParameter
            {
                Query = "dbo.usp_GenerateSapReportTermination",
                CommandType = CommandType.StoredProcedure,
                ParameterCollection = new DbParameterCollection(
                    new DbParameter("documentNumbers", "(" + string.Join("),(", documentNumbers) + ")")
                )
            });

            var ds = adoService.ExecuteStoredProcedure();
            var now = DateTime.Now;
            var tableNames = new[] { "Relationship_Delimit", "EmployeeAction", "OrganizationalAssignment" };
            for (var i = 0; i < tableNames.Length; i++)
            {
                ds.Tables[i].TableName = tableNames[i];
            }

            var configs = new List<ExcelConfig>(new[] {
                new ExcelConfig()
                {
                    Name = "EmployeeAction",
                    SourceFile = string.Format("005_EMP_IT0000_{0:ddMMyyyy}_{1:hhmm}.xlsx", now, now),
                    SheetName = "output",
                    ExcelColumnConfig = new[] {
                        new ExcelColumnConfig { ColumnLabel = "A", Field = "PERNR" },
                        new ExcelColumnConfig { ColumnLabel = "B", Field = "SUBTY" },
                        new ExcelColumnConfig { ColumnLabel = "C", Field = "OBJPS" },
                        new ExcelColumnConfig { ColumnLabel = "D", Field = "SPRPS" },
                        new ExcelColumnConfig { ColumnLabel = "E", Field = "ENDDA" },
                        new ExcelColumnConfig { ColumnLabel = "F", Field = "BEGDA" },
                        new ExcelColumnConfig { ColumnLabel = "G", Field = "SEQNR" },
                        new ExcelColumnConfig { ColumnLabel = "H", Field = "AEDTM" },
                        new ExcelColumnConfig { ColumnLabel = "I", Field = "UNAME" },
                        new ExcelColumnConfig { ColumnLabel = "J", Field = "HISTO" },
                        new ExcelColumnConfig { ColumnLabel = "K", Field = "ITXEX" },
                        new ExcelColumnConfig { ColumnLabel = "L", Field = "REFEX" },
                        new ExcelColumnConfig { ColumnLabel = "M", Field = "ORDEX" },
                        new ExcelColumnConfig { ColumnLabel = "N", Field = "ITBLD" },
                        new ExcelColumnConfig { ColumnLabel = "O", Field = "PREAS" },
                        new ExcelColumnConfig { ColumnLabel = "P", Field = "FLAG1" },
                        new ExcelColumnConfig { ColumnLabel = "Q", Field = "FLAG2" },
                        new ExcelColumnConfig { ColumnLabel = "R", Field = "FLAG3" },
                        new ExcelColumnConfig { ColumnLabel = "S", Field = "FLAG4" },
                        new ExcelColumnConfig { ColumnLabel = "T", Field = "RESE1" },
                        new ExcelColumnConfig { ColumnLabel = "U", Field = "RESE2" },
                        new ExcelColumnConfig { ColumnLabel = "V", Field = "GRPVL" },
                        new ExcelColumnConfig { ColumnLabel = "W", Field = "MASSN" },
                        new ExcelColumnConfig { ColumnLabel = "X", Field = "MASSG" },
                        new ExcelColumnConfig { ColumnLabel = "Y", Field = "STAT1" },
                        new ExcelColumnConfig { ColumnLabel = "Z", Field = "STAT2" },
                        new ExcelColumnConfig { ColumnLabel = "AA", Field = "STAT3" },
                    }
                },
                new ExcelConfig()
                {
                    Name = "OrganizationalAssignment",
                    SourceFile = string.Format("006_EMP_IT0001_{0:ddMMyyyy}_{1:hhmm}.xlsx", now, now),
                    SheetName = "output",
                    ExcelColumnConfig = new[] {
                        new ExcelColumnConfig { ColumnLabel = "A", Field = "PERNR" },
                        new ExcelColumnConfig { ColumnLabel = "B", Field = "SUBTY" },
                        new ExcelColumnConfig { ColumnLabel = "C", Field = "OBJPS" },
                        new ExcelColumnConfig { ColumnLabel = "D", Field = "SPRPS" },
                        new ExcelColumnConfig { ColumnLabel = "E", Field = "ENDDA" },
                        new ExcelColumnConfig { ColumnLabel = "F", Field = "BEGDA" },
                        new ExcelColumnConfig { ColumnLabel = "G", Field = "SEQNR" },
                        new ExcelColumnConfig { ColumnLabel = "H", Field = "AEDTM" },
                        new ExcelColumnConfig { ColumnLabel = "I", Field = "UNAME" },
                        new ExcelColumnConfig { ColumnLabel = "J", Field = "HISTO" },
                        new ExcelColumnConfig { ColumnLabel = "K", Field = "ITXEX" },
                        new ExcelColumnConfig { ColumnLabel = "L", Field = "REFEX" },
                        new ExcelColumnConfig { ColumnLabel = "M", Field = "ORDEX" },
                        new ExcelColumnConfig { ColumnLabel = "N", Field = "ITBLD" },
                        new ExcelColumnConfig { ColumnLabel = "O", Field = "PREAS" },
                        new ExcelColumnConfig { ColumnLabel = "P", Field = "FLAG1" },
                        new ExcelColumnConfig { ColumnLabel = "Q", Field = "FLAG2" },
                        new ExcelColumnConfig { ColumnLabel = "R", Field = "FLAG3" },
                        new ExcelColumnConfig { ColumnLabel = "S", Field = "FLAG4" },
                        new ExcelColumnConfig { ColumnLabel = "T", Field = "RESE1" },
                        new ExcelColumnConfig { ColumnLabel = "U", Field = "RESE2" },
                        new ExcelColumnConfig { ColumnLabel = "V", Field = "GRPVL" },
                        new ExcelColumnConfig { ColumnLabel = "W", Field = "BUKRS" },
                        new ExcelColumnConfig { ColumnLabel = "X", Field = "WERKS" },
                        new ExcelColumnConfig { ColumnLabel = "Y", Field = "PERSG" },
                        new ExcelColumnConfig { ColumnLabel = "Z", Field = "PERSK" },
                        new ExcelColumnConfig { ColumnLabel = "AA", Field = "VDSK1" },
                        new ExcelColumnConfig { ColumnLabel = "AB", Field = "GSBER" },
                        new ExcelColumnConfig { ColumnLabel = "AC", Field = "BTRTL" },
                        new ExcelColumnConfig { ColumnLabel = "AD", Field = "JUPER" },
                        new ExcelColumnConfig { ColumnLabel = "AE", Field = "ABKRS" },
                        new ExcelColumnConfig { ColumnLabel = "AF", Field = "ANSVH" },
                        new ExcelColumnConfig { ColumnLabel = "AG", Field = "KOSTL" },
                        new ExcelColumnConfig { ColumnLabel = "AH", Field = "ORGEH" },
                        new ExcelColumnConfig { ColumnLabel = "AI", Field = "PLANS" },
                        new ExcelColumnConfig { ColumnLabel = "AJ", Field = "STELL" },
                        new ExcelColumnConfig { ColumnLabel = "AK", Field = "MSTBR" },
                        new ExcelColumnConfig { ColumnLabel = "AL", Field = "SACHA" },
                        new ExcelColumnConfig { ColumnLabel = "AM", Field = "SACHP" },
                        new ExcelColumnConfig { ColumnLabel = "AN", Field = "SACHZ" },
                        new ExcelColumnConfig { ColumnLabel = "AO", Field = "SNAME" },
                        new ExcelColumnConfig { ColumnLabel = "AP", Field = "ENAME" },
                        new ExcelColumnConfig { ColumnLabel = "AQ", Field = "OTYPE" },
                        new ExcelColumnConfig { ColumnLabel = "AR", Field = "SBMOD" },
                        new ExcelColumnConfig { ColumnLabel = "AS", Field = "KOKRS" },
                        new ExcelColumnConfig { ColumnLabel = "AT", Field = "FISTL" },
                        new ExcelColumnConfig { ColumnLabel = "AU", Field = "GEBER" },
                        new ExcelColumnConfig { ColumnLabel = "AV", Field = "FKBER" },
                        new ExcelColumnConfig { ColumnLabel = "AW", Field = "GRANT_NBR" },
                        new ExcelColumnConfig { ColumnLabel = "AX", Field = "SGMNT" },
                        new ExcelColumnConfig { ColumnLabel = "AY", Field = "BUDGET_PD" },
                        new ExcelColumnConfig { ColumnLabel = "AZ", Field = "EFDAT" },
                        new ExcelColumnConfig { ColumnLabel = "BA", Field = "SUBCLS" },
                        new ExcelColumnConfig { ColumnLabel = "BB", Field = "LABTP" }
                    }
                },
                new ExcelConfig()
                {
                    Name = "Relationship_Delimit",
                    SourceFile = string.Format("004_REL_{0:ddMMyyyy}_DELIMIT.xlsx", now),
                    SheetName = "output",
                    ExcelColumnConfig = new[] {
                        new ExcelColumnConfig { ColumnLabel = "A", Field = "ObjectType" },
                        new ExcelColumnConfig { ColumnLabel = "B", Field = "ObjectID" },
                        new ExcelColumnConfig { ColumnLabel = "C", Field = "RelationType" },
                        new ExcelColumnConfig { ColumnLabel = "D", Field = "RelationObject" },
                        new ExcelColumnConfig { ColumnLabel = "E", Field = "RelationObjectType" },
                        new ExcelColumnConfig { ColumnLabel = "F", Field = "RelationObjectID" },
                        new ExcelColumnConfig { ColumnLabel = "G", Field = "ValidFrom" },
                        new ExcelColumnConfig { ColumnLabel = "H", Field = "ValidTo" },
                        new ExcelColumnConfig { ColumnLabel = "I", Field = "DeletionFlag" },
                        new ExcelColumnConfig { ColumnLabel = "J", Field = "Percentage" }
                    }
                }
                
            });

            return ds.SaveToStreams(configs);
        }

        public IEnumerable<ZipEntry> GenerateExcelUpdateContract(string[] documentNumbers)
        {
            var storedConnection = this.UnitOfWork.GetConnection();
            var adoService = new AdoService(storedConnection, new AdoServiceParameter
            {
                Query = "dbo.usp_GenerateSapReportUpdateContract",
                CommandType = CommandType.StoredProcedure,
                ParameterCollection = new DbParameterCollection(
                    new DbParameter("documentNumbers", "(" + string.Join("),(", documentNumbers) + ")")
                )
            });

            var ds = adoService.ExecuteStoredProcedure();
            var now = DateTime.Now;
            var tableNames = new[] { "EmployeeAction", "OrganizationalAssignment" };
            for (var i = 0; i < tableNames.Length; i++)
            {
                ds.Tables[i].TableName = tableNames[i];
            }

            var configs = new List<ExcelConfig>(new[] {
                new ExcelConfig()
                {
                    Name = "EmployeeAction",
                    SourceFile = string.Format("005_EMP_IT0000_{0:ddMMyyyy}_{1:hhmm}.xlsx", now, now),
                    SheetName = "output",
                    ExcelColumnConfig = new[] {
                        new ExcelColumnConfig { ColumnLabel = "A", Field = "PERNR" },
                        new ExcelColumnConfig { ColumnLabel = "B", Field = "SUBTY" },
                        new ExcelColumnConfig { ColumnLabel = "C", Field = "OBJPS" },
                        new ExcelColumnConfig { ColumnLabel = "D", Field = "SPRPS" },
                        new ExcelColumnConfig { ColumnLabel = "E", Field = "ENDDA" },
                        new ExcelColumnConfig { ColumnLabel = "F", Field = "BEGDA" },
                        new ExcelColumnConfig { ColumnLabel = "G", Field = "SEQNR" },
                        new ExcelColumnConfig { ColumnLabel = "H", Field = "AEDTM" },
                        new ExcelColumnConfig { ColumnLabel = "I", Field = "UNAME" },
                        new ExcelColumnConfig { ColumnLabel = "J", Field = "HISTO" },
                        new ExcelColumnConfig { ColumnLabel = "K", Field = "ITXEX" },
                        new ExcelColumnConfig { ColumnLabel = "L", Field = "REFEX" },
                        new ExcelColumnConfig { ColumnLabel = "M", Field = "ORDEX" },
                        new ExcelColumnConfig { ColumnLabel = "N", Field = "ITBLD" },
                        new ExcelColumnConfig { ColumnLabel = "O", Field = "PREAS" },
                        new ExcelColumnConfig { ColumnLabel = "P", Field = "FLAG1" },
                        new ExcelColumnConfig { ColumnLabel = "Q", Field = "FLAG2" },
                        new ExcelColumnConfig { ColumnLabel = "R", Field = "FLAG3" },
                        new ExcelColumnConfig { ColumnLabel = "S", Field = "FLAG4" },
                        new ExcelColumnConfig { ColumnLabel = "T", Field = "RESE1" },
                        new ExcelColumnConfig { ColumnLabel = "U", Field = "RESE2" },
                        new ExcelColumnConfig { ColumnLabel = "V", Field = "GRPVL" },
                        new ExcelColumnConfig { ColumnLabel = "W", Field = "MASSN" },
                        new ExcelColumnConfig { ColumnLabel = "X", Field = "MASSG" },
                        new ExcelColumnConfig { ColumnLabel = "Y", Field = "STAT1" },
                        new ExcelColumnConfig { ColumnLabel = "Z", Field = "STAT2" },
                        new ExcelColumnConfig { ColumnLabel = "AA", Field = "STAT3" },
                    }
                },
                new ExcelConfig()
                {
                    Name = "OrganizationalAssignment",
                    SourceFile = string.Format("006_EMP_IT0001_{0:ddMMyyyy}_{1:hhmm}.xlsx", now, now),
                    SheetName = "output",
                    ExcelColumnConfig = new[] {
                        new ExcelColumnConfig { ColumnLabel = "A", Field = "PERNR" },
                        new ExcelColumnConfig { ColumnLabel = "B", Field = "SUBTY" },
                        new ExcelColumnConfig { ColumnLabel = "C", Field = "OBJPS" },
                        new ExcelColumnConfig { ColumnLabel = "D", Field = "SPRPS" },
                        new ExcelColumnConfig { ColumnLabel = "E", Field = "ENDDA" },
                        new ExcelColumnConfig { ColumnLabel = "F", Field = "BEGDA" },
                        new ExcelColumnConfig { ColumnLabel = "G", Field = "SEQNR" },
                        new ExcelColumnConfig { ColumnLabel = "H", Field = "AEDTM" },
                        new ExcelColumnConfig { ColumnLabel = "I", Field = "UNAME" },
                        new ExcelColumnConfig { ColumnLabel = "J", Field = "HISTO" },
                        new ExcelColumnConfig { ColumnLabel = "K", Field = "ITXEX" },
                        new ExcelColumnConfig { ColumnLabel = "L", Field = "REFEX" },
                        new ExcelColumnConfig { ColumnLabel = "M", Field = "ORDEX" },
                        new ExcelColumnConfig { ColumnLabel = "N", Field = "ITBLD" },
                        new ExcelColumnConfig { ColumnLabel = "O", Field = "PREAS" },
                        new ExcelColumnConfig { ColumnLabel = "P", Field = "FLAG1" },
                        new ExcelColumnConfig { ColumnLabel = "Q", Field = "FLAG2" },
                        new ExcelColumnConfig { ColumnLabel = "R", Field = "FLAG3" },
                        new ExcelColumnConfig { ColumnLabel = "S", Field = "FLAG4" },
                        new ExcelColumnConfig { ColumnLabel = "T", Field = "RESE1" },
                        new ExcelColumnConfig { ColumnLabel = "U", Field = "RESE2" },
                        new ExcelColumnConfig { ColumnLabel = "V", Field = "GRPVL" },
                        new ExcelColumnConfig { ColumnLabel = "W", Field = "BUKRS" },
                        new ExcelColumnConfig { ColumnLabel = "X", Field = "WERKS" },
                        new ExcelColumnConfig { ColumnLabel = "Y", Field = "PERSG" },
                        new ExcelColumnConfig { ColumnLabel = "Z", Field = "PERSK" },
                        new ExcelColumnConfig { ColumnLabel = "AA", Field = "VDSK1" },
                        new ExcelColumnConfig { ColumnLabel = "AB", Field = "GSBER" },
                        new ExcelColumnConfig { ColumnLabel = "AC", Field = "BTRTL" },
                        new ExcelColumnConfig { ColumnLabel = "AD", Field = "JUPER" },
                        new ExcelColumnConfig { ColumnLabel = "AE", Field = "ABKRS" },
                        new ExcelColumnConfig { ColumnLabel = "AF", Field = "ANSVH" },
                        new ExcelColumnConfig { ColumnLabel = "AG", Field = "KOSTL" },
                        new ExcelColumnConfig { ColumnLabel = "AH", Field = "ORGEH" },
                        new ExcelColumnConfig { ColumnLabel = "AI", Field = "PLANS" },
                        new ExcelColumnConfig { ColumnLabel = "AJ", Field = "STELL" },
                        new ExcelColumnConfig { ColumnLabel = "AK", Field = "MSTBR" },
                        new ExcelColumnConfig { ColumnLabel = "AL", Field = "SACHA" },
                        new ExcelColumnConfig { ColumnLabel = "AM", Field = "SACHP" },
                        new ExcelColumnConfig { ColumnLabel = "AN", Field = "SACHZ" },
                        new ExcelColumnConfig { ColumnLabel = "AO", Field = "SNAME" },
                        new ExcelColumnConfig { ColumnLabel = "AP", Field = "ENAME" },
                        new ExcelColumnConfig { ColumnLabel = "AQ", Field = "OTYPE" },
                        new ExcelColumnConfig { ColumnLabel = "AR", Field = "SBMOD" },
                        new ExcelColumnConfig { ColumnLabel = "AS", Field = "KOKRS" },
                        new ExcelColumnConfig { ColumnLabel = "AT", Field = "FISTL" },
                        new ExcelColumnConfig { ColumnLabel = "AU", Field = "GEBER" },
                        new ExcelColumnConfig { ColumnLabel = "AV", Field = "FKBER" },
                        new ExcelColumnConfig { ColumnLabel = "AW", Field = "GRANT_NBR" },
                        new ExcelColumnConfig { ColumnLabel = "AX", Field = "SGMNT" },
                        new ExcelColumnConfig { ColumnLabel = "AY", Field = "BUDGET_PD" },
                        new ExcelColumnConfig { ColumnLabel = "AZ", Field = "EFDAT" },
                        new ExcelColumnConfig { ColumnLabel = "BA", Field = "SUBCLS" },
                        new ExcelColumnConfig { ColumnLabel = "BB", Field = "LABTP" }
                    }
                }

            });

            return ds.SaveToStreams(configs);
        }
    }
}

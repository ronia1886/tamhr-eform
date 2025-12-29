using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Helpers;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainEvents;
using Agit.Common;
using Agit.Domain;
using Agit.Common.Extensions;
using Agit.Domain.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Newtonsoft.Json;
using Z.EntityFramework.Plus;
using System.Globalization;
using TAMHR.ESS.Infrastructure.Responses;
using Dapper;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
//using Telerik.Reporting;
using Newtonsoft.Json;
//using Telerik.Reporting.Processing;
using Agit.Domain.Extensions;
using Microsoft.EntityFrameworkCore.Internal;
using System.Data.SqlClient;
using System.Data;
using OfficeOpenXml;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle others module.
    /// </summary>
    public class EmployeeProfileService : DomainServiceBase
    {
        #region Domain Repositories
        /// <summary>
        /// Employee Profile View repository object.
        /// </summary>
        protected IRepository<EmployeProfileView> EmployeeProfileViewRepository => UnitOfWork.GetRepository<EmployeProfileView>();
        protected IRepository<EmployeProfileActualView> EmployeeProfileActualViewRepository => UnitOfWork.GetRepository<EmployeProfileActualView>();
        protected IRepository<CommonFile> CommonFileRepository => UnitOfWork.GetRepository<CommonFile>();
        protected IRepository<GeneralCategoryData> generalCategory=> UnitOfWork.GetRepository<GeneralCategoryData>();
        protected IRepository<GeneralCategory> generalCategoryESS => UnitOfWork.GetRepository<GeneralCategory>();
        protected IRepository<PersonalDataInformationView> personalDataInformation => UnitOfWork.GetRepository<PersonalDataInformationView>();
        protected IRepository<PersonalDataFamilyView> personalDataFamilyRepository => UnitOfWork.GetRepository<PersonalDataFamilyView>();
        protected IRepository<PersonalDataEducation> personalDataEducationRepository => UnitOfWork.GetRepository<PersonalDataEducation>();
        protected IRepository<PersonalDataFamilyDetailView> personalDataFamilyDetail => UnitOfWork.GetRepository<PersonalDataFamilyDetailView>();
        protected IRepository<Bank> bankRepository => UnitOfWork.GetRepository<Bank>();
        protected IRepository<PersonalDataBankAccount> personaldatabankRepository => UnitOfWork.GetRepository<PersonalDataBankAccount>();
        protected IRepository<PersonalDataTaxStatus> personaldataTaxStatus => UnitOfWork.GetRepository<PersonalDataTaxStatus>();
        protected IRepository<PersonalDataEducationDetailView> personaldataEducations => UnitOfWork.GetRepository<PersonalDataEducationDetailView>();
        protected IRepository<Location> LocationRepository => UnitOfWork.GetRepository<Location>();
        protected IRepository<Major> MajorRepository => UnitOfWork.GetRepository<Major>();

        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public EmployeeProfileService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        public IEnumerable<EmployeProfileView> getEmployee()
        {
            return EmployeeProfileViewRepository.Fetch()
                .AsNoTracking().ToList();
        }
        public IEnumerable<PersonalDataInformationView> getPersonalDatas()
        {
            return personalDataInformation.Fetch()
                .AsNoTracking()
                .OrderByDescending(x => x.Noreg) 
                .ToList();
        }

        public IQueryable<PersonalDataFamilyDetailView> getFamilyDatas()
        {
            return personalDataFamilyDetail.Fetch()
                .AsNoTracking()
                .OrderByDescending(x => x.Noreg);
        }

        public IEnumerable<PersonalDataEducationDetailView> getEducationDatas()
        {
            return personaldataEducations.Fetch()
                .AsNoTracking()
                .OrderByDescending(x => x.Noreg)
                .ToList();
        }

        public IEnumerable<PersonalDataFamilyView> getPersonalDataFamily()
        {
            return personalDataFamilyRepository.Fetch()
                .AsNoTracking().ToList();
        }

        public IEnumerable<EmployeProfileActualView> getEmployeeActual()
        {
            return EmployeeProfileActualViewRepository.Fetch()
                .AsNoTracking().ToList();
        }

        public IEnumerable<string> GetInvitationUniqueColumnValuesDashboard(string fieldName)
        {
                var attribute = typeof(EmployeProfileView).GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;
                //var attribute = typeof(EmployeProfileActualView).GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;
                string sql = $"SELECT DISTINCT {fieldName} FROM {attribute.Name} WHERE {fieldName} IS NOT NULL";

                return UnitOfWork.GetConnection().Query<string>(sql);
        }

        public IEnumerable<EmployeeOrganizationObjectStoredEntityNew> GetEmployeeOrganizationObjects(string noReg)
        {
            return UnitOfWork.UspQuery<EmployeeOrganizationObjectStoredEntityNew>(new { NoReg = noReg });
        }

        public PersonalDataAttributeStroredEntity GetProfile(string noReg)
        {
            return UnitOfWork.UspQuery<PersonalDataAttributeStroredEntity>(new { NoReg = noReg }).FirstOrDefault();
        }

        public IEnumerable<string> GetNationalityList()
        {
            var data = generalCategory.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "Nationality" && !string.IsNullOrEmpty(x.Name))
                .Select(x => x.Name)
                .Distinct()
                .OrderBy(x => x) // Sort alphabetically (A → Z)
                .ToList();

            return data;
        }

        public IEnumerable<string> GetRegionList()
        {
            var data = generalCategoryESS.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "Region" && !string.IsNullOrEmpty(x.Name))
                .Select(x => x.Name)
                .Distinct()
                .OrderBy(x => x) // Sort alphabetically (A → Z)
                .ToList();

            return data;
        }

        public IEnumerable<string> GetDistrictList()
        {
            var data = generalCategory.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "District" && !string.IsNullOrEmpty(x.Name))
                .Select(x => x.Name)
                .Distinct()
                .OrderBy(x => x) // Sort alphabetically (A → Z)
                .ToList();

            return data;
        }

        public IEnumerable<string> GetSubDistrictList()
        {
            var data = generalCategoryESS.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "SubDistrict" && !string.IsNullOrEmpty(x.Name))
                .Select(x => x.Name)
                .Distinct()
                .OrderBy(x => x) // Sort alphabetically (A → Z)
                .ToList();

            return data;
        }

        public IEnumerable<string> GetPostalCodeList()
        {
            var data = generalCategoryESS.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "postCode" && !string.IsNullOrEmpty(x.Name))
                .Select(x => x.Name)
                .Distinct()
                .OrderBy(x => x) // Sort alphabetically (A → Z)
                .ToList();

            return data;
        }

        public IEnumerable<string> GetAdministrativeVillageList()
        {
            var data = generalCategory.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "AdministrativeVillage" && !string.IsNullOrEmpty(x.Name))
                .Select(x => x.Name)
                .Distinct()
                .OrderBy(x => x) // Sort alphabetically (A → Z)
                .ToList();

            return data;
        }

        public IEnumerable<string> SearchAdministrativeVillage(string keyword)
        {
            return generalCategory.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "AdministrativeVillage" &&
                            !string.IsNullOrEmpty(x.Name) &&
                            x.Name.Contains(keyword))
                .Select(x => x.Name)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }


        public IEnumerable<string> GetPersonalSubArea()
        {
            var data = LocationRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.RowStatus
                         && DateTime.Now >= x.StartDate
                         && DateTime.Now <= x.EndDate)
                .Select(x => x.Name)
                .ToList();

            return data;
        }

        public IEnumerable<string> GetPersonalArea()
        {
            var data = generalCategoryESS.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "PersonalArea")
                .Select(x => x.Name).ToList();

            return data;
        }

        public IEnumerable<string> GetMaritalStatus()
        {
            var data = generalCategoryESS.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "maritalStatusCode")
                .Select(x => x.Name).ToList();

            return data;
        }

        public IEnumerable<string> GetReligion()
        {
            var data = generalCategory.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "religioncode")
                .OrderBy(x => x.Name == "-" ? 0 : 1)
                .Select(x => x.Name)
                .ToList();

            return data;
        }

        public IOrderedEnumerable<string> GetEducationLevel()
        {
            var educationLevel = generalCategoryESS.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "EducationLevel" && !string.IsNullOrEmpty(x.Name))
                .Select(x => x.Name)
                .Distinct().ToList();

            var data = educationLevel.OrderBy(name => GetEducationOrder(name));

            return data;
        }

        public IEnumerable<string> GetResidentialStatus()
        {
            var data = generalCategory.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "ResidentialStatus" && !string.IsNullOrEmpty(x.Name))
                .Select(x => x.Name)
                .Distinct()
                .OrderBy(name => name)
                .ToList();

            return data;
        }

        int GetEducationOrder(string name)
        {
            if (name == "SD & Sederajat") return 1;
            else if (name == "SMP & Sederajat") return 2;
            else if (name == "SMA & Sederajat") return 3;
            else if (name == "Diploma 1") return 4;
            else if (name == "Diploma 2") return 5;
            else if (name == "Diploma 3") return 6;
            else if (name == "Diploma 4") return 7;
            else if (name == "Strata 1") return 8;
            else if (name == "Strata 2") return 9;
            else if (name == "Strata 3") return 10;
            else return 99;
        }

        public IEnumerable<string> GetFamilyType()
        {
            var data = generalCategory.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "familyTypeCode" && !string.IsNullOrEmpty(x.Name))
                .OrderBy(x => x.Name)
                .Select(x => x.Name).ToList();

            return data;
        }

        public IEnumerable<string> GetBank()
        {
            var data = bankRepository.Fetch()
                .AsNoTracking()
                .Where(x => !string.IsNullOrEmpty(x.BankName))
                .Select(x => x.BankName)
                .Distinct()
                .OrderBy(x => x) 
                .ToList();

            return data;
        }


        public string SelectedBank(string noReg)
        {
            var now = DateTime.Now.Date;
            return (from pa in personaldatabankRepository.Fetch().AsNoTracking()
                    join ba in bankRepository.Fetch().AsNoTracking()
                    on pa.BankCode equals ba.BankKey
                    where pa.NoReg == noReg && now >= pa.StartDate && now <= pa.EndDate 
                    select ba.BankName)
                    .FirstOrDefault() ?? ""; // Ensure it never returns null
        }


        public IEnumerable<string> GetCollage()
        {
            var data = generalCategoryESS.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "Collage" && !string.IsNullOrEmpty(x.Name))
                .Select(x => x.Name)
                .Distinct() // Ensure unique values
                .OrderBy(x => x) // Order by Name descending
                .ToList();

            return data;
        }


        public IEnumerable<string> GetBloodType()
        {
            var data = generalCategory.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "bloodType")
                .OrderBy(x => x.Name)
                .Select(x => x.Name)
                .ToList();

            return data;
        }

        public string GetStatus(string noReg)
        {
            var data = EmployeeProfileViewRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Noreg == noReg)
                .Select(x => x.Expr1).FirstOrDefault();

            return data;
        }

        public List<string> GetTaxStatus(string noReg)
        {
            var data = personaldataTaxStatus.Fetch()
                .AsNoTracking()
                .Select(x => x.TaxStatus)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return data;
        }

        public string SelectedTax(string noReg)
        {
            var data = personaldataTaxStatus.Fetch()
                .AsNoTracking()
                .Where(x => x.RowStatus
                    && x.NoReg == noReg
                    && DateTime.Now >= x.StartDate
                    && (x.EndDate == null || DateTime.Now <= x.EndDate))
                .Select(x => x.TaxStatus)
                .FirstOrDefault();

            return data;
        }

        public IEnumerable<string> GetWorkContract()
        {
            var data = generalCategoryESS.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "WorkContract")
                .Select(x => x.Name).ToList();

            return data;
        }

        public IEnumerable<string> GetBirthPlace()
        {
            var data = generalCategory.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "BirthLocationCode")
                .Select(x => x.Name.Trim())
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return data;
        }

        public IEnumerable<string> GetPhoneCode()
        {
            var data = generalCategory.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "PhoneCode")
                .Select(x => x.Name)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            return data;
        }

        public IEnumerable<string> GetMajors()
        {
            //var data = generalCategoryESS.Fetch()
            //    .AsNoTracking()
            //    .Where(x => x.Category == "Majors" && !string.IsNullOrEmpty(x.Name))
            //    .Select(x => x.Name)
            //    .Distinct()
            //    .OrderBy(x => x)
            //    .ToList();

            var majorsFromGeneralCategory = generalCategoryESS.Fetch()
                .AsNoTracking()
                .Where(x => x.Category == "Majors" && !string.IsNullOrEmpty(x.Name))
                .Select(x => x.Name.ToLower()); // Convert to lowercase

            var majorsFromMajorRepository = MajorRepository.Fetch()
                .AsNoTracking()
                .Where(x => !string.IsNullOrEmpty(x.Name))
                .Select(x => x.Name.ToLower()); // Convert to lowercase

            // Union (gabungkan dua sumber data)
            var data = majorsFromGeneralCategory
                .Concat(majorsFromMajorRepository)
                .Select(x => x)
                .Distinct() // buang duplikat
                .OrderBy(x => x)
                .ToList();

            return data;
        }

        public IEnumerable<dynamic> GetPersonalDataEducation(string noreg)
        {
            // Mengambil semua data kategori dan major dalam satu query
            // Ini lebih efisien daripada memanggil .ToList() berulang kali
            var allCategories = MajorRepository.Fetch().AsNoTracking()
                .Select(m => new GeneralCategory { Id = m.Id, Code = m.Code, Name = m.Name })
                .ToList();

            var generalCategories = generalCategoryESS.Fetch().AsNoTracking().ToList().Union(allCategories);

            var dataGeneralRms = generalCategory.Fetch()
                .AsNoTracking()
                .Where(g => g.Category == "Nationality")
                .ToList();

            // Lakukan join di memori setelah semua data diambil
            // Ini adalah kompromi yang baik jika Anda perlu menggabungkan data dari repository yang berbeda.
            var result = personalDataEducationRepository.Fetch()
                .AsNoTracking()
                .Where(pde => pde.NoReg == noreg)
                .ToList()
                .Select(pde =>
                {
                    var gcEdu = generalCategories.FirstOrDefault(gcs => gcs.Code == pde.EducationTypeCode);
                    var gcMaj = generalCategories.FirstOrDefault(gcss => gcss.Code == pde.Major);
                    var country = dataGeneralRms.FirstOrDefault(gcCou => gcCou.Code == pde.CountryCode);

                    return new
                    {
                        pde.Id,
                        pde.NoReg,
                        pde.EducationTypeCode,
                        EducationTypeName = gcEdu?.Name ?? string.Empty,
                        pde.Institute,
                        pde.CountryCode,
                        CountryName = country?.Name ?? string.Empty,
                        pde.Major,
                        MajorName = gcMaj?.Name ?? string.Empty,
                        pde.FinalGrade,

                        StartEducationDate = pde.StartEducationDate.HasValue ? pde.StartEducationDate.Value.ToString("yyyy-MM-dd") : null,
                        GraduationDate = pde.GraduationDate.HasValue ? pde.GraduationDate.Value.ToString("yyyy-MM-dd") : null,
                    };
                })
                .ToList();

            return result.OrderBy(x => GetEducationOrder(x.EducationTypeName));
        }


        public string getContranct(string noReg)
        {
            var data = EmployeeProfileViewRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Noreg == noReg)
                .Select(x => x.WorkContractText).FirstOrDefault();

            return data;
        }

        public IEnumerable<string> getClass()
        {
            var data = EmployeeProfileViewRepository.Fetch()
                .AsNoTracking()
                .Select(x => x.EmployeeSubGroupText).Distinct().ToList();

            return data;
        }

        public void SaveCommonFile(CommonFile commonFile)
        {
            UnitOfWork.Transact(() =>
            {
                CommonFileRepository.Upsert<Guid>(commonFile);

                UnitOfWork.SaveChanges();
            });
        }

        public void SaveProfileData(string noReg, string pob, DateTime dob, string nik, string address, string kk, string accountnumber,
                                 string religion, string bg, string taxstatus, string bpjs, string insuranceno, string npwp)
        {
            try
            {

                // Create parameters object
                var parameters = new
                {
                    NoReg = noReg,
                    POB = pob,
                    DOB = dob,
                    NIK = nik,
                    Address = address,
                    KK = kk,
                    NPWP = npwp,
                    AccountNumber = accountnumber,
                    Religion = religion,
                    BloodGroup = bg,
                    TaxStatus = taxstatus,
                    BPJS = bpjs,
                    InsuranceNo = insuranceno
                };

                // Execute the stored procedure with parameters
                UnitOfWork.UspQuery("dbo.sp_UpdateProfileData", parameters);
            }
            catch (Exception ex)
            {
                // Handle exception (e.g., log the error)
                throw new Exception("Error while updating profile data.", ex);
            }
        }

        public bool UpdateProfileData(UpdateProfileViewModel model)
        {
            try
            {
                UnitOfWork.Transact(trans =>
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "@Noreg", model.Noreg },
                        { "@Pob", model.Pob },
                        { "@Dob", model.Dob },
                        { "@Nik",  model.Nik  },
                        { "@Address", model.Address },
                        { "@Region", model.Region },
                        { "@District", model.District },
                        { "@SubDistrict", model.SubDistrict },
                        { "@AdmVillage", model.AdmVillage },
                        { "@PostalCode", model.PostalCode },
                        { "@Rt", model.Rt },
                        { "@Rw", model.Rw },
                        { "@ResidentStatus", model.ResidentStatus },
                        { "@DomicileRegion", model.DomicileRegion },
                        { "@DomicileDistrict", model.DomicileDistrict },
                        { "@DomicileSubDistrict", model.DomicileSubDistrict },
                        { "@DomicileAdmVillage", model.DomicileAdmVillage },
                        { "@DomicilePostalCode", model.DomicilePostalCode },
                        { "@DomicileRt", model.DomicileRt },
                        { "@DomicileRw", model.DomicileRw },
                        { "@DomicileResidentStatus", model.DomicileResidentStatus },
                        { "@DomicileTotalFamily", model.DomicileTotalFamily },
                        { "@Kk", model.Kk },
                        { "@Npwp", model.Npwp },
                        { "@AccountNumber", model.AccountNumber },
                        { "@AccountName", model.AccountName },
                        { "@Religion", model.Religion },
                        { "@BloodGroup", model.BloodGroup },
                        { "@TaxStatus",  model.TaxStatus  },
                        { "@Bpjs", model.Bpjs },
                        { "@Bpjsen", model.Bpjsen },
                        { "@InsuranceNo", model.InsuranceNo },
                        { "@ModifiedBy", model.ModifiedBy },
                        { "@Name", model.Name },
                        { "@Branch", model.Branch },
                        { "@Gender", model.Gender },
                        { "@Status", model.Status },
                        { "@Nationality", model.Nationality },
                        { "@PhoneNumber", model.PhoneNumber},
                        { "@IdentityCardName", model.IdentityCardName},
                        { "@SimANumber", model.SimANumber},
                        { "@SimCNumber", model.SimCNumber},
                        { "@PassportNumber", model.PassportNumber},
                        { "@PersonalEmail", model.PersonalEmail},
                        { "@WhatsappNumber", model.WhatsappNumber},
                        { "@Domicile", model.Domicile},
                        { "@DanaPensiun", model.DanaPensiun},
                        { "@CertificateMarried", model.MarriedNumber},
                        { "@CertificateDivorce", model.DivorceNumber},
                        { "@MarriedDate", model.MarriedDate},
                        { "@DivorceDate", model.DivorceDate},
                    };

                    UnitOfWork.UspQuery("sp_UpdateProfileData", parameters, trans);

                    UnitOfWork.SaveChanges();
                });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateEmergencyContact(EmergencyContanctViewModel model)
        {
            try
            {
                UnitOfWork.Transact(trans =>
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "@Noreg", model.Noreg },
                        { "@EmcName", model.EmergencyCallName },
                        { "@EmcPhone", model.EmergencyCallPhoneNumber },
                        { "@EmcType", model.EmergencyCallRelationshipCode },
                        { "@EmcName2", model.EmergencyCallName2 },
                        { "@EmcPhone2", model.EmergencyCallPhoneNumber2 },
                        { "@EmcType2", model.EmergencyCallRelationshipCode2 },
                        { "@ModifiedBy", model.ModifiedBy }
                    };

                    UnitOfWork.UspQuery("SP_UPDATE_EMERGENCY_CONTACT", parameters, trans);

                    UnitOfWork.SaveChanges();
                });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateProfileMain(PersonalMainProfileViewModel model)
        {
            try
            {
                UnitOfWork.Transact(trans =>
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "@Noreg", model.Noreg },
                        { "@EntryDate", model.EntryDate },
                        { "@AstraDate", model.AstraDate },
                        { "@WorkLocation",  model.WorkLocation  },
                        { "@ModifiedBy",  model.ModifiedBy  }
                    };

                    UnitOfWork.UspQuery("SP_UPDATE_MAIN_PROFILE", parameters, trans);

                    UnitOfWork.SaveChanges();
                });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateEducation(EducationUpdateViewModel model)
        {
            try
            {
                UnitOfWork.Transact(trans =>
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "@Noreg", model.Noreg },
                        { "@EducationLevel", model.EducationLevel },
                        { "@Collage", model.Collage },
                        { "@Other",  model.Other  },
                        { "@StartEducationDate", model.StartEducationDate },
                        { "@GraduationDate", model.GraduationDate },
                        { "@Major", model.Major },
                        { "@GPA", model.GPA },
                        { "@Country", model.Country},
                        { "@Id", model.Id },
                        { "@ModifiedBy", model.ModifiedBy }
                    };

                    UnitOfWork.UspQuery("SP_UPDATE_EDUCATION_PROFILE", parameters, trans);

                    UnitOfWork.SaveChanges();
                });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void DeleteEducation(Guid id)
        {
            UnitOfWork.Transact(trans =>
            {
                personalDataEducationRepository.DeleteById(id);

                UnitOfWork.SaveChanges();
            });
        }

        public bool AddEducation(EducationUpdateViewModel model)
        {
            try
            {
                UnitOfWork.Transact(trans =>
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "@Noreg", model.Noreg },
                        { "@EducationLevel", model.EducationLevel },
                        { "@Collage", model.Collage },
                        { "@Other",  model.Other  },
                        { "@StartEducationDate", model.StartEducationDate },
                        { "@GraduationDate", model.GraduationDate },
                        { "@Major", string.IsNullOrEmpty(model.Major) ? "m00" : model.Major },
                        { "@Country", model.Country },
                        { "@GPA", model.GPA }
                    };

                    UnitOfWork.UspQuery("SP_ADD_EDUCATION_PROFILE", parameters, trans);

                    UnitOfWork.SaveChanges();
                });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UpdateFamilyMember(FamilyMemberViewModel model)
        {
            try
            {
                UnitOfWork.Transact(trans =>
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "@Id", model.Id },
                        { "@Name", model.Name },
                        { "@BirthDate", model.BirthDate },
                        { "@BirthPlace",  model.BirthPlace  },
                        { "@Gender", model.Gender },
                        { "@FamilyType", model.FamilyType },
                        { "@Other", model.Other },
                        { "@Bpjs", model.Bpjs },
                        { "@InsuranceNumber", model.InsuranceNumber },
                        { "@Noreg", model.Noreg },
                        { "@ModifiedBy", model.ModifiedBy },
                        { "@LifeStatus", model.LifeStatus },
                        { "@DeathDate", model.DeathDate },
                        { "@EducationLevelCode", model.EducationLevel },
                        { "@Job", model.Job },
                        { "@PhoneNumber", model.PhoneNumber },
                        { "@Nationality", model.Nationality },
                        { "@Domicile", model.Domicile },
                        { "@Religion", model.Religion },
                        { "@AddressStatusCode", model.AddressStatusCode },
                        { "@Nik", model.NIK },
                        { "@ChildStatus", model.ChildStatus}

                    };

                    UnitOfWork.UspQuery("SP_UPDATE_FAMILY_MEMBER", parameters, trans);

                    UnitOfWork.SaveChanges();
                });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteFamilyMember(FamilyMemberViewModel model)
        {
            try
            {
                UnitOfWork.Transact(trans =>
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "@FamilyMemberId", model.Id }
                    };

                    UnitOfWork.UspQuery("SP_DELETE_FAMILY_MEMBER", parameters, trans);

                    UnitOfWork.SaveChanges();
                });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddFamilyMember(FamilyMemberViewModel model)
        {
            try
            {
                UnitOfWork.Transact(trans =>
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "@Noreg", model.Noreg },
                        { "@Name", model.Name },
                        { "@BirthDate", model.BirthDate },
                        { "@BirthPlace",  model.BirthPlace  },
                        { "@Gender", model.Gender },
                        { "@FamilyType", model.FamilyType },
                        { "@Other", model.Other },
                        { "@Bpjs", model.Bpjs },
                        { "@InsuranceNumber", model.InsuranceNumber },
                        { "@LifeStatus", model.LifeStatus },
                        { "@DeathDate", model.DeathDate },
                        { "@EducationLevelCode", model.EducationLevel },
                        { "@Job", model.Job },
                        { "@PhoneNumber", model.PhoneNumber },
                        { "@Nationality", model.Nationality },
                        { "@Domicile", model.Domicile },
                        { "@Religion", model.Religion },
                        { "@AddressStatusCode", model.AddressStatusCode },
                        { "@Nik", model.NIK },
                        { "@ChildStatus", model.ChildStatus}
                    };

                    UnitOfWork.UspQuery("SP_ADD_FAMILY_MEMBER", parameters, trans);

                    UnitOfWork.SaveChanges();
                });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string ProcessUploadedFile(Stream fileStream)
        {
            var errorList = new List<string>();

            try
            {
                using (var package = new ExcelPackage(fileStream))
                {
                    var personalDataSheet = package.Workbook.Worksheets["Personal Data"];
                    var educationSheet = package.Workbook.Worksheets["Education"];
                    var familyMemberSheet = package.Workbook.Worksheets["Family Member"];

                    if (personalDataSheet == null || educationSheet == null || familyMemberSheet == null)
                    {
                        return "Error: One or more required sheets are missing in the uploaded file.";
                    }

                    var employees = ExtractEmployeeData(personalDataSheet, errorList);
                    var educations = ExtractEducationData(educationSheet);
                    var families = ExtractFamilyData(familyMemberSheet);

                    if (errorList.Any())
                    {
                        return "Errors found:<br>" + string.Join("<br>", errorList);
                    }

                    bool isInserted = InsertDataIntoDatabase(employees, educations, families);
                    return isInserted ? "SUCCESS" : "Failed to insert data into the database.";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private List<AddEmployeeProfileViewModel> ExtractEmployeeData(ExcelWorksheet sheet, List<string> errorList)
        {
            var list = new List<AddEmployeeProfileViewModel>();
            if (sheet.Dimension == null) return list; // Handle empty sheet

            int rowCount = sheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                string noreg = sheet.Cells[row, 1].Text.Trim();
                if (string.IsNullOrEmpty(noreg)) break; // Stop processing if Noreg is empty

                // Get values from Excel
                string idnValue = sheet.Cells[row, 17].Text.Trim();
                string familyCardNumberValue = sheet.Cells[row, 18].Text.Trim();
                string npwpValue = sheet.Cells[row, 19].Text.Trim();
                string accountNumberValue = sheet.Cells[row, 27].Text.Trim();
                string bpjsHealthNumberValue = sheet.Cells[row, 30].Text.Trim();
                string insuranceValue = sheet.Cells[row, 32].Text.Trim();

                // Collect validation errors instead of stopping the process
                if (!string.IsNullOrEmpty(idnValue))
                {
                    // Ensure IDN contains exactly 16 digits and only numbers
                    if (!Regex.IsMatch(idnValue, @"^\d{16}$"))
                    {
                        errorList.Add($"Row {row}: IDN must contain exactly 16 digits and only numbers.");
                    }
                }

                if (!string.IsNullOrEmpty(familyCardNumberValue))
                {
                    // Ensure Family Card Number contains exactly 16 digits and only numbers
                    if (!Regex.IsMatch(familyCardNumberValue, @"^\d{16}$"))
                    {
                        errorList.Add($"Row {row}: Family Card Number must contain exactly 16 digits and only numbers.");
                    }
                }

                if (!string.IsNullOrEmpty(accountNumberValue))
                {
                    // Ensure Account Number contains only numbers and is between 10 and 16 digits
                    if (!Regex.IsMatch(accountNumberValue, @"^\d{10,16}$"))
                    {
                        errorList.Add($"Row {row}: Account Number must contain only numbers and be between 10 and 16 digits.");
                    }
                }

                if (!string.IsNullOrEmpty(bpjsHealthNumberValue))
                {
                    // Ensure BPJS Health Number contains only numbers and is exactly 13 digits
                    if (!Regex.IsMatch(bpjsHealthNumberValue, @"^\d{13}$"))
                    {
                        errorList.Add($"Row {row}: BPJS Health Number must contain only numbers and be exactly 13 digits.");
                    }
                }

                if (!string.IsNullOrEmpty(npwpValue))
                {
                    // Remove dots and dashes for digit count check
                    string numericOnly = npwpValue.Replace(".", "").Replace("-", "");

                    // Ensure it contains exactly 16 digits (excluding dots and dashes)
                    if (!Regex.IsMatch(numericOnly, @"^\d{16}$"))
                    {
                        errorList.Add($"Row {row}: NPWP must contain exactly 16 digits (excluding dots and dashes).");
                    }
                    // Ensure it does not end with a dot or dash
                    else if (npwpValue.EndsWith(".") || npwpValue.EndsWith("-"))
                    {
                        errorList.Add($"Row {row}: NPWP must not end with a dot (.) or dash (-).");
                    }
                    // Ensure only numbers, dots, and dashes are allowed
                    else if (!Regex.IsMatch(npwpValue, @"^[\d.-]+$"))
                    {
                        errorList.Add($"Row {row}: NPWP must contain only numbers, dots (.), and dashes (-).");
                    }
                }

                if (!string.IsNullOrEmpty(insuranceValue))
                {
                    // Ensure only numbers and a single dash (-), with no dash at the start or end
                    if (!Regex.IsMatch(insuranceValue, @"^(?!-)(\d+-?\d*)(?<!-)$"))
                    {
                        errorList.Add($"Row {row}: Insurance Number must contain only numbers and at most one dash (-), which cannot be at the start or end.");
                    }
                }

                DateTime? entryDate = null;
                string entryDateRaw = sheet.Cells[row, 5].Text.Trim();

                if (!string.IsNullOrEmpty(entryDateRaw))
                {
                    if (DateTime.TryParseExact(entryDateRaw, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedEntryDate))
                    {
                        entryDate = parsedEntryDate.Date;
                    }
                    else
                    {
                        errorList.Add($"Row {row}: Entry Date TAM '{entryDateRaw}' is not in a valid date format (expected dd-MM-yyyy).");
                    }
                }

                DateTime? astraDate = null;
                string astraDateRaw = sheet.Cells[row, 6].Text.Trim();

                if (!string.IsNullOrEmpty(astraDateRaw))
                {
                    if (DateTime.TryParseExact(astraDateRaw, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedAstraDate))
                    {
                        astraDate = parsedAstraDate.Date;
                    }
                    else
                    {
                        errorList.Add($"Row {row}: Entry Astra Date '{astraDateRaw}' is not in a valid date format (expected dd-MM-yyyy).");
                    }
                }

                DateTime? dobDate = null;
                string dobRaw = sheet.Cells[row, 15].Text.Trim();

                if (!string.IsNullOrEmpty(dobRaw))
                {
                    if (DateTime.TryParseExact(dobRaw, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDob))
                    {
                        dobDate = parsedDob.Date;
                    }
                    else
                    {
                        errorList.Add($"Row {row}: Date of Birth '{dobRaw}' is not in a valid date format (expected dd-MM-yyyy).");
                    }
                }

                list.Add(new AddEmployeeProfileViewModel
                {
                    //Noreg = noreg,
                    //Employee = sheet.Cells[row, 2].Text,
                    //EntryDate = entryDate ?? DateTime.MinValue,
                    ////EntryDate = sheet.Cells[row, 3].GetValue<DateTime?>()?.Date ?? DateTime.MinValue,
                    //EmployeeCategory = sheet.Cells[row, 4].Text,
                    //EmployeeStatus = sheet.Cells[row, 5].Text,
                    //PersArea = sheet.Cells[row, 6].Text,
                    //SubArea = sheet.Cells[row, 7].Text,
                    //Class = sheet.Cells[row, 8].Text,
                    //Nationality = sheet.Cells[row, 9].Text,
                    //Pob = sheet.Cells[row, 10].Text,
                    //Dob = dobDate ?? DateTime.MinValue,
                    ////Dob = sheet.Cells[row, 11].GetValue<DateTime?>()?.Date ?? DateTime.MinValue,
                    //GenderCode = sheet.Cells[row, 12].Text,
                    //Idn = idnValue,
                    //FamilyCardNumber = familyCardNumberValue,
                    //NPWP = npwpValue,
                    //Email = sheet.Cells[row, 16].Text,
                    //Address = sheet.Cells[row, 17].Text,
                    //Status = sheet.Cells[row, 18].Text,
                    //Religion = sheet.Cells[row, 19].Text,
                    //BloodGroup = sheet.Cells[row, 20].Text,
                    //AccountNumber = accountNumberValue,
                    //Branch = sheet.Cells[row, 22].Text,
                    //TaxStatus = sheet.Cells[row, 23].Text,
                    //BPJS = bpjsHealthNumberValue,
                    //BPJSEmployment = sheet.Cells[row, 25].Text,
                    //Insuranceno = insuranceValue,
                    //Nodanapensiun = sheet.Cells[row, 27].Text,
                    //Division = sheet.Cells[row, 28].Text,
                    //Department = sheet.Cells[row, 29].Text,
                    //Section = sheet.Cells[row, 30].Text

                    Noreg = noreg,
                    Employee = sheet.Cells[row, 2].Text,
                    IdentityCardName = sheet.Cells[row, 3].Text,
                    Class = sheet.Cells[row, 4].Text,
                    EntryDate = entryDate ?? DateTime.MinValue,
                    AstraDate = astraDate ?? DateTime.MinValue,
                    //EntryDate = sheet.Cells[row, 3].GetValue<DateTime?>()?.Date ?? DateTime.MinValue,
                    SubArea = sheet.Cells[row, 7].Text, //Working Location
                    EmployeeCategory = sheet.Cells[row, 8].Text,
                    EmployeeStatus = sheet.Cells[row, 9].Text,                    
                    PassportNumber = sheet.Cells[row, 10].Text,
                    Nationality = sheet.Cells[row, 11].Text,
                    PhoneNumber = sheet.Cells[row, 12].Text,
                    WhatsappNumber = sheet.Cells[row, 13].Text,
                    Pob = sheet.Cells[row, 14].Text,
                    Dob = dobDate ?? DateTime.MinValue,
                    //Dob = sheet.Cells[row, 11].GetValue<DateTime?>()?.Date ?? DateTime.MinValue,
                    GenderCode = sheet.Cells[row, 16].Text,
                    Idn = idnValue,
                    FamilyCardNumber = familyCardNumberValue,
                    NPWP = npwpValue,
                    Email = sheet.Cells[row, 20].Text,
                    PersonalEmail = sheet.Cells[row, 21].Text,
                    Address = sheet.Cells[row, 22].Text,
                    Domicile = sheet.Cells[row, 23].Text,
                    Status = sheet.Cells[row, 24].Text,
                    Religion = sheet.Cells[row, 25].Text,
                    BloodGroup = sheet.Cells[row, 26].Text,
                    AccountNumber = accountNumberValue,
                    Branch = sheet.Cells[row, 28].Text,
                    TaxStatus = sheet.Cells[row, 29].Text,
                    BPJS = bpjsHealthNumberValue,
                    BPJSEmployment = sheet.Cells[row, 31].Text,
                    Insuranceno = insuranceValue,
                    Nodanapensiun = sheet.Cells[row, 33].Text,
                    SimANumber = sheet.Cells[row, 34].Text,
                    SimCNumber = sheet.Cells[row, 35].Text,
                    Division = sheet.Cells[row, 36].Text,
                    Department = sheet.Cells[row, 37].Text,
                    Section = sheet.Cells[row, 38].Text
                });
            }
            return list;
        }


        private List<AddEducationProfile> ExtractEducationData(ExcelWorksheet sheet)
        {
            var list = new List<AddEducationProfile>();
            if (sheet.Dimension == null) return list; // Handle empty sheet

            int rowCount = sheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                string noreg = sheet.Cells[row, 1].Text.Trim();
                if (string.IsNullOrWhiteSpace(noreg)) break; // Stop if Noreg is empty

                list.Add(new AddEducationProfile
                {
                    NoregEducation = noreg,
                    EmployeeEducation = sheet.Cells[row, 2].Text,
                    Education = sheet.Cells[row, 3].Text,
                    NameEducation = sheet.Cells[row, 4].Text,
                    Major = sheet.Cells[row, 5].Text,
                    Country = sheet.Cells[row, 6].Text
                });
            }
            return list;
        }

        private List<AddFamilyProfile> ExtractFamilyData(ExcelWorksheet sheet)
        {
            var list = new List<AddFamilyProfile>();
            if (sheet.Dimension == null) return list; // Handle empty sheet

            int rowCount = sheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                string noreg = sheet.Cells[row, 1].Text.Trim();
                if (string.IsNullOrWhiteSpace(noreg)) break; // Stop if Noreg is empty

                //Get values from excel
                string lifeStatusText = sheet.Cells[row, 6].Text.Trim().ToLower();
                int? lifeStatus = null;

                if (lifeStatusText == "alive")
                {
                    lifeStatus = 1;
                }
                else if (lifeStatusText == "passed away")
                {
                    lifeStatus = 0;
                }

                list.Add(new AddFamilyProfile
                {
                    NoregFamily = noreg,
                    EmployeeFamily = sheet.Cells[row, 2].Text,
                    FamilyType = sheet.Cells[row, 3].Text,
                    NameFamily = sheet.Cells[row, 4].Text,
                    FamilyIdentityNumber = sheet.Cells[row, 5].Text,
                    LifeStatus = lifeStatus,
                    FamilyDob = sheet.Cells[row, 7].GetValue<DateTime?>()?.Date ?? DateTime.MinValue,
                    FamilyPob = sheet.Cells[row, 8].Text,
                    FamilyDod = sheet.Cells[row, 9].GetValue<DateTime?>()?.Date ?? DateTime.MinValue,
                    GenderFamily = sheet.Cells[row, 10].Text,
                    ChildStatus = sheet.Cells[row, 11].Text,
                    ChildOrder = sheet.Cells[row, 12].Text,
                    FamilyPhoneNumber = sheet.Cells[row, 13].Text,
                    FamilyDomicile = sheet.Cells[row, 14].Text,
                    FamilyEdu = sheet.Cells[row, 15].Text,
                    FamilyJob = sheet.Cells[row, 16].Text,
                    FamilyBPJS = sheet.Cells[row, 17].Text,
                    FamilyInsuranceNo = sheet.Cells[row, 18].Text
                });
            }
            return list;
        }

        private bool InsertDataIntoDatabase(
          List<AddEmployeeProfileViewModel> employees,
          List<AddEducationProfile> educations,
          List<AddFamilyProfile> families)
            {
            try
            {
                UnitOfWork.Transact(trans =>
                {
                    foreach (var employee in employees)
                    {
                        var empParams = CreateEmployeeParams(employee);
                        UnitOfWork.UspQuery("SP_ADD_EMPLOYEE_DATA", empParams, trans);
                    }

                    foreach (var edu in educations)
                    {
                        var eduParams = CreateEducationParams(edu);
                        UnitOfWork.UspQuery("SP_ADD_EMPLOYEE_DATA", eduParams, trans);
                    }

                    foreach (var fam in families)
                    {
                        var famParams = CreateFamilyParams(fam);
                        UnitOfWork.UspQuery("SP_ADD_EMPLOYEE_DATA", famParams, trans);
                    }
                });

                return true;
            }
            catch (Exception ex)
            {
                // Replace with proper logging if needed
                Console.WriteLine($"Error inserting data: {ex.Message}");
                return false;
            }
        }

        private Dictionary<string, object> CreateEmployeeParams(AddEmployeeProfileViewModel employee)
        {
            return new Dictionary<string, object>
        {
        { "@ActionType", "EMPLOYEE" },
        { "@Noreg", employee.Noreg },
        { "@Employee", employee.Employee },
        { "@EntryDate", employee.EntryDate == DateTime.MinValue ? (object)(DateTime?)null : employee.EntryDate.Date },
        { "@EmployeeCategory", employee.EmployeeCategory },
        { "@EmployeeStatus", employee.EmployeeStatus },
        { "@PersArea", employee.PersArea },
        { "@SubArea", employee.SubArea },
        { "@Class", employee.Class },
        { "@Nationality", employee.Nationality },
        { "@Pob", employee.Pob },
        { "@Dob", employee.Dob == DateTime.MinValue ? (object)(DateTime?)null : employee.Dob.Date },
        { "@GenderCode", employee.GenderCode },
        { "@Idn", employee.Idn },
        { "@FamilyCardNumber", employee.FamilyCardNumber },
        { "@NPWP", employee.NPWP },
        { "@Email", employee.Email },
        { "@Address", employee.Address },
        { "@Status", employee.Status },
        { "@Religion", employee.Religion },
        { "@BloodGroup", employee.BloodGroup },
        { "@AccountNumber", employee.AccountNumber },
        { "@Branch", employee.Branch },
        { "@TaxStatus", employee.TaxStatus },
        { "@BPJS", employee.BPJS },
        { "@BPJSEmployment", employee.BPJSEmployment },
        { "@Insuranceno", employee.Insuranceno },
        { "@Nodanapensiun", employee.Nodanapensiun },
        { "@Division", employee.Division },
        { "@Department", employee.Department },
        { "@Section", employee.Section },
        { "@IdentityCardName", employee.IdentityCardName},
        { "@AstraDate", employee.AstraDate},
        { "@PassportNumber", employee.PassportNumber },
        { "@PersonalEmail", employee.PersonalEmail},
        { "@Domicile", employee.Domicile },
        { "@PhoneNumber", employee.PhoneNumber },
        { "@WhatsappNumber", employee.WhatsappNumber },
        { "@SimANumber", employee.SimANumber },
        { "@SimCNumber", employee.SimCNumber }
    };
        }

        private Dictionary<string, object> CreateEducationParams(AddEducationProfile edu)
        {
            return new Dictionary<string, object>
    {
        { "@ActionType", "EDUCATION" },
        { "@NoregEducation", edu.NoregEducation },
        { "@EmployeeEducation", edu.EmployeeEducation },
        { "@Education", edu.Education },
        { "@NameEducation", edu.NameEducation },
        { "@Major", edu.Major },
        { "@Country", edu.Country },
    };
        }

        private Dictionary<string, object> CreateFamilyParams(AddFamilyProfile fam)
        {
            return new Dictionary<string, object>
    {
        { "@ActionType", "FAMILY" },
        { "@NoregFamily", fam.NoregFamily },
        { "@EmployeeFamily", fam.EmployeeFamily },
        { "@FamilyType", fam.FamilyType },
        { "@NameFamily", fam.NameFamily },
        { "@FamilyPob", fam.FamilyPob },
        { "@FamilyDob", fam.FamilyDob == DateTime.MinValue ? (object)(DateTime?)null : fam.FamilyDob.Date },
        { "@GenderFamily", fam.GenderFamily },
        { "@FamilyBPJS", fam.FamilyBPJS },
        { "@FamilyInsuranceNo", fam.FamilyInsuranceNo },
        { "@FamilyIdentityNumber", fam.FamilyIdentityNumber },
        { "@LifeStatus", fam.LifeStatus },
        { "@FamilyDod", fam.FamilyDod == DateTime.MinValue ? (object)(DateTime?)null : fam.FamilyDod.Date },
        { "@ChildStatus", fam.ChildStatus },
        { "@ChildOrder", fam.ChildOrder },
        { "@FamilyPhoneNumber", fam.FamilyPhoneNumber },
        { "@FamilyDomicile", fam.FamilyDomicile },
        { "@FamilyEdu", fam.FamilyEdu },
        { "@FamilyJob", fam.FamilyJob },
    };

        }
    }
}


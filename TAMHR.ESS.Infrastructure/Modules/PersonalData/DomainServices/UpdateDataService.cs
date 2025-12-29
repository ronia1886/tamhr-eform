using TAMHR.ESS.Domain;
using Agit.Domain;
using Agit.Domain.UnitOfWork;
using Agit.Domain.Repository;
using System.Linq;
using System;
using System.Text;
using Agit.Domain.Extensions;
using Dapper;
using TAMHR.ESS.Infrastructure.ViewModels;
using Z.EntityFramework.Plus;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Agit.Common;
using Newtonsoft.Json;


namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle vaccine master data.
    /// </summary>
    //public class VaccineService : GenericDomainServiceBase<Vaccine>
    public class UpdateDataService : DomainServiceBase
    {

        protected IRepository<PersonalDataDivorceView> PersonalDataDivorceRepository => UnitOfWork.GetRepository<PersonalDataDivorceView>();
        protected IRepository<PersonalDataEducationView> PersonalDataEducationViewRepository => UnitOfWork.GetRepository<PersonalDataEducationView>();
        protected IRepository<PersonalDataKKView> PersonalDataKKViewViewRepository => UnitOfWork.GetRepository<PersonalDataKKView>();
        protected IRepository<GeneralCategory> GeneralCategory => UnitOfWork.GetRepository<GeneralCategory>();

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public UpdateDataService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        public IQueryable<Menu> GetMenuUpdateData()
        {
            var query =
                       from config in UnitOfWork.GetRepository<Config>().Fetch().Where(x => x.ConfigKey == "UpdateData" && x.RowStatus == true).AsNoTracking()
                       join menu in UnitOfWork.GetRepository<Menu>().Fetch().AsNoTracking() on config.ConfigValue equals menu.Title
                       select new Menu { Title = menu.Title, IconClass = menu.IconClass, ConfigText = config.ConfigText, Id = menu.Id };

            return query;
        }

        public IQueryable<PersonalDataKKView> GetPersonalDataKK(string kknumber)
        {
            return UnitOfWork.GetRepository<PersonalDataKKView>().Fetch().AsNoTracking().Where(x => x.KKNumber == kknumber && x.RowStatus == true);
        }
        public IQueryable<PersonalDataFamilyView> GetPersonalDataFamily(string noreg)
        {
            return UnitOfWork.GetRepository<PersonalDataFamilyView>().Fetch().AsNoTracking().Where(x => x.NoReg == noreg);
        }

        public IQueryable<PersonalDataIdentityView> GetPersonalDataIdentity(string noreg)
        {
            return UnitOfWork.GetRepository<PersonalDataIdentityView>().Fetch().AsNoTracking().Where(x => x.NoReg == noreg);
        }
        public IQueryable<PersonalDataPassportView> GetPersonalDataPassport(string noreg)
        {
            return UnitOfWork.GetRepository<PersonalDataPassportView>().Fetch().AsNoTracking().Where(x => x.NoReg == noreg);
        }

        public IQueryable<PersonalDataDomicileView> GetPersonalDataDomicile(string noreg)
        {
            return UnitOfWork.GetRepository<PersonalDataDomicileView>().Fetch().AsNoTracking().Where(x => x.NoReg == noreg);
        }

        public IQueryable<PersonalDataFamilyView> GetPersonalDataFamilyRegistration(string noreg)
        {
            return UnitOfWork.GetRepository<PersonalDataFamilyView>().Fetch().AsNoTracking().Where(x => x.NoReg == noreg);
        }

        public IQueryable<PersonalDataExcerptMarriageCertificateView> GetPersonalDataExcerptMarriage(string noreg)
        {
            return UnitOfWork.GetRepository<PersonalDataExcerptMarriageCertificateView>().Fetch().AsNoTracking().Where(x => x.NoReg == noreg);
        }

        public IQueryable<PersonalDataDriverLicenseView> GetPersonalDataDriverLicense(string noreg)
        {
            return UnitOfWork.GetRepository<PersonalDataDriverLicenseView>().Fetch().AsNoTracking().Where(x => x.NoReg == noreg);
        }

        public IEnumerable<PersonalDataDivorceView> GetDataDivorce(string NoReg)
        {
            var output = PersonalDataDivorceRepository.Fetch()
                .Where(x => x.NoReg == NoReg)
                .AsNoTracking();

            return output;
        }
        public IEnumerable<PersonalDataEducationView> GetDataEducation(string NoReg)
        {
            var output = PersonalDataEducationViewRepository.Fetch()
                .Where(x => x.NoReg == NoReg)
                .AsNoTracking();

            return output;
        }

        public IQueryable<PersonalDataOtherInformation> GetPersonalDataContactHobbies(string noreg)
        {
            return UnitOfWork.GetRepository<PersonalDataOtherInformation>().Fetch().AsNoTracking().Where(x => x.NoReg == noreg);
        }

        public IQueryable<GeneralCategory> GetGeneralCategories(string category)
        {
            return UnitOfWork.GetRepository<GeneralCategory>().Fetch().AsNoTracking().Where(x => x.Category == category);
        }

        public IQueryable<PersonalDataUpdate> GetProgress(string noreg)
        {
            return UnitOfWork.GetRepository<PersonalDataUpdate>().Fetch().AsNoTracking().Where(x => x.NoReg == noreg && x.RowStatus == true);
        }
        public IQueryable<GeneralCategory> GetGeneralCategories()
        {
            // Get general category query objects by category without object tracking ordered by name.
            return GeneralCategory.Fetch()
                .AsNoTracking()
                .Where(x=> x.Category == "Region")
                .OrderBy(x => x.Name);
        }
        public bool ConfirmUpdateData(PersonalDataUpdate entity)
        {
            UnitOfWork.GetRepository<PersonalDataUpdate>().Fetch()
                                     .Where(x => x.NoReg == entity.NoReg && x.RowStatus == true)
                                     .Update(x => new PersonalDataUpdate
                                     {
                                         Progress = entity.Progress,
                                         UpdateDataStatus = entity.UpdateDataStatus,
                                         ModifiedBy = entity.NoReg,
                                         ModifiedOn = DateTime.Now
                                     });
            return true;
        }
        public IEnumerable<PersonalDataKKView> GetPersonalDataKKUpdate(PersonalDataKKView entity)
        {
            return PersonalDataKKViewViewRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Nik == entity.Nik && x.KKNumber == entity.KKNumber);
        }
        public T DeserializeFromJson<T>(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }

        public bool DeletePersonalDataKK(PersonalDataKkMember entity)
        {
            UnitOfWork.GetRepository<PersonalDataKkMember>().Fetch()
                                     .Where(x => x.Id == entity.Id)
                                     .Update(x => new PersonalDataKkMember
                                     {
                                         RowStatus = false,
                                         ModifiedBy = entity.ModifiedBy,
                                         ModifiedOn = DateTime.Now
                                     });
            return true;
        }

        public bool SubmitKK(PersonalDataKkMember entity)
        {
            var set = UnitOfWork.GetRepository<PersonalDataKkMember>();
            // Update or insert training batch date.
            set.Upsert<Guid>(entity, new[] {
                  "KKNumber"
                  ,"KKStatusCode"
                  ,"Name"
                  ,"Nik"
                  ,"GenderCode"
                  ,"BirthDate"
                  ,"BirthPlace"
                  ,"ReligionCode"
                  ,"EducationCode"
                  ,"WorkTypeCode"
                  ,"BloodTypeCode"
                  ,"MaritalStatusCode"
                  ,"MaritalDate"
                  ,"NationalityCode"
                  ,"FatherName"
                  ,"MatherName"
                  ,"PassportNumber"
                  ,"KitapNumber"
                  ,"PhoneNumber"
                  ,"RowStatus" });
            //set.Upsert<Guid>(entity);
            return UnitOfWork.SaveChanges() > 0;
        }

        public bool UpdateAddress(AddressViewModel entity)
        {
            UnitOfWork.GetRepository<PersonalDataCommonAttribute>().Fetch()
                                     .Where(x => x.Nik == entity.Nik && x.KKNumber == entity.KKNumber)
                                     .Update(x => new PersonalDataCommonAttribute
                                     {
                                         RegionCode = entity.Provice,
                                         DistrictCode = entity.DistrictCode,
                                         SubDistrictCode = entity.SubDistrictCode,
                                         PostalCode = entity.PostalCode,
                                         CityCode = entity.City,
                                         Address = entity.Address,
                                         Rt = entity.RT,
                                         Rw = entity.RW,
                                         ModifiedBy = entity.NoReg,
                                         ModifiedOn = DateTime.Now
                                     });
            return true;
        }
        public bool UpdateFamily(FamilyRegistViewModel entity)
        {
            UnitOfWork.GetRepository<PersonalDataFamilyMember>().Fetch()
                                     .Where(x => x.CommonAttributeId == entity.Id)
                                     .Update(x => new PersonalDataFamilyMember
                                     { FamilyTypeCode = entity.ChildStatus });
            UnitOfWork.GetRepository<PersonalDataCommonAttribute>().Fetch()
                                     .Where(x => x.Id == entity.Id)
                                     .Update(x => new PersonalDataCommonAttribute
                                     {
                                         Name = entity.Name,
                                         BirthPlace = entity.PlaceOfBirthCode,
                                         BirthDate = entity.DateOfBirth,
                                         GenderCode = entity.GenderCode,
                                         BloodTypeCode = entity.BloodTypeCode,
                                         RegionCode = entity.Provinsi,
                                         DistrictCode = entity.Kecamatan,
                                         SubDistrictCode = entity.Kelurahan,
                                         PostalCode = entity.PostalCode,
                                         CityCode = entity.Kota,
                                         ReligionCode = entity.ReligionCode,
                                         Address = entity.Address,
                                         Rt = entity.RT,
                                         Rw = entity.RW,
                                         ModifiedOn = DateTime.Now
                                     });
            return true;
        }
        public IQueryable<PersonalDataFamilyMember> GetPersonalDataFamilyMemberUpdate(string objectData)
        {
            var entity = JsonConvert.DeserializeObject<PersonalDataFamilyMember>(objectData);
            return UnitOfWork.GetRepository<PersonalDataFamilyMember>().Fetch().AsNoTracking().Where(x => x.NoReg == entity.NoReg && x.Id == entity.Id);
        }

        public bool SubnmitCommontAttribute(PersonalDataCommonAttribute entity)
        {
            UnitOfWork.GetRepository<PersonalDataCommonAttribute>().Fetch()
                                      .Where(x => x.Id == entity.Id && x.RowStatus == true)
                                      .Update(x => new PersonalDataCommonAttribute
                                      {
                                          ModifiedBy = entity.ModifiedBy,
                                          ModifiedOn = DateTime.Now
                                      });

            return true;
        }

        public IQueryable<GeneralCategory> GetGeneralCategory(GeneralCategory objectData)
        {
            return UnitOfWork.GetRepository<GeneralCategory>().Fetch().AsNoTracking().Where(x => x.Category == objectData.Category && x.Code == objectData.Code);
        }

        public Menu SetKeyMenu(string key)
        {
            Menu menu = new Menu();
            menu.ConfigText = key;
            return menu;
        }

    }
}

using Agit.Domain;
using Agit.Domain.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Domain.Models.Core.StoredEntities;


namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class ProfileService : DomainServiceBase
    {
        #region Repositories
        protected IRepository<ActualOrganizationStructure> ActualOrganizationStructureRepository => UnitOfWork.GetRepository<ActualOrganizationStructure>();
        protected IRepository<PersonalData> PersonalDataRepository => UnitOfWork.GetRepository<PersonalData>();
        protected IRepository<PersonalDataEducation> PersonalDataEducationRepository => UnitOfWork.GetRepository<PersonalDataEducation>();
        protected IRepository<PersonalDataTraining> PersonalDataTrainingRepository => UnitOfWork.GetRepository<PersonalDataTraining>();
        protected IRepository<PersonalDataFamilyMember> PersonalDataFamilyMemberRepository => UnitOfWork.GetRepository<PersonalDataFamilyMember>();
        protected IRepository<PersonalDataCommonAttribute> PersonalDataCommonAttributeRepository => UnitOfWork.GetRepository<PersonalDataCommonAttribute>();
        protected IReadonlyRepository<PersonalDataAttributeStroredEntity> PersonalDataAttributeRepository => UnitOfWork.GetRepository<PersonalDataAttributeStroredEntity>();
        protected IReadonlyRepository<GeneralCategory> GeneralCategoryRepository => UnitOfWork.GetRepository<GeneralCategory>();
        protected IRepository<EmployeProfileView> EmployeeProfileViewRepository => UnitOfWork.GetRepository<EmployeProfileView>();
        protected IRepository<TrainingHistory> TrainingHistoryRepository => UnitOfWork.GetRepository<TrainingHistory>();
        protected IRepository<TrainingHistoryView> TrainingHistoryViewRepository => UnitOfWork.GetRepository<TrainingHistoryView>();
        protected IRepository<TrainingHistoryParticipantView> TrainingHistoryParticipantViewRepository => UnitOfWork.GetRepository<TrainingHistoryParticipantView>();

        #endregion

        #region Constructor
        public ProfileService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        public PersonalDataAttributeStroredEntity GetProfile(string noReg)
        {
            return UnitOfWork.UspQuery<PersonalDataAttributeStroredEntity>(new { NoReg = noReg }).FirstOrDefault();
        }
        public PersonalDataAllProfileStoredEntity GetProfiles(string noReg)
        {
            return UnitOfWork.UspQuery<PersonalDataAllProfileStoredEntity>(new { NoReg = noReg }).FirstOrDefault();
        }

        public IEnumerable<PersonalDataFamilyMemberStoredEntity> GetFamilyMembers(string noReg)
        {
            return UnitOfWork.UspQuery<PersonalDataFamilyMemberStoredEntity>(new { NoReg = noReg }).Where(x => x.DivorceDate == null).ToList();
        }

        public PersonalDataEmergencyContactStoredProcedure GetEmergencyContact(string noReg)
        {
            return UnitOfWork.UspQuery<PersonalDataEmergencyContactStoredProcedure>(new { NoReg = noReg }).FirstOrDefault();
        }


        public IEnumerable<OrganizationalAssignmentStoredEntity> GetOrganizationalAssignments(string noreg)
        {
            return UnitOfWork.UdfQuery<OrganizationalAssignmentStoredEntity>(new { noreg }).OrderByDescending(x => x.StartDate);
        }

        public IEnumerable<ProfileDataEducationStoreEntity> GetProfileDataEducation(string noreg)
        {
            return UnitOfWork.UspQuery<ProfileDataEducationStoreEntity>(new { noreg });//.OrderByDescending(x => x.StartDate);
        }
        public IEnumerable<ProfileDataVoucherStoreEntity> GetProfileDataVoucher(string noreg)
        {
            return UnitOfWork.UspQuery<ProfileDataVoucherStoreEntity>(new { noreg });//.OrderByDescending(x => x.StartDate);
        }

        public IEnumerable<HistoryClaimStoreEntity> GetProfileDataClaimBenefit(string noreg)
        {
            return UnitOfWork.UspQuery<HistoryClaimStoreEntity>(new { noreg }).OrderByDescending(x => x.TransactionDate);
        }

        public IEnumerable<object> GetInfoEducation(string Noreg)
        {
            var data = from ED in PersonalDataEducationRepository.Fetch()
                       join EducationTypeCode in GeneralCategoryRepository.Fetch()
                       on ED.EducationTypeCode equals EducationTypeCode.Code
                       join Major in GeneralCategoryRepository.Fetch()
                       on ED.Major equals Major.Code
                       join Institute in GeneralCategoryRepository.Fetch()
                       on ED.Institute equals Institute.Code
                       where ED.NoReg == Noreg
                       select new {
                           EducationTypeCode = EducationTypeCode.Name,
                           Major = Major.Name,
                           Institute = Institute.Name
                       };
          
            return data;
        }

        public IEnumerable<object> GetDataTraining(string Noreg)
        {
            var data = from dt in PersonalDataTrainingRepository.Fetch()
                       where dt.NoReg == Noreg
                       orderby dt.Year descending
                       select new
                       {
                           Year = dt.Year,
                           TrainingName = dt.TrainingName,
                           Institution = dt.Institution,

                       };

            return data.ToList();
        }

        public IQueryable<object> GetInfoFamilyMember(string Noreg)
        {
            var data = from FM in PersonalDataFamilyMemberRepository.Fetch()
                       join CA in PersonalDataCommonAttributeRepository.Fetch()
                       on FM.CommonAttributeId equals CA.Id
                       where FM.NoReg == Noreg
                       select new
                       {
                           FamilyTypeCode = FM.FamilyTypeCode,
                           Name = CA.Name,
                           BirthPlace = CA.BirthPlace,
                           BirthDate = CA.BirthDate,
                           GenderCode = CA.GenderCode
                       };

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

        public string getContranct(string noReg)
        {
            var data = EmployeeProfileViewRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Noreg == noReg)
                .Select(x => x.WorkContractText).FirstOrDefault();

            return data;
        }

        public IEnumerable<object> GetHistoryTraining(string Noreg)
        {
            // Gunakan array untuk membuat kondisi WHERE lebih ringkas dan mudah dibaca
            var allowedStatusCodes = new[] { "accomplished", "failed", "incomplete" };

            var allowedStatusCodesQuery = allowedStatusCodes.AsQueryable();

            // Lakukan kueri pertama dengan kondisi yang lebih jelas
            var data = from dt in 
                           TrainingHistoryParticipantViewRepository.Fetch()
                       .Join(allowedStatusCodesQuery, // Gunakan IQueryable dari daftar ID
                          thp => thp.StatusCode,
                          idItem => idItem,
                          (thp, idItem) => thp)
                       where dt.NoReg == Noreg
                       select new
                       {
                           Year = dt.Period,
                           TrainingName = dt.TrainingName,
                           TrainingProvider = dt.TrainingProvider
                       };

            // Lakukan kueri kedua dengan kondisi yang sama
            var data1 = from dt in TrainingHistoryRepository.Fetch()
                        .Join(allowedStatusCodesQuery, // Gunakan IQueryable dari daftar ID
                          thp => thp.StatusCode,
                          idItem => idItem,
                          (thp, idItem) => thp)
                        where dt.NoReg == Noreg
                        select new
                        {
                            Year = dt.Period,
                            TrainingName = dt.TrainingName,
                            TrainingProvider = dt.TrainingProvider
                        };

            // Gabungkan data
            // Gunakan .Union() jika Anda ingin menggabungkan dan menghapus duplikat
            // Gunakan .Concat() jika Anda ingin menggabungkan dan mempertahankan duplikat
            var combinedData = data.Concat(data1).OrderByDescending(d => d.Year).ToList();

            return combinedData;
        }
    }
}

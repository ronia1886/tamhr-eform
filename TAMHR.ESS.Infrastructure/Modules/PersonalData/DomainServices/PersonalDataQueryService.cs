using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using Agit.Domain;
using Agit.Common.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Z.EntityFramework.Plus;
using TAMHR.ESS.Infrastructure.ViewModels;
using System;
using Agit.Domain.Extensions;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class PersonalDataQueryService : DomainServiceBase
    {
        #region Repositories
        protected IRepository<PersonalDataTaxStatus> PersonalDataTaxStatusRepository => UnitOfWork.GetRepository<PersonalDataTaxStatus>();
        protected IRepository<ActualOrganizationStructure> ActualOrganizationStructureRepository => UnitOfWork.GetRepository<ActualOrganizationStructure>();
        protected IRepository<AllowanceDetail> AllowanceDetailRepository => UnitOfWork.GetRepository<AllowanceDetail>();
        protected IRepository<PersonalData> PersonalDataRepository => UnitOfWork.GetRepository<PersonalData>();
        protected IRepository<Form> FormRepository => UnitOfWork.GetRepository<Form>();
        protected IRepository<EmployeeSubgroupNP> EmployeeSubgroupNPRepository => UnitOfWork.GetRepository<EmployeeSubgroupNP>();
        protected IRepository<GeneralCategory> GeneralCategoryRepository => UnitOfWork.GetRepository<GeneralCategory>();
        #endregion
        
        #region Constructor
        public PersonalDataQueryService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        public PersonalDataCommonAttribute GetPersonalDataCommonAttribute(string noreg)
        {
            return PersonalDataRepository.Fetch()
                .AsNoTracking()
                .Include(x => x.PersonalDataCommonAttribute)
                .Where(x => x.NoReg == noreg)
                .Select(x => x.PersonalDataCommonAttribute)
                .FirstOrDefault();
        }

        public IQueryable<object> GetInfo(string Noreg)
        {
            var data = from aoe in ActualOrganizationStructureRepository.Fetch()
                       join ts in PersonalDataTaxStatusRepository.Fetch()
                       on aoe.NoReg equals ts.NoReg
                       join ad in AllowanceDetailRepository.Fetch()
                       on ts.TaxStatus equals ad.SubType
                       join np in EmployeeSubgroupNPRepository.Fetch()
                       on aoe.EmployeeSubgroup equals np.EmployeeSubgroup
                       where aoe.NoReg == Noreg && ts.RowStatus && (np.NP <= ad.ClassTo) && ad.Type == "medicalbpk"
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

        public IQueryable<object> GetInfoTitle(string formKey)
        {
            var data = from form in FormRepository.Fetch()
                       where form.FormKey == formKey
                       select new
                       {
                          Title= form.Title,
                       };

            return data;
        }

        public IQueryable<object> GetInfoTunjangan(int NP)
        {
            var data = from ad in AllowanceDetailRepository.Fetch()
                       where (NP >= ad.ClassFrom && NP <= ad.ClassTo) && ad.Type == "marriageallowance"
                       select new
                       {
                           AmmountTunjangan = ad.Ammount
                       };

            return data;
        }

        public IQueryable<object> GetInfoBPJS()
        {
            var data = from ad in GeneralCategoryRepository.Fetch()
                       where ad.Category == "MedicalBenefitType" && ad.Code == "bpjs"
                       select new
                       {                           
                           Name = ad.Name
                       };

            return data;
        }

        public IQueryable<object> GetInfoAVIVA()
        {
            var data = from ad in GeneralCategoryRepository.Fetch()
                            where ad.Category == "MedicalBenefitType" && ad.Code == "aviva"
                            select new
                            {                             
                                Name = ad.Name
                            };

            return data;
        }

        public IQueryable<object> GetNameNonMainFamily(string Code)
        {
            var data = from ad in GeneralCategoryRepository.Fetch()
                       where ad.Code == Code && ad.Category == "NonMainFamily"
                       select new
                       {
                           Name = ad.Name
                       };

            return data;
        }

        public PersonalDataConfirmationViewModel GetPersonalDataConfirmationViewModels(string noreg, DateTime keyDate)
        {
            var parameters = new { noreg, keyDate };

            var output = UnitOfWork.UspQuery<PersonalDataConfirmationViewModel.PersonalDataGroupViewModel, PersonalDataConfirmationStoredEntity>(parameters);

            var viewModel = new PersonalDataConfirmationViewModel
            {
                Groups = output.ToArray()
            };

            return viewModel;
        }
    }
}

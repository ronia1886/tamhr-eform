using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using TAMHR.ESS.Domain;
using Agit.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using System.Data.Entity;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class ClaimBenefitQueryService : DomainServiceBase
    {
        #region Domain Repositories
        /// <summary>
        /// Allowance detail repository object.
        /// </summary>
        protected IRepository<AllowanceDetail> AllowanceDetailRepository => UnitOfWork.GetRepository<AllowanceDetail>();

        /// <summary>
        /// Time management shift repository object.
        /// </summary>
        protected IRepository<TimeManagementShift> TimeManagementShiftRepository => UnitOfWork.GetRepository<TimeManagementShift>();

        /// <summary>
        /// Employee work schedule subtitute repository object.
        /// </summary>
        protected IRepository<EmpWorkSchSubtitute> EmpWorkSchSubtituteRepository => UnitOfWork.GetRepository<EmpWorkSchSubtitute>();

        /// <summary>
        /// Personal data family member repository object.
        /// </summary>
        protected IRepository<PersonalDataFamilyMember> PersonalDataFamilyMemberRepository => UnitOfWork.GetRepository<PersonalDataFamilyMember>();

        /// <summary>
        /// Personal data common attribute repository object.
        /// </summary>
        protected IRepository<PersonalDataCommonAttribute> PersonalDataCommonAttributeRepository => UnitOfWork.GetRepository<PersonalDataCommonAttribute>();

        /// <summary>
        /// Personal data repository object.
        /// </summary>
        protected IRepository<PersonalData> PersonalDataRepository => UnitOfWork.GetRepository<PersonalData>();

        /// <summary>
        /// Letter of guarantee repository object.
        /// </summary>
        protected IRepository<LetterGuarantee> LetterGuaranteeRepository => UnitOfWork.GetRepository<LetterGuarantee>();

        /// <summary>
        /// DPA member repository object.
        /// </summary>
        protected IRepository<DpaMember> DpaMemberRepository => UnitOfWork.GetRepository<DpaMember>();

        /// <summary>
        /// Recreation reward member repository object.
        /// </summary>
        protected IRepository<RecreationRewardMember> RecreationRewardMemberRepository => UnitOfWork.GetRepository<RecreationRewardMember>();

        /// <summary>
        /// Recreation reward repository object.
        /// </summary>
        protected IRepository<RecreationReward> RecreationRewardRepository => UnitOfWork.GetRepository<RecreationReward>();
        protected IRepository<ConceptIdeaView> ConceptIdeaRepository => UnitOfWork.GetRepository<ConceptIdeaView>();
        #endregion

        #region Variables & Properties
        private IStringLocalizer<IUnitOfWork> _localizer;
        #endregion

        #region Constructor
        public ClaimBenefitQueryService(IUnitOfWork unitOfWork, IStringLocalizer<IUnitOfWork> localizer)
            : base(unitOfWork)
        {
            _localizer = localizer;
        }
        #endregion

        public IEnumerable<DateTime> GetDatesFromPeriod(DateTime period)
        {
            DateTime date = period;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            var dates = new List<DateTime>();

            for (var dt = firstDayOfMonth; dt <= lastDayOfMonth; dt = dt.AddDays(1))
            {
                dates.Add(dt);
            }

            return dates;
        }

        public IQueryable<AllowanceDetail> GetAllowances(string type)
        {
           return AllowanceDetailRepository.Fetch().Where(x=> x.Type == type && x.RowStatus);
        }

        public IEnumerable<DpaMember> GetDpaMembers(string noReg)
        {
            return DpaMemberRepository.Find(x => x.NoReg.Equals(noReg) && x.RowStatus);
        }

        public IQueryable<object> GetChild(string Noreg)
        {
            var data = from FM in PersonalDataFamilyMemberRepository.Fetch()
                       join CA in PersonalDataCommonAttributeRepository.Fetch()
                       on FM.CommonAttributeId equals CA.Id
                       where FM.NoReg == Noreg && FM.FamilyTypeCode == "anakkandung" && FM.StartDate <= DateTime.Now && FM.EndDate >= DateTime.Now
                       select new
                       {
                           Name = CA.Name,
                           CommonAttributeId = FM.CommonAttributeId
                       };

            return data;
        }

        public IQueryable<object> GetChildStatus(string Noreg , string FamilyTypeCode)
        {
            var data = from FM in PersonalDataFamilyMemberRepository.Fetch()
                       join CA in PersonalDataCommonAttributeRepository.Fetch()
                       on FM.CommonAttributeId equals CA.Id
                       where FM.NoReg == Noreg && FM.FamilyTypeCode == FamilyTypeCode && FM.StartDate <= DateTime.Now && FM.EndDate >= DateTime.Now
                       select new
                       {
                           Name = CA.Name,
                           CommonAttributeId = FM.CommonAttributeId,
                           Gender = CA.GenderCode
                       };

            return data;
        }

        public IQueryable<object> GetInfoAmmountConceptIdea(int value, string type)
        {
            var now = DateTime.Now;
            //if(value <= 168)
            //{
                var data = from ad in AllowanceDetailRepository.Fetch()
                           where (value >= ad.ClassFrom && value <= ad.ClassTo) && ad.Type == type
                           && now >= ad.StartDate && now <= ad.EndDate
                           select new
                           {
                               Ammount = ad.Ammount
                           };
                return data;
            //}
            //else
            //{
            //    var newValue = (decimal)(value - 168) / 10;
            //    var total = newValue * 30000;
            //    var data = from ad in AllowanceDetailRepository.Fetch()
            //               where (ad.ClassTo==168) && ad.Type == type
            //               select new
            //               {
            //                   Ammount = ad.Ammount + total
            //               };
                
                
            //    return data;
            //}


            
        }

        public IQueryable<object> GetPartner(string Noreg, bool ismainfamily)
        {
            var data = from FM in PersonalDataFamilyMemberRepository.Fetch()
                       join CA in PersonalDataCommonAttributeRepository.Fetch()
                       on FM.CommonAttributeId equals CA.Id
                       where FM.NoReg == Noreg && FM.IsMainFamily == ismainfamily && FM.FamilyTypeCode == "suamiistri" && FM.StartDate <= DateTime.Now && FM.EndDate >= DateTime.Now
                       select new
                       {
                           Name = CA.Name,
                           BirthDate = CA.BirthDate,
                           NIK = CA.Nik,
                           Gender = CA.GenderCode,
                           FamilyMemberId = FM.Id
                       };

            return data;
        }

        public IQueryable<object> GetInfoEmployee(string Noreg, bool ismainfamily)
        {
            var data = from PD in PersonalDataRepository.Fetch()
                       join CA in PersonalDataCommonAttributeRepository.Fetch()
                       on PD.CommonAttributeId equals CA.Id
                       where PD.NoReg == Noreg
                       && PD.RowStatus
                       select new
                       {
                           Name = CA.Name,
                           BirthDate = CA.BirthDate,
                           NIK = CA.Nik
                       };

            return data;
        }

        public IQueryable<object> GetInfoChild(string Noreg, string Name)
        {
            var data = from FM in PersonalDataFamilyMemberRepository.Fetch()
                       join CA in PersonalDataCommonAttributeRepository.Fetch()
                       on FM.CommonAttributeId equals CA.Id
                       where FM.NoReg == Noreg && CA.Name == Name && FM.StartDate <= DateTime.Now && FM.EndDate >= DateTime.Now
                       select new
                       {
                           BirthDate = CA.BirthDate,
                           NIK = CA.Nik,
                           FamilyMemberId = FM.Id
                       };

            return data;
        }

        public IQueryable<object> GetCheckUpCount(string Noreg, string ControlCriteria, string  Diagnosa)
        {
            return LetterGuaranteeRepository.Fetch().Where(x => x.NoReg == Noreg && x.ControlCriteria == ControlCriteria && x.SurgeryType == Diagnosa);
        }

        public IQueryable<object> ContolPascaRawatInap(string Noreg, string ControlCriteria)
        {
            return LetterGuaranteeRepository.Fetch().Where(x => x.NoReg == Noreg && x.ControlCriteria == ControlCriteria);
        }

        public IQueryable<object> ContolNonPascaRawatInap(string Noreg, string Diagnosa, string ControlCriteria)
        {
            return LetterGuaranteeRepository.Fetch().Where(x => x.NoReg == Noreg && x.SurgeryType == Diagnosa && x.ControlCriteria == ControlCriteria);
        }

        public IQueryable<object> GetCalculationLoan(int Class)
        {
            var data = from AD in AllowanceDetailRepository.Fetch()
                       where (Class >= AD.ClassFrom && Class <= AD.ClassTo) && AD.Type == "CalculationLoan"
                       select new
                       {
                           Ammount = AD.Ammount
                       };

            return data;

        }

        public decimal GetClaimReward(string Noreg, string type )
        {
            var dateNow = DateTime.Now.Year;

            var data = (from r in RecreationRewardMemberRepository.Fetch()
                       join u in RecreationRewardRepository.Fetch()
                       on r.RecreationRewardId equals u.Id
                       where r.NoReg == Noreg && u.BenefitType == type
                       select new { r.Amount }).Sum( x => x.Amount);

            return data;

        }

        public IQueryable<AllowanceDetail> GetMaxClaimReward(string type)
        {
            return AllowanceDetailRepository.Fetch().Where(x => x.Type == type && x.RowStatus && x.SubType == "Max");
        }

        public int GetMaximumConceptIdeaAllowance()
        {
            return AllowanceDetailRepository.Fetch()
                .Where(x => x.Type == "IdeaConceptMatrix")
                .Select(x => x.ClassTo)
                .DefaultIfEmpty()
                .Max();
        }

        public int GetMiniumConceptIdeaAllowance()
        {
            return AllowanceDetailRepository.Fetch()
                .Where(x => x.Type == "IdeaConceptMatrix")
                .Select(x => x.ClassFrom)
                .DefaultIfEmpty()
                .Min();
        }

        public DataSourceResult GetConceptIdeaReport(DataSourceRequest request, DateTime? startDate, DateTime? endDate)
        {
            var query = ConceptIdeaRepository.Fetch().AsNoTracking();

            if (startDate.HasValue)
                query = query.Where(x => x.CreatedOn >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.CreatedOn <= endDate.Value);

            return query.ToDataSourceResult(request);
        }
    }
}

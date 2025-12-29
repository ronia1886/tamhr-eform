using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.ViewModels;
using Agit.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Agit.Domain.Extensions;
using Dapper;
using Z.EntityFramework.Plus;
using Agit.Common.Extensions;
using Agit.Common;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class AnnualPlanningService : DomainServiceBase
    {
        #region Repositories

        protected IRepository<PersonalAnnualPlanningDashboardView> PersonalAnnualPlanningDashboardViewRepository => UnitOfWork.GetRepository<PersonalAnnualPlanningDashboardView>();
        protected IRepository<SubordinateAnnualPlanningDashboardView> SubordinateAnnualPlanningDashboardViewRepository => UnitOfWork.GetRepository<SubordinateAnnualPlanningDashboardView>();

        #endregion
        public AnnualPlanningService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IEnumerable<AnnualPlanningHierarchiesStoredEntity> GetHierarchies(string NoReg)
        {
            return UnitOfWork.UdfQuery<AnnualPlanningHierarchiesStoredEntity>(new { NoReg });
        }

        public IEnumerable<AnnualPlanningEmployeeHierarchiesStoredEntity> GetEmployeeHierarchies(string NoReg,string PostCode, DateTime KeyDate)
        {
            return UnitOfWork.UdfQuery<AnnualPlanningEmployeeHierarchiesStoredEntity>(new { noreg = NoReg, postCode = PostCode, keyDate = KeyDate });
        }

        public IEnumerable<AnnualPlanningSummaryStoredEntity> GetAnnualPlanningSummary(int Period, string NoReg, string PostCode)
        {
            return UnitOfWork.UspQuery<AnnualPlanningSummaryStoredEntity>(new { Period = Period, NoReg = NoReg, PostCode = PostCode });
        }

        public IEnumerable<AnnualPlanningReportStoredEntity> GetAnnualPlanningReport(DateTime dateFrom, DateTime dateTo, string NoReg, string PostCode)
        {
            return UnitOfWork.UspQuery<AnnualPlanningReportStoredEntity>(new { DateFrom = dateFrom, DateTo = dateTo, NoReg = NoReg, PostCode = PostCode });
        }

        public IEnumerable<AnnualLeavePlanningDetail> GetAnnualLeavePlanningDetail(int Period, string NoReg)
        {
            List<AnnualLeavePlanningDetail> dataDetail = new List<AnnualLeavePlanningDetail>();
            var data = UnitOfWork.GetRepository<AnnualLeavePlanning>().Fetch().Where(wh => wh.YearPeriod == Period && wh.NoReg == NoReg).FirstOrDefault();
            if(data != null)
            {
                dataDetail = UnitOfWork.GetRepository<AnnualLeavePlanningDetail>().Fetch().Where(wh => wh.AnnualLeavePlanningId == data.Id).ToList();
            }
            return dataDetail;
        }
        public IEnumerable<AnnualLeavePlanningDetailStoredEntity> GetAnnualLeavePlanningDetailSummary(int Period, string NoReg)
        {
            return UnitOfWork.UdfQuery<AnnualLeavePlanningDetailStoredEntity>(new { Period, NoReg });
        }

        public IEnumerable<AnnualWFHPlanningDetailStoredEntity> GetAnnualWFHPlanningDetailSummary(int Period, string NoReg)
        {
            return UnitOfWork.UdfQuery<AnnualWFHPlanningDetailStoredEntity>(new { Period, NoReg });
        }

        public IEnumerable<AnnualBDJKPlanningDetailStoredEntity> GetAnnualBDJKPlanningDetailSummary(int Period, string NoReg)
        {
            return UnitOfWork.UdfQuery<AnnualBDJKPlanningDetailStoredEntity>(new { Period, NoReg });
        }

        public IEnumerable<PersonalAnnualPlanningDashboardView> GetPersonalAnnualPlanningDashboard(string noReg, int? yearPeriod, string formKey)
        {
            var result = PersonalAnnualPlanningDashboardViewRepository.Fetch().Where(p => p.NoReg == noReg);

            if (yearPeriod.HasValue)
                result = result.Where(r => r.YearPeriod == yearPeriod.Value);

            if (!string.IsNullOrEmpty(formKey))
                result = result.Where(r => r.FormKey == formKey);

            return result;
        }

        public IEnumerable<SubordinateAnnualPlanningDashboardView> GetSubordinateAnnualPlanningDashboard(string noReg, int? yearPeriod, string formKey)
        {
            var result = SubordinateAnnualPlanningDashboardViewRepository.Fetch().Where(p =>
                (p.SuperiorNoReg == noReg && p.DocumentStatusCode != "draft") ||
                (p.NoReg == noReg && (p.FormKey != "annual-leave-planning"))
            );

            if (yearPeriod.HasValue)
                result = result.Where(r => r.YearPeriod == yearPeriod.Value);

            if (!string.IsNullOrEmpty(formKey))
                result = result.Where(r => r.FormKey == formKey);

            return result;
        }

        //public IEnumerable<AnnualPlanningGetSubordinateStoredEntity> GetSubordinateAnnualPlanningDashboard(string noReg, string postCode, int? yearPeriod, string formKey)
        //{
        //    var result = UnitOfWork.UdfQuery<AnnualPlanningGetSubordinateStoredEntity>(new {NoReg = noReg, PostCode = postCode});

        //    if (yearPeriod.HasValue)
        //        result = result.Where(r => r.YearPeriod == yearPeriod.Value);

        //    if (!string.IsNullOrEmpty(formKey))
        //        result = result.Where(r => r.FormKey == formKey);

        //    return result;
        //}

        public IEnumerable<int> GetYearPeriod(string noReg, string postCode)
        {
            List<int> personalPeriods = GetPersonalAnnualPlanningDashboard(noReg, null, null).Select(p => p.YearPeriod).Distinct().ToList();
            List<int> suboridnatePeriods = GetSubordinateAnnualPlanningDashboard(noReg, null, null).Select(p => p.YearPeriod).Distinct().ToList();

            List<int> results = new List<int>();
            results.AddRange(personalPeriods);
            results.AddRange(suboridnatePeriods);

            return results.Distinct();
        }

        public IEnumerable<AnnualPlanningAbnormalitiesStoredEntity> GetAbnormalities(string noReg, DateTime keyDate)
        {
            return UnitOfWork.UspQuery<AnnualPlanningAbnormalitiesStoredEntity>(new { NoReg = noReg, KeyDate= keyDate });
        }
    }
}

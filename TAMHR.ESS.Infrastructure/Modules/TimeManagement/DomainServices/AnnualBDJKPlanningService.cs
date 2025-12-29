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
using Newtonsoft.Json;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class AnnualBDJKPlanningService : DomainServiceBase
    {
        #region Repositories
        /// <summary>
        /// Document approval repository
        /// </summary>
        protected IRepository<DocumentApproval> DocumentApprovalRepository { get { return UnitOfWork.GetRepository<DocumentApproval>(); } }

        /// <summary>
        /// BDJK request repository
        /// </summary>
        protected IRepository<AnnualBDJKPlanning> AnnualBDJKPlanningRepository { get { return UnitOfWork.GetRepository<AnnualBDJKPlanning>(); } }
        protected IRepository<AnnualBDJKPlanningDetail> AnnualBDJKPlanningDetailRepository { get { return UnitOfWork.GetRepository<AnnualBDJKPlanningDetail>(); } }
        protected IRepository<AnnualBDJKPlanningDetailView> AnnualBDJKPlanningDetailViewRepository { get { return UnitOfWork.GetRepository<AnnualBDJKPlanningDetailView>(); } }
        protected IRepository<EmployeeClass7UpView> EmployeeClass7UpViewRepository { get { return UnitOfWork.GetRepository<EmployeeClass7UpView>(); } }
        /// <summary>
        /// Time management readonly repository
        /// </summary>
        protected IReadonlyRepository<TimeManagement> TimeManagementReadonlyRepository { get { return UnitOfWork.GetRepository<TimeManagement>(); } }
        protected IRepository<ActualOrganizationStructure> ActualOrganizationStructureRepository { get { return UnitOfWork.GetRepository<ActualOrganizationStructure>(); } }
        protected IReadonlyRepository<EmployeeWorkScheduleView> EmployeeWorkScheduleReadonlyRepository => UnitOfWork.GetRepository<EmployeeWorkScheduleView>();

        #endregion

        #region Constructor
        public AnnualBDJKPlanningService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        public IQueryable<AnnualBDJKPlanning> GetAnnualBDJKPlannings()
        {
            return AnnualBDJKPlanningRepository.Fetch()
                .AsNoTracking();
        }

        public IQueryable<AnnualBDJKPlanningDetail> GetAnnualBDJKPlanningDetails()
        {
            return AnnualBDJKPlanningDetailRepository.Fetch()
                .AsNoTracking();
        }

        public IQueryable<AnnualBDJKPlanning> GetAnnualBDJKPlanningByDocumentApprovalId(Guid documentApprovalId)
        {
            return GetAnnualBDJKPlannings().Where(x => x.DocumentApprovalId == documentApprovalId);
        }

        public AnnualBDJKPlanning GetAnnualBDJKPlanning(Guid id)
        {
            return GetAnnualBDJKPlannings().Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        public AnnualBDJKPlanning GetByKey(string noReg, string postCode, int periodYear)
        {
            //var result = from da in DocumentApprovalRepository.Fetch()
            //             join bdjk in AnnualBDJKPlanningRepository.Fetch() on da.Id equals bdjk.DocumentApprovalId
            //             where da.DocumentStatusCode == DocumentStatus.Completed && bdjk.Submitter == noReg && bdjk.YearPeriod == periodYear
            //             select bdjk;

            var result = UnitOfWork.GetConnection().Query<AnnualBDJKPlanning>(@"
            SELECT AWP.DocumentApprovalId FROM TB_R_ANNUAL_BDJK_PLANNING AWP INNER JOIN TB_R_ANNUAL_BDJK_PLANNING_DETAIL AWPD
            ON AWP.Id=AWPD.AnnualBDJKPlanningId
            INNER JOIN TB_R_DOCUMENT_APPROVAL da ON da.ID = AWP.DocumentApprovalId
            INNER JOIN (
		            SELECT
			            RIGHT(TargetNoReg, 6) AS NoReg,
			            TargetPostCode AS PostCode
		            FROM 
			            dbo.SF_GET_EMPLOYEE_HIERARCHIES(@NoReg, @PostCode, CONVERT(DATE, GETDATE()))
	            ) superior ON superior.NoReg = AWPD.NoReg
            WHERE AWP.Submitter = @NoReg AND AWP.YearPeriod = @Period
            AND da.DocumentStatusCode IN ('inprogress','completed')
            GROUP BY AWP.DocumentApprovalId
            ", new { NoReg = noReg, PostCode = postCode, Period = periodYear });

            return result.FirstOrDefault();
        }
       
        /// <summary>
        /// Update annual BDJK planning by document approval id and temporary id
        /// </summary>
        /// <param name="documentApprovalId">Document Approval Id</param>
        public void UpsertAnnualBDJKPlanningRequest(Guid documentApprovalId, DocumentRequestDetailViewModel<AnnualBDJKPlanningViewModel> d, string action = null)
        {
            string objectValue = JsonConvert.SerializeObject(d.Object);
            AnnualBDJKPlanning annualBDJKPlanning = d.Object.AnnualBDJKPlanning;
            List<AnnualBDJKPlanningDetailView> annualBDJKPlanningDetails = d.Object.AnnualBDJKPlanningDetails;
            var dt = annualBDJKPlanningDetails.Select(x => new
            {
                x.NoReg,
                x.StartDate,
                x.EndDate,
                x.Days,
                x.BDJKCode,
                x.ActivityCode,
                x.Taxi,
                x.UangMakanDinas
            });

            UnitOfWork.Transact(trans =>
            {
                UnitOfWork.UspQuery("SP_UPSERT_ANNUALBDJKPLANNING_REQUEST", new { documentApprovalId, submitter = annualBDJKPlanning.Submitter, period = annualBDJKPlanning.YearPeriod, data = dt.ConvertToDataTable().AsTableValuedParameter("TVP_ANNUAL_BDJK_PLANNING_DETAIL"), ObjectValue = objectValue, Action = action }, trans);
            });
        }
        public bool HasNewerVersion(Guid documentApprovalId)
        {
            bool hasNewerVersion = false;
            var firstVersion = AnnualBDJKPlanningRepository.Find(a => a.DocumentApprovalId == documentApprovalId && a.Version == 1).FirstOrDefault();
            if (firstVersion != null)
            {
                hasNewerVersion = AnnualBDJKPlanningRepository.Fetch()
                    .Where(a => a.Submitter == firstVersion.Submitter && a.YearPeriod == firstVersion.YearPeriod && a.Version > firstVersion.Version)
                    .Count() > 0;
            }
            return hasNewerVersion;
        }
        public IEnumerable<AnnualBDJKPlanningDetailView> GetDetails(Guid documentApprovalId)
        {
            IEnumerable<AnnualBDJKPlanningDetailView> result = new List<AnnualBDJKPlanningDetailView>();

            result = AnnualBDJKPlanningDetailViewRepository.Fetch()
                .Where(a => a.DocumentApprovalId == documentApprovalId)
                .OrderBy(a => a.StartDate);

            return result;
        }

        public IEnumerable<AnnualBDJKPlanningDetailView> GetBDJKPlanningByNoReg(string Superior, string NoReg, int Period)
        {
            IEnumerable<AnnualBDJKPlanningDetailView> result = new List<AnnualBDJKPlanningDetailView>();

            result = AnnualBDJKPlanningDetailViewRepository.Fetch()
                .Where(a => a.NoReg == NoReg && a.YearPeriod == Period && a.CreatedBy != Superior && (a.DocumentStatusCode == "inprogress" || a.DocumentStatusCode == "completed"))
                .OrderBy(a => a.StartDate);

            return result;
        }

        public IEnumerable<EmployeeClass7UpView> GetSubordinates(string noreg)
        {
            IEnumerable<EmployeeClass7UpView> result = new List<EmployeeClass7UpView>();

            result = EmployeeClass7UpViewRepository.Fetch()
                .Where(a => a.SuperiorNoReg == noreg && a.NoReg!=noreg)
                .OrderBy(a => a.Class);

            return result;
        }

        public IEnumerable<AnnualBDJKPlanningSummaryStoredEntity> GetAnnualBDJKPlanningSummaryExcelData(Guid documentApprovalId)
        {
            var results = UnitOfWork.UspQuery<AnnualBDJKPlanningSummaryStoredEntity>(new { documentApprovalId });

            return results;
        }

        public IEnumerable<AnnualBDJKPlanningSummaryPdfStoredEntity> GetAnnualBDJKPlanningSummaryPdfData(Guid documentApprovalId)
        {
            var results = UnitOfWork.UspQuery<AnnualBDJKPlanningSummaryPdfStoredEntity>(new { documentApprovalId });

            return results;
        }

        public IEnumerable<SubordinateStroredEntity> GetManPower(string noReg)
        {
            IEnumerable<SubordinateStroredEntity> result = new List<SubordinateStroredEntity>();
            ActualOrganizationStructure aos = ActualOrganizationStructureRepository.Fetch().Where(a => a.NoReg == noReg).FirstOrDefault();

            if (aos != null)
                result = UnitOfWork.UdfQuery<SubordinateStroredEntity>(new { OrgCode = aos.OrgCode, OrgLevel = aos.OrgLevel });

            return result;
        }

        public string GetDivisionByNoReg(string NoReg)
        {
            return UnitOfWork.GetConnection().Query<string>(@"
            select Divisi from vw_personal_data_information WHERE NoReg=@NoReg
            ", new { NoReg }).FirstOrDefault();
        }

        public IEnumerable<EmployeeWorkScheduleView> GetActiveWorkSchedule(string noreg, DateTime startDate, DateTime endDate)
        {
            var data = EmployeeWorkScheduleReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.Date >= startDate && x.Date <= endDate && x.ShiftCode != "OFF");

            return data;
        }

        public IEnumerable<EmployeeWorkScheduleView> GetOffWorkSchedule(string noreg, DateTime startDate, DateTime endDate)
        {
            var data = EmployeeWorkScheduleReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.Date >= startDate && x.Date <= endDate && x.ShiftCode == "OFF");

            return data;
        }

    }
}

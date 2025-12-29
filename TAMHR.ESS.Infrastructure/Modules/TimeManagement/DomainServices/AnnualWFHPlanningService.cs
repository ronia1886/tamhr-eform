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
    public class AnnualWFHPlanningService : DomainServiceBase
    {
        #region Repositories
       
        protected IRepository<DocumentApproval> DocumentApprovalRepository { get { return UnitOfWork.GetRepository<DocumentApproval>(); } }
        protected IRepository<AnnualWFHPlanning> AnnualWFHPlanningRepository { get { return UnitOfWork.GetRepository<AnnualWFHPlanning>(); } }
        protected IRepository<AnnualWFHPlanningDetail> AnnualWFHPlanningDetailRepository { get { return UnitOfWork.GetRepository<AnnualWFHPlanningDetail>(); } }
        protected IRepository<AnnualWFHPlanningDetailView> AnnualWFHPlanningDetailViewRepository { get { return UnitOfWork.GetRepository<AnnualWFHPlanningDetailView>(); } }
        protected IReadonlyRepository<TimeManagement> TimeManagementReadonlyRepository { get { return UnitOfWork.GetRepository<TimeManagement>(); } }
        protected IRepository<ActualOrganizationStructure> ActualOrganizationStructureRepository { get { return UnitOfWork.GetRepository<ActualOrganizationStructure>(); } }

        #endregion

        #region Constructor
        public AnnualWFHPlanningService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        public IQueryable<AnnualWFHPlanning> GetAnnualWFHPlannings()
        {
            return AnnualWFHPlanningRepository.Fetch()
                .AsNoTracking();
        }
        public IQueryable<AnnualWFHPlanningDetail> GetAnnualWFHPlanningDetails()
        {
            return AnnualWFHPlanningDetailRepository.Fetch()
                .AsNoTracking();
        }
        public IQueryable<AnnualWFHPlanning> GetAnnualWFHPlanningByDocumentApprovalId(Guid documentApprovalId)
        {
            return GetAnnualWFHPlannings().Where(x => x.DocumentApprovalId == documentApprovalId);
        }

        public AnnualWFHPlanning GetAnnualWFHPlanning(Guid id)
        {
            return GetAnnualWFHPlannings().Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }
        public void UpsertAnnualWFHPlanningRequest(Guid documentApprovalId, DocumentRequestDetailViewModel<AnnualWFHPlanningViewModel> d, string action = null)
        {
            string objectValue = JsonConvert.SerializeObject(d.Object);
            AnnualWFHPlanning annualWFHPlanning = d.Object.AnnualWFHPlanning;
            List<AnnualWFHPlanningDetailView> annualWFHPlanningDetails = d.Object.AnnualWFHPlanningDetails;
            var dt = annualWFHPlanningDetails.Select(x => new
            {
                x.NoReg,
                x.StartDate,
                x.EndDate,
                x.Days,
                x.WorkPlace
            });

            UnitOfWork.Transact(trans =>
            {
                UnitOfWork.UspQuery("SP_UPSERT_ANNUALWFHPLANNING_REQUEST", new { documentApprovalId, submitter = annualWFHPlanning.Submitter, period = annualWFHPlanning.YearPeriod, data = dt.ConvertToDataTable().AsTableValuedParameter("TVP_ANNUAL_WFH_PLANNING_DETAIL"), ObjectValue = objectValue, Action = action }, trans);
                UnitOfWork.SaveChanges();
            });
        }
        public bool HasNewerVersion(Guid documentApprovalId)
        {
            bool hasNewerVersion = false;
            var firstVersion = AnnualWFHPlanningRepository.Find(a => a.DocumentApprovalId == documentApprovalId && a.Version == 1).FirstOrDefault();
            if (firstVersion != null)
            {
                hasNewerVersion = AnnualWFHPlanningRepository.Fetch()
                    .Where(a => a.Submitter == firstVersion.Submitter && a.YearPeriod == firstVersion.YearPeriod && a.Version > firstVersion.Version)
                    .Count() > 0;
            }
            return hasNewerVersion;
        }
        public IEnumerable<AnnualWFHPlanningDetailView> GetDetails(Guid documentApprovalId)
        {
            IEnumerable<AnnualWFHPlanningDetailView> result = new List<AnnualWFHPlanningDetailView>();

            result = AnnualWFHPlanningDetailViewRepository.Fetch()
                .Where(a => a.DocumentApprovalId == documentApprovalId)
                .OrderBy(a => a.Name)
                .OrderBy(a => a.StartDate);

            return result;
        }

        public IEnumerable<AnnualWFHPlanningDetailView> GetWFHPlanningByNoReg(string Superior,string NoReg, int Period)
        {
            IEnumerable<AnnualWFHPlanningDetailView> result = new List<AnnualWFHPlanningDetailView>();

            result = AnnualWFHPlanningDetailViewRepository.Fetch()
                .Where(a => a.NoReg == NoReg && a.YearPeriod == Period  && a.CreatedBy != Superior && (a.DocumentStatusCode == "inprogress" || a.DocumentStatusCode == "completed"))
                .OrderBy(a => a.Name)
                .OrderBy(a => a.StartDate);

            return result;
        }
        public IEnumerable<AnnualWFHPlanningSummaryExcelFileStoredEntity> GetAnnualWFHPlanningSummaryExcelFileData(Guid documentApprovalId)
        {
            var results = UnitOfWork.UspQuery<AnnualWFHPlanningSummaryExcelFileStoredEntity>(new { documentApprovalId });
            return results;
        }
        public IEnumerable<AnnualWFHPlanningTotalWorkingDaysStoredEntity> GetTotalWorkingDaysExcelFileData(int periodYear, string noReg)
        {
            var results = UnitOfWork.UspQuery<AnnualWFHPlanningTotalWorkingDaysStoredEntity>(new { YearPeriod = periodYear, NoReg = noReg });
            return results;
        }

        public IEnumerable<AnnualWFHPlanningSummaryPdfStoredEntity> GetAnnualWFHPlanningSummaryPdfData(Guid documentApprovalId)
        {
            var results = UnitOfWork.UspQuery<AnnualWFHPlanningSummaryPdfStoredEntity>(new { DocumentApprovalId = documentApprovalId });
            return results;
        }

        public IEnumerable<SubordinateStroredEntity> GetManPower(string noReg)
        {
            IEnumerable<SubordinateStroredEntity> result = new List<SubordinateStroredEntity>();
            ActualOrganizationStructure aos = ActualOrganizationStructureRepository.Fetch().Where(a => a.NoReg == noReg).FirstOrDefault();
            
            if(aos != null)
                result = UnitOfWork.UdfQuery<SubordinateStroredEntity>(new { OrgCode = aos.OrgCode, OrgLevel = aos.OrgLevel });

            return result;
        }

        public WFHPlanningByDate GetWFHPlanningByDate(string noReg, DateTime date)
        {
            return UnitOfWork.UspQuery<WFHPlanningByDate>(new { NoReg = noReg, Date = date }).FirstOrDefault();
        }

        public AnnualWFHPlanning GetByKey(string noReg, string postCode, int periodYear)
        {
            //var result = from da in DocumentApprovalRepository.Fetch()
            //             join wfh in AnnualWFHPlanningRepository.Fetch() on da.Id equals wfh.DocumentApprovalId
            //             where da.DocumentStatusCode == DocumentStatus.Completed && wfh.Submitter == noReg && wfh.YearPeriod == periodYear
            //             select wfh;

            var result = UnitOfWork.GetConnection().Query<AnnualWFHPlanning>(@"
            SELECT AWP.DocumentApprovalId FROM TB_R_ANNUAL_WFH_PLANNING AWP INNER JOIN TB_R_ANNUAL_WFH_PLANNING_DETAIL AWPD
            ON AWP.Id=AWPD.AnnualWFHPlanningId
            INNER JOIN TB_R_DOCUMENT_APPROVAL da ON da.ID = AWP.DocumentApprovalId
            INNER JOIN (
		            SELECT
			            RIGHT(TargetNoReg, 6) AS NoReg,
			            TargetPostCode AS PostCode
		            FROM 
			            dbo.SF_GET_EMPLOYEE_HIERARCHIES(@NoReg, @PostCode, CONVERT(DATE, GETDATE()))
	            ) superior ON superior.NoReg = AWPD.NoReg
            WHERE AWP.Submitter = @NoReg
            AND da.DocumentStatusCode='inprogress'
            GROUP BY AWP.DocumentApprovalId
            ", new { NoReg = noReg, PostCode = postCode});
            
            return result.FirstOrDefault();
        }
    }
}

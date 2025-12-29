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
    public class AnnualOTPlanningService : DomainServiceBase
    {
        #region Repositories
       
        protected IRepository<DocumentApproval> DocumentApprovalRepository { get { return UnitOfWork.GetRepository<DocumentApproval>(); } }
        protected IRepository<AnnualOTPlanning> AnnualOTPlanningRepository { get { return UnitOfWork.GetRepository<AnnualOTPlanning>(); } }
        protected IRepository<AnnualOTPlanningDetail> AnnualOTPlanningDetailRepository { get { return UnitOfWork.GetRepository<AnnualOTPlanningDetail>(); } }
        protected IRepository<AnnualOTPlanningDetailView> AnnualOTPlanningDetailViewRepository { get { return UnitOfWork.GetRepository<AnnualOTPlanningDetailView>(); } }
        protected IReadonlyRepository<TimeManagement> TimeManagementReadonlyRepository { get { return UnitOfWork.GetRepository<TimeManagement>(); } }
        protected IRepository<ActualOrganizationStructure> ActualOrganizationStructureRepository { get { return UnitOfWork.GetRepository<ActualOrganizationStructure>(); } }

        #endregion

        #region Constructor
        public AnnualOTPlanningService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        public IQueryable<AnnualOTPlanning> GetAnnualOTPlannings()
        {
            return AnnualOTPlanningRepository.Fetch()
                .AsNoTracking();
        }
        public IQueryable<AnnualOTPlanningDetail> GetAnnualOTPlanningDetails()
        {
            return AnnualOTPlanningDetailRepository.Fetch()
                .AsNoTracking();
        }
        public IQueryable<AnnualOTPlanning> GetAnnualOTPlanningByDocumentApprovalId(Guid documentApprovalId)
        {
            return GetAnnualOTPlannings().Where(x => x.DocumentApprovalId == documentApprovalId);
        }

        public AnnualOTPlanning GetAnnualOTPlanning(Guid id)
        {
            return GetAnnualOTPlannings().Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }
        public void UpsertAnnualOTPlanningRequest(Guid documentApprovalId, DocumentRequestDetailViewModel<AnnualOTPlanningViewModel> d, string action = null)
        {
            string objectValue = JsonConvert.SerializeObject(d.Object);
            AnnualOTPlanning annualOTPlanning = d.Object.AnnualOTPlanning;
            List<AnnualOTPlanningDetailView> annualOTPlanningDetails = d.Object.AnnualOTPlanningDetails;
            var dt = annualOTPlanningDetails.Select(x => new
            {
                x.CategoryCode,
                x.LabourType,
                x.Jan,
                x.Feb,
                x.Mar,
                x.Apr,
                x.May,
                x.Jun,
                x.Jul,
                x.Aug,
                x.Sep,
                x.Oct,
                x.Nov,
                x.Dec
            });

            UnitOfWork.Transact(trans =>
            {
                UnitOfWork.UspQuery("SP_UPSERT_ANNUALOTPLANNING_REQUEST", new { documentApprovalId, NoReg = annualOTPlanning.NoReg, YearPeriod = annualOTPlanning.YearPeriod, data = dt.ConvertToDataTable().AsTableValuedParameter("TVP_ANNUAL_OT_PLANNING_DETAIL"), ObjectValue = objectValue, Action = action }, trans);
                UnitOfWork.SaveChanges();
            });
        }
        public bool HasNewerVersion(Guid documentApprovalId)
        {
            bool hasNewerVersion = false;
            var firstVersion = AnnualOTPlanningRepository.Find(a => a.DocumentApprovalId == documentApprovalId && a.Version == 1).FirstOrDefault();
            if (firstVersion != null)
            {
                hasNewerVersion = AnnualOTPlanningRepository.Fetch()
                    .Where(a => a.NoReg == firstVersion.NoReg && a.YearPeriod == firstVersion.YearPeriod && a.Version > firstVersion.Version)
                    .Count() > 0;
            }
            return hasNewerVersion;
        }
        public IEnumerable<AnnualOTPlanningDetailView> GetDetails(Guid documentApprovalId)
        {
            IEnumerable<AnnualOTPlanningDetailView> result = new List<AnnualOTPlanningDetailView>();

            result = AnnualOTPlanningDetailViewRepository.Fetch()
                .Where(a => a.DocumentApprovalId == documentApprovalId)
                .OrderBy(a => a.OrderSequence);

            return result;
        }

        public IEnumerable<AnnualOTPlanningDetailView> GetInitial(string noReg, string postCode, int yearPeriod)
        {
            var data = new List<AnnualOTPlanningDetailView>();
            var results = UnitOfWork.UspQuery<AnnualOTPlanningDetailInitial>(new { NoReg = noReg, PostCode = postCode, YearPeriod = yearPeriod });
            foreach(AnnualOTPlanningDetailInitial result in results)
            {
                AnnualOTPlanningDetailView tmp = new AnnualOTPlanningDetailView();
                tmp.Division = result.Division;
                tmp.CategoryCode = result.CategoryCode;
                tmp.Category = result.Category;
                tmp.OrderSequence = result.OrderSequence;
                tmp.LabourType = result.LabourType;
                tmp.Jan = result.Jan;
                tmp.Feb = result.Feb;
                tmp.Mar = result.Mar;
                tmp.Apr = result.Apr;
                tmp.May = result.May;
                tmp.Jun = result.Jun;
                tmp.Jul = result.Jul;
                tmp.Aug = result.Aug;
                tmp.Sep = result.Sep;
                tmp.Oct = result.Oct;
                tmp.Nov = result.Nov;
                tmp.Dec = result.Dec;
                if (result.CategoryCode.ToLower().Contains("remark"))
                {
                    tmp.Total = "";
                }
                else
                {
                    tmp.Total = (decimal.Parse(result.Jan) + decimal.Parse(result.Feb) + decimal.Parse(result.Mar) + decimal.Parse(result.Apr) + decimal.Parse(result.May) + decimal.Parse(result.Jun) + decimal.Parse(result.Jul) + decimal.Parse(result.Aug) + decimal.Parse(result.Sep) + decimal.Parse(result.Oct) + decimal.Parse(result.Nov) + decimal.Parse(result.Dec)).ToString();
                }
                
                data.Add(tmp);
            }

            return data;
        }
        public IEnumerable<AnnualOTPlanningSummaryExcelFileStoredEntity> GetAnnualOTPlanningSummaryExcelFileData(Guid documentApprovalId)
        {
            var results = UnitOfWork.UspQuery<AnnualOTPlanningSummaryExcelFileStoredEntity>(new { documentApprovalId });
            return results;
        }
        public IEnumerable<AnnualOTPlanningTotalWorkingDaysStoredEntity> GetTotalWorkingDaysExcelFileData(int periodYear, string noReg)
        {
            var results = UnitOfWork.UspQuery<AnnualOTPlanningTotalWorkingDaysStoredEntity>(new { YearPeriod = periodYear, NoReg = noReg });
            return results;
        }

        public IEnumerable<AnnualOTPlanningSummaryPdfStoredEntity> GetAnnualOTPlanningSummaryPdfData(Guid documentApprovalId)
        {
            var results = UnitOfWork.UspQuery<AnnualOTPlanningSummaryPdfStoredEntity>(new { DocumentApprovalId = documentApprovalId });
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

        //public OTPlanningByDate GetOTPlanningByDate(string noReg, DateTime date)
        //{
        //    return UnitOfWork.UspQuery<OTPlanningByDate>(new { NoReg = noReg, Date = date }).FirstOrDefault();
        //}

        public AnnualOTPlanning GetByKey(string noReg, string postCode, int periodYear)
        {
            //var result = from da in DocumentApprovalRepository.Fetch()
            //             join alp in AnnualOTPlanningRepository.Fetch() on da.Id equals alp.DocumentApprovalId
            //             where da.DocumentStatusCode == DocumentStatus.Completed && alp.NoReg == noReg && alp.YearPeriod == periodYear
            //             select alp;
            var result = UnitOfWork.GetConnection().Query<AnnualOTPlanning>(@"
            SELECT AWP.DocumentApprovalId FROM TB_R_ANNUAL_OT_PLANNING AWP INNER JOIN TB_R_ANNUAL_OT_PLANNING_DETAIL AWPD
            ON AWP.Id=AWPD.AnnualOTPlanningId
            INNER JOIN TB_R_DOCUMENT_APPROVAL da ON da.ID = AWP.DocumentApprovalId
            INNER JOIN (
		            SELECT
			            RIGHT(TargetNoReg, 6) AS NoReg,
			            TargetPostCode AS PostCode
		            FROM 
			            dbo.SF_GET_EMPLOYEE_HIERARCHIES(@NoReg, @PostCode, CONVERT(DATE, GETDATE()))
	            ) superior ON superior.NoReg = AWP.NoReg
            WHERE AWP.NoReg = @NoReg AND AWP.YearPeriod = @Period
            AND da.DocumentStatusCode IN ('inprogress','completed')
            GROUP BY AWP.DocumentApprovalId
            ", new { NoReg = noReg, PostCode = postCode, Period = periodYear });

            return result.FirstOrDefault();
        }

        public IEnumerable<MonthlyOvertimePlanVsActualStoredEntity> GetMonthlyOvertimePlanVsActual(int yearPeriod, int monthPeriod, string noReg)
        {
            var results = UnitOfWork.UspQuery<MonthlyOvertimePlanVsActualStoredEntity>(new { YearPeriod = yearPeriod, MonthPeriod = monthPeriod, NoReg = noReg });
            return results;
        }
    }
}

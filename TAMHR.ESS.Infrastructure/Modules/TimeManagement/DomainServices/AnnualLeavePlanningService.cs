using Agit.Domain;
using Agit.Domain.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.ViewModels;
using Agit.Domain.Extensions;
using Agit.Common.Extensions;
using Dapper;
using Agit.Domain.Repository;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices.ComTypes;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class AnnualLeavePlanningService : DomainServiceBase
    {
        #region Repositories

        protected IRepository<AnnualLeavePlanning> AnnualLeavePlanningRepository => UnitOfWork.GetRepository<AnnualLeavePlanning>();
        protected IRepository<AnnualLeavePlanningDetailView> AnnualLeavePlanningDetailRepository => UnitOfWork.GetRepository<AnnualLeavePlanningDetailView>();
        protected IRepository<DocumentApproval> DocumentApprovalRepository => UnitOfWork.GetRepository<DocumentApproval>();
        protected IRepository<DocumentRequestDetail> DocumentRequestDetailRepository => UnitOfWork.GetRepository<DocumentRequestDetail>();
        protected IRepository<Form> FormRepository => UnitOfWork.GetRepository<Form>();
        protected IRepository<EmployeeLeave> EmployeeLeaveRepository => UnitOfWork.GetRepository<EmployeeLeave>();
        protected IRepository<EmployeeAnnualLeave> EmployeeAnnualLeaveRepository => UnitOfWork.GetRepository<EmployeeAnnualLeave>();
        protected IReadonlyRepository<EmployeeWorkScheduleView> EmployeeWorkScheduleReadonlyRepository => UnitOfWork.GetRepository<EmployeeWorkScheduleView>();

        #endregion

        #region Constructor
        public AnnualLeavePlanningService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Methods

        public bool LeaveDateValid(DateTime checkedDate, List<AnnualLeavePlanningDetailView> annualLeavePlanningDetails)
        {
            int numberOfIntersection = annualLeavePlanningDetails.Where(a => checkedDate >= a.StartDate && checkedDate <= a.EndDate).Count();

            return numberOfIntersection == 1;
        }

        public void SaveAnnualLeavePlanning(Guid documentApprovalId, DocumentRequestDetailViewModel<AnnualLeavePlanningViewModel> d, string action = null)
        {
            AnnualLeavePlanning annualLeavePlanning = d.Object.AnnualLeavePlanning;
            List<AnnualLeavePlanningDetailView> annualLeavePlanningDetails = d.Object.AnnualLeavePlanningDetails;
            string objectValue = JsonConvert.SerializeObject(d.Object);

            var dt = annualLeavePlanningDetails.Select(x => new
            {
                x.AbsentId,
                x.StartDate,
                x.EndDate,
                x.Days
            });

            UnitOfWork.Transact(trans =>
            {
                UnitOfWork.UspQuery("SP_SAVE_ANNUAL_LEAVE_PLANNING", new { 
                    DocumentApprovalId = documentApprovalId,
                    YearPeriod = annualLeavePlanning.YearPeriod,
                    NoReg = annualLeavePlanning.NoReg,
                    Action = action,
                    ObjectValue = objectValue,
                    Details = dt.ConvertToDataTable().AsTableValuedParameter("TVP_ANNUAL_LEAVE_PLANNING_DETAIL")
                }, trans);

                UnitOfWork.SaveChanges();
            });
        }

        public IEnumerable<AnnualLeavePlanningDetailView> GetDetails(Guid documentApprovalId)
        {
            IEnumerable<AnnualLeavePlanningDetailView> result = new List<AnnualLeavePlanningDetailView>();

            result = AnnualLeavePlanningDetailRepository.Fetch()
                .Where(a => a.DocumentApprovalId == documentApprovalId)
                .OrderBy(a => a.StartDate);

            return result;
        }

        public AnnualLeavePlanningDetailView GetById(Guid id)
        {
            AnnualLeavePlanningDetailView result = AnnualLeavePlanningDetailRepository.FindById(id);

            return result;
        }

        public AnnualLeavePlanning GetHeader(Guid documentApprovalId)
        {
            return AnnualLeavePlanningRepository.Find(a => a.DocumentApprovalId == documentApprovalId).FirstOrDefault();
        }

        public AnnualLeavePlanning GetByKey(string noReg, string postCode, int yearPeriod)
        {
            //var result = from da in DocumentApprovalRepository.Fetch()
            //           join alp in AnnualLeavePlanningRepository.Fetch() on da.Id equals alp.DocumentApprovalId
            //           where da.DocumentStatusCode == DocumentStatus.Completed && alp.NoReg == noReg && alp.YearPeriod == yearPeriod
            //           select alp;

            var result = UnitOfWork.GetConnection().Query<AnnualLeavePlanning>(@"
            SELECT AWP.DocumentApprovalId FROM TB_R_ANNUAL_LEAVE_PLANNING AWP INNER JOIN TB_R_ANNUAL_LEAVE_PLANNING_DETAIL AWPD
            ON AWP.Id=AWPD.AnnualLeavePlanningId
            INNER JOIN TB_R_DOCUMENT_APPROVAL da ON da.ID = AWP.DocumentApprovalId
            WHERE AWP.NoReg = @NoReg AND AWP.YearPeriod = @Period
            AND da.DocumentStatusCode IN ('inprogress','completed')
            GROUP BY AWP.DocumentApprovalId
            ", new { NoReg = noReg, PostCode = postCode, Period = yearPeriod });


            return result.FirstOrDefault();
        }

        public bool HasNewerVersion(Guid documentApprovalId)
        {
            bool hasNewerVersion = false;
            var firstVersion = AnnualLeavePlanningRepository.Find(a => a.DocumentApprovalId == documentApprovalId && a.Version == 1).FirstOrDefault();
            if(firstVersion != null)
            {
                hasNewerVersion = AnnualLeavePlanningRepository.Fetch()
                    .Where(a => a.NoReg == firstVersion.NoReg && a.YearPeriod == firstVersion.YearPeriod && a.Version > firstVersion.Version)
                    .Count() > 0;
            }
            return hasNewerVersion;
        }

        public IEnumerable<AnnualLeavePlanningSummaryStoredEntity> GetAnnualLeavePlanningSummaryExcelData(Guid documentApprovalId)
        {
            var results = UnitOfWork.UspQuery<AnnualLeavePlanningSummaryStoredEntity>(new { documentApprovalId });

            return results;
        }

        public IEnumerable<AnnualLeavePlanningSummaryPdfStoredEntity> GetAnnualLeavePlanningSummaryPdfData(Guid documentApprovalId)
        {
            var results = UnitOfWork.UspQuery<AnnualLeavePlanningSummaryPdfStoredEntity>(new { documentApprovalId });

            return results;
        }

        public IEnumerable<AvailableAnnualLeavePlanStoredEntity> GetAvailableAnnualLeavePlan(string absenceType, string noReg)
        {
            var results = UnitOfWork.UspQuery<AvailableAnnualLeavePlanStoredEntity>(new { AbsenceType = absenceType, NoReg = noReg });

            return results;
        }
        public IEnumerable<AnnualLeavePlanStoredEntity> GetAnnualLeavePlan(string absenceType, string noReg)
        {
            var results = UnitOfWork.UspQuery<AnnualLeavePlanStoredEntity>(new { AbsenceType = absenceType, NoReg = noReg });

            return results;
        }

        public EmployeeLeave GetLeave(string formKey, string noReg)
        {
            var objLeave = EmployeeLeaveRepository.Fetch()
                .Where(x => x.NoReg == noReg)
                .FirstOrDefaultIfEmpty();

            var data = from da in DocumentApprovalRepository.Fetch()
                       join dd in DocumentRequestDetailRepository.Fetch() on da.Id equals dd.DocumentApprovalId
                       join f in FormRepository.Fetch() on da.FormId equals f.Id
                       where (da.DocumentStatusCode == DocumentStatus.InProgress) && f.FormKey == formKey
                       select dd;

            var objLeaveOnprogress = data.Where(x => x.CreatedBy == noReg).Select(x => JsonConvert.DeserializeObject<AbsenceViewModel>(x.ObjectValue));

            if(objLeaveOnprogress != null)
            {
                var totalCuti = 0;
                var totalCutiPanjang = 0;

                foreach (var item in objLeaveOnprogress)
                {
                    if (item.ReasonType == "cuti")
                    {
                        totalCuti += int.Parse(item.TotalAbsence);
                    }

                    if (item.ReasonType == "cutipanjang")
                    {
                        totalCutiPanjang += int.Parse(item.TotalAbsence);
                    }
                }

                objLeave.AnnualLeave -= totalCuti;
                objLeave.LongLeave -= totalCutiPanjang;

            }

            return objLeave;
        }

        public IEnumerable<EmployeeWorkScheduleView> GetActiveWorkSchedule(string noreg, DateTime startDate, DateTime endDate)
        {
            var data = EmployeeWorkScheduleReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.Date >= startDate && x.Date <= endDate && x.ShiftCode != "OFF");

            return data;
        }

        public int GetStartAnnualLeaveNextYear(string noreg, DateTime startDate)
        {
            return EmployeeAnnualLeaveRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.Period == startDate.Year)
                .Select(x => (int?)x.AnnualLeave)
                .FirstOrDefault() ?? 0; 
        }

        public int GetEndAnnualLeaveNextYear(string noreg, DateTime endDate)
        {
            return EmployeeAnnualLeaveRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.Period == endDate.Year)
                .Select(x => (int?)x.AnnualLeave)
                .FirstOrDefault() ?? 0;
        }

        public IEnumerable<EmployeeWorkScheduleView> GetOffWorkSchedule(string noreg, DateTime startDate, DateTime endDate)
        {
            var data = EmployeeWorkScheduleReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.Date >= startDate && x.Date <= endDate && x.ShiftCode == "OFF");

            return data;
        }

        public int CountConsecutiveLeaveDays(ConsecutiveLeavePlanningViewModel vm)
        {
            List<DateTime> activeWorkSchedule = new List<DateTime>();
            string noReg = string.Empty;

            vm.Details.Add(vm.NewLeave);

            foreach (AnnualLeavePlanningDetailView detail in vm.Details.OrderBy(d => d.StartDate))
            {
                noReg = detail.NoReg;
                var employeeWorkSchedules = GetActiveWorkSchedule(detail.NoReg, detail.StartDate, detail.EndDate);
                foreach(EmployeeWorkScheduleView workSchedule in employeeWorkSchedules)
                {
                    activeWorkSchedule.Add(workSchedule.Date);
                }
            }

            int count = 0;
            List<int> consecutiveLeaveDaysList = new List<int>();
            DateTime lastDate = DateTime.Today;
            DateTime currentDate = DateTime.Today;

            for(int i = 0; i < activeWorkSchedule.Count(); i++)
            {
                currentDate = activeWorkSchedule[i];

                if(i != 0)
                {
                    lastDate = activeWorkSchedule[i - 1];
                    int offDaysBetweenCurrentAndLastLeaveDay = GetOffWorkSchedule(noReg, lastDate, currentDate).Count();
                    if ((currentDate - lastDate).Days - offDaysBetweenCurrentAndLastLeaveDay != 1)
                    {
                        consecutiveLeaveDaysList.Add(count);
                        count = 0;
                    }
                }
                count++;

                if(i == activeWorkSchedule.Count() - 1)
                {
                    consecutiveLeaveDaysList.Add(count);
                }
            }

            if (consecutiveLeaveDaysList.Count() > 0)
                count = consecutiveLeaveDaysList.Max();

            return count;
        }

        public int CountConsecutiveLeaveDays(List<AnnualLeavePlanningDetailView> details)
        {
            List<DateTime> activeWorkSchedule = new List<DateTime>();
            string noReg = string.Empty;

            foreach (AnnualLeavePlanningDetailView detail in details.OrderBy(d => d.StartDate))
            {
                noReg = detail.NoReg;
                var employeeWorkSchedules = GetActiveWorkSchedule(detail.NoReg, detail.StartDate, detail.EndDate);
                foreach (EmployeeWorkScheduleView workSchedule in employeeWorkSchedules)
                {
                    activeWorkSchedule.Add(workSchedule.Date);
                }
            }

            int count = 0;
            List<int> consecutiveLeaveDaysList = new List<int>();
            DateTime lastDate = DateTime.Today;
            DateTime currentDate = DateTime.Today;

            for (int i = 0; i < activeWorkSchedule.Count(); i++)
            {
                currentDate = activeWorkSchedule[i];

                if (i != 0)
                {
                    lastDate = activeWorkSchedule[i - 1];
                    int offDaysBetweenCurrentAndLastLeaveDay = GetOffWorkSchedule(noReg, lastDate, currentDate).Count();
                    if ((currentDate - lastDate).Days - offDaysBetweenCurrentAndLastLeaveDay != 1)
                    {
                        consecutiveLeaveDaysList.Add(count);
                        count = 0;
                    }
                }
                count++;

                if (i == activeWorkSchedule.Count() - 1)
                {
                    consecutiveLeaveDaysList.Add(count);
                }
            }

            if (consecutiveLeaveDaysList.Count() > 0)
                count = consecutiveLeaveDaysList.Max();

            return count;
        }

        #endregion
    }
}

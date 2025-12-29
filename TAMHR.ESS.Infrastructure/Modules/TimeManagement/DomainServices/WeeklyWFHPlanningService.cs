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
using TAMHR.ESS.Infrastructure.DomainEvents;
using System.Threading.Tasks;
using TAMHR.ESS.Domain.Models.TimeManagement;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.SqlClient;
using Scriban;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class WeeklyWFHPlanningService : DomainServiceBase
    {
        #region Repositories

        protected IRepository<DocumentApproval> DocumentApprovalRepository { get { return UnitOfWork.GetRepository<DocumentApproval>(); } }
        protected IRepository<WeeklyWFHPlanning> WeeklyWFHPlanningRepository { get { return UnitOfWork.GetRepository<WeeklyWFHPlanning>(); } }
        protected IRepository<WeeklyWFHPlanningDetail> WeeklyWFHPlanningDetailRepository { get { return UnitOfWork.GetRepository<WeeklyWFHPlanningDetail>(); } }
        protected IRepository<WeeklyWFHPlanningDetailView> WeeklyWFHPlanningDetailViewRepository { get { return UnitOfWork.GetRepository<WeeklyWFHPlanningDetailView>(); } }
        protected IReadonlyRepository<TimeManagement> TimeManagementReadonlyRepository { get { return UnitOfWork.GetRepository<TimeManagement>(); } }
        protected IRepository<ActualOrganizationStructure> ActualOrganizationStructureRepository { get { return UnitOfWork.GetRepository<ActualOrganizationStructure>(); } }
        protected IRepository<Config> ConfigRepository { get { return UnitOfWork.GetRepository<Config>(); } }
        protected IRepository<GeneralCategory> GeneralCategoryRepository { get { return UnitOfWork.GetRepository<GeneralCategory>(); } }

        protected readonly DomainEventManager DomainEventManager;
        protected IReadonlyRepository<ActualReportingStructureView> ActualReportingStructureReadonlyRepository => UnitOfWork.GetRepository<ActualReportingStructureView>();
        #endregion

        private IStringLocalizer<IUnitOfWork> _localizer;

        private static WorkScheduleService _workScheduleService;

        #region Constructor
        public WeeklyWFHPlanningService(IUnitOfWork unitOfWork, DomainEventManager domainEventManager, IStringLocalizer<IUnitOfWork> localizer = null, WorkScheduleService workScheduleService = null)
            : base(unitOfWork)
        {
            DomainEventManager = domainEventManager;
            _localizer = localizer;
            _workScheduleService = workScheduleService;
        }
        #endregion

        public IQueryable<WeeklyWFHPlanning> GetWeeklyWFHPlannings()
        {
            return WeeklyWFHPlanningRepository.Fetch()
                .AsNoTracking();
        }
        public IQueryable<WeeklyWFHPlanningDetail> GetWeeklyWFHPlanningDetails()
        {
            return WeeklyWFHPlanningDetailRepository.Fetch()
                .AsNoTracking();
        }
        public IQueryable<WeeklyWFHPlanning> GetWeeklyWFHPlanningByDocumentApprovalId(Guid documentApprovalId)
        {
            return GetWeeklyWFHPlannings().Where(x => x.DocumentApprovalId == documentApprovalId);
        }

        public WeeklyWFHPlanning GetWeeklyWFHPlanning(Guid id)
        {
            return GetWeeklyWFHPlannings().Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        public void UpsertWeeklyWFHPlanningRequest(Guid documentApprovalId, DocumentRequestDetailViewModel<WeeklyWFHPlanningViewModel> d, string action, ActualOrganizationStructure actualOrganizationStructure)
        {
            string objectValue = JsonConvert.SerializeObject(d.Object);
            WeeklyWFHPlanning weeklyWFHPlanning = d.Object.WeeklyWFHPlanning;
            List<WeeklyWFHPlanningDetailView> weeklyWFHPlanningDetails = d.Object.WeeklyWFHPlanningDetails;
            var dt = weeklyWFHPlanningDetails.Select(x => new
            {
                x.NoReg,
                x.WorkingDate,
                x.WorkPlace,
                x.IsOff
            });

            UnitOfWork.Transact(trans =>
            {
                //UnitOfWork.UspQuery("SP_UPSERT_WEEKLYWFHPLANNING_REQUEST", new { documentApprovalId = documentApprovalId, submitter = weeklyWFHPlanning.Submitter, startDate = weeklyWFHPlanning.StartDate, endDate = weeklyWFHPlanning.EndDate, data = dt.ConvertToDataTable().AsTableValuedParameter("TVP_WEEKLY_WFH_PLANNING_DETAIL"), ObjectValue = objectValue, action = action}, trans);
                UnitOfWork.UspQuery("SP_UPSERT_MONTHLYWFHPLANNING_REQUEST", new { documentApprovalId = documentApprovalId, submitter = weeklyWFHPlanning.Submitter, startDate = weeklyWFHPlanning.StartDate, endDate = weeklyWFHPlanning.EndDate, data = dt.ConvertToDataTable().AsTableValuedParameter("TVP_WEEKLY_WFH_PLANNING_DETAIL"), ObjectValue = objectValue, action = action }, trans);
                UnitOfWork.SaveChanges();
            });

            if (action.ToLower() == "edit")
            {
                var documentApproval = DocumentApprovalRepository.Fetch()
                    .AsNoTracking()
                    .Where(x => x.Id == documentApprovalId)
                    .Include(x => x.Form)
                    .FirstOrDefault();
                var needToRaiseEvent = !documentApproval.ParentId.HasValue;

                if (needToRaiseEvent)
                {
                    DomainEventManager.Raise(new DocumentApprovalEvent("initiate", documentApproval, actualOrganizationStructure, null));
                }
            }
        }
        public bool HasNewerVersion(Guid documentApprovalId)
        {
            bool hasNewerVersion = false;
            var firstVersion = WeeklyWFHPlanningRepository.Find(a => a.DocumentApprovalId == documentApprovalId && a.Version == 1).FirstOrDefault();
            if (firstVersion != null)
            {
                hasNewerVersion = WeeklyWFHPlanningRepository.Fetch()
                    .Where(a => a.Submitter == firstVersion.Submitter && a.StartDate == firstVersion.StartDate && a.EndDate == firstVersion.EndDate && a.Version > firstVersion.Version)
                    .Count() > 0;
            }
            return hasNewerVersion;
        }

        public IEnumerable<WeeklyWFHPlanningDetailView> GetDetails(Guid documentApprovalId, string noreg)
        {
            
            IEnumerable<WeeklyWFHPlanningDetailView> result = new List<WeeklyWFHPlanningDetailView>();

            int maxVersion = WeeklyWFHPlanningDetailViewRepository.Fetch()
                .Where(a => a.DocumentApprovalId == documentApprovalId)
                .OrderByDescending(a => a.Version)
                .Select(a => a.Version)
                .FirstOrDefault();

            result = WeeklyWFHPlanningDetailViewRepository.Fetch()
                .Where(a => a.DocumentApprovalId == documentApprovalId && a.Version == maxVersion)
                .OrderBy(a => a.Name)
                .OrderBy(a => a.WorkingDate);

            var getData = result.FirstOrDefault();

            var actualOrganization = ActualOrganizationStructureRepository.Fetch().FirstOrDefault(x => x.NoReg == getData.NoReg && x.Staffing == 100);


            var directSup = ActualReportingStructureReadonlyRepository.Fetch().FirstOrDefault(x => x.NoReg == actualOrganization.NoReg && x.PostCode==actualOrganization.PostCode && x.HierarchyLevel == 1);

            if (getData.NoReg != noreg && directSup.ParentNoReg != noreg)
            {
                throw new Exception("NoReg Login not same with result data");
            }

            return result;
        }

        public IEnumerable<WeeklyWFHPlanningDetailView> GetPreviousDetails(Guid documentApprovalId, string noreg)
        {
            IEnumerable<WeeklyWFHPlanningDetailView> result = new List<WeeklyWFHPlanningDetailView>();

            int maxVersion = WeeklyWFHPlanningDetailViewRepository.Fetch()
                .Where(a => a.DocumentApprovalId == documentApprovalId)
                .OrderByDescending(a => a.Version)
                .Select(a => a.Version)
                .FirstOrDefault();

            result = WeeklyWFHPlanningDetailViewRepository.Fetch()
                .Where(a => a.DocumentApprovalId == documentApprovalId && a.Version == maxVersion - 1)
                .OrderBy(a => a.Name)
                .OrderBy(a => a.WorkingDate);

            var getData = result.FirstOrDefault();

            var actualOrganization = ActualOrganizationStructureRepository.Fetch().FirstOrDefault(x => x.NoReg == getData.NoReg && x.Staffing == 100);

            var directSup = ActualReportingStructureReadonlyRepository.Fetch().FirstOrDefault(x => x.NoReg == actualOrganization.NoReg && x.PostCode == actualOrganization.PostCode && x.HierarchyLevel == 1);

            if (getData.NoReg != noreg && directSup.ParentNoReg != noreg)
            {
                throw new Exception("NoReg Login not same with result data");
            }
            return result;
        }

        public IEnumerable<WeeklyWFHPlanningDetailView> GetWFHPlanningByNoReg(string Superior, string NoReg, DateTime StartDate, DateTime EndDate)
        {
            IEnumerable<WeeklyWFHPlanningDetailView> result = new List<WeeklyWFHPlanningDetailView>();

            result = WeeklyWFHPlanningDetailViewRepository.Fetch()
                .Where(a => a.NoReg == NoReg && a.StartDate == StartDate && a.EndDate == EndDate && a.CreatedBy != Superior && (a.DocumentStatusCode == "inprogress" || a.DocumentStatusCode == "completed"))
                .OrderBy(a => a.Name)
                .OrderBy(a => a.StartDate);

            return result;
        }
        public IEnumerable<SubordinateStroredEntity> GetManPower(string noReg)
        {
            IEnumerable<SubordinateStroredEntity> result = new List<SubordinateStroredEntity>();
            ActualOrganizationStructure aos = ActualOrganizationStructureRepository.Fetch().Where(a => a.NoReg == noReg).FirstOrDefault();

            if (aos != null)
                result = UnitOfWork.UdfQuery<SubordinateStroredEntity>(new { OrgCode = aos.OrgCode, OrgLevel = aos.OrgLevel });

            return result;
        }

        public WFHGeneratePlanningDateWeekly GetWeeklyWFHPlanningByDate(DateTime date, int gap, bool isReport)
        {
            return UnitOfWork.UspQuery<WFHGeneratePlanningDateWeekly>(new { Date = date, gap = gap, IsReport = isReport }).FirstOrDefault();
        }

        public WFHGeneratePlanningDateMonthly GetMonthlyWFHPlanningByDate(DateTime date)
        {
            return UnitOfWork.UspQuery<WFHGeneratePlanningDateMonthly>(new { Date = date }).FirstOrDefault();
        }

        public WeeklyWFHPlanning GetByKey(string noReg, string postCode, DateTime startDate, DateTime endDate)
        {
            var result = UnitOfWork.GetConnection().Query<WeeklyWFHPlanning>(@"
            SELECT AWP.DocumentApprovalId FROM TB_R_WEEKLY_WFH_PLANNING AWP INNER JOIN TB_R_WEEKLY_WFH_PLANNING_DETAIL AWPD
            ON AWP.Id=AWPD.WeeklyWFHPlanningId
            INNER JOIN TB_R_DOCUMENT_APPROVAL da ON da.ID = AWP.DocumentApprovalId
            WHERE AWP.Submitter = @NoReg
            AND (da.DocumentStatusCode='inprogress' OR da.DocumentStatusCode='completed' OR da.DocumentStatusCode='draft')
            AND AWP.startDate BETWEEN @StartDate
            AND @EndDate
            GROUP BY AWP.DocumentApprovalId
            ", new { NoReg = noReg, PostCode = postCode, StartDate = startDate, EndDate = endDate });

            return result.FirstOrDefault();
        }

        public List<Vaccine> GetClass()
        {
            return UnitOfWork.GetConnection().Query<Vaccine>(@"
            select DISTINCT NK_SubKelas as Class from vw_personal_data_information Order By NK_SubKelas
            ").ToList();
        }

        public string GetDivisionByNoReg(string NoReg)
        {
            return UnitOfWork.GetConnection().Query<string>(@"
            select ObjectCode from SF_GET_EMPLOYEE_DIVISION_BY_NOREG (@NoReg)
            ", new { NoReg }).FirstOrDefault();
        }

        public IEnumerable<UserPositionView> GetUserReminderWeeklyWFHPlanning(DateTime startDate, DateTime endDate)
        {
            var result = UnitOfWork.GetConnection().Query<UserPositionView>(@"
            exec SP_GET_USER_REMINDER_WEEKLY_WFH_PLANNING @StartDate,@EndDate 
            ", new { StartDate = startDate, EndDate = endDate });

            return result.ToList();
        }

        public IEnumerable<WeeklyWFHPlanningUserSummaryStoredEntity> GetUserReminderSummaryWeeklyWFHPlanning(DateTime startDate, DateTime endDate)
        {
            var result = UnitOfWork.GetConnection().Query<WeeklyWFHPlanningUserSummaryStoredEntity>(@"
            exec [dbo].[SP_GET_WEEKLY_WFH_EMAIL_SUMMARY] @StartDate,@EndDate 
            ", new { StartDate = startDate, EndDate = endDate });

            return result.ToList();
        }

        public class WeekCount
        {
            public decimal Week { get; set; }
            public decimal TotalCount { get; set; }
        }
        public string WFHValidation(List<WeeklyWFHPlanningDetailView> WeeklyWFHPlanningDetails)
        {
            var output = "";

            var minWFOCount = ConfigRepository.Fetch().AsNoTracking().Where(wh => wh.ConfigKey == "WeeklyWFHPlanning.MinWFOCount").FirstOrDefault().ConfigValue;

            decimal wfoCount = 0;
            decimal absCount = 0;
            decimal exceptAbsOFFCount = 0;
            decimal temp = 0;
            decimal tempAbs = 0;
            decimal tempExceptAbs = 0;
            var listWfoCount = new List<WeekCount>();
            var listAbsCount = new List<WeekCount>();
            var listexceptAbsOFFCount = new List<WeekCount>();

            decimal index = 0;

            var dataWorkPlace = GeneralCategoryRepository.Fetch().AsNoTracking().Where(wh => wh.Category == "HybridSchedule").ToList();
            var dataOFF = new GeneralCategory();
            dataOFF.Name = "OFF";
            dataWorkPlace.Add(dataOFF);
            bool firstMon = false;
            foreach (var data in WeeklyWFHPlanningDetails)
            {
                //cari hari senin (special case januari)
                if(data.WorkingDate.ToString("ddd") == "Mon")
                {
                    firstMon = true;
                }

                if (firstMon)
                {
                    //check if WorkPlace not in general category
                    if (dataWorkPlace.Where(wh => wh.Name == data.WorkPlace).Count() == 0)
                    {
                        output += _localizer["Workplace in " + data.WorkingDate.ToString("dd-MMM-yyyy")].Value + " not defined \n";
                    }

                    var tempDate = data.WorkingDate;
                    var week = Math.Floor(index / 7) + 1;
                    if (data.WorkPlace == "WFO" || data.WorkPlace == "WFF" || data.WorkPlace == "WFH")
                    {
                        exceptAbsOFFCount += 1;

                        if (tempExceptAbs != week && week != 1)
                        {
                            tempExceptAbs = week;
                            exceptAbsOFFCount = 1;
                        }

                        var exceptData = new WeekCount();
                        exceptData.Week = week;
                        exceptData.TotalCount = exceptAbsOFFCount;

                        listexceptAbsOFFCount.Add(exceptData);
                    }

                    //if (data.WorkPlace == "WFO" || data.WorkPlace == "WFF")
                    string[] arrWFO = { "WFO", "WFF" };
                    string[] arrWeekend = { "Saturday", "Monday" };
                    if (arrWFO.Contains(data.WorkPlace) || data.WorkPlace == "ABS" 
                        || (data.WorkPlace == "OFF" && data.RequireWFO) || (arrWeekend.Contains(data.WorkingDate.DayOfWeek.ToString()) && data.WorkPlace!="OFF"))
                    //(absCount > 3 && exceptAbsOFFCount < 3)) 
                    {
                        wfoCount += 1;

                        if (temp != week && week != 1)
                        {
                            temp = week;
                            wfoCount = 1;
                        }

                        var wfoData = new WeekCount();
                        wfoData.Week = week;
                        wfoData.TotalCount = wfoCount;

                        listWfoCount.Add(wfoData);
                    }

                    if (data.WorkPlace == "ABS" || (data.WorkPlace == "OFF" && !data.RequireWFO))
                    {
                        absCount += 1;
                        //alert(tempItem.WorkPlace);
                        if (tempAbs != week && week != 1)
                        {
                            tempAbs = week;
                            absCount = 1;
                        }

                        var absData = new WeekCount();
                        absData.Week = week;
                        absData.TotalCount = absCount;

                        listAbsCount.Add(absData);

                    }

                    index++;
                }
                
            }

            //int wfoCount = WeeklyWFHPlanningDetails.Where(wh => wh.WorkPlace == "WFO" || wh.WorkPlace == "WFF").Count();
            //int absCount = WeeklyWFHPlanningDetails.Where(wh => wh.WorkPlace == "ABS" || wh.WorkPlace == "OFF").Count();

            
            var totalAbsList = listAbsCount.Where(x => x.TotalCount == 5).Count();

            //cari maksimal wfo per minggu
            var totalWFOGroup = listWfoCount.GroupBy(gb => gb.Week).Select(sel => new
            {
                Week = sel.Key,
                TotalCount = sel.Max(m => m.TotalCount),
            }).ToList();

            //cari maksimal off per minggu
            var totalOffAbs2Up = listAbsCount.GroupBy(gb => gb.Week).Select(sel => new
            {
                Week = sel.Key,
                TotalCount = sel.Max(m => m.TotalCount),
            }).ToList();

            int i = 0;
            foreach (var dataWfo in listWfoCount)
            {
                foreach(var dataWfoGroup in totalWFOGroup)
                {
                    //cari total paling besar
                    if(dataWfo.Week == dataWfoGroup.Week && dataWfo.TotalCount == dataWfoGroup.TotalCount && dataWfo.TotalCount < 3)
                    {
                        var checkOffAbse2Up = totalOffAbs2Up.Where(wh => wh.Week == dataWfo.Week).FirstOrDefault();
                        if (checkOffAbse2Up != null) {
                            //cek total off per minggu, jika lebih dari 2 hari maka hari selanjutnya dianggap WFO
                            if (checkOffAbse2Up.TotalCount > 2)
                            {
                                var overloadOff = checkOffAbse2Up.TotalCount - 2;
                                listWfoCount[i].TotalCount += overloadOff;
                                if (listWfoCount[i].TotalCount > 3)
                                {
                                    listWfoCount[i].TotalCount = 3;
                                }
                            }
                        }
                        //foreach (var dataOffAbs2Up in totalOffAbs2Up)
                        //{
                        //    if (dataOffAbs2Up.)
                        //}
                    }
                }
                i++;
            }

            var totalWFOList = listWfoCount.Where(x => x.TotalCount == Convert.ToDecimal(minWFOCount)).Count();


            //var totalAbsList = listAbsCount.Where(x => x.TotalCount == 4).Count();
            var totalListExcept = listexceptAbsOFFCount.Where(x => x.TotalCount == 3).Count();

            var minWeek = 4;
            var startDate = WeeklyWFHPlanningDetails.Min(mn => mn.WorkingDate);
            var endDate = WeeklyWFHPlanningDetails.Max(mn => mn.WorkingDate);
            //var lastDateOfMonth = new DateTime.DaysInMonth(startDate.Year, startDate.Month);
            var lastDateOfMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);

            //var lastWorkingSchedule = WeeklyWFHPlanningDetails.Max(mx => mx.WorkingDate);
            
            var lastWorkingSchedule = _workScheduleService.GetLatestWorkSchedules().OrderByDescending(x => x.Date).FirstOrDefault().Date;

            //if (((startDate.Day == 3 && lastDateOfMonth > 30) ||
            //    (startDate.Day == 2 && lastDateOfMonth > 29) ||
            //    (startDate.Day == 1 && lastDateOfMonth > 28 && startDate.Month!=1)) && (endDate < lastWorkingSchedule))
            //{
            //    minWeek = 5;
            //}
            // alert("minWeek before : " + minWeek);
            // alert("minWeek : " + minWeek);
            //if (totalAbsList > 0 && totalListExcept != minWeek)
            //{
            //    minWeek = minWeek - totalAbsList;
            //}

            if (totalWFOList < minWeek)
            {
                output += _localizer["Total WFO / WFF Day minimum each Week is"].Value + " " + minWFOCount;// + "[totalWFOList=" + totalWFOList+ "|minWeek=" + minWeek + "]";
            }

                return output;
        }

        public ActualReportingStructureView GetActualReportingStructure(string noreg, string postCode)
        {
            return ActualReportingStructureReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.PostCode == postCode && x.HierarchyLevel == 1).FirstOrDefault();
        }

    }
}
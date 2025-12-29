using System;
using System.Linq;
using TAMHR.ESS.Domain;
using Agit.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Z.EntityFramework.Plus;
using Newtonsoft.Json;
using TAMHR.ESS.Infrastructure.ViewModels;
using Agit.Common.Extensions;
using System.Collections.Generic;
using Agit.Domain.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.EntityFrameworkCore;
using Agit.Common.Utility;
using Agit.Common;
using System.Threading.Tasks;
using TAMHR.ESS.Domain.Models.TimeManagement;
//using Remotion.Linq.Clauses;
using System.Diagnostics;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    public class TimeManagementService : DomainServiceBase
    {
        #region Repositories
        /// <summary>
        /// Employee work schedule readonly repository
        /// </summary>
        protected IReadonlyRepository<EmployeeWorkScheduleView> EmployeeWorkScheduleReadonlyRepository => UnitOfWork.GetRepository<EmployeeWorkScheduleView>();

        /// <summary>
        /// Employee Absence readonly repository
        /// </summary>
        protected IReadonlyRepository<EmployeeAbsenceView> EmployeeAbsenceReadonlyRepository => UnitOfWork.GetRepository<EmployeeAbsenceView>();

        /// <summary>
        /// Document approval repository
        /// </summary>
        protected IRepository<DocumentApproval> DocumentApprovalRepository => UnitOfWork.GetRepository<DocumentApproval>();

        /// <summary>
        /// Document approval file repository
        /// </summary>
        protected IRepository<DocumentApprovalFile> DocumentApprovalFileRepository => UnitOfWork.GetRepository<DocumentApprovalFile>();

        /// <summary>
        /// Document request detail repository
        /// </summary>
        protected IRepository<DocumentRequestDetail> DocumentRequestDetailRepository => UnitOfWork.GetRepository<DocumentRequestDetail>();

        /// <summary>
        /// Form repository
        /// </summary>
        protected IRepository<Form> FormRepository => UnitOfWork.GetRepository<Form>();

        /// <summary>
        /// Actual organization structure repository
        /// </summary>
        protected IRepository<ActualOrganizationStructure> ActualOrganizationStructureRepository => UnitOfWork.GetRepository<ActualOrganizationStructure>();

        /// <summary>
        /// Absence repository
        /// </summary>
        protected IRepository<Absence> AbsenceRepository => UnitOfWork.GetRepository<Absence>();

        /// <summary>
        /// SAP general category repository
        /// </summary>
        protected IRepository<SapGeneralCategoryMap> SapGeneralCategoryMapRepository => UnitOfWork.GetRepository<SapGeneralCategoryMap>();

        /// <summary>
        /// Employee absent repository
        /// </summary>
        protected IRepository<EmployeeAbsent> EmployeeAbsentRepository => UnitOfWork.GetRepository<EmployeeAbsent>();

        /// <summary>
        /// Personal data repository
        /// </summary>
        protected IRepository<PersonalData> PersonalDataRepository => UnitOfWork.GetRepository<PersonalData>();

        /// <summary>
        /// Personal data common attribute repository
        /// </summary>
        protected IRepository<PersonalDataCommonAttribute> PersonalDataCommonAttributeRepository => UnitOfWork.GetRepository<PersonalDataCommonAttribute>();

        /// <summary>
        /// Employee work schedule repository
        /// </summary>
        protected IRepository<EmployeeWorkSchedule> EmployeeWorkScheduleRepository => UnitOfWork.GetRepository<EmployeeWorkSchedule>();

        /// <summary>
        /// Work schedule repository
        /// </summary>
        protected IRepository<WorkSchedule> WorkScheduleRepository => UnitOfWork.GetRepository<WorkSchedule>();

        /// <summary>
        /// Daily work schedule repository
        /// </summary>
        protected IRepository<DailyWorkSchedule> DailyWorkScheduleRepository => UnitOfWork.GetRepository<DailyWorkSchedule>();

        /// <summary>
        /// Form validation matrix repository
        /// </summary>
        protected IRepository<FormValidationMatrix> FormValidationMatrixRepository => UnitOfWork.GetRepository<FormValidationMatrix>();

        /// <summary>
        /// Job chief repository
        /// </summary>
        protected IRepository<JobChief> JobChiefRepository => UnitOfWork.GetRepository<JobChief>();

        /// <summary>
        /// Employee work schedule subtitute repository
        /// </summary>
        protected IRepository<EmpWorkSchSubtitute> EmpWorkSchSubtituteRepository => UnitOfWork.GetRepository<EmpWorkSchSubtitute>();

        /// <summary>
        /// BDJK repository
        /// </summary>
        protected IRepository<BDJK> BDJKRepository => UnitOfWork.GetRepository<BDJK>();

        /// <summary>
        /// Employee leave repository
        /// </summary>
        protected IRepository<EmployeeLeave> EmployeeLeaveRepository => UnitOfWork.GetRepository<EmployeeLeave>();

        /// <summary>
        /// User repository
        /// </summary>
        protected IRepository<User> UserRepository => UnitOfWork.GetRepository<User>();

        /// <summary>
        /// Employee subgroup repository
        /// </summary>
        protected IRepository<EmployeeSubgroupNP> EmployeeSubgroupNPRepository => UnitOfWork.GetRepository<EmployeeSubgroupNP>();

        /// <summary>
        /// Allowance repository
        /// </summary>
        protected IRepository<AllowanceDetail> AllowanceDetailRepository => UnitOfWork.GetRepository<AllowanceDetail>();

        /// <summary>
        /// General category repository
        /// </summary>
        protected IRepository<GeneralCategory> GeneralCategoryRepository => UnitOfWork.GetRepository<GeneralCategory>();

        /// <summary>
        /// Tracking approval repository
        /// </summary>
        protected IRepository<TrackingApproval> TrackingApprovalRepository => UnitOfWork.GetRepository<TrackingApproval>();

        /// <summary>
        /// Leave Mapping repository (mapping between actual leave vs annual leave plan)
        /// </summary>
        protected IRepository<LeaveMapping> LeaveMappingRepository => UnitOfWork.GetRepository<LeaveMapping>();

        protected IRepository<EmployeeLeaveHistorys> EmployeeLeaveHistorys => UnitOfWork.GetRepository<EmployeeLeaveHistorys>();
        protected IRepository<SpklRequest> SpklRequestRepository => UnitOfWork.GetRepository<SpklRequest>();

        #endregion

        private IStringLocalizer<IUnitOfWork> _localizer;

        #region Constructor
        public TimeManagementService(IUnitOfWork unitOfWork, IStringLocalizer<IUnitOfWork> localizer)
            : base(unitOfWork)
        {
            _localizer = localizer;
        }
        #endregion

        #region ShiftPlanning
        public List<ShiftPlanningReportViewModel> GetListShiftPlanningkReport(string noreg, string approver)
        {
            //var dataRequester = (from dad in documentRequestDetailRepository.Fetch()
            //            join da in documentApprovalRepository.Fetch() on dad.DocumentApprovalId equals da.Id
            //            join f in FormRepository.Fetch().Where(x => x.FormKey == "shift-planning") on da.FormId equals f.Id
            //            join ao in ActualOrganizationStructureRepository.Fetch().Where(x => x.Staffing == 100) on da.CreatedBy equals ao.NoReg
            //            where JsonConvert.DeserializeObject<ShiftPlanningViewModel>(dad.ObjectValue).Request.Where(x => x.noreg == noreg).Count() > 0
            //            select new ShiftPlanningReportViewModel()
            //            {
            //                Id = da.Id,
            //                DocumentNumber = da.DocumentNumber,
            //                DocumentStatusCode = da.DocumentStatusCode,
            //                CreatedBy = ao.Name,
            //                CreatedOn = da.CreatedOn.Date,
            //                Approver = da.CurrentApprover
            //            }).ToList();


            var data = (from da in DocumentApprovalRepository.Fetch()
                        from dad in DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == da.Id).DefaultIfEmpty()
                        join f in FormRepository.Fetch().Where(x => x.FormKey == "shift-planning")
                        on da.FormId equals f.Id
                        join ao in ActualOrganizationStructureRepository.Fetch().Where(x => x.Staffing == 100)
                        on da.CreatedBy equals ao.NoReg
                        where
                        ((((da.DocumentStatusCode == "completed" || da.DocumentStatusCode == "rejected") && da.LastApprovedBy == noreg) ||
                        (da.CurrentApprover.Contains("(" + approver + ")") && da.DocumentStatusCode == "inprogress") || da.CreatedBy == noreg)
                         && da.RowStatus)
                        select new ShiftPlanningReportViewModel()
                        {
                            Id = da.Id,
                            DocumentNumber = da.DocumentNumber,
                            DocumentStatusCode = da.DocumentStatusCode,
                            CreatedBy = ao.Name,
                            CreatedOn = da.CreatedOn.Date,
                            Approver = da.CurrentApprover
                        }).ToList();

            //data.AddRange(dataRequester);
            //var dataResult = data.Distinct().ToList();

            return data;
        }

        public List<ShiftPlanningReportViewModel> GetListShiftPlanningkNonChiefReport(string noreg)
        {
            var data = (from dad in DocumentRequestDetailRepository.Fetch()
                        join da in DocumentApprovalRepository.Fetch() on dad.DocumentApprovalId equals da.Id
                        join f in FormRepository.Fetch().Where(x => x.FormKey == "shift-planning") on da.FormId equals f.Id
                        where da.DocumentStatusCode == DocumentStatus.Completed && JsonConvert.DeserializeObject<ShiftPlanningViewModel>(dad.ObjectValue).Request.Where(x => x.noreg == noreg).Count() > 0
                        select new
                        {
                            dataShift = new ShiftPlanningReportViewModel()
                            {
                                Id = da.Id,
                                DocumentNumber = da.DocumentNumber,
                                DocumentStatusCode = da.DocumentStatusCode,
                                CreatedOn = da.CreatedOn.Date,
                                Approver = da.CurrentApprover
                            },
                            dataRequest = JsonConvert.DeserializeObject<ShiftPlanningViewModel>(dad.ObjectValue).Request.Where(x => x.noreg == noreg),
                        });

            List<ShiftPlanningReportViewModel> retData = new List<ShiftPlanningReportViewModel>();
            foreach (var item in data)
            {
                foreach (var req in item.dataRequest)
                {
                    retData.Add(new ShiftPlanningReportViewModel()
                    {
                        Id = item.dataShift.Id,
                        DocumentNumber = item.dataShift.DocumentNumber,
                        DocumentStatusCode = item.dataShift.DocumentStatusCode,
                        ShiftCode = req.shift,
                        DateShift = req.date.Date,
                        CreatedOn = req.date.Date,
                        Approver = item.dataShift.Approver
                    });
                }
            }

            return retData.OrderBy(x => x.CreatedOn).ToList();
        }
        #endregion

        #region Absence
        public IQueryable<AbsenceReportViewModel> GetListAbsenceReport(string noreg, string approver)
        {
            var data = from da in DocumentApprovalRepository.Fetch()
                       join dd in DocumentRequestDetailRepository.Fetch()
                       on da.Id equals dd.DocumentApprovalId
                       join f in FormRepository.Fetch()
                       on da.FormId equals f.Id
                       join ao in ActualOrganizationStructureRepository.Fetch().Where(x => x.Staffing == 100)
                       on da.CreatedBy equals ao.NoReg
                       where f.FormKey == "absence" &&
                       (((da.DocumentStatusCode == "completed" || da.DocumentStatusCode == "rejected") && da.LastApprovedBy == noreg) ||
                       (da.CurrentApprover.Contains("(" + approver + ")") && da.DocumentStatusCode == "inprogress") || da.CreatedBy == noreg)
                        && da.RowStatus
                       select new AbsenceReportViewModel()
                       {
                           Id = da.Id,
                           Noreg = da.CreatedBy,
                           Name = ao.Name,
                           Reason = JsonConvert.DeserializeObject<AbsenceViewModel>(dd.ObjectValue).Reason,
                           StartDate = JsonConvert.DeserializeObject<AbsenceViewModel>(dd.ObjectValue).StartDate,
                           EndDate = JsonConvert.DeserializeObject<AbsenceViewModel>(dd.ObjectValue).EndDate,
                           Status = da.DocumentStatusCode,
                           Date = da.CreatedOn,
                           Approver = da.CurrentApprover
                       };

            return data;
        }


        public string GetAbsenceCode(Guid id)
        {
            var data = AbsenceRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == id).Select(x => x.Code).FirstOrDefault();

            return data;
        }

        public IEnumerable<EmployeeAbsenceView> GetListAbsenceByPeriod(string noreg, int period)
        {
            var data = EmployeeAbsenceReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.StartDate.Year == period);

            return data;
        }

        public IEnumerable<EmployeeWorkScheduleView> GetListWorkSchEmp(string noreg, DateTime startDate, DateTime endDate)
        {
            var data = EmployeeWorkScheduleReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.Date >= startDate && x.Date <= endDate);

            return data;
        }

        public DateTime GetAbsenceEndDate(string noreg, DateTime startDate, int maxAbsentDays, int initialCounter = 0, int repetition = 1)
        {
            // Set max repetition (one month).
            if (repetition > 6) return startDate;

            // Set max offset (ten days).
            var maxOffset = 10;
            var outputEndDate = startDate.AddDays(maxAbsentDays - 1);
            var endDate = startDate.AddDays(maxOffset);
            var output = maxAbsentDays;

            var workSchedules = EmployeeWorkScheduleReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.Date >= startDate && x.Date <= endDate)
                .OrderBy(x => x.Date);

            var counter = initialCounter;

            foreach (var workSchedule in workSchedules)
            {
                if (workSchedule.Off) continue;

                if (++counter >= maxAbsentDays)
                {
                    outputEndDate = workSchedule.Date;
                    break;
                }
            }

            if (counter < maxAbsentDays)
            {
                var lastDate = workSchedules.LastOrDefault();

                if (lastDate != null)
                {
                    return GetAbsenceEndDate(noreg, lastDate.Date.AddDays(1), maxAbsentDays, counter, repetition + 1);
                }
            }

            return outputEndDate;
        }

        public int GetListOffWorkSchEmp(string noreg, DateTime startDate, DateTime endDate)
        {
            var data = EmployeeWorkScheduleReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.Date >= startDate && x.Date <= endDate && ((x.ShiftCode == "OFF" || x.ShiftCode == "OFFS") || (x.Holiday && !x.ChangeShift)))
                .Count();

            return data;
        }
        #endregion


        public void PreValidateGenderMan(string noreg)
        {
            var message = string.Empty;

            var data = from pd in PersonalDataRepository.Fetch().AsNoTracking()
                       join ca in PersonalDataCommonAttributeRepository.Fetch().AsNoTracking()
                       on pd.CommonAttributeId equals ca.Id
                       where pd.NoReg == noreg
                       select new
                       {
                           ca.GenderCode
                       };

            if (data.FirstOrDefault().GenderCode == "lakilaki")
            {
                message = "Your gender is Male, cannot create Maternity Leave";
            }

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }
        public bool PreValidateMarried(string noreg)
        {
            var message = string.Empty;



            var data = from pd in PersonalDataRepository.Fetch().AsNoTracking()
                       join ca in PersonalDataCommonAttributeRepository.Fetch().AsNoTracking()
                       on pd.CommonAttributeId equals ca.Id
                       where pd.NoReg == noreg
                       select new
                       {
                           pd.MaritalStatusCode
                       };

            if (data.FirstOrDefault().MaritalStatusCode != "menikah")
            {
                return false;
                //throw new Exception("You Dont have Access, Please Update Your Marriage Status");
               
            }
            return true;
            //throw new Exception(message);
        }

        public void PreValidateMaternityLeave(string noreg)
        {
            var message = string.Empty;
            //data
            var dataTax = (from da in DocumentApprovalRepository.Fetch().AsNoTracking()
                           join f in FormRepository.Fetch().AsNoTracking()
                           on da.FormId equals f.Id
                           where da.RowStatus && da.CreatedBy == noreg && (da.DocumentStatusCode == "draft" || da.DocumentStatusCode == "inprogress") &&
                           f.FormKey == "maternity-leave"
                           select da.Id).Count();

            if (dataTax > 0)
            {
                message = _localizer["Maternity Leave Request must be completed before create request."].Value;
            }

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        public void PreValidateShiftPlanning(string noreg, string username, string formkey, string postCode)
        {
            var message = string.Empty;

            var jobCode = ActualOrganizationStructureRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.NoReg == noreg && x.PostCode == postCode)
                ?.JobCode;

            var dataMatrix = (from fm in FormValidationMatrixRepository.Fetch().AsNoTracking()
                              join f in FormRepository.Fetch() on fm.FormId equals f.Id
                              where f.FormKey == formkey
                              select fm).FirstOrDefault();

            if (dataMatrix != null)
            {
                var data = JobChiefRepository.Find(x => x.JobFamily.ToLower() == dataMatrix.AdditionalFlag.ToLower() && x.JobCode == jobCode);

                if (data.Count() == 0)
                {
                    message = $"User {username} is not Section Head above, cannot create Shift Planning.";
                }
            }

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        public void PreValidateAbsence(string noreg, DateTime strDate, DateTime endDate)
        {
            var message = string.Empty;

            var ListEmpAbsence = EmployeeAbsentRepository.Find(x => x.NoReg == noreg);
            foreach (var absence in ListEmpAbsence)
            {
                if (absence.StartDate <= endDate && absence.EndDate >= strDate)
                {
                    message = $"Date is allready claimed.";
                    break;
                }
            }

            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        public int PreValidateLimitCrossYear(string noreg)
        {
            var currentYear = DateTime.Now.Year;

                var totalAbsence = EmployeeAbsentRepository.Fetch()
                    .AsNoTracking()
                    .Where(x => x.NoReg == noreg &&
                                (x.StartDate.Year == currentYear + 1 || x.EndDate.Year == currentYear + 1) &&
                                x.ReasonCode == "p-CutiYearly")
                    .Sum(x => (int?)x.AbsentDuration) ?? 0;

                return totalAbsence; // Return the sum of AbsentDuration
        }


        public void PreValidateAbsentLimit(string noreg, DateTime strDate, DateTime endDate)
        {
            var message = string.Empty;
            var currentYear = DateTime.Now.Year;

            var overtimeId = SpklRequestRepository
                .Find(x => x.NoReg == noreg &&
                           x.OvertimeDate == strDate.Date &&
                           x.DocumentApprovalId != null)
                .Select(x => x.DocumentApprovalId)
                .FirstOrDefault(); 

            DocumentApproval docCheck = null; 

            if (overtimeId != null) 
            {
                var validStatuses = new HashSet<string> { "completed", "inprogress" };

                docCheck = DocumentApprovalRepository
                    .Fetch()
                    .AsNoTracking()
                    .Where(x => x.Id == overtimeId && validStatuses.Contains(x.DocumentStatusCode))
                    .FirstOrDefault(); 
            }

            if (docCheck != null) 
            {
                var overtimeDates = SpklRequestRepository
                    .Find(x => x.NoReg == noreg &&
                               x.OvertimeDate >= strDate.Date &&
                               x.OvertimeDate <= endDate.Date &&
                               x.DocumentApprovalId != null)
                    .Select(x => x.OvertimeDate)
                    .ToList();

                //message = $"You have an overtime schedule on {overtimeDates:dd-MM-yyyy}.";
                //message = $"<b>You have been assigned to work overtime by your supervisor on {overtimeDates:dd-MM-yyyy}.</b>\nIf you need to cancel or make any changes to this overtime assignment, \nplease contact your supervisor directly to request the cancellation.";
                if (overtimeDates.Any())
                {
                    var formattedDates = string.Join(", ", overtimeDates.Select(d => d.ToString("dd-MM-yyyy")));
                    message = $"<div style='font-weight: bold; font-size: 16px;'>You have been assigned to work overtime by your supervisor on {formattedDates}.</div>" +
                                "<div style='font-size: 16px;'>If you need to cancel or make any changes to the overtime schedule, " +
                                "please contact your supervisor directly to request the cancellation.</div>";
                }


                //int strDateInt = Convert.ToInt32(strDate.ToString("yyyyMMdd"));
                //int endDateInt = Convert.ToInt32(endDate.ToString("yyyyMMdd"));

                //var documentRequestDetails = DocumentRequestDetailRepository.Fetch()
                //                                 .Where(ab => noreg.Equals(ab.CreatedBy))
                //                                 .ToList();

                //var leaveStatus = new HashSet<string> { "Cuti Tahunan", "Cuti Panjang / Besar" };

                //var existingRecord = (from x in documentRequestDetails
                //                      orderby x.CreatedOn descending
                //                      let objJson = x.ObjectValue
                //                      let obj = !string.IsNullOrEmpty(objJson) && !objJson.StartsWith("[")
                //                          ? JsonConvert.DeserializeObject<AbsenceViewModel>(objJson)
                //                          : null
                //                      where obj != null &&
                //                             obj.StartDate.HasValue &&
                //                             obj.EndDate.HasValue &&
                //                            leaveStatus.Contains(obj.Reason) &&
                //                            Convert.ToInt32(obj.StartDate.Value.ToString("yyyyMMdd")) <= endDateInt &&
                //                            Convert.ToInt32(obj.EndDate.Value.ToString("yyyyMMdd")) >= strDateInt && noreg.Equals(x.CreatedBy)
                //                      select new { x.DocumentApprovalId,
                //                          obj.Reason,
                //                          obj.StartDate,
                //                          obj.EndDate
                //                      }).FirstOrDefault();

                //var a = existingRecord.DocumentApprovalId;
                //var b = existingRecord.StartDate;


                //if (existingRecord != null)
                //{
                //    Guid idApproval = existingRecord.DocumentApprovalId;

                //    var documentApprovalExists = DocumentApprovalRepository
                //        .Find(x => x.Id == idApproval ) 
                //        //&& x.DocumentStatusCode == "draft")
                //        .Any();


            }


            if (!string.IsNullOrEmpty(message))
            {
                throw new Exception(message);
            }
        }

        public string calculateDuration(string OTin, string OTout)
        {
            if (string.IsNullOrEmpty(OTin) || string.IsNullOrEmpty(OTout) || OTin == "-" || OTout == "-")
            {
                return 0.ToString();
            }

            string result;

            TimeSpan dt = Convert.ToDateTime(OTout) - Convert.ToDateTime(OTin);
            int minutes = (int)dt.TotalMinutes;

            double dur = minutes;

            double dur1 = Math.Floor(dur / 60);
            double durationSisa = dur - (dur1 * 60);
            double dur2 = 0;
            if (durationSisa >= 0 && durationSisa < 30)
            {
                dur2 = 0;
            }
            else if (durationSisa >= 30 && durationSisa < 60)
            {
                dur2 = 0.5;
            }
            result = (dur1 + dur2).ToString();

            return result;
        }

        #region completeaction
        public void CompleteAbsence(string noregCurentApprover, DocumentApproval documentApproval)
        {
            var actualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Find(x => x.NoReg == documentApproval.CreatedBy).FirstOrDefaultIfEmpty();
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            var obj = JsonConvert.DeserializeObject<AbsenceViewModel>(documentRequestDetail.ObjectValue);
            var dataAbsenece = AbsenceRepository.Find(x => x.Id == obj.ReasonId).FirstOrDefault();

            var currentYear = DateTime.Now.Year;

            var workSchedule = GetListWorkSchEmp(documentApproval.CreatedBy, obj.StartDate.Value, obj.EndDate.Value)
                       .Where(x => !x.Off) // Exclude all off days (including holidays if not a change shift)
                       .OrderBy(x => x.Date) // Ensure dates are sorted
                       .ToList(); // Convert to List for processing

            // Check if the absence crosses into the next year
            if (obj.StartDate.HasValue && obj.EndDate.HasValue &&
                obj.StartDate.Value.Year == currentYear && obj.EndDate.Value.Year == currentYear + 1 &&
                dataAbsenece.Code == "p-CutiYearly")
            {
                int totalAbsence = int.Parse(obj.TotalAbsence); // Total absence duration

                // Identify the working days in the current year (excluding holidays)
                var workingDays2025 = workSchedule.Where(x => x.Date.Year == currentYear).ToList();
                int daysIn2025 = workingDays2025.Count;

                // Next year does NOT check work schedule, just counts remaining days
                int daysIn2026 = totalAbsence - daysIn2025;

                // If there are no working days in 2025, don't insert for that year
                if (daysIn2025 > 0)
                {
                    var employeeAbsent2025 = new EmployeeAbsent()
                    {
                        NoReg = documentApproval.CreatedBy,
                        StartDate = workingDays2025.First().Date, // First available working day
                        EndDate = workingDays2025.Last().Date,   // Last available working day
                        AbsentDuration = daysIn2025,
                        ReasonCode = dataAbsenece.Code,
                        Description = obj.Description
                    };
                    EmployeeAbsentRepository.Add(employeeAbsent2025);
                }

                // If there are remaining days for 2026, insert for that year
                if (daysIn2026 > 0)
                {
                    var employeeAbsent2026 = new EmployeeAbsent()
                    {
                        NoReg = documentApproval.CreatedBy,
                        StartDate = new DateTime(currentYear + 1, 1, 1), // start day of next year
                        EndDate = obj.EndDate.Value, // Actual end date
                        AbsentDuration = daysIn2026,
                        ReasonCode = dataAbsenece.Code,
                        Description = obj.Description
                    };
                    EmployeeAbsentRepository.Add(employeeAbsent2026);
                }

                UnitOfWork.SaveChanges(); 

                if (daysIn2025 > 0 &&
                       !string.IsNullOrEmpty(documentApproval.CreatedBy) &&
                       obj.StartDate.HasValue) 
                        {
                              UnitOfWork.Transact(trans =>
                              {
                              var parameters = new Dictionary<string, object>
                        {
                            { "@NoReg", documentApproval.CreatedBy },
                            { "@AbsDuration", daysIn2025 },
                            { "@StartDate", obj.StartDate.Value } 
                        };

                        UnitOfWork.UspQuery("SP_UPDATE_LEAVE_CROSS_YEAR", parameters, trans);
                        UnitOfWork.SaveChanges();
                    });
                }

                if (daysIn2026 > 0 &&
                    !string.IsNullOrEmpty(documentApproval.CreatedBy))
                {
                    UnitOfWork.Transact(trans =>
                    {
                        var parameters = new Dictionary<string, object>
                        {
                            { "@NoReg", documentApproval.CreatedBy },
                            { "@AbsDuration", daysIn2026 },
                            { "@AbsentType", "nextyear" }
                        };

                        UnitOfWork.UspQuery("SP_ADD_EMPLOYEE_CROSS_YEAR", parameters, trans);
                        UnitOfWork.SaveChanges();
                    });
                }

                return;

            }

            if (obj.StartDate.HasValue && obj.EndDate.HasValue &&
               obj.StartDate.Value.Year == currentYear + 1 && obj.EndDate.Value.Year == currentYear + 1 &&
               dataAbsenece.Code == "p-CutiYearly")
            {

                var employeesAbsents = new EmployeeAbsent()
                {
                    NoReg = documentApproval.CreatedBy,
                    StartDate = obj.StartDate.Value,
                    EndDate = obj.EndDate.Value,
                    AbsentDuration = int.Parse(obj.TotalAbsence),
                    ReasonCode = dataAbsenece.Code,
                    Description = obj.Description
                };

                EmployeeAbsentRepository.Add(employeesAbsents);
                UnitOfWork.SaveChanges();

                if (!string.IsNullOrEmpty(documentApproval.CreatedBy))
                {

                    UnitOfWork.Transact(trans =>
                    {
                        var parameters = new Dictionary<string, object>
                        {
                            { "@NoReg", documentApproval.CreatedBy },
                            { "@AbsDuration", int.Parse(obj.TotalAbsence) },
                            { "@AbsentType", "purenextyear" }
                        };

                        UnitOfWork.UspQuery("SP_ADD_EMPLOYEE_CROSS_YEAR", parameters, trans);
                        UnitOfWork.SaveChanges();
                    });
                }

                return;

            }

            var employeeAbsent = new EmployeeAbsent()
            {
                NoReg = documentApproval.CreatedBy,
                StartDate = obj.StartDate.Value,
                EndDate = obj.EndDate.Value,
                AbsentDuration = int.Parse(obj.TotalAbsence),
                ReasonCode = dataAbsenece.Code,
                Description = obj.Description,
                KategoriPenyakit = obj.KategoriPenyakit,
                SpesifikPenyakit = obj.SpesifikPenyakit
            };

            EmployeeAbsentRepository.Add(employeeAbsent);
            UnitOfWork.SaveChanges();

            var dataleave = EmployeeLeaveRepository.Fetch().FirstOrDefault(x => x.NoReg == documentApproval.CreatedBy);

            if (ObjectHelper.IsIn(obj.ReasonType, "cuti", "cutipanjang"))
            {
                Assert.ThrowIf(dataleave == null, "Cannot approve, there was no leave quota remain");

                if (obj.ReasonType == "cuti")
                {
                    dataleave.AnnualLeave -= int.Parse(obj.TotalAbsence);

                    Assert.ThrowIf(dataleave.AnnualLeave < 0, "Cannot process because the total leave duration is greater than total annual leave");
                }
                else if (obj.ReasonType == "cutipanjang")
                {
                    dataleave.LongLeave -= int.Parse(obj.TotalAbsence);

                    Assert.ThrowIf(dataleave.LongLeave < 0, "Cannot process because the total leave duration is greater than total long leave");
                }

                // Insert into leave mapping
                if (obj.AnnualLeavePlanningDetailId.HasValue && obj.AnnualLeavePlanningDetailId != Guid.Empty)
                {
                    var leaveMapping = new LeaveMapping()
                    {
                        ActualLeaveId = employeeAbsent.Id,
                        AnnualLeavePlanningDetailId = obj.AnnualLeavePlanningDetailId.Value
                    };

                    LeaveMappingRepository.Add(leaveMapping);
                }

                UnitOfWork.SaveChanges();
            }
        }

        public void CompleteMaternity(ApprovalService approvalService, string noregCurentApprover, DocumentApproval documentApproval)
        {
            var actualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == documentApproval.CreatedBy && x.Staffing == 100)
                .FirstOrDefaultIfEmpty();

            var approverActualOrganizationStructure = UnitOfWork.GetRepository<ActualOrganizationStructure>().Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noregCurentApprover && x.Staffing == 100)
                .FirstOrDefaultIfEmpty();

            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).FirstOrDefault();
            var objMaternityLeaveViewModel = JsonConvert.DeserializeObject<MaternityLeaveViewModel>(documentRequestDetail.ObjectValue);

            //find absence object cutiMelahirkan
            var dataAbsence = AbsenceRepository.Find(x => x.Code == "cutiMelahirkan").FirstOrDefault();
            //get off days
            var offDays = GetListOffWorkSchEmp(documentApproval.CreatedBy, objMaternityLeaveViewModel.StartMaternityLeave.Value, objMaternityLeaveViewModel.BackToWork.Value);
            //get remaining off day
            //var objCuti = new CoreService(UnitOfWork).GetInfoAllowance(documentApproval.CreatedBy, "cutiYearly");
            //var cuti = (decimal)objCuti.GetType().GetProperty("Ammount").GetValue(objCuti);
            //var objCutiPanjang = new CoreService(UnitOfWork).GetInfoAllowance(documentApproval.CreatedBy, "cutiPanjang");
            //var cutiPanjang = (decimal)objCutiPanjang.GetType().GetProperty("Ammount").GetValue(objCutiPanjang);

            var leaveQuota = EmployeeLeaveRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == documentApproval.CreatedBy)
                .FirstOrDefaultIfEmpty();

            //attachment
            var documentApprovalFileRequest = DocumentApprovalFileRepository.Fetch().Where(x => x.DocumentApprovalId == documentApproval.Id).ToList();
            var documentApprovalMaternity = new List<DocumentApprovalFile>() { };
            foreach (var item in documentApprovalFileRequest)
            {
                documentApprovalMaternity.Add(new DocumentApprovalFile()
                {
                    CommonFileId = item.CommonFileId,
                    FieldCategory = "Object.SupportingAttachmentPath"
                });
            }

            var absenceRequestDetail = new DocumentRequestDetailViewModel<AbsenceViewModel>()
            {
                FormKey = "absence",
                Object = new AbsenceViewModel()
                {
                    StartDate = objMaternityLeaveViewModel.StartMaternityLeave,
                    EndDate = objMaternityLeaveViewModel.BackToWork,
                    ReasonId = dataAbsence.Id,
                    Reason = dataAbsence.Name,
                    ReasonType = dataAbsence.AbsenceType,
                    Description = string.Empty,
                    TotalAbsence = ((objMaternityLeaveViewModel.BackToWork - objMaternityLeaveViewModel.StartMaternityLeave).Value.TotalDays - offDays).ToString(),
                    RemainingDaysOff = leaveQuota.AnnualLeave,
                    RemainingLongLeave = leaveQuota.LongLeave,
                    MandatoryAttachment = dataAbsence.MandatoryAttachment.HasValue,
                    IsPlanning = true
                },
                Attachments = documentApprovalMaternity
            };

            approvalService.CreateAutoRequestApprovalDocumentAsync(noregCurentApprover, documentApproval, actualOrganizationStructure, approverActualOrganizationStructure, absenceRequestDetail);

            UnitOfWork.SaveChanges();
        }

        public void CompleteShiftPlanning(string noregCurentApprover, DocumentApproval documentApproval)
        {
            var textlog = string.Empty;
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch()
                .FirstOrDefault(x => x.DocumentApprovalId == documentApproval.Id);

            var shiftPlanning = JsonConvert.DeserializeObject<ShiftPlanningViewModel>(documentRequestDetail.ObjectValue);

            var dataShift = GeneralCategoryRepository.Find(x => x.Category == "ShiftCode");

            foreach (var item in shiftPlanning.Request)
            {
                textlog += item.shift + Environment.NewLine;

                var empWorkSchSubtitute = EmpWorkSchSubtituteRepository.Find(x => x.NoReg == item.noreg && x.Date == item.date)
                    .FirstOrDefault();

                empWorkSchSubtitute = empWorkSchSubtitute ?? new EmpWorkSchSubtitute();

                var currShiftCode = (
                    from ews in EmployeeWorkScheduleRepository.Fetch()
                    join ws in WorkScheduleRepository.Fetch() on ews.WorkScheduleRule equals ws.WorkScheduleRule
                    where ews.NoReg == item.noreg && ws.Date.Date == item.date.Date &&
                    DateTime.Now >= ews.StartDate && ews.EndDate >= DateTime.Now
                    select ws.ShiftCode
                ).FirstOrDefault();

                var currShift = item.shift.Split(" - ");
                var shiftCode = string.Empty;

                shiftCode = dataShift.FirstOrDefault(x => x.Name == currShift[0] && x.Description == currShift[1]).Code;

                var SapCode = string.Empty;

                if (!string.IsNullOrEmpty(shiftCode))
                {
                    var dataSapCode = SapGeneralCategoryMapRepository.Fetch()
                        .AsNoTracking()
                        .FirstOrDefault(x => x.GeneralCategoryCode == shiftCode);

                    SapCode = dataSapCode == null ? "" : dataSapCode.SapCode;

                    if (item.date.DayOfWeek == DayOfWeek.Friday)
                    {
                        dataSapCode = SapGeneralCategoryMapRepository.Fetch()
                            .AsNoTracking()
                            .FirstOrDefault(x => x.GeneralCategoryCode == shiftCode + "j");

                        SapCode = dataSapCode != null ? dataSapCode.SapCode : SapCode;
                    }
                }

                empWorkSchSubtitute.NoReg = item.noreg;
                empWorkSchSubtitute.Date = item.date;
                empWorkSchSubtitute.ShiftCode = currShiftCode;
                empWorkSchSubtitute.ShiftCodeUpdate = SapCode;
                empWorkSchSubtitute.CreatedBy = noregCurentApprover;
                empWorkSchSubtitute.CreatedOn = DateTime.Now;

                if (empWorkSchSubtitute.Id != default)
                {
                    EmpWorkSchSubtituteRepository.Attach(empWorkSchSubtitute);
                }
                else
                {
                    EmpWorkSchSubtituteRepository.Add(empWorkSchSubtitute);
                }

                UnitOfWork.SaveChanges();
            }
        }
        #endregion

        public IEnumerable<TimeManagementDashboardStoredEntity> GetDashboard(string noreg, DateTime keyDate)
        {
            return UnitOfWork.UdfQuery<TimeManagementDashboardStoredEntity>(new { noreg, keyDate });
        }

        public IEnumerable<TimeManagementDetailsStoredEntity> GetTimeManagementDetails(string jobCode, string orgCode, DateTime startDate, DateTime endDate, string filter, string employeeSubgroup, string workContract)
        {
            return UnitOfWork.UdfQuery<TimeManagementDetailsStoredEntity>(new { jobCode, orgCode, startDate, endDate, filter = filter ?? string.Empty, employeeSubgroup = employeeSubgroup ?? string.Empty, workContract = workContract ?? string.Empty });
        }

        public IQueryable<object> GetActivity(string Kode)
        {
            var data = from ad in GeneralCategoryRepository.Fetch()
                       where ad.Code == Kode
                       select new
                       {
                           Name = ad.Name,
                           Description = ad.Description
                       };

            return data;
        }

        public IQueryable<object> GetUserName(string noreg)
        {
            var data = (from user in UserRepository.Fetch()
                        where user.NoReg == noreg
                        select new
                        {
                            Username = user.Username,
                        });

            return data;
        }

        public Task DeleteAbsence(Guid id, string noreg)
        {
            var documentApproval = DocumentApprovalRepository.Fetch().FirstOrDefault(x => x.Id == id);

            Assert.ThrowIf(documentApproval == null, "Document is not found");
            Assert.ThrowIf(documentApproval.CreatedBy != noreg, "Document is not registered");
            Assert.ThrowIf(documentApproval.DocumentStatusCode != DocumentStatus.Completed, "Cannot drop document, document status is not completed yet");

            var documentRequestDetail = DocumentRequestDetailRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.DocumentApprovalId == id);

            var employeeAbsence = JsonConvert.DeserializeObject<AbsenceViewModel>(documentRequestDetail.ObjectValue);
            var absence = AbsenceRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == employeeAbsence.ReasonId)
                .FirstOrDefaultIfEmpty();

            documentApproval.DocumentStatusCode = DocumentStatus.Cancelled;

            EmployeeAbsentRepository.Fetch()
                .Where(x => x.NoReg == noreg && x.StartDate == employeeAbsence.StartDate && x.EndDate == employeeAbsence.EndDate && x.ReasonCode == absence.Code)
                .Delete();

            var trackingApproval = TrackingApprovalRepository.Fetch()
                .FirstOrDefault(x => x.NoReg == noreg && x.DocumentApprovalId == id && x.ApprovalLevel == 1);

            trackingApproval.ApprovalActionCode = ApprovalAction.Cancel;

            const int annualLeave = 7;
            const int longLeave = 8;

            if (absence.CodePresensi == annualLeave || absence.CodePresensi == longLeave)
            {
                var employeeAbsent = EmployeeAbsentRepository.Fetch().FirstOrDefault(x => x.NoReg == noreg && x.StartDate == employeeAbsence.StartDate && x.EndDate == employeeAbsence.EndDate && x.ReasonCode == absence.Code);
                var duration = employeeAbsent.AbsentDuration;

                var employeeLeave = EmployeeLeaveRepository.Fetch().FirstOrDefault(x => x.NoReg == noreg);

                if (employeeLeave != null)
                {
                    if (absence.CodePresensi == annualLeave)
                    {
                        employeeLeave.AnnualLeave += duration;
                    }
                    else if (absence.CodePresensi == longLeave)
                    {
                        employeeLeave.LongLeave += duration;
                    }
                }

                // Remove absent from leave mapping
                if (employeeAbsent != null)
                {
                    LeaveMappingRepository
                        .Fetch()
                        .Where(e => e.ActualLeaveId == employeeAbsent.Id)
                        .Delete();
                }
            }

            return UnitOfWork.SaveChangesAsync();
        }

        public MaternityLeaveViewModel GetMaternityLeaveViewModel(Guid id)
        {
            var documentRequestDetail = DocumentRequestDetailRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.Id == id);

            return JsonConvert.DeserializeObject<MaternityLeaveViewModel>(documentRequestDetail.ObjectValue);
        }

        public void UpdateDayOfBirth(string actor, MaternityLeaveDobViewModel viewModel)
        {
            var documentRequestDetail = DocumentRequestDetailRepository.FindById(viewModel.Id);
            var docApproval = DocumentApprovalRepository.FindById(documentRequestDetail.DocumentApprovalId);

            //Assert.ThrowIf(!(docApproval.CreatedBy == actor || docApproval.SubmitBy == actor), "You dont have permission to update day of birth of this form");

            var approvalService = new ApprovalService(this.UnitOfWork, null, _localizer);

            var inputViewModel = JsonConvert.DeserializeObject<MaternityLeaveViewModel>(documentRequestDetail.ObjectValue);
            inputViewModel.DayOfBirth = viewModel.DayOfBirth;
            inputViewModel.BackToWork = viewModel.BackToWork;

            documentRequestDetail.ObjectValue = JsonConvert.SerializeObject(inputViewModel);

            if (viewModel != null && viewModel.Attachments != null)
            {
                var arrayFields = viewModel.Attachments.Where(x => x.FieldCategory.Contains("["))
                    .Select(x => x.FieldCategory.Substring(0, x.FieldCategory.IndexOf('[')))
                    .Distinct();

                if (arrayFields.Count() > 0)
                {
                    DocumentApprovalFileRepository.Fetch()
                        .Where(x => x.DocumentApprovalId == documentRequestDetail.DocumentApprovalId && arrayFields.Any(y => x.FieldCategory.Contains(y + "[%%]")))
                        .Delete();
                }

                foreach (var item in viewModel.Attachments)
                {
                    item.CommonFile = approvalService.GetCommonFileById(item.CommonFileId);
                    item.DocumentApprovalId = documentRequestDetail.DocumentApprovalId;
                }

                documentRequestDetail.DocumentApproval = approvalService.GetDocumentApprovalById(documentRequestDetail.DocumentApprovalId);
                documentRequestDetail.DocumentApproval.DocumentApprovalFiles = viewModel.Attachments;
            }

            UnitOfWork.SaveChanges();
            approvalService.MoveCommonFile(documentRequestDetail.DocumentApproval);


            docApproval.CanSubmit = true;

            DocumentApprovalRepository.Upsert<Guid>(docApproval);

            UnitOfWork.SaveChanges();
        }

        public async Task UpdateMaternityTrackingApproval(string noreg, Guid documentApprovalId)
        {
            var initiatorApproval = TrackingApprovalRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.NoReg == noreg && x.ApprovalLevel != 1);

            await TrackingApprovalRepository.Fetch()
                .Where(x => x.DocumentApprovalId == documentApprovalId && x.ApprovalLevel != 1 && x.ApprovalActionCode == null && x.ApprovalLevel < initiatorApproval.ApprovalLevel)
                .UpdateAsync(x => new TrackingApproval { ApprovalActionCode = ApprovalAction.Approve });

            var trackingApprovals = TrackingApprovalRepository.Fetch()
                        .AsNoTracking()
                        .Where(x => x.DocumentApprovalId == documentApprovalId)
                        .ToList();

            var minApprovalLevel = trackingApprovals.Where(x => x.ApprovalLevel >= initiatorApproval.ApprovalLevel)
                .DefaultIfEmpty(new TrackingApproval())
                .Min(x => x.ApprovalLevel);

            var nextApprovers = trackingApprovals.Where(x => x.ApprovalLevel == minApprovalLevel);

            var isComplete = nextApprovers == null || nextApprovers.Count() == 0;

            var total = trackingApprovals
                .Select(x => x.ApprovalLevel)
                .Distinct()
                .Count();

            var totalDone = trackingApprovals
                .Where(x => !string.IsNullOrEmpty(x.ApprovalActionCode))
                .Select(x => x.ApprovalLevel)
                .Distinct()
                .Count();

            var progress = isComplete
                ? 100
                : (int)Math.Round(100 * totalDone / (decimal)total);

            var documentApproval = DocumentApprovalRepository.Fetch()
                .FirstOrDefault(x => x.Id == documentApprovalId);

            documentApproval.Progress = progress;
            documentApproval.DocumentStatusCode = !isComplete ? DocumentStatus.InProgress : DocumentStatus.Completed;
            documentApproval.CurrentApprover = !isComplete
                ? "(" + string.Join("),(", UserRepository.Find(x => nextApprovers.Any(y => y.NoReg == x.NoReg)).Select(x => x.Username)) + ")"
                : null;

            UnitOfWork.SaveChanges();
        }
    }
}

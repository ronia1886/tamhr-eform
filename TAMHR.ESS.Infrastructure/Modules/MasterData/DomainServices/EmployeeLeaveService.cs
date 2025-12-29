using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Common.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using System.Threading.Tasks;
using Agit.Domain.Extensions;
using System;
using TAMHR.ESS.Infrastructure.ViewModels;
using Agit.Common;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using TAMHR.ESS.Domain.Models.TimeManagement;
using Newtonsoft.Json;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle employee leave master data.
    /// </summary>
    public class EmployeeLeaveService : GenericDomainServiceBase<EmployeeLeave>
    {
        #region Domain Repositories
        /// <summary>
        /// User repository object.
        /// </summary>
        protected IRepository<User> UserRepository => UnitOfWork.GetRepository<User>();
        protected IRepository<ActualOrganizationStructure> ActualOrganizationRepository => UnitOfWork.GetRepository<ActualOrganizationStructure>();
        protected IRepository<EmployeeLeaveHistoryView> EmployeeLeaveRepository => UnitOfWork.GetRepository<EmployeeLeaveHistoryView>();
        protected IRepository<EmployeeManualAbsent> EmployeeManualAbsent => UnitOfWork.GetRepository<EmployeeManualAbsent>();
        protected IRepository<EmployeeLeave> EmployeeLeaveRepositorys => UnitOfWork.GetRepository<EmployeeLeave>();
        protected IRepository<DateSpecification> DateSpecificationRepository => UnitOfWork.GetRepository<DateSpecification>();
        #endregion

        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for employee leave entity.
        /// </summary>
        protected override string[] Properties => new[] { "LongLeave", "AnnualLeave" };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public EmployeeLeaveService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get employee leave query.
        /// </summary>
        /// <returns>This list of <see cref="EmployeeLeave"/> objects.</returns>
        public override IQueryable<EmployeeLeave> GetQuery()
        {
            var q =
        from e in CommonRepository.Fetch().AsNoTracking()
        join a in ActualOrganizationRepository.Fetch().AsNoTracking().Where(x => x.Staffing == 100)
            on e.NoReg equals a.NoReg
        join l in EmployeeLeaveRepositorys.Fetch().AsNoTracking()
            on e.NoReg equals l.NoReg into leaveGroup
        from l in leaveGroup.DefaultIfEmpty() // left join
        select new EmployeeLeave
        {
            Id = e.Id,
            NoReg = e.NoReg,
            Name = a.Name,

            // contoh ambil field dari EmployeeLeaveRepositorys
            AnnualLeave = l != null ? l.AnnualLeave : 0,
            LongLeave = l != null ? l.LongLeave : 0,

            // tambahkan properti lain yang kamu butuh di grid
        };

            return q;
        }

        public IEnumerable<EmployeeLeaveReportsViewModel> GetEmployeeReports()
        {
            var leaveQuery = GetQuery();

            // Ambil data cuti terbaru per noreg dan tipe cuti
            var latestLeaves = EmployeeLeaveRepository.Fetch()
                .AsNoTracking()
                .GroupBy(x => new { x.noreg, x.leavetype })
                .Select(g => g.OrderByDescending(x => x.Period).FirstOrDefault())
                .ToList(); // 🔹 eksekusi di DB dulu

            // Pisahkan antara annual-leave dan long-leave
            var annualLeaveHistory = latestLeaves
                .Where(x => x.leavetype == "annual-leave")
                .ToList(); // 🔹 materialize lagi untuk join aman

            var longLeaveHistory = latestLeaves
                .Where(x => x.leavetype == "long-leave")
                .ToList();

            // Ambil data spesifikasi tanggal terakhir per NoReg
            var dateSpecQuery = DateSpecificationRepository.Fetch()
                .AsNoTracking()
                .GroupBy(x => x.NoReg)
                .Select(g => g.OrderByDescending(x => x.AstraDate).FirstOrDefault())
                .ToList(); // 🔹 ToList agar tidak bikin EF Core error

            // Materialisasi leaveQuery juga untuk join di memory
            var leaveData = leaveQuery
                .AsNoTracking()
                .ToList();

            // Lakukan join di sisi memory (LINQ to Objects)
            var result = from leave in leaveData
                         join annual in annualLeaveHistory
                             on leave.NoReg equals annual.noreg into annualGroup
                         from annual in annualGroup.DefaultIfEmpty()

                         join longleave in longLeaveHistory
                             on leave.NoReg equals longleave.noreg into longGroup
                         from longleave in longGroup.DefaultIfEmpty()

                         join datespec in dateSpecQuery
                             on leave.NoReg equals datespec.NoReg into dateSpecGroup
                         from datespec in dateSpecGroup.DefaultIfEmpty()

                         orderby leave.NoReg
                         select new EmployeeLeaveReportsViewModel
                         {
                             noreg = leave.NoReg,
                             Name = leave.Name,

                             TotalAnnualLeave = annual?.TotalLeave ?? 0,
                             UsedAnnualLeave = annual?.UsedLeave ?? 0,
                             AnnualLeave = annual?.RemainingLeave ?? 0,

                             TotalLongLeave = longleave?.TotalLeave ?? 0,
                             UsedLongLeave = longleave?.UsedLeave ?? 0,
                             LongLeave = longleave?.RemainingLeave ?? 0,

                             DateIn = datespec?.AstraDate,
                             CreatedOn = longleave?.CreatedOn,
                             perioddateleave = longleave?.PeriodDateLeave
                         };

            return result.ToList();
        }

        public EmployeeLeave GetByNoreg(string noreg)
        {
            return CommonRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg)
                .FirstOrDefaultIfEmpty();
        }

        public EmployeeLeave GetByNoregAll(string noreg)
        {
            var employeeLeave = CommonRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg)
                .FirstOrDefaultIfEmpty();

            var approvalService = new ApprovalService(UnitOfWork, null, null);
            var timeManagementService = new TimeManagementService(UnitOfWork, null);
            var objLeaveOnprogress = approvalService.GetInprogressDraftRequestDetails("absence").Where(x => x.CreatedBy == noreg).Select(x => JsonConvert.DeserializeObject<AbsenceViewModel>(x.ObjectValue));
            if (objLeaveOnprogress != null)
            {
                var totalCuti = 0;
                var totalCutiPanjang = 0;
                int currentYear = DateTime.Now.Year;

                foreach (var item in objLeaveOnprogress)
                {
                    // Ambil start dan end date
                    var start = item.StartDate.Value;
                    var end = item.EndDate.Value;

                    // Ambil list tanggal kerja dari service
                    var workDays = timeManagementService.GetListWorkSchEmp(noreg, start, end)
                        .Where(w => w.Date.Year == currentYear && !w.Off && !w.Holiday) // hanya hari kerja tahun ini
                        .Select(w => w.Date)
                        .ToList();

                    int validDaysCount = workDays.Count;

                    if (item.ReasonType == "cuti")
                    {
                        totalCuti += validDaysCount;
                    }

                    if (item.ReasonType == "cutipanjang")
                    {
                        totalCutiPanjang += validDaysCount;
                    }
                }

                employeeLeave.AnnualLeave -= totalCuti;
                employeeLeave.LongLeave -= totalCutiPanjang;
            }

            return employeeLeave;
        }
        

        public EmployeeLeave GetByNoregCurrentYear(string noreg)
        {
            return GetByNoregAndYear(noreg, DateTime.Now.Year);
        }

        public EmployeeLeave GetByNoregAndYear(string noreg, int year)
        {
            //var annualLeave = EmployeeLeaveRepository.Fetch()
            //    .AsNoTracking()
            //    .Where(x => x.noreg == noreg && x.leavetype == "annual-leave" && x.Period == year)
            //    .FirstOrDefault();

            //var longLeave = EmployeeLeaveRepository.Fetch()
            //    .AsNoTracking()
            //    .Where(x => x.noreg == noreg
            //        && x.leavetype == "long-leave"
            //        && year >= (x.Period - 5)
            //        && year <= x.Period)
            //    .OrderByDescending(x => x.Period)
            //    .FirstOrDefault();
            var leaveData = EmployeeLeaveRepositorys.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg)
                .FirstOrDefault();

            return new EmployeeLeave
            {
                NoReg = noreg,
                AnnualLeave = leaveData?.AnnualLeave ?? 0,
                LongLeave = leaveData?.LongLeave ?? 0
            };

            //return new EmployeeLeave
            //{
            //    NoReg = noreg,
            //    AnnualLeave = annualLeave?.RemainingLeave ?? 0,
            //    LongLeave = longLeave?.RemainingLeave ?? 0
            //};
        }

        public void ValidateWorkSchedule(string noreg, DateTime startDate, DateTime endDate)
        {
            var workScheduleRepository = UnitOfWork.GetRepository<WorkSchedule>();

            var absenceDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(offset => startDate.AddDays(offset))
                .ToList();

            var workScheduleDates = workScheduleRepository.Fetch()
                .AsNoTracking()
                .Where(ws => ws.Date >= startDate && ws.Date <= endDate)
                .Select(ws => ws.Date)
                .ToList();

            var missingDates = absenceDates.Except(workScheduleDates).ToList();

            if (missingDates.Any())
            {
                throw new Exception($"Can't request leave because work schedule hasn't been uploaded yet for next year");
            }
        }

        public List<EmployeeLeaveHistoryView> GetEmployeeLeaveHistory(string noreg)
        {
            return EmployeeLeaveRepository.Fetch()
         .AsNoTracking()
         .Where(x => x.noreg == noreg)
         .ToList() ?? new List<EmployeeLeaveHistoryView>();
        }

        public int GetUsedAnnualLeaveWithoutManual(string noReg, string period)
        {
            if (!int.TryParse(period, out int periodInt))
                return 0;

            // Ambil total used dari view
            var totalUsed = EmployeeLeaveRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.noreg == noReg && x.Period == periodInt && x.leavetype == "annual-leave")
                .Select(x => (int?)x.UsedLeave)
                .FirstOrDefault() ?? 0;

            // Ambil total dari manual absent
            var manualUsed = EmployeeManualAbsent.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noReg
                            && x.LeaveType == "annual-Leave"
                            && x.RowStatus
                            && x.Period.Year == periodInt)
                .Sum(x => (int?)x.UsedEdited) ?? 0;

            return totalUsed - manualUsed;
        }

        public bool UpdateLeave(EmployeeLeaveViewModel model)
        {
            try
            {
                UnitOfWork.Transact(trans =>
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "@NoReg", model.noreg },
                        { "@TotalLeave", model.TotalLeave },
                        { "@Period", model.Period },
                        { "@ModifiedBy",  model.ModifiedBy },
                        { "@UsedLeave", model.UsedLeave },
                        { "@RemainingLeave", model.RemainingLeave }
                    };

                    UnitOfWork.UspQuery("SP_UPDATE_EMPLOYEE_LEAVE", parameters, trans);

                    UnitOfWork.SaveChanges();
                });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public int GetUsedLongLeaveWithoutManual(string noReg, string period)
        {
            if (!int.TryParse(period, out int periodInt))
                return 0;

            // Ambil total used dari view
            var totalUsed = EmployeeLeaveRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.noreg == noReg && x.Period == periodInt && x.leavetype == "long-leave")
                .Select(x => (int?)x.UsedLeave)
                .FirstOrDefault() ?? 0;

            // Ambil total dari manual absent
            var manualUsed = EmployeeManualAbsent.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noReg
                            && x.LeaveType == "Long-Leave"
                            && x.RowStatus
                            && x.Period.Year == periodInt)
                .Sum(x => (int?)x.UsedEdited) ?? 0;

            return totalUsed - manualUsed;
        }

        public bool UpdateLongLeave(EmployeeLeaveViewModel model)
        {
            try
            {
                UnitOfWork.Transact(trans =>
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "@NoReg", model.noreg },
                        { "@TotalLeave", model.TotalLeave },
                        { "@UsedLeave", model.UsedLeave },
                        { "@RemainingLeave", model.RemainingLeave },
                        { "@Period", model.Period },
                        { "@ModifiedBy",  model.ModifiedBy }
                    };

                    UnitOfWork.UspQuery("SP_UPDATE_EMPLOYEE_LONG_LEAVE", parameters, trans);

                    UnitOfWork.SaveChanges();
                });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddLeaveEmployee(AddEmployeeLeaveViewModel model)
        {
            try
            {
                UnitOfWork.Transact(trans =>
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "@NoReg", model.noreg },
                        { "@TotalLeave", model.TotalLeave },
                        { "@UsedLeave", model.UsedLeave },
                        { "@RemainingLeave", model.RemainingLeave },
                        { "@Period", model.Period },
                        { "@ModifiedBy",  model.ModifiedBy },
                        { "@TotalLongLeave", model.TotalLongLeave},
                        { "@UsedLongLeave", model.UsedLongLeave },
                        { "@RemainingLongLeave", model.RemainingLongLeave },
                        { "@PeriodLongLeave", model.PeriodLongLeave }
                    };

                    UnitOfWork.UspQuery("SP_ADD_EMPLOYEE_LEAVE", parameters, trans);

                    UnitOfWork.SaveChanges();
                });

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task ProcessUploadAsync(IFormFile file, string userNoReg)
        {
            if (file == null)
                throw new Exception("Upload failed. Please choose a file.");

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                ms.Position = 0; // Ensure stream starts from the beginning
                ProcessUpload(ms, userNoReg);
            }
        }

        private void ProcessUpload(Stream stream, string userNoReg)
        {
            using (var package = new ExcelPackage(stream))
            {
                var workSheet = package.Workbook.Worksheets.FirstOrDefault();
                if (workSheet == null) return;

                var seenRecords = new HashSet<(string NoReg, int Year)>(); // Track NoReg + Year
                int rowIndex = 2; // Assuming row 1 is headers

                while (true)
                {
                    var noReg = workSheet.Cells[rowIndex, 1].Text?.Trim(); // NoReg should be a numeric string
                    if (string.IsNullOrEmpty(noReg)) break;

                    // Validation: Ensure NoReg contains only digits (no letters or symbols)
                    if (!Regex.IsMatch(noReg, @"^\d+$"))
                    {
                        throw new Exception($"Invalid NoReg at row {rowIndex}: Only numbers are allowed.");
                    }

                    string yearText = workSheet.Cells[rowIndex, 2].Text.Trim();
                    string annualLeaveText = workSheet.Cells[rowIndex, 3].Text.Trim();
                    string longLeaveText = workSheet.Cells[rowIndex, 4].Text.Trim();

                    // Validation: Ensure Year, AnnualLeave, and LongLeave contain only numbers (no letters or symbols)
                    if (!Regex.IsMatch(yearText, @"^\d+$") ||
                         (!string.IsNullOrEmpty(annualLeaveText) && !Regex.IsMatch(annualLeaveText, @"^\d+$")) ||
                         (!string.IsNullOrEmpty(longLeaveText) && !Regex.IsMatch(longLeaveText, @"^\d+$")))
                    {
                        throw new Exception($"Invalid data at row {rowIndex}: Year must contain only numbers. AnnualLeave and LongLeave must be numbers if provided.");
                    }


                    // Convert to int after validation, allowing empty values
                    int year = Convert.ToInt32(yearText);
                    int? annualLeave = string.IsNullOrEmpty(annualLeaveText) ? (int?)null : Convert.ToInt32(annualLeaveText);
                    int? longLeave = string.IsNullOrEmpty(longLeaveText) ? (int?)null : Convert.ToInt32(longLeaveText);


                    var key = (NoReg: noReg, Year: year);

                    if (!seenRecords.Contains(key))
                    {
                        var parameters = new Dictionary<string, object>
                                        {
                                            { "@NoReg", noReg },
                                            { "@Year", year },              
                                            { "@AnnualLeave", annualLeave }, 
                                            { "@LongLeave", longLeave },   
                                            { "@ModifiedBy", userNoReg },
                                        };

                        UnitOfWork.UspQuery("SP_UPDATE_ANNUAL_EMPLOYEE_LEAVE", parameters);
                        seenRecords.Add(key);
                    }
                    else
                    {
                        throw new Exception($"Duplicate NoReg and Year combination found at row {rowIndex}: NoReg {noReg}, Year {year}.");
                    }

                    rowIndex++;
                }

                UnitOfWork.SaveChanges();
            }
        }


        private int ConvertToInt(string value)
        {
            return int.TryParse(value, out int result) ? result : 0;
        }


        #endregion
    }
}

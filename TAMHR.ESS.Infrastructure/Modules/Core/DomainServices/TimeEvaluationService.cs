using System;
using System.Text;
using Agit.Domain;
using Agit.Common.Archieve;
using Agit.Domain.UnitOfWork;
using Agit.Domain.Repository;
using TAMHR.ESS.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Agit.Domain.Extensions;
using Dapper;
using System.Collections.Generic;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle time evaluation
    /// </summary>
    public class TimeEvaluationService : DomainServiceBase
    {
        #region Repositories
        /// <summary>
        /// Absence readonly repository
        /// </summary>
        protected IReadonlyRepository<Absence> AbsenceReadonlyRepository => UnitOfWork.GetRepository<Absence>();

        /// <summary>
        /// Employee absent readonly repository
        /// </summary>
        protected IReadonlyRepository<EmployeeAbsent> EmployeeAbsentReadonlyRepository => UnitOfWork.GetRepository<EmployeeAbsent>();

        /// <summary>
        /// Time management readonly repository
        /// </summary>
        protected IReadonlyRepository<TimeManagement> TimeManagementReadonlyRepository => UnitOfWork.GetRepository<TimeManagement>();

        /// <summary>
        /// BDJK readonly repository
        /// </summary>
        protected IReadonlyRepository<BDJK> BdjkReadonlyRepository => UnitOfWork.GetRepository<BDJK>();
        
        /// <summary>
        /// SPKL readonly repository
        /// </summary>
        protected IReadonlyRepository<TimeManagementSpkl> SpklReadonlyRepository => UnitOfWork.GetRepository<TimeManagementSpkl>();

        /// <summary>
        /// Employee work schedule subtitue readonly repository
        /// </summary>
        protected IReadonlyRepository<EmpWorkSchSubtitute> EmpWorkSchSubtituteReadonlyRepository => UnitOfWork.GetRepository<EmpWorkSchSubtitute>();
        
        #endregion

        private string[] _attendencesCodes = new[] { "29", "32", "33","45" };
        private string[] _exceptionsCodes = new[] { "0", "1", "39" };

        
        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="unitOfWork">Unit of Work Object</param>
        public TimeEvaluationService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Generate absences
        /// </summary>
        /// <param name="keyDate">Key Date</param>
        /// <returns>Absences Zip Entry</returns>
        public ZipEntry GenerateAbsences(DateTime keyDate)
        {
            var sb = new StringBuilder();
            
        var absences = UnitOfWork.GetConnection().Query<TimeEvaluation>(@"
                SELECT t.* FROM dbo.TB_R_TIME_EVALUATION t with (nolock)
                LEFT JOIN dbo.MDM_POSITION_EMPLOYEE_REL per with (nolock) ON per.NoReg = t.NoReg AND t.WorkingDate BETWEEN per.StartDate AND per.EndDate AND per.Staffing = 100
                LEFT JOIN dbo.MDM_ORGANIZATIONAL_ASSIGNMENT oa with (nolock) ON oa.NoReg = t.NoReg AND t.WorkingDate BETWEEN oa.StartDate AND oa.EndDate
                LEFT JOIN dbo.MDM_EMPLOYEE_SUBGROUPNP np with (nolock) ON np.EmployeeSubgroup = oa.EmployeeSubgroup
                WHERE per.Id IS NOT NULL AND YEAR(WorkingDate) = @year AND MONTH(WorkingDate) = @month AND (ISNULL(np.NP, 0) < 7 OR (ISNULL(np.NP, 0) >= 7 AND PresenceCode IN (7, 8, 9, 10))) AND PresenceCode NOT IN (0, 1, 39, 29, 32, 33, 34, 35, 36, 38, 2, 3, 4, 5, 6, 41, 42, 45)
                ORDER BY t.NoReg, t.WorkingDate
            ", new { year = keyDate.Year, month = keyDate.Month });

            string finalString = sb.ToString();
            bool IsEncrypt = Convert.ToBoolean(UnitOfWork.GetRepository<Config>().Fetch().Where(wh => wh.ConfigKey == "IsEncrypt").FirstOrDefault().ConfigValue);

            foreach (var absence in absences)
            {
                finalString = string.Format("{0}\t{1:dd.MM.yyyy}\t{1:dd.MM.yyyy}\t{2}\t\t", absence.NoReg, absence.WorkingDate, absence.PresenceCode.ToString("D4"));
                if (IsEncrypt)
                {
                    finalString = Base64Encode(finalString);
                }
                sb.AppendLine(finalString);
                
            }

            return new ZipEntry("IT2001.TXT", Encoding.ASCII.GetBytes(sb.ToString()));
        }

        /// <summary>
        /// Generate time events
        /// </summary>
        /// <param name="keyDate">Key Date</param>
        /// <returns>Time Events Zip Entry</returns>
        public ZipEntry GenerateTimeEvents(DateTime keyDate)
        {
            var sb = new StringBuilder();

            var timeEvents = UnitOfWork.GetConnection().Query<TimeEvaluation>(@"
                SELECT t.Id, t.NoReg, t.WorkingDate, t.WorkingTimeIn, t.WorkingTimeOut, t.CreatedOn FROM dbo.TB_R_TIME_EVALUATION t with (nolock)
                LEFT JOIN dbo.MDM_POSITION_EMPLOYEE_REL per with (nolock) ON per.NoReg = t.NoReg AND t.WorkingDate BETWEEN per.StartDate AND per.EndDate AND per.Staffing = 100
                LEFT JOIN dbo.MDM_ORGANIZATIONAL_ASSIGNMENT oa with (nolock) ON oa.NoReg = t.NoReg AND t.WorkingDate BETWEEN oa.StartDate AND oa.EndDate
                LEFT JOIN dbo.MDM_EMPLOYEE_SUBGROUPNP np with (nolock) ON np.EmployeeSubgroup = oa.EmployeeSubgroup
                WHERE per.Id IS NOT NULL AND YEAR(WorkingDate) = @year AND MONTH(WorkingDate) = @month AND t.WorkingTimeIn IS NOT NULL AND ISNULL(np.NP, 0) < 7
                ORDER BY t.NoReg, t.WorkingDate
            ", new { year = keyDate.Year, month = keyDate.Month });

            string finalString = "";
            bool IsEncrypt = Convert.ToBoolean(UnitOfWork.GetRepository<Config>().Fetch().Where(wh => wh.ConfigKey == "IsEncrypt").FirstOrDefault().ConfigValue);

            foreach (var timeEvent in timeEvents)
            {
                finalString = string.Format("{0}\t{1:dd.MM.yyyy}\t{1:HH:mm}\tP10", timeEvent.NoReg, timeEvent.WorkingTimeIn);
                if (IsEncrypt)
                {
                    finalString = Base64Encode(finalString);
                }
                sb.AppendLine(finalString);

                finalString = string.Format("{0}\t{1:dd.MM.yyyy}\t{1:HH:mm}\tP20", timeEvent.NoReg, timeEvent.WorkingTimeOut);
                if (IsEncrypt)
                {
                    finalString = Base64Encode(finalString);
                }
                sb.AppendLine(finalString);
            }

            return new ZipEntry("IT2011.TXT", Encoding.ASCII.GetBytes(sb.ToString()));
        }

        /// <summary>
        /// Generate attendences
        /// </summary>
        /// <param name="keyDate">Key Date</param>
        /// <returns>Attendences Zip Entry</returns>
        public ZipEntry GenerateAttendances(DateTime keyDate)
        {
            var sb = new StringBuilder();

            var attendances = UnitOfWork.GetConnection().Query<TimeEvaluation>(@"
                SELECT t.* FROM dbo.TB_R_TIME_EVALUATION t with (nolock)
                LEFT JOIN dbo.MDM_POSITION_EMPLOYEE_REL per with (nolock) ON per.NoReg = t.NoReg AND t.WorkingDate BETWEEN per.StartDate AND per.EndDate AND per.Staffing = 100
                LEFT JOIN dbo.MDM_ORGANIZATIONAL_ASSIGNMENT oa with (nolock) ON oa.NoReg = t.NoReg AND t.WorkingDate BETWEEN oa.StartDate AND oa.EndDate
                LEFT JOIN dbo.MDM_EMPLOYEE_SUBGROUPNP np ON np.EmployeeSubgroup = oa.EmployeeSubgroup
                WHERE per.Id IS NOT NULL AND YEAR(WorkingDate) = @year AND MONTH(WorkingDate) = @month AND ISNULL(np.NP, 0) < 7 AND PresenceCode IN (29, 32, 33,45)
                ORDER BY t.NoReg, t.WorkingDate
            ", new { year = keyDate.Year, month = keyDate.Month });

            string finalString = "";
            bool IsEncrypt = Convert.ToBoolean(UnitOfWork.GetRepository<Config>().Fetch().Where(wh => wh.ConfigKey == "IsEncrypt").FirstOrDefault().ConfigValue);

            foreach (var attendance in attendances)
            {
                finalString = string.Format("{0}\t{1:dd.MM.yyyy}\t{2:dd.MM.yyyy}\t{3}", attendance.NoReg, attendance.WorkingDate, attendance.WorkingDate, attendance.PresenceCode.ToString("D4"));
                if (IsEncrypt)
                {
                    finalString = Base64Encode(finalString);
                }

                sb.AppendLine(finalString);
            }

            return new ZipEntry("IT2002.TXT", Encoding.ASCII.GetBytes(sb.ToString()));
        }

        /// <summary>
        /// Generate overtime
        /// </summary>
        /// <param name="keyDate">Key Date</param>
        /// <returns>Overtime Zip Entry</returns>
        public ZipEntry GenerateOvertime(DateTime keyDate)
        {
            var sb = new StringBuilder();

            var overtimes = UnitOfWork.GetConnection().Query<TimeEvaluation>(@"
                SELECT
	                t.Id,
	                t.NoReg,
	                CAST(t.OvertimeIn AS DATE) AS WorkingDate,
	                t.OvertimeIn AS WorkingTimeIn,
	                t.OvertimeOut AS WorkingTimeOut,
	                t.CreatedBy,
	                t.CreatedOn
                FROM dbo.TB_R_TIME_EVALUATION_OVERTIME t with (nolock)
                --LEFT JOIN dbo.VW_EMPLOYEE_WORK_SCHEDULE ews with (nolock) ON ews.NoReg = t.NoReg AND ews.Date = t.OvertimeDate
                LEFT JOIN dbo.MDM_POSITION_EMPLOYEE_REL per with (nolock) ON per.NoReg = t.NoReg AND t.OvertimeDate BETWEEN per.StartDate AND per.EndDate AND per.Staffing = 100
                WHERE per.Id IS NOT NULL AND ((YEAR(t.OvertimeDate) = @year AND MONTH(t.OvertimeDate) = @month) OR 
                (YEAR(t.ModifiedOn) = @year AND MONTH(t.ModifiedOn) = @month AND t.Remarks = 'Abnormality'))
                AND CONVERT(varchar(12),t.OvertimeIn,14) <= CONVERT(varchar(12),t.OvertimeOut,14)

                ORDER BY t.NoReg, t.OvertimeIn


            ", new { year = keyDate.Year, month = keyDate.Month });

            string finalString = "";
            bool IsEncrypt = Convert.ToBoolean(UnitOfWork.GetRepository<Config>().Fetch().Where(wh => wh.ConfigKey == "IsEncrypt").FirstOrDefault().ConfigValue);

            foreach (var overtime in overtimes)
            {
                
                finalString = string.Format("{0}\t{1:dd.MM.yyyy}\t{1:dd.MM.yyyy}\t01\t{1:HH:mm}\t{2:HH:mm}", overtime.NoReg, overtime.WorkingTimeIn, overtime.WorkingTimeOut);
                if (IsEncrypt)
                {
                    finalString = Base64Encode(finalString);
                }
                sb.AppendLine(finalString);

            }

            return new ZipEntry("IT2007.TXT", Encoding.ASCII.GetBytes(sb.ToString()));
        }

        public ZipEntry GenerateOvertimeCrossDay(DateTime keyDate)
        {
            var sb = new StringBuilder();

            var overtimes = UnitOfWork.GetConnection().Query<TimeEvaluation>(@"
                SELECT
	                t.Id,
	                t.NoReg,
	                CAST(t.OvertimeIn AS DATE) AS WorkingDate,
	                t.OvertimeIn AS WorkingTimeIn,
	                t.OvertimeOut AS WorkingTimeOut,
	                t.CreatedBy,
	                t.CreatedOn
                FROM dbo.TB_R_TIME_EVALUATION_OVERTIME t with (nolock)
                --LEFT JOIN dbo.VW_EMPLOYEE_WORK_SCHEDULE ews with (nolock) ON ews.NoReg = t.NoReg AND ews.Date = t.OvertimeDate
                LEFT JOIN dbo.MDM_POSITION_EMPLOYEE_REL per with (nolock) ON per.NoReg = t.NoReg AND t.OvertimeDate BETWEEN per.StartDate AND per.EndDate AND per.Staffing = 100
                WHERE per.Id IS NOT NULL AND (YEAR(t.OvertimeDate) = @year AND MONTH(t.OvertimeDate) = @month
                    OR
                    (YEAR(t.ModifiedOn) = @year AND MONTH(t.ModifiedOn) = @month AND t.Remarks = 'Abnormality')
                )
                AND CONVERT(varchar(12),t.OvertimeIn,14) > CONVERT(varchar(12),t.OvertimeOut,14)
                ORDER BY t.NoReg, t.OvertimeIn
            ", new { year = keyDate.Year, month = keyDate.Month });

            string finalString = sb.ToString();
            bool IsEncrypt = Convert.ToBoolean(UnitOfWork.GetRepository<Config>().Fetch().Where(wh => wh.ConfigKey == "IsEncrypt").FirstOrDefault().ConfigValue);

            foreach (var overtime in overtimes)
            {

                finalString = string.Format("{0}\t{1:dd.MM.yyyy}\t{1:dd.MM.yyyy}\t01\t{1:HH:mm}\t{2:HH:mm}", overtime.NoReg, overtime.WorkingTimeIn, overtime.WorkingTimeOut);
                if (IsEncrypt)
                {
                    finalString = Base64Encode(finalString);
                }

                sb.AppendLine(finalString);
            }

            return new ZipEntry("IT2007-CROSS DAY.TXT", Encoding.ASCII.GetBytes(sb.ToString()));
        }

        /// <summary>
        /// Generate shift
        /// </summary>
        /// <param name="keyDate">Key Date</param>
        /// <returns>Shift Zip Entry</returns>
        public ZipEntry GenerateShift(DateTime keyDate)
        {
            var sb = new StringBuilder();

            var shifts = UnitOfWork.GetConnection().Query<EmpWorkSchSubtitute>(@"
                SELECT
	                t.Id,
	                t.NoReg,
	                t.WorkingDate AS Date,
	                t.ShiftCode AS ShiftCodeUpdate,
	                IIF(wd.Holiday = 1, 'OFF', wd.ShiftCode) AS ShiftCode,
	                t.CreatedBy,
	                t.CreatedOn,
	                t.ModifiedBy,
	                t.ModifiedOn,
	                t.RowStatus
                FROM dbo.TB_R_TIME_EVALUATION t with (nolock)
                LEFT JOIN dbo.VW_NORMAL_WORKING_DAYS wd with (nolock) ON wd.NoReg = t.NoReg and wd.Date = t.WorkingDate AND YEAR(wd.Date) = @year AND MONTH(wd.Date) = @month
                LEFT JOIN dbo.MDM_POSITION_EMPLOYEE_REL per with (nolock) ON per.NoReg = t.NoReg AND t.WorkingDate BETWEEN per.StartDate AND per.EndDate AND per.Staffing = 100
                WHERE per.Id IS NOT NULL AND YEAR(t.WorkingDate) = @year AND MONTH(t.WorkingDate) = @month AND IIF(wd.Holiday = 1, 'OFF', wd.ShiftCode) <> t.ShiftCode
                ORDER BY t.NoReg, t.WorkingDate
            ", new { year = keyDate.Year, month = keyDate.Month });

            string finalString = sb.ToString();
            bool IsEncrypt = Convert.ToBoolean(UnitOfWork.GetRepository<Config>().Fetch().Where(wh => wh.ConfigKey == "IsEncrypt").FirstOrDefault().ConfigValue);

            foreach (var shift in shifts)
            {

                finalString = string.Format("{0}\t{1:dd.MM.yyyy}\t{2:dd.MM.yyyy}\t{3}", shift.NoReg, shift.Date, shift.Date, shift.ShiftCodeUpdate);
                if (IsEncrypt)
                {
                    finalString = Base64Encode(finalString);
                }

                sb.AppendLine(finalString);
            }

            return new ZipEntry("IT2003.TXT", Encoding.ASCII.GetBytes(sb.ToString()));
        }

        public void GenerateTimeEvaluation(Guid jobId, int year, int month)
        {
            UnitOfWork.UspQuery("dbo.SP_GENERATE_TIME_EVALUATION", new { jobId, year, month });
        }

        public void GenerateProxy(DateTime from, DateTime to)
        {
            UnitOfWork.UspQuery("dbo.SP_GENERATE_RANGE_PROXY", new { date1 = from, date2 = to });
        }

        public IEnumerable<OvertimeEvaluationStoredEntity> GenerateOvertimeEvaluations(int month, int year)
        {
            var overtimeEvaluations = UnitOfWork.UspQuery<OvertimeEvaluationStoredEntity>(new { month, year });

            return overtimeEvaluations;
        }
        public IEnumerable<BdjkSummaryStoredEntity> GenerateBDJKEvaluations(int month, int year)
        {
            var bdjkSummary = UnitOfWork.UspQuery<BdjkSummaryStoredEntity>(new { month, year });

            return bdjkSummary;
        }

        public ZipEntry GenerateBdjk(int year, int month)
        {
            var sb = new StringBuilder();

            var bdjkSummary = UnitOfWork.UspQuery<BdjkSummaryStoredEntity>(new { year, month });

            sb.AppendLine(string.Format("NoReg\tYear\tMonth\tA\tB\tC\tD\tT"));
            foreach (var summary in bdjkSummary)
            {
                sb.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}", summary.NoReg, year, month, summary.A, summary.B, summary.C, summary.D, summary.T));
            }

            return new ZipEntry("IT2010.TXT", Encoding.ASCII.GetBytes(sb.ToString()));
        }

        public ZipEntry GenerateBdjk(DateTime keyDate)
        {
            var sb = new StringBuilder();

            var bdjks = UnitOfWork.GetConnection().Query<TimeEvaluationBDJK>(@"
                SELECT
	                t.Id,
	                t.NoReg,
	                t.WorkingDate,
	                t.A,
	                t.B,
	                t.C,
	                t.D,
	                t.T,
	                t.CreatedBy,
	                t.CreatedOn
                FROM dbo.TB_R_TIME_EVALUATION_BDJK t with (nolock)
                LEFT JOIN dbo.VW_EMPLOYEE_WORK_SCHEDULE ews with (nolock) ON ews.NoReg = t.NoReg AND ews.Date = t.WorkingDate
                LEFT JOIN dbo.MDM_POSITION_EMPLOYEE_REL per with (nolock) ON per.NoReg = t.NoReg AND t.WorkingDate BETWEEN per.StartDate AND per.EndDate AND per.Staffing = 100
                WHERE per.Id IS NOT NULL AND ((YEAR(t.WorkingDate) = @year AND MONTH(t.WorkingDate) = @month) OR 
                (YEAR(t.ModifiedOn) = @year AND MONTH(t.ModifiedOn) = @month AND t.Remarks = 'Abnormality'))

                ORDER BY t.NoReg, t.WorkingDate
            ", new { year = keyDate.Year, month = keyDate.Month });

            string finalString = sb.ToString();
            bool IsEncrypt = Convert.ToBoolean(UnitOfWork.GetRepository<Config>().Fetch().Where(wh => wh.ConfigKey == "IsEncrypt").FirstOrDefault().ConfigValue);

            foreach (var bdjk in bdjks)
            {
                finalString = string.Format("{0}\t{1:dd.MM.yyyy}\t{2}\t{3}\t{4}\t{5}\t{6}", bdjk.NoReg, bdjk.WorkingDate, bdjk.A, bdjk.B, bdjk.C, bdjk.D, bdjk.T);
                if (IsEncrypt)
                {
                    finalString = Base64Encode(finalString);
                }

                sb.AppendLine(finalString);
            }

            return new ZipEntry("IT2010.TXT", Encoding.ASCII.GetBytes(sb.ToString()));
        }
    }
}

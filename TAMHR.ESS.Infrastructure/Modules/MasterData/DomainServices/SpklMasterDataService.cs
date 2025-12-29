using System;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Helpers;
using Agit.Domain.Utility;
using Agit.Common.Extensions;
using Agit.Domain.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle spkl master data.
    /// </summary>
    public class SpklMasterDataService : GenericDomainServiceBase<TimeManagementSpkl>
    {
        #region Domain Repositories
        /// <summary>
        /// SPKL master data readonly repository object.
        /// </summary>
        protected IReadonlyRepository<SpklMasterDataView> SpklMasterDataReadonlyRepository => UnitOfWork.GetRepository<SpklMasterDataView>();

        /// <summary>
        /// Time management repository object.
        /// </summary>
        protected IReadonlyRepository<TimeManagement> TimeManagementReadonlyRepository => UnitOfWork.GetRepository<TimeManagement>();
        #endregion

        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated.
        /// </summary>
        protected override string[] Properties => new[] {
            "OvertimeInPlan",
            "OvertimeOutPlan",
            "OvertimeInAdjust",
            "OvertimeOutAdjust",
            "OvertimeBreakPlan",
            "OvertimeBreakAdjust",
            "DurationPlan",
            "DurationAdjust",
            "OvertimeCategoryCode",
            "OvertimeReason"
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public SpklMasterDataService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        public IEnumerable<SpklMasterDataView> GetDataViews(string noreg, string name, DateTime? startDate, DateTime? endDate)
        {
            var emptyAll = string.IsNullOrEmpty(noreg) && string.IsNullOrEmpty(name);

            return GetView()
                .Where(x => (emptyAll || x.Name.Contains(name) || x.NoReg.Contains(noreg)) && (!startDate.HasValue || x.OvertimeDate >= startDate.Value) && (!endDate.HasValue || x.OvertimeDate <= endDate.Value))
                .OrderBy(x => x.Name)
                .ThenBy(x => x.OvertimeDate);
        }

        public IQueryable<SpklMasterDataView> GetView()
        {
            return SpklMasterDataReadonlyRepository.Fetch()
                .AsNoTracking();
        }

        public SpklMasterDataView GetViewById(Guid id)
        {
            return GetView().Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        public IEnumerable<TimeEvaluationOvertimeStoredEntity> GetTimeEvaluationOvertime(int year, int month)
        {
            return UnitOfWork.UspQuery<TimeEvaluationOvertimeStoredEntity>(new { year, month });
        }

        public override void Upsert(TimeManagementSpkl data)
        {
            var timeManagement = TimeManagementReadonlyRepository.Fetch()
                .AsNoTracking()
                .FirstOrDefault(x => x.NoReg == data.NoReg && x.WorkingDate.Date == data.OvertimeDate.Date);

            var normalTimeIn = timeManagement?.NormalTimeIn;
            var normalTimeOut = timeManagement?.NormalTimeOut;
            var shiftCodeFlexi = new string[] { "1NS8", "1NJ8" };
            var maxNormalTimeIn = DateTime.Now;
            var maxNormalTimeOut = DateTime.Now;
            if (shiftCodeFlexi.Contains(timeManagement.ShiftCode))
            {
                maxNormalTimeIn = timeManagement.NormalTimeIn.Value.AddHours(1);
                maxNormalTimeOut = timeManagement.NormalTimeOut.Value.AddHours(1);
            
                if(timeManagement?.WorkingTimeIn > maxNormalTimeIn)
                {
                    normalTimeIn = maxNormalTimeIn;
                    normalTimeOut = maxNormalTimeOut;
                }

                if( timeManagement?.WorkingTimeIn > normalTimeIn && timeManagement?.WorkingTimeIn <= maxNormalTimeIn)
                {
                    normalTimeIn = timeManagement?.WorkingTimeIn;
                }

                if (timeManagement.ShiftCode == "1NS8" && timeManagement?.WorkingTimeIn > normalTimeIn && timeManagement?.WorkingTimeIn <= maxNormalTimeIn)
                {
                    normalTimeOut = timeManagement?.WorkingTimeIn.Value.AddMinutes(525);
                }

                if (timeManagement.ShiftCode == "1NJ8" && timeManagement?.WorkingTimeIn > normalTimeIn && timeManagement?.WorkingTimeIn <= maxNormalTimeIn)
                {
                    normalTimeOut = timeManagement?.WorkingTimeIn.Value.AddMinutes(540);
                }
            }

            var durationPlan = ServiceHelper.CalculateProxyDuration(data.OvertimeInPlan, data.OvertimeOutPlan, data.OvertimeInPlan, data.OvertimeOutPlan, normalTimeIn, normalTimeOut, data.OvertimeBreakPlan);
            var durationAdjust = ServiceHelper.CalculateProxyDuration(data.OvertimeInPlan, data.OvertimeOutPlan, data.OvertimeInAdjust, data.OvertimeOutAdjust, normalTimeIn, normalTimeOut, data.OvertimeBreakAdjust);

            data.DurationPlan = durationPlan;
            data.DurationAdjust = durationAdjust;

            base.Upsert(data);
        }

        public void Merge(string actor, DataTable dataTable)
        {
            var connection = UnitOfWork.GetConnection() as SqlConnection;
            Exception exception = null;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                using (var transaction = connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    var tableName = dataTable.TableName;

                    command.Transaction = transaction;
                    command.CommandType = CommandType.Text;

                    command.CommandText = $"CREATE TABLE {dataTable.TableName}(NoReg VARCHAR(20), OvertimeDate DATE, OvertimeDuration DECIMAL(18,2))";
                    command.ExecuteNonQuery();

                    BulkCopier.BulkCopy(connection, dataTable, transaction);

                    command.CommandText = string.Format("DELETE p FROM dbo.TB_M_TIME_MANAGEMENT_SPKL p INNER JOIN {0} tmp ON tmp.NoReg = p.NoReg AND tmp.OvertimeDate = p.OvertimeDate", tableName);
                    command.ExecuteNonQuery();

                    command.CommandText = string.Format(@"
                        MERGE INTO dbo.TB_M_TIME_MANAGEMENT_SPKL AS TARGET
                        USING (
                            SELECT
                                it.NoReg,
                                it.OvertimeDate,
                                0 AS OvertimeBreak,
                                CASE
                                    WHEN vtm.ShiftCode NOT IN ('OFF', 'OFFS') AND vtm.NormalTimeIn IS NOT NULL AND DAY(vtm.NormalTimeOut) <> DAY(vtm.WorkingDate) THEN DATEADD(MINUTE, -60 * it.OvertimeDuration, vtm.NormalTimeIn)
                                    ELSE ISNULL(vtm.NormalTimeOut, ISNULL(vtm.WorkingTimeIn, CAST(CONVERT(VARCHAR, it.OvertimeDate, 112) + ' 08:00' AS DATETIME)))
                                END AS OvertimeIn,
                                CASE
                                    WHEN vtm.ShiftCode NOT IN ('OFF', 'OFFS') AND vtm.NormalTimeIn IS NOT NULL AND DAY(vtm.NormalTimeOut) <> DAY(vtm.WorkingDate) THEN vtm.NormalTimeIn
                                    ELSE DATEADD(MINUTE, 60 * it.OvertimeDuration, ISNULL(vtm.NormalTimeOut, ISNULL(vtm.WorkingTimeIn, CAST(CONVERT(VARCHAR, it.OvertimeDate, 112) + ' 08:00' AS DATETIME))))
                                END AS OvertimeOut,
                                it.OvertimeDuration
                            FROM {0} it
                            LEFT JOIN dbo.TB_R_TIME_MANAGEMENT vtm ON vtm.NoReg = it.NoReg AND CAST(vtm.WorkingDate AS DATE) = it.OvertimeDate
                            WHERE vtm.Id IS NOT NULL AND it.OvertimeDuration > 0
                        ) AS SOURCE ON TARGET.NoReg = SOURCE.NoReg AND TARGET.OvertimeDate = SOURCE.OvertimeDate AND TARGET.OvertimeInPlan = SOURCE.OvertimeIn
                        WHEN MATCHED THEN UPDATE SET
                            TARGET.OvertimeOutAdjust = SOURCE.OvertimeOut,
                            TARGET.DurationAdjust = SOURCE.OvertimeDuration,
                            TARGET.ModifiedBy = '{1}',
                            TARGET.CreatedOn = GETDATE(),
                            TARGET.ModifiedOn = GETDATE()
                        WHEN NOT MATCHED THEN
                            INSERT(NoReg, OvertimeDate, OvertimeInPlan, OvertimeOutPlan, OvertimeBreakPlan, OvertimeInAdjust, OvertimeOutAdjust, OvertimeBreakAdjust, DurationPlan, DurationAdjust, OvertimeCategoryCode, OvertimeReason, CreatedBy, CreatedOn, RowStatus)
                            VALUES(SOURCE.NoReg, SOURCE.OvertimeDate, SOURCE.OvertimeIn, SOURCE.OvertimeOut, SOURCE.OvertimeBreak, SOURCE.OvertimeIn, SOURCE.OvertimeOut, SOURCE.OvertimeBreak, SOURCE.OvertimeDuration, SOURCE.OvertimeDuration, 'pekerjaantambahan', 'Manual upload', '{1}', GETDATE(), 1);
                    ", tableName, actor);
                    command.ExecuteNonQuery();

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }

            if (exception != null) throw exception;
        }
        #endregion
    }
}

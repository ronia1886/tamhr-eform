using System;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Domain.Utility;
using Agit.Common.Extensions;
using Agit.Domain.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle employee work schedule master data.
    /// </summary>
    public class EmployeeWorkScheduleService : GenericDomainServiceBase<EmployeeWorkSchedule>
    {
        #region Domain Repositories
        protected IReadonlyRepository<EmployeeWorkScheduleMasterView> EmployeeWorkScheduleMasterReadonlyRepository => UnitOfWork.GetRepository<EmployeeWorkScheduleMasterView>();

        protected IReadonlyRepository<EmployeeSubstitutionView> EmployeeSubstitutionReadonlyRepository => UnitOfWork.GetRepository<EmployeeSubstitutionView>();

        protected IRepository<EmpWorkSchSubtitute> EmpWorkSchSubtituteRepository => UnitOfWork.GetRepository<EmpWorkSchSubtitute>();
        #endregion

        #region Variables & Properties
        protected override string[] Properties => new[] { "WorkScheduleRule" };
        protected string[] SubstitutionProperties => new[] { "ShiftCodeUpdate" };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public EmployeeWorkScheduleService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        public IQueryable<EmployeeWorkScheduleMasterView> GetMasterView()
        {
            return EmployeeWorkScheduleMasterReadonlyRepository.Fetch()
                .AsNoTracking();
        }

        /// <summary>
        /// Normalize employee work schedule
        /// </summary>
        public void NormalizeEmployeeWorkSchedule()
        {
            UnitOfWork.Transact(trans =>
            {
                UnitOfWork.UspQuery("SP_NORMALIZE_EMPLOYEE_WORK_SCHEDULE", new { }, trans);
            });
        }

        public IQueryable<EmployeeSubstitutionView> GetSubstitutions()
        {
            return EmployeeSubstitutionReadonlyRepository.Fetch()
                .AsNoTracking();
        }

        public EmployeeSubstitutionView GetSubstitution(Guid id)
        {
            return GetSubstitutions().Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        public void UpsertSubstitution(EmpWorkSchSubtitute input)
        {
            if (input.Id == default(Guid))
            {
                var substitution = EmpWorkSchSubtituteRepository.Fetch().FirstOrDefault(x => x.NoReg == input.NoReg && x.Date == input.Date);

                if (substitution != null)
                {
                    substitution.ShiftCodeUpdate = input.ShiftCodeUpdate;
                }
                else
                {
                    EmpWorkSchSubtituteRepository.Add(input);
                }
            }
            else
            {
                EmpWorkSchSubtituteRepository.Attach(input, SubstitutionProperties);
            }

            UnitOfWork.SaveChanges();
        }

        public void RemoveSubstitution(Guid id)
        {
            EmpWorkSchSubtituteRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        }

        public IEnumerable<EmployeeShiftStoredEntity> GetEmployeeShift(int year, int month)
        {
            return UnitOfWork.UdfQuery<EmployeeShiftStoredEntity>(new { year, month });
        }
        public IEnumerable<EmployeeShiftNoRegStoredEntity> GetEmployeeShiftByNoreg(string noReg, DateTime startDate, DateTime endDate)
        {
            return UnitOfWork.UdfQuery<EmployeeShiftNoRegStoredEntity>(new { noReg, startDate, endDate });
        }

        public void MergeSubstitution(string actor, DataTable dataTable)
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

                    command.CommandText = $"CREATE TABLE {dataTable.TableName}(NoReg VARCHAR(20), Date DATE, ShiftCode VARCHAR(20))";
                    command.ExecuteNonQuery();

                    //BulkCopier.BulkCopy(connection, dataTable, transaction);
                    using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                    {
                        bulkCopy.DestinationTableName = dataTable.TableName;
                        bulkCopy.WriteToServer(dataTable);
                    }

                    command.CommandText = string.Format(@"
                        MERGE INTO dbo.TB_R_TIME_MANAGEMENT_EMP_WORK_SCHEDULE_SUBSTITUTE AS TARGET
                        USING (
                            SELECT
                                it.*,
                                IIF(es.Holiday = 1 AND es.ChangeShift = 0, 'OFF', es.ShiftCode) AS ShiftCodeNormal
                            FROM {0} it
                            LEFT JOIN dbo.VW_EMPLOYEE_WORK_SCHEDULE es ON es.NoReg = it.NoReg AND es.Date = it.Date
                            WHERE it.ShiftCode <> IIF(es.Holiday = 1 AND es.ChangeShift = 0, 'OFF', es.ShiftCode)
                        ) AS SOURCE ON TARGET.NoReg = SOURCE.NoReg AND TARGET.Date = SOURCE.Date
                        WHEN MATCHED THEN UPDATE SET
                            TARGET.ShiftCodeUpdate = SOURCE.ShiftCode,
                            TARGET.ModifiedBy = '{1}',
                            TARGET.CreatedOn = GETDATE(),
                            TARGET.ModifiedOn = GETDATE()
                        WHEN NOT MATCHED THEN
                            INSERT(NoReg, Date, ShiftCode, ShiftCodeUpdate, CreatedBy, CreatedOn, RowStatus)
                            VALUES(SOURCE.NoReg, SOURCE.Date, SOURCE.ShiftCodeNormal, SOURCE.ShiftCode, '{1}', GETDATE(), 1);
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

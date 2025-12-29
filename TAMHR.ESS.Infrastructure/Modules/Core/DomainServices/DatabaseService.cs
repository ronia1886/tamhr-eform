using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Common;
using Agit.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Z.EntityFramework.Plus;
using System.Globalization;
using Agit.Common.Extensions;
using TAMHR.ESS.Infrastructure.ViewModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using Agit.Domain.Utility;
using Microsoft.Data.SqlClient;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle custom database configuration and query
    /// </summary>
    public class DatabaseService : DomainServiceBase
    {
        #region Repositories
        /// <summary>
        /// Database object schema readonly repository
        /// </summary>
        protected IReadonlyRepository<DatabaseObjectSchemaView> DatabaseObjectSchemaReadonlyRepository => UnitOfWork.GetRepository<DatabaseObjectSchemaView>();
        #endregion

        #region Constructor
        /// <summary>
        /// Constructore
        /// </summary>
        /// <param name="unitOfWork">Concrete UnitOfWork</param>
        public DatabaseService(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        /// <summary>
        /// Field that hold tag labels for caching
        /// </summary>
        private readonly string[] _tags = new[] { "menus", "roles", "permissions" };

        #endregion

        #region Database Object Schema Area
        public IEnumerable<DatabaseObjectSchemaView> GetSchema(Type tableType)
        {
            var attribute = tableType.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;
            var tableName = attribute.Name;

            return GetSchema(tableName);
        }

        public IEnumerable<DatabaseObjectSchemaView> GetSchema<T>() where T : IEntityBase<Guid>
        {
            return GetSchema(typeof(T));
        }

        public IEnumerable<DatabaseObjectSchemaView> GetSchema(string tableName)
        {
            return DatabaseObjectSchemaReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.TableName == tableName)
                .ToList();
        }

        public IEnumerable<DatabaseObjectSchemaView> GetSchema(Type tableType, IEnumerable<string> columns)
        {
            return GetSchema(tableType).Where(x => columns.Contains(x.ColumnName));
        }

        public IEnumerable<DatabaseObjectSchemaView> GetSchema<T>(IEnumerable<string> columns) where T : IEntityBase<Guid>
        {
            return GetSchema(typeof(T), columns);
        }

        public IEnumerable<DatabaseObjectSchemaView> GetSchema(string tableName, IEnumerable<string> columns)
        {
            return GetSchema(tableName).Where(x => columns.Contains(x.ColumnName));
        }

        public void Merge(DataTable dataTable, Type tableType, string[] columns, string[] keys, string actor, Dictionary<string, string> foreignKeys = null, Action<SqlCommand, string> callback = null)
        {
            var attribute = tableType.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;
            var tableName = attribute.Name;

            var schema = GetSchema(tableType, columns);
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
                    var onKeys = string.Join(" AND ", keys.Select(x => $"TARGET.{x} = SOURCE.{x}"));
                    var onUpdate = string.Join(", ", schema.Select(x => $"TARGET.{x.ColumnName} = SOURCE.{x.ColumnName}"));
                    var onCreateDefinition = string.Join(", ", schema.Select(x => x.ColumnName));
                    var onCreate = string.Join(", ", schema.Select(x => $"SOURCE.{x.ColumnName}"));
                    var tableAlias = "it";
                    var columnDefinitions = columns.Select(x => tableAlias + "." + x).ToArray();
                    var join = string.Empty;

                    if (foreignKeys != null)
                    {
                        var counter = 0;
                        foreach (var key in keys)
                        {
                            if (!foreignKeys.ContainsKey(key)) continue;

                            var foreignKey = foreignKeys[key].Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                            var idField = foreignKey[0];
                            var textFields = foreignKey[1].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            var foreignTableName = foreignKey[2];
                            var aliasName = $"mt{counter++}";
                            var joinColumns = string.Join(" AND ", textFields.Select(x => aliasName + "." + x.Split(':')[0] + " = " + tableAlias + "." + x.Split(':')[1]));

                            schema.FirstOrDefault(x => x.ColumnName == key).ColumnDefinition = $"{key} varchar(MAX)";
                            columnDefinitions[Array.IndexOf(columnDefinitions, $"{tableAlias}.{key}")] = $"{aliasName}.{idField} AS {key}";

                            join += $" LEFT JOIN {foreignTableName} {aliasName} ON {joinColumns}";
                        }
                    }

                    var columnStr = string.Join(", ", columnDefinitions);
                    var query = $"SELECT {columnStr} FROM {dataTable.TableName} " + tableAlias + join;

                    command.Transaction = transaction;
                    command.CommandType = CommandType.Text;

                    command.CommandText = $"CREATE TABLE {dataTable.TableName}({string.Join(", ", schema.Select(x => x.ColumnDefinition))})";
                    command.ExecuteNonQuery();

                    using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                    {
                        bulkCopy.DestinationTableName = dataTable.TableName;
                        bulkCopy.WriteToServer(dataTable);
                    }

                    callback?.Invoke(command, dataTable.TableName);

                    command.CommandText = string.Format(@"
                        MERGE INTO {0} AS TARGET
                        USING ({1}) AS SOURCE ON {2}
                        WHEN MATCHED THEN UPDATE SET {3}, TARGET.ModifiedBy = '{6}', TARGET.ModifiedOn = GETDATE()
                        WHEN NOT MATCHED THEN
                            INSERT({4}, CreatedBy, CreatedOn, RowStatus)
                            VALUES({5}, '{6}', GETDATE(), 1);
                    ", tableName, query, onKeys, onUpdate, onCreateDefinition, onCreate, actor);
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

        public void Merge<T>(DataTable dataTable, string[] columns, string[] keys, string actor) where T : IEntityBase<Guid>
        {
            var type = typeof(T);
            var attribute = type.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;
            var tableName = attribute.Name;

            var schema = GetSchema<T>(columns);
            var connection = UnitOfWork.GetConnection() as SqlConnection;
            var exceptionMessage = string.Empty;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                using (var transaction = connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    var onKeys = string.Join(" AND ", keys.Select(x => $"TARGET.{x} = SOURCE.{x}"));
                    var onUpdate = string.Join(", ", schema.Select(x => $"TARGET.{x.ColumnName} = SOURCE.{x.ColumnName}"));
                    var onCreateDefinition = string.Join(", ", schema.Select(x => x.ColumnName));
                    var onCreate = string.Join(", ", schema.Select(x => $"SOURCE.{x.ColumnName}"));

                    command.Transaction = transaction;
                    command.CommandType = CommandType.Text;

                    command.CommandText = $"CREATE TABLE {dataTable.TableName}({string.Join(", ", schema.Select(x => x.ColumnDefinition))})";
                    command.ExecuteNonQuery();

                    using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
                    {
                        bulkCopy.DestinationTableName = dataTable.TableName;
                        bulkCopy.WriteToServer(dataTable);
                    }

                    command.CommandText = string.Format(@"
                        MERGE INTO {0} AS TARGET
                        USING {1} AS SOURCE ON {2}
                        WHEN MATCHED THEN UPDATE SET {3}, TARGET.ModifiedBy = '{6}', TARGET.ModifiedOn = GETDATE()
                        WHEN NOT MATCHED THEN
                            INSERT({4}, CreatedBy, CreatedOn, RowStatus)
                            VALUES({5}, '{6}', GETDATE(), 1);
                    ", tableName, dataTable.TableName, onKeys, onUpdate, onCreateDefinition, onCreate, actor);

                    command.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
            catch(Exception ex) {
                exceptionMessage = ex.Message;
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }

            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                throw new Exception(exceptionMessage);
            }
        }
        #endregion
    }
}

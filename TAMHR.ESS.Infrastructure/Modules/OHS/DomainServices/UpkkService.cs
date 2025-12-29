using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Domain;
using Agit.Domain.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Agit.Common.Extensions;
using Dapper;
using Z.EntityFramework.Plus;
using TAMHR.ESS.Infrastructure.ViewModels;
using Agit.Common;
using OfficeOpenXml;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Globalization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Security.Cryptography;
using Agit.Domain.Utility;
using System.ComponentModel.DataAnnotations.Schema;
using FastMember;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle master data management
    /// </summary>
    public class UpkkService : DomainServiceBase
    {
        #region Repositories
        /// <summary>
        protected IRepository<UPKK> UpkkRepository => UnitOfWork.GetRepository<UPKK>();
        protected IReadonlyRepository<VisitUpkkView> UpkkViewReadonlyRepository => UnitOfWork.GetRepository<VisitUpkkView>();

        /// </summary>
        protected IReadonlyRepository<ActualReportingStructureView> ActualReportingStructureReadonlyRepository => UnitOfWork.GetRepository<ActualReportingStructureView>();

        protected IReadonlyRepository<DatabaseObjectSchemaView> DatabaseObjectSchemaReadonlyRepository => UnitOfWork.GetRepository<DatabaseObjectSchemaView>();
        #endregion
        private readonly string[] Properties = new[] {
            //"Id",
            "LokasiUPKK",
            "KategoriKunjungan",
            "TanggalKunjungan",
            "Divisi",
            "Noreg",
            "Usia",
            "JenisPekerjaan",
            "AreaId",
            "LokasiKerja",
            "Keluhan",
            "TDSistole",
            "TDDiastole",
            "Nadi",
            "Respirasi",
            "Suhu",
            "Diagnosa",
            "KategoriPenyakit",
            "SpesifikPenyakit",
            "JenisKasus",
            "Treatment",
            "Pemeriksa",
            "NamaPemeriksa",
            "HasilAkhir"



        };

        #region Constructor
        public UpkkService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion


        #region get schema
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
        #endregion

        protected object ChangeType(object value, Type dataType)
        {
            var castedValue = value;

            if (dataType == typeof(TimeSpan))
            {
                if (value is DateTime)
                {
                    var dt = (DateTime)value;
                    castedValue = dt.TimeOfDay;
                }
                else if (value is string)
                {
                    castedValue = TimeSpan.Parse((string)value);
                }
            }
            else if (dataType == typeof(DateTime))
            {
                if (value is string)
                {
                    var cleanValue = (((string)value) ?? string.Empty).Trim();

                    // Menggunakan overload ParseExact dengan string[] untuk mendukung format jamak
                    if (!DateTime.TryParseExact(
                            cleanValue,
                            new[] { "yyyy-MM-dd", "yyyy-MM-dd HH:mm", "dd/MM/yyyy", "dd/MM/yyyy HH:mm" },
                            CultureInfo.CurrentCulture,
                            DateTimeStyles.None,
                            out var result))
                    {
                        throw new FormatException($"The value '{cleanValue}' is not in a valid date format.");
                    }

                    castedValue = result;
                }
            }
            else if (dataType == typeof(Guid))
            {
                castedValue = Guid.Parse((string)value);
            }

            return Convert.ChangeType(castedValue, dataType);
        }
        public void UploadAndMergeTamAsync<T>(string Noreg, ExcelWorksheet workSheet, Dictionary<string, Type> columns, string[] columnKeys, string[] excludedColumns, Dictionary<string, string> foreignKeys = null, Action<SqlCommand, string> callback = null, Func<DataColumn, ExcelRange, int, object, object> valueCallback = null) where T : IEntityBase<Guid>
        {
            MergeTam<T>(Noreg, workSheet, columns, columnKeys, excludedColumns, foreignKeys, callback, valueCallback);
        }
        public void MergeTam<T>(string Noreg, ExcelWorksheet workSheet, Dictionary<string, Type> columns, string[] columnKeys, string[] excludedColumns, Dictionary<string, string> foreignKeys = null, Action<SqlCommand, string> callback = null, Func<DataColumn, ExcelRange, int, object, object> valueCallback = null) where T : IEntityBase<Guid>
        {
            if (workSheet == null) return;

            var tempData = string.Empty;
            var cols = columns.Select(x => x.Key).ToList();
            var counter = 1;
            var columnKeyIndex = 1;

            cols = cols.Where(x => !excludedColumns.Contains(x)).ToList();

            columns = columns.Where(x => !excludedColumns.Contains(x.Key)).ToDictionary(i => i.Key, i => i.Value);

            counter = 3;
            var dataTableName = "#temp_" + Guid.NewGuid().ToString().Replace("-", string.Empty);
            var dt = new DataTable(dataTableName);
            var totalColumns = columns.Count();

            columns.ForEach(x => dt.Columns.Add(x.Key, Nullable.GetUnderlyingType(x.Value) ?? x.Value));

            Assert.ThrowIf(columns.Count() == columnKeys.Count(), "Merge failed. Columns must be defined");
            string[] exclude = excludedColumns;
            do
            {
                excludedColumns = exclude;
                var key = workSheet.Cells[counter, columnKeyIndex].Text;

                if (string.IsNullOrEmpty(key))
                {
                    break;
                }

                var row = dt.NewRow();
                var arrayList = new ArrayList();
                var i = 1;

                foreach (DataColumn column in dt.Columns)
                {

                    //foreach (string ex in excludedColumns)
                    //{
                    //    if (ex.Contains((string)workSheet.Cells[1, i].Value))
                    //    {
                    //        i++;
                    //        excludedColumns = excludedColumns.Where(n => n != ex).ToArray();
                    //        continue;
                    //    }
                    //}

                    if (column.ColumnName == "CreatedOn")
                    {
                        arrayList.Add(DateTime.Now);
                        continue;
                    }
                    if (column.ColumnName == "Company")
                    {
                        arrayList.Add("TAM");
                        continue;
                    }
                    if (column.ColumnName == "Divisi")
                    {
                        var noregCell = workSheet.Cells[counter, 4].Text;

                        //var users = new EmployeeProfileService(this.UnitOfWork).getEmployee();
                        //var divisi = users.Where(x => x.Noreg == noregCell)
                        //                  .Select(x => x.Divisi)  // Ambil hanya Divisi
                        //                  .Distinct()
                        //                  .FirstOrDefault();  // Ambil yang pertama jika ada

                        var set = UnitOfWork.GetRepository<EmployeProfileView>();
                        var div = set.Fetch()
                                     .AsNoTracking()
                                     .Where(x => x.Noreg == noregCell) // Filter berdasarkan Noreg dari Excel
                                     .Select(x => x.Divisi)  // Ambil hanya Divisi
                                     .Distinct()
                                     .FirstOrDefault(); // Ambil satu hasil
                        arrayList.Add(div ?? "");
                        continue;
                    }
                    object castedValue = null;

                    var cell = workSheet.Cells[counter, i++];
                    var value = column.DataType == typeof(string) ? cell.Text : cell.Value;


                    if (valueCallback != null)
                    {
                        castedValue = valueCallback(column, workSheet.Cells, counter, value);
                    }

                    if (castedValue == null)
                    {
                        castedValue = column.AllowDBNull && (value == null || string.IsNullOrEmpty(value.ToString()))
                            ? DBNull.Value
                            : ChangeType(value, column.DataType);
                    }

                    arrayList.Add(castedValue);


                }
                row.ItemArray = arrayList.ToArray();
                dt.Rows.Add(row);
                counter++;
            }
            while (true);

            Assert.ThrowIf(dt.Rows.Count == 0, "Merge failed. There is no data to be merge");
            // var databaseService = new DatabaseService(UnitOfWork);
            //databaseService.Merge(dt, typeof(T), cols.ToArray(), columnKeys, Noreg, foreignKeys, callback);
            MergeUpkkTam(dt, typeof(T), cols.ToArray(), columnKeys, Noreg, foreignKeys, callback);
        }

        public bool MergeUpkkTam(DataTable dataTable, Type tableType, string[] columns, string[] keys, string actor, Dictionary<string, string> foreignKeys = null, Action<SqlCommand, string> callback = null)
        {
            var attribute = tableType.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;
            var tableName = attribute.Name;

            var schema = GetSchema(tableType, columns);

            var connection = UnitOfWork.GetConnection() as SqlConnection;
            //if (connection == null)
            //{
            //    connection = new SqlConnection(UnitOfWork.GetConnection().ConnectionString);
            //}

            Exception exception = null;
            var valid = false;

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

                    //BulkCopier.BulkCopy(connection, dataTable, transaction);
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
                            INSERT({4}, CreatedBy, RowStatus)
                            VALUES({5}, '{6}', 1);
                    ", tableName, query, onKeys, onUpdate, onCreateDefinition, onCreate, actor);
                    command.ExecuteNonQuery();

                    transaction.Commit();
                    valid = true;
                }
            }
            catch (Exception ex)
            {
                valid = false;
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
            return valid;
        }
        public bool MergeUpkkVendorDb(DataTable dataTable, Type tableType, string[] columns, string[] keys, string actor, Dictionary<string, string> foreignKeys = null, Action<SqlCommand, string> callback = null)
        {
            var attribute = tableType.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;
            var tableName = attribute.Name;

            var schema = GetSchema(tableType, columns);
            var connection = UnitOfWork.GetConnection() as SqlConnection;
            //if (connection == null)
            //{
            //    connection = new SqlConnection(UnitOfWork.GetConnection().ConnectionString);
            //}
            Exception exception = null;
            var valid = false;

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

                    //BulkCopier.BulkCopy(connection, dataTable, transaction);
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
                            INSERT({4}, CreatedBy, RowStatus)
                            VALUES({5}, '{6}', 1);
                    ", tableName, query, onKeys, onUpdate, onCreateDefinition, onCreate, actor);
                    command.ExecuteNonQuery();

                    transaction.Commit();
                    valid = true;
                }
            }
            catch (Exception ex)
            {
                valid = false;
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
            return valid;
        }

        public void UploadAndMergeUpkkVendorAsync<T>(string Noreg, ExcelWorksheet workSheet, Dictionary<string, Type> columns, string[] columnKeys, string[] excludedColumns, Dictionary<string, string> foreignKeys = null, Action<SqlCommand, string> callback = null, Func<DataColumn, ExcelRange, int, object, object> valueCallback = null) where T : IEntityBase<Guid>
        {
            MergeUpkkvendor<T>(Noreg, workSheet, columns, columnKeys, excludedColumns, foreignKeys, callback, valueCallback);
        }
        public void MergeUpkkvendor<T>(string Noreg, ExcelWorksheet workSheet, Dictionary<string, Type> columns, string[] columnKeys, string[] excludedColumns, Dictionary<string, string> foreignKeys = null, Action<SqlCommand, string> callback = null, Func<DataColumn, ExcelRange, int, object, object> valueCallback = null) where T : IEntityBase<Guid>
        {
            if (workSheet == null) return;

            var tempData = string.Empty;
            var cols = columns.Select(x => x.Key).ToList();
            var counter = 1;
            var columnKeyIndex = 1;

            cols = cols.Where(x => !excludedColumns.Contains(x)).ToList();

            columns = columns.Where(x => !excludedColumns.Contains(x.Key)).ToDictionary(i => i.Key, i => i.Value);

            counter = 3;
            var dataTableName = "#temp_" + Guid.NewGuid().ToString().Replace("-", string.Empty);
            var dt = new DataTable(dataTableName);
            var totalColumns = columns.Count();

            columns.ForEach(x => dt.Columns.Add(x.Key, Nullable.GetUnderlyingType(x.Value) ?? x.Value));

            Assert.ThrowIf(columns.Count() == columnKeys.Count(), "Merge failed. Columns must be defined");
            string[] exclude = excludedColumns;
            do
            {
                excludedColumns = exclude;
                var key = workSheet.Cells[counter, columnKeyIndex].Text;

                if (string.IsNullOrEmpty(key))
                {
                    break;
                }

                var row = dt.NewRow();
                var arrayList = new ArrayList();
                var i = 1;

                foreach (DataColumn column in dt.Columns)
                {
                    if (column.ColumnName == "Noreg")
                    {
                        var Company = workSheet.Cells[counter, 4].Text;
                        var Divisi = workSheet.Cells[counter, 5].Text;
                        var Nama = workSheet.Cells[counter, 6].Text;
                        var TgllahirString = workSheet.Cells[counter, 7].Text;
                        var JenisKelamin = workSheet.Cells[counter, 9].Text;
                        DateTime? Tgllahir = null;
                        // Coba parsing string ke DateTime
                        if (DateTime.TryParseExact(TgllahirString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                        {
                            Tgllahir = parsedDate;
                        }
                        var exists = GetDataUpkkVendor(Company, Divisi, Nama, Tgllahir, JenisKelamin);

                        string newNumber;
                        if (exists != null)
                        {
                            // Jika data sudah ada, gunakan Noreg dari data yang ditemukan
                            newNumber = exists;
                        }
                        else

                        {
                            var set = UnitOfWork.GetRepository<UPKK>();

                            var lastNumberVendor = set.Fetch()
                                                    .Where(v => v.Noreg.StartsWith("V"))
                                                    .OrderByDescending(v => v.Noreg)
                                                    .Select(v => v.Noreg)
                                                    .FirstOrDefault();

                            // Kalau tidak ada data, mulai dari V00001

                            if (string.IsNullOrEmpty(lastNumberVendor))
                            {
                                newNumber = "V00001";
                            }
                            else
                            {
                                // Ambil angka dari format VXXXXX
                                string numericPart = lastNumberVendor.Substring(1); // Ambil bagian angka setelah 'V'

                                if (int.TryParse(numericPart, out int maxNumber))
                                {
                                    maxNumber++; // Tambah 1
                                    newNumber = $"V{maxNumber:D5}"; // Format ulang jadi V000XX
                                }
                                else
                                {
                                    // Jika ada error parsing, mulai dari awal
                                    newNumber = "V00001";
                                }
                            }

                        }
                        arrayList.Add(newNumber);
                        Console.WriteLine($"Nomor baru: {newNumber}");

                    }

                    else
                    {
                        if (column.ColumnName == "CreatedOn")
                        {
                            arrayList.Add(DateTime.Now);
                            continue;
                        }
                        object castedValue = null;
                        var cell = workSheet.Cells[counter, i++];
                        var value = column.DataType == typeof(string) ? cell.Text : cell.Value;

                        //object value = null;
                        //object castedValue = null;
                        //var cell = workSheet.Cells[counter, i++];

                        //// Periksa tipe data kolom
                        //if (column.DataType == typeof(string))
                        //{
                        //    value = cell.Text;  // Jika string, gunakan teks langsung
                        //}
                        //else if (cell.Value is double dblValue)
                        //{
                        //    value = DateTime.FromOADate(dblValue);  // Konversi angka Excel ke DateTime
                        //}
                        //else
                        //{
                        //    value = cell.Value;  // Biarkan nilai apa adanya untuk tipe lain
                        //}



                        if (valueCallback != null)
                        {
                            castedValue = valueCallback(column, workSheet.Cells, counter, value);
                        }

                        if (castedValue == null)
                        {
                            castedValue = column.AllowDBNull && (value == null || string.IsNullOrEmpty(value.ToString()))
                                ? DBNull.Value
                                : ChangeType(value, column.DataType);
                        }
                        arrayList.Add(castedValue);
                    }

                }

                row.ItemArray = arrayList.ToArray();
                dt.Rows.Add(row);
                counter++;
            }
            while (true);

            Assert.ThrowIf(dt.Rows.Count == 0, "Merge failed. There is no data to be merge");
            //var databaseService = new DatabaseService(UnitOfWork);
            //databaseService.Merge(dt, typeof(T), cols.ToArray(), columnKeys, Noreg, foreignKeys, callback);
            MergeUpkkVendorDb(dt, typeof(T), cols.ToArray(), columnKeys, Noreg, foreignKeys, callback);

        }

        public bool GetDataUpkkTam(string Noreg)
        {
            var set = UnitOfWork.GetRepository<UPKK>();

            // Filter data berdasarkan kondisi
            var exists = set.Fetch()
                            .AsNoTracking()
                            .Any(x => x.Noreg == Noreg);

            // Return hasil pengecekan
            return exists;
        }
        public string GetDataUpkkVendor(string Company, string Divisi, string Nama, DateTime? Tgllahir, string JenisKelamin)
        {
            var set = UnitOfWork.GetRepository<UPKK>();

            //// Ambil data UPKK berdasarkan filter
            //var existingData = set.Fetch()
            //                      .AsNoTracking()
            //                      .FirstOrDefault(x => x.Company == Company
            //                                        && x.Divisi == Divisi
            //                                        && x.NamaEmployeeVendor == Nama
            //                                        && x.JenisKelaminEmployeeVendor == JenisKelamin
            //                                        && x.TanggalLahirVendor == Tgllahir);

            //// Return hasil pengecekan
            //return existingData.Noreg;

            // Ambil hanya Noreg dari data yang sesuai dengan filter

            var noreg = set.Fetch()
                           .AsNoTracking()
                           .Where(x => x.Company == Company
                                    && x.Divisi == Divisi
                                    && x.NamaEmployeeVendor == Nama
                                    && x.JenisKelaminEmployeeVendor == JenisKelamin
                                    && x.TanggalLahirVendor == Tgllahir)
                           .Select(x => x.Noreg)  // Ambil hanya kolom Noreg
                           .FirstOrDefault();      // Ambil satu hasil atau null jika tidak ada

            return noreg; // Bisa null jika data tidak ditemukan
        }


        public IEnumerable<UpkkDetailStoredEntity> GetUpkkDetail(string noreg, DateTime? dateFrom, DateTime? dateTo)
        {
            return UnitOfWork.UspQuery<UpkkDetailStoredEntity>(new { Noreg = noreg, DateFrom = dateFrom, DateTo = dateTo });
        }

        public VisitUpkkView GetPopUpUpkk(Guid id)
        {
            return UpkkViewReadonlyRepository.Fetch().AsNoTracking().Where(x => x.Id == id).FirstOrDefaultIfEmpty();
        }
        public void Upsert(UPKK param)
        {
            UpkkRepository.Upsert<Guid>(param, Properties);

            UnitOfWork.SaveChanges();
        }
        public void Delete(Guid id)
        {
            var item = UpkkRepository.Fetch()
                .Where(x => x.Id == id)
                .FirstOrDefault();

            if (item != null)
            {
                item.RowStatus = false;

                UpkkRepository.Upsert<Guid>(item, Properties);
                UnitOfWork.SaveChanges();
            }
        }

    }
}

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
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Domain.Utility;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle master data management
    /// </summary>
    public class McuService : DomainServiceBase
    {
        #region Repositories
        protected IReadonlyRepository<McuView> McuViewReadonlyRepository => UnitOfWork.GetRepository<McuView>();
        protected IRepository<PersonalDataMedicalHistory> McuRepository => UnitOfWork.GetRepository<PersonalDataMedicalHistory>();
        protected IReadonlyRepository<DatabaseObjectSchemaView> DatabaseObjectSchemaReadonlyRepository => UnitOfWork.GetRepository<DatabaseObjectSchemaView>();

        #endregion
        private readonly string[] Properties = new[] {
            //"Id",
            "NoReg",
            //"YearPeriod",
            "GlukosaPuasa",
            "SGOT",
            "SGPT",
            "HBsAg",
            "AntiHBs",
            "AsamUrat",
            "KolesterolTotal",
            "StatusBMI",
            "TekananDarah",
            "Kreatinin",
            "HasilEKG",
            "HasilRontgen",
            "HealthEmployeeStatus",
            "Paket",
            "LokasiMCU",
            "Usia",
            "PenyakitPernahDiderita",
            "Perokok",
            "JumlahBatangPerHari",
            "Miras",
            "Olahraga",
            "FrequencyPerMinggu",
            "Kebisingan",
            "SuhuExtremePanasAtauDingin",
            "Radiasi",
            "GetaranLokal",
            "GetaranSeluruhTubuh",
            "LainnyaFisika",
            "Debu",
            "Asap",
            "LimbahB3",
            "LainnyaKimia",
            "BakteriAtaVirusAtauJamurAtauParasit",
            "LainnyaBiologi",
            "GerakanBerulangDenganTangan",
            "AngkatBerat",
            "DudukLama",
            "BerdiriLama",
            "PosisiTubuhTidakErgonomis",
            "PencahayaanTidakSesuai",
            "BekerjaDepanLayar",
            "LainnyaErgonomis",
            "RiwayatHipertensi",
            "RiwayatDiabetes",
            "RiwayatPenyakitJantung",
            "RiwayatPenyakitGinjal",
            "RiwayatGangguanMental",
            "RiwayatPenyakitLain",
            "PenyakitSaatIni",
            "SedangBerobat",
            "ObatYangDiberikan",
            "Tinggi",
            "Berat",
            "TekananDarahSistol",
            "TekananDarahDiastol",
            "KesimpulanEKG",
            "KesimpulanTreadmill",
            "KesanPhotoRontgen",
            "KesimpulanUsgAbdomen",
            "HasilUsgMammae",
            "KesimpulanUsgMammae",
            "KesimpulanFisik",
            "KesimpulanButaWarna",
            "KesimpulanPemVisusMata",
            "HasilPapsmear",
            "KesimpulanPamsmear",
            "Hemoglobine",
            "Hematocrit",
            "Leucocyte",
            "TotalPlatelets",
            "Eryrocyte",
            "MCV",
            "MCH",
            "MCHC",
            "ESR",
            "HbA1c",
            "GammaGT",
            "Ureum",
            "GFR",
            "HDL",
            "LDL",
            "Triglyceride",
            "PlateletAggregation",
            "Fibrinogen",
            "CEA",
            "PSA",
            "Ca125",
            "VitD25OH",
            "UrinDarah",
            "UrinBakteri",
            "UrinKristal",
            "UrinLeukosit",
            "ScoreAmbiguity",
            "KesimpulanAmbiguity",
            "ScoreConflict",
            "KesimpulanConflict",
            "ScoreQuantitative",
            "KesimpulanQuantitative",
            "ScoreQualitative",
            "KesimpulanQualitative",
            "ScoreCareerDevelopment",
            "KesimpulanCareerDevelopment",
            "ScoreResponsibilityforPeople",
            "KesimpulanResponsibilityforPeople",
            "KesimpulanLab",
            "Saran"

        };


        #region Constructor
        public McuService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion
        #region get schema ulpload
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
        public void UploadAndMergeAsync<T>(string Noreg, ExcelWorksheet workSheet, Dictionary<string, Type> columns, string[] columnKeys, string[] excludedColumns, Dictionary<string, string> foreignKeys = null, Action<SqlCommand, string> callback = null, Func<DataColumn, ExcelRange, int, object, object> valueCallback = null) where T : IEntityBase<Guid>
        {
            Merge<T>(Noreg, workSheet, columns, columnKeys, excludedColumns, foreignKeys, callback, valueCallback);
        }
        public void Merge<T>(string Noreg, ExcelWorksheet workSheet, Dictionary<string, Type> columns, string[] columnKeys, string[] excludedColumns, Dictionary<string, string> foreignKeys = null, Action<SqlCommand, string> callback = null, Func<DataColumn, ExcelRange, int, object, object> valueCallback = null) where T : IEntityBase<Guid>
        {
            Exception exception = null;
            string columnError = "";
            string cellError = "";
            try
            {
                if (workSheet == null) return;

                var tempData = string.Empty;
                var cols = columns.Select(x => x.Key).ToList();
                var counter = 3;
                var columnKeyIndex = 4;

                cols = cols.Where(x => !excludedColumns.Contains(x)).ToList();

                columns = columns.Where(x => !excludedColumns.Contains(x.Key)).ToDictionary(i => i.Key, i => i.Value);


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
                        columnError = column.ToString();

                        foreach (string ex in excludedColumns)
                        {
                            if (ex.Contains((string)workSheet.Cells[1, i].Value))
                            {
                                i++;
                                excludedColumns = excludedColumns.Where(n => n != ex).ToArray();
                                continue;
                            }
                        }

                        //if (column.ColumnName == "YearPeriod")
                        //{
                        //    arrayList.Add(DateTime.Now.Year);
                        //    continue;
                        //}
                        if (column.ColumnName == "CreatedOn")
                        {
                            arrayList.Add(DateTime.Now);
                            continue;
                        }

                        object castedValue = null;
                        var cell = workSheet.Cells[counter, i++];
                        cellError = cell.ToString();
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
                //var databaseService = new DatabaseService(UnitOfWork);
                //databaseService.Merge(dt, typeof(T), cols.ToArray(), columnKeys, Noreg, foreignKeys, callback);
                bool valid = MergeMcuTam(dt, typeof(T), cols.ToArray(), columnKeys, Noreg, foreignKeys, callback);
                if (valid)//email
                {

                    foreach (DataRow row in dt.Rows)
                    {
                        string noregKey = row["NoReg"].ToString(); // Sesuaikan nama kolom "Noreg"
                        string HealthEmployeeStatus = row["HealthEmployeeStatus"].ToString();
                        string Saran = row["Saran"].ToString();
                        if (!string.IsNullOrEmpty(noregKey) && (HealthEmployeeStatus == "TEMPORARY UNFIT" || HealthEmployeeStatus == "FIT DENGAN RESTRIKSI"))
                        {
                            SendEmailAsync(noregKey, HealthEmployeeStatus, Saran); // Panggil fungsi pengiriman email
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                exception = ex;
                string message = ex.Message + " kolom : "  + columnError + " Baris ke : " + cellError;
                Assert.ThrowIf(message != null, message);

            }

        }
        public void SendEmailAsync(string noregKey, string HealthEmployeeStatus, string Saran)
        {

            var users = new UserService(this.UnitOfWork).GetByNoReg(noregKey);
            var emailService = new EmailService(UnitOfWork);
            var coreService = new CoreService(UnitOfWork);

            var emailTemplate = coreService.GetEmailTemplate("ohs-mcu-reminder-email");
            var mailSubject = emailTemplate.Subject;
            var mailFrom = emailTemplate.MailFrom;
            var template = Scriban.Template.Parse(emailTemplate.MailContent);

            var mailManager = emailService.CreateEmailManager();
            var mailContent = template.Render(new
            {
                names = users.Name,
                hasil = HealthEmployeeStatus,
                saran = Saran,
                from = "OHS Admin",
                year = DateTime.Now.Year
            });

            mailManager.SendAsync(mailFrom, mailSubject, mailContent, string.Join(",", users.Email));
        }
        public bool MergeMcuTam(DataTable dataTable, Type tableType, string[] columns, string[] keys, string actor, Dictionary<string, string> foreignKeys = null, Action<SqlCommand, string> callback = null)
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


                    foreach (var column in schema)
                    {
                        
                        // Atau kalau Anda ingin general untuk semua kolom decimal tanpa skala:
                        if (column.ColumnDefinition != null &&
                            column.ColumnDefinition.Contains("decimal") &&
                            !column.ColumnDefinition.Contains("("))
                        {
                            column.ColumnDefinition = $"{column.ColumnName} decimal(18,2)";
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
                            VALUES({5}, '{6}',1);
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

        public IEnumerable<McuDetailStoredEntity> GetMcuDetail(string noreg, DateTime? dateFrom, DateTime? dateTo)
        {
            return UnitOfWork.UspQuery<McuDetailStoredEntity>(new { Noreg = noreg, DateFrom = dateFrom, DateTo = dateTo });
        }
        public PersonalDataMedicalHistory GetPopUpMcu(Guid id)
        {
            return McuRepository.Fetch().AsNoTracking().Where(x => x.Id == id).FirstOrDefaultIfEmpty();
        }
        public IQueryable<EmployeProfileView> Getnoreg()
        {
            var set = UnitOfWork.GetRepository<EmployeProfileView>();
            var div = set.Fetch().AsNoTracking().Where(x => x.Noreg != null);
            return div.Select(x => new EmployeProfileView { Noreg = x.Noreg }).Distinct().OrderBy(x => x.Noreg);

            //string fieldName = "Noreg";
            //var attribute = typeof(EmployeProfileView).GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault() as TableAttribute;

            //string sql = $"SELECT DISTINCT {fieldName} FROM {attribute.Name} WHERE {fieldName} IS NOT NULL";

            //return UnitOfWork.GetConnection().Query<string>(sql);

        }
        public bool GetDataMcu(string noreg, int period)
        {
            var set = UnitOfWork.GetRepository<PersonalDataMedicalHistory>();

            // Filter data berdasarkan kondisi
            var exists = set.Fetch()
                            .AsNoTracking()
                            .Any(x => x.NoReg == noreg && x.YearPeriod == period);

            // Return hasil pengecekan
            return exists;
        }
        public void Upsert(PersonalDataMedicalHistory param)
        {
            McuRepository.Upsert<Guid>(param, Properties);

            UnitOfWork.SaveChanges();
            if (param.HealthEmployeeStatus == "TEMPORARY UNFIT" || param.HealthEmployeeStatus == "FIT DENGAN RESTRIKSI") 
            {
                SendEmailAsync(param.NoregMcu, param.HealthEmployeeStatus, param.Saran); // Panggil fungsi pengiriman email
            }
            
        }
        public void Delete(Guid id)
        {
            var item = McuRepository.Fetch()
                .Where(x => x.Id == id)
                .FirstOrDefault();

            if (item != null)
            {
                item.RowStatus = false;

                McuRepository.Upsert<Guid>(item, Properties);
                UnitOfWork.SaveChanges();
            }
        }
    }
}

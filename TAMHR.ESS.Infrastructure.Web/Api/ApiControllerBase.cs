using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Globalization;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Antiforgery.Internal;
using TAMHR.ESS.Infrastructure.Helpers;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using Agit.Common;
using Agit.Common.Extensions;
using Kendo.Mvc;
using OfficeOpenXml;
using System.Drawing;

namespace TAMHR.ESS.Infrastructure.Web
{
    /// <summary>
    /// Api controller base class
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    [IgnoreAntiforgeryToken]
    public abstract class ApiControllerBase : ControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Core service object.
        /// </summary>
        public CoreService CoreService => ServiceProxy.GetService<CoreService>();

        /// <summary>
        /// Config service object.
        /// </summary>
        public ConfigService ConfigService => ServiceProxy.GetService<ConfigService>();

        /// <summary>
        /// Log service object.
        /// </summary>
        public LogService LogService => ServiceProxy.GetService<LogService>();

        /// <summary>
        /// Database service object.
        /// </summary>
        public DatabaseService DatabaseService => ServiceProxy.GetService<DatabaseService>();
        #endregion

        #region Variables & Properties
        /// <summary>
        /// Field that hold service proxy object.
        /// </summary>
        protected readonly ServiceProxy ServiceProxy;

        /// <summary>
        /// Access control list helper.
        /// </summary>
        protected AclHelper AclHelper => ServiceProxy.GetAclHelper();
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        public ApiControllerBase()
        {
            ServiceProxy = new ServiceProxy(this);
            OnInit();
        }
        #endregion

        /// <summary>
        /// On init event
        /// </summary>
        protected virtual void OnInit() { }

        /// <summary>
        /// Get content type by filename
        /// </summary>
        /// <param name="path">Filename Path</param>
        /// <returns>Content Type</returns>
        protected string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();

            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }

        /// <summary>
        /// Get list of MIME type
        /// </summary>
        /// <returns>List of MIME Type</returns>
        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
        }

        protected async Task<IActionResult> DownloadFromPath(string fullPath)
        {
            if (System.IO.File.Exists(fullPath))
            {
                using (var memory = new MemoryStream())
                {
                    using (var stream = new FileStream(fullPath, FileMode.Open))
                    {
                        await stream.CopyToAsync(memory);
                    }

                    memory.Position = 0;

                    return File(memory.ToArray(), GetContentType(fullPath), Path.GetFileName(fullPath));
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Log action to database
        /// </summary>
        /// <param name="message">Action Message</param>
        [Authorize(Policy ="Allowed")]
        public void LogAction(string message)
        {
            try
            {
                var username = ServiceProxy.UserClaim.Username;
                var isWebApi = !this.RouteData.DataTokens.Keys.Contains("area");
                var ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();
                var browser = HttpContext.Request.Headers["User-Agent"].FirstOrDefault();
                var area = RouteData.Values["area"];
                var controller = RouteData.Values["controller"];
                var action = RouteData.Values["action"];

                LogService.LogSuccess(username, ipAddress, browser, string.Format($"<b>Area: {area}</b><br/><b>Controller: {controller}</b><br/><b>Action: {action}</b>"), message);
            }
            catch (Exception)
            {
            }

        }

        /// <summary>
        /// Export to excel
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <param name="list">List of Data based on Selected Data Type</param>
        /// <param name="name">Exported Filename</param>
        /// <param name="excludes">Excluded Fields</param>
        /// <param name="autoFit">Autofit</param>
        /// <returns>Excel File</returns>
        [Authorize(Policy = "Allowed")]
        public ActionResult ExportToXlsx<T>(IEnumerable<T> list, string name, string[] excludes = null, bool autoFit = true, Action<string, object, ExcelRange> callback = null)
        {
            using (ExcelPackage package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(Path.GetFileNameWithoutExtension(name));
                var totalRows = list.Count();
                var columns = new List<string>();
                excludes = excludes ?? new[] { "Id", "CreatedBy", "CreatedOn", "ModifiedBy", "ModifiedOn", "RowStatus" };
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.PropertyType.Name != "EntityCollection`1" && p.PropertyType.Name != "EntityReference`1" && p.PropertyType.Name != p.Name && (excludes == null || !excludes.Contains(p.Name))).ToArray();

                foreach (PropertyInfo p in properties)
                {
                    columns.Add(p.Name);
                }

                for (int i = 0; i < columns.Count(); i++)
                {
                    var style = worksheet.Cells[1, i + 1].Style;
                    style.Font.Bold = true;
                    style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    style.Fill.BackgroundColor.SetColor(Color.Black);
                    style.Font.Color.SetColor(Color.White);

                    worksheet.Cells[1, i + 1].Value = Regex.Replace(columns[i], "(\\B[A-Z])", " $1");
                }

                var row = 1;
                foreach (T item in list)
                {
                    row++;
                    for (int x = 0; x < columns.Count(); x++)
                    {
                        var cell = worksheet.Cells[row, x + 1];

                        var style = cell.Style;
                        style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                        var o = properties[x].GetValue(item, null);
                        var value = o == null ? "" : (o is DateTime ? ((DateTime)o).ToString("dd/MM/yyyy") : o.ToString());
                        cell.Value = value;

                        callback?.Invoke(properties[x].Name, o, cell);
                    }
                }

                if (autoFit)
                {
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                }

                using (var memoryStream = new MemoryStream())
                {
                    package.SaveAs(memoryStream);

                    var fileName = name.ToLower().EndsWith(".xlsx") ? name : $"Exported-{name}-" + DateTime.Now.ToString("dd-MM-yyyy") + ".xlsx";
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                    memoryStream.Position = 0;

                    return File(memoryStream.ToArray(), contentType, fileName);
                }
            }
        }

        /// <summary>
        /// Export template to excel
        /// </summary>
        /// <typeparam name="T">Data Type</typeparam>
        /// <returns>Excel File</returns>
        [Authorize(Policy = "Allowed")]
        public ActionResult GenerateTemplate<T>(string name, string[] excludes = null, bool autoFit = true) where T : IEntityBase<Guid>
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add(Path.GetFileNameWithoutExtension(name));
                var columns = new List<string>();
                excludes = excludes ?? new[] { "Id", "CreatedBy", "CreatedOn", "ModifiedBy", "ModifiedOn", "RowStatus" };
                var schema = DatabaseService.GetSchema<T>();
                var schemaColumns = schema.Where(x => !excludes.Contains(x.ColumnName));

                foreach (var p in schemaColumns)
                {
                    columns.Add(p.ColumnName);
                }

                for (int i = 0; i < columns.Count(); i++)
                {
                    var style = worksheet.Cells[1, i + 1].Style;
                    style.Font.Bold = true;
                    style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    style.Fill.BackgroundColor.SetColor(Color.Black);
                    style.Font.Color.SetColor(Color.White);

                    worksheet.Cells[1, i + 1].Value = Regex.Replace(columns[i], "(\\B[A-Z])", " $1");
                }

                if (autoFit)
                {
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                }

                using (var memoryStream = new MemoryStream())
                {
                    package.SaveAs(memoryStream);

                    var fileName = name.ToLower().EndsWith(".xlsx") ? name : $"Template-{name}.xlsx";
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                    memoryStream.Position = 0;

                    return File(memoryStream.ToArray(), contentType, fileName);
                }
            }
        }

        protected void Merge<T>(Stream stream, Dictionary<string, Type> columns, string[] columnKeys, Dictionary<string, string> foreignKeys = null, Action<SqlCommand, string> callback = null, Func<DataColumn, ExcelRange, int, object, object> valueCallback = null) where T : IEntityBase<Guid>
        {
            using (var package = new ExcelPackage(stream))
            {
                var workSheet = package.Workbook.Worksheets.FirstOrDefault();

                if (workSheet == null) return;

                var tempData = string.Empty;
                var cols = columns.Select(x => x.Key).ToList();
                var counter = 1;
                var columnKeyIndex = 1;
                var excludedColumns = new[] { "CreatedBy", "CreatedOn", "ModifiedBy", "ModifiedOn", "RowStatus" };

                cols = cols.Where(x => !excludedColumns.Contains(x)).ToList();

                counter = 2;
                var dataTableName = "#temp_" + Guid.NewGuid().ToString().Replace("-", string.Empty);
                var dt = new DataTable(dataTableName);
                var totalColumns = columns.Count();

                columns.ForEach(x => dt.Columns.Add(x.Key, Nullable.GetUnderlyingType(x.Value) ?? x.Value));

                Assert.ThrowIf(columns.Count() == columnKeys.Count(), "Merge failed. Columns must be defined");

                do
                {
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
                        var cell = workSheet.Cells[counter, i++];
                        var value = column.DataType == typeof(string) ? cell.Text : cell.Value;
                        object castedValue = null;

                        if (valueCallback != null)
                        {
                            castedValue = valueCallback(column, workSheet.Cells, counter, value);
                        }
                        
                        if (castedValue == null) {
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

                DatabaseService.Merge(dt, typeof(T), cols.ToArray(), columnKeys, ServiceProxy.UserClaim.NoReg, foreignKeys, callback);
            }

            if (stream != null)
            {
                stream.Dispose();
            }
        }

        protected void Merge<T>(ExcelWorksheet workSheet, Dictionary<string, Type> columns, string[] columnKeys, string[] excludedColumns, Dictionary<string, string> foreignKeys = null, Action<SqlCommand, string> callback = null, Func<DataColumn, ExcelRange, int, object, object> valueCallback = null) where T : IEntityBase<Guid>
        {
            if (workSheet == null) return;

            var tempData = string.Empty;
            var cols = columns.Select(x => x.Key).ToList();
            var counter = 1;
            var columnKeyIndex = 1;

            cols = cols.Where(x => !excludedColumns.Contains(x)).ToList();

            columns = columns.Where(x => !excludedColumns.Contains(x.Key)).ToDictionary(i => i.Key, i => i.Value);

            counter = 2;
            var dataTableName = "#temp_" + Guid.NewGuid().ToString().Replace("-", string.Empty);
            var dt = new DataTable(dataTableName);
            var totalColumns = columns.Count();

            columns.ForEach(x => dt.Columns.Add(x.Key, Nullable.GetUnderlyingType(x.Value) ?? x.Value));

            Assert.ThrowIf(columns.Count() == columnKeys.Count(), "Merge failed. Columns must be defined");

            do
            {
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
                    if (excludedColumns.Contains(workSheet.Cells[1, i].Value))
                    {
                        i++;
                    }

                    var cell = workSheet.Cells[counter, i++];
                    var value = column.DataType == typeof(string) ? cell.Text : cell.Value;
                    object castedValue = null;

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

            DatabaseService.Merge(dt, typeof(T), cols.ToArray(), columnKeys, ServiceProxy.UserClaim.NoReg, foreignKeys, callback);
        }

        protected void UploadAndMerge<T>(Stream stream, string[] columnKeys) where T : IEntityBase<Guid>
        {
            using (var package = new ExcelPackage(stream))
            {
                var workSheet = package.Workbook.Worksheets.FirstOrDefault();

                if (workSheet == null) return;

                var firstRow = 1;
                var col = 1;
                var tempData = string.Empty;
                var cols = new List<string>();
                var treshold = 100;
                var counter = 1;
                var columnKeyIndex = 1;
                var excludedColumns = new[] { "CreatedBy", "CreatedOn", "ModifiedBy", "ModifiedOn", "RowStatus" };

                do
                {
                    tempData = (string)workSheet.Cells[firstRow, col++].Value;

                    if (string.IsNullOrEmpty(tempData))
                    {
                        break;
                    }

                    if (tempData == columnKeys[0])
                    {
                        columnKeyIndex = counter;
                    }

                    cols.Add(tempData.Replace(" ", string.Empty));
                }
                while (counter++ <= treshold);

                cols = cols.Where(x => !excludedColumns.Contains(x)).ToList();

                counter = 2;
                var dataTableName = "#temp_" + Guid.NewGuid().ToString().Replace("-", string.Empty);
                var dt = new DataTable(dataTableName);
                var schema = DatabaseService.GetSchema<T>(cols).ToDictionary(x => x.ColumnName);
                var totalColumns = schema.Count();

                cols.ForEach(x =>
                {
                    var column = schema[x];
                    dt.Columns.Add(column.ColumnName, DataTypeHelper.GetClrType(column.DataType));
                });

                Assert.ThrowIf(schema.Count() == columnKeys.Count(), "Merge failed. Columns must be defined");

                do
                {
                    var key = (string)workSheet.Cells[counter, columnKeyIndex].Text;

                    if (string.IsNullOrEmpty(key))
                    {
                        break;
                    }

                    var row = dt.NewRow();
                    var arrayList = new ArrayList();
                    var i = 1;

                    foreach (DataColumn column in dt.Columns)
                    {
                        var cell = workSheet.Cells[counter, i++];
                        var value = column.DataType == typeof(string) ? cell.Text : cell.Value;

                        var castedValue = column.AllowDBNull && (value == null || string.IsNullOrEmpty(value.ToString()))
                            ? DBNull.Value
                            : ChangeType(value, column.DataType);

                        arrayList.Add(castedValue);
                    }

                    row.ItemArray = arrayList.ToArray();
                    dt.Rows.Add(row);
                    counter++;
                }
                while (true);

                Assert.ThrowIf(dt.Rows.Count == 0, "Merge failed. There is no data to be merge");

                DatabaseService.Merge<T>(dt, cols.ToArray(), columnKeys, ServiceProxy.UserClaim.NoReg);
            }

            if (stream != null)
            {
                stream.Dispose();
            }
        }

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
                    var cleanValue = (((string)value)??string.Empty).Trim();
                    castedValue = DateTime.ParseExact(cleanValue, new[] { "yyyy-MM-dd", "yyyy-MM-dd HH:mm", "dd/MM/yyyy", "dd/MM/yyyy HH:mm" }, CultureInfo.CurrentCulture);
                }
            }else if (dataType == typeof(Guid))
            {
                castedValue = Guid.Parse((string)value);
            }

            return Convert.ChangeType(castedValue, dataType);
        }

        protected void UploadAndMerge<T>(string filePath, string[] columnKeys) where T : IEntityBase<Guid>
        {
            string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            filePath = Path.GetFileName(filePath);
            string filePaths = Path.Combine(wwwRootPath, "uploads", "excel-template", filePath);
            UploadAndMerge<T>(System.IO.File.OpenRead(filePaths), columnKeys);
        }

        protected async Task UploadAndMergeAsync<T>(IFormFile file, string[] columnKeys) where T : IEntityBase<Guid>
        {
            Assert.ThrowIf(file == null, "Upload failed. Please choose a file");

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);

                UploadAndMerge<T>(ms, columnKeys);
            }
        }

        protected async Task UploadAndMergeAsync<T>(ExcelWorksheet workSheet, Dictionary<string, Type> columns, string[] columnKeys, string[] excludedColumns, Dictionary<string, string> foreignKeys = null, Action<SqlCommand, string> callback = null, Func<DataColumn, ExcelRange, int, object, object> valueCallback = null) where T : IEntityBase<Guid>
        {
            Merge<T>(workSheet, columns, columnKeys, excludedColumns, foreignKeys, callback, valueCallback);
        }

        protected async Task UploadAndMergeAsync<T>(IFormFile file, Dictionary<string, Type> columns, string[] columnKeys, Dictionary<string, string> foreignKeys = null, Action<SqlCommand, string> callback = null, Func<DataColumn, ExcelRange, int, object, object> valueCallback = null) where T : IEntityBase<Guid>
        {
            Assert.ThrowIf(file == null, "Upload failed. Please choose a file");

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);

                Merge<T>(ms, columns, columnKeys, foreignKeys, callback, valueCallback);
            }
        }

        protected virtual void RemoveFilter(IList<IFilterDescriptor> filters, string field)
        {
            foreach (IFilterDescriptor filterDescriptor in filters)
            {
                if (filterDescriptor is CompositeFilterDescriptor)
                {
                    var composite = filterDescriptor as CompositeFilterDescriptor;

                    RemoveFilter(composite.FilterDescriptors, field);
                }
                else
                {
                    var filter = filterDescriptor as FilterDescriptor;

                    if (filter.Member == field)
                    {
                        filters.Remove(filter);
                        return;
                    }
                }
            }
        }

        protected bool ValidateAntiForgery(string requestToken)
        {
            var antiforgery = ServiceProxy.GetAntiForgery();
            var options = ServiceProxy.ServiceProvider.GetService<IOptions<AntiforgeryOptions>>().Value;

            //typeof(DefaultAntiforgery).GetMethod("CheckSSLConfig", BindingFlags.NonPublic | BindingFlags.Instance)
            //    ?.Invoke(antiforgery, new object[] { HttpContext });

            var tokens = new AntiforgeryTokenSet(requestToken, HttpContext.Request.Cookies[options.Cookie.Name], options.FormFieldName, options.HeaderName);

            if (tokens.CookieToken == null)
            {
                throw new AntiforgeryValidationException("Cookie token cannot be null");
            }

            if (tokens.RequestToken == null)
            {
                throw new AntiforgeryValidationException("Request token cannot be null");
            }

            try
            {
                //typeof(DefaultAntiforgery).GetMethod("ValidateTokens", BindingFlags.NonPublic | BindingFlags.Instance)
                //    ?.Invoke(antiforgery, new object[] { HttpContext, tokens });

                return true;
            }
            catch
            {
                return false;
            }
        }



        /// <summary>
        /// Move file from given url to specified path.
        /// </summary>
        /// <param name="path">This source path.</param>
        /// <param name="fileUrl">This source url.</param>
        /// <param name="newFileName">This new filename if any.</param>
        /// <returns>This source url.</returns>
        protected string MoveFile(string path, string fileUrl, string newFileName = null)
        {
            var pathProvider = ServiceProxy.GetPathProvider();
            var assetsPath = pathProvider.ContentPath(path);
            var tempPath = pathProvider.ContentPath("temps");
            var extension = Path.GetExtension(fileUrl);
            var fileName = string.IsNullOrEmpty(newFileName) ? Path.GetFileName(fileUrl) : Path.GetFileName(newFileName);
            //var fileName = string.IsNullOrEmpty(newFileName) ? Path.GetFileName(fileUrl) : (newFileName + extension);
            var tempFileName = Path.GetFileName(fileUrl);
            var tempFilePath = Path.Combine(tempPath, tempFileName);
            var targetFilePath = Path.Combine(assetsPath, fileName);

            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            if (!Directory.Exists(assetsPath))
            {
                Directory.CreateDirectory(assetsPath);
            }

            if (System.IO.File.Exists(tempFilePath))
            {
                System.IO.File.Move(tempFilePath, targetFilePath);

                return $"~/{path}/{fileName}";
            }

            return fileUrl;
        }
    }
}

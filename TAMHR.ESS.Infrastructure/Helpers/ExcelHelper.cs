using System.IO;
using System.Data;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Globalization;
using OfficeOpenXml;
using TAMHR.ESS.Infrastructure.ViewModels;
using Agit.Common.Archieve;

namespace TAMHR.ESS.Infrastructure.Helpers
{
    public static class ExcelHelper
    {
        public static DataTable ToDataTable(this ExcelConfig excelConfig, string sourceFile)
        {
            var dt = new DataTable();
            var excelColumnConfig = excelConfig.ExcelColumnConfig.ToList();

            excelColumnConfig.ForEach(x => dt.Columns.Add(x.Field));

            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                using (var stream = File.OpenRead(sourceFile))
                {
                    package.Load(stream);
                }

                var ws = package.Workbook.Worksheets[excelConfig.SheetName];
                var startRow = excelConfig.StartRow;
                DataRow row;

                while (ws.Cells["A" + startRow].Value != null)
                {
                    row = dt.NewRow();
                    var vals = new ArrayList();

                    foreach (var col in excelConfig.ExcelColumnConfig)
                    {
                        vals.Add(ws.Cells[col.ColumnLabel + startRow].Value);
                    }

                    row.ItemArray = vals.ToArray();
                    dt.Rows.Add(row);
                    startRow++;
                }
            }

            return dt;
        }

        public static IEnumerable<ZipEntry> SaveToStreams(this DataSet set, IEnumerable<ExcelConfig> configs)
        {
            #region Comment
            //if (configs == null || configs.Count() == 0) return null;

            //var streams = new List<ContentEntry>();
            //using (var package = new OfficeOpenXml.ExcelPackage())
            //{

            //    foreach (DataTable dt in set.Tables) {
            //        if (!configs.Any(x => x.Name == dt.TableName) || dt.Rows.Count == 0) continue;

            //        var config = configs.FirstOrDefault(x => x.Name == dt.TableName);
            //        var columns = config.ExcelColumnConfig.ToList();
            //        var excelColumnConfig = columns.ToDictionary(x => x.Field);
            //        var startRow = config.StartRow <= 1 ? 2 : config.StartRow;


            //        var ws = package.Workbook.Worksheets.Add(config.SheetName);
            //        //ws.DefaultColWidth = 30;
            //        int i = 1;
            //        foreach (var col in columns)
            //        {
            //            var cell = ws.Cells[col.ColumnLabel + "1"];
            //            cell.Value = col.Field;
            //            cell.Style.Font.Bold = true;

            //            cell.Style.WrapText = true;
            //            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            //            cell.Style.Font.Color.SetColor(Color.White);

            //            var backgroundColor = col.IsEditable == true?Color.Green: Color.Gray;
            //            cell.Style.Fill.BackgroundColor.SetColor(backgroundColor);
            //            cell.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            //            cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            //            cell.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            //            cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;

            //            if (col.Width > 0)
            //            {
            //                ws.Column(i).Width = col.Width.Value;
            //            }
            //            else
            //            {
            //                cell.AutoFitColumns();
            //            }
            //            i++;

            //        }

            //        foreach (DataRow row in dt.Rows)
            //        {
            //            foreach (DataColumn col in dt.Columns)
            //            {
            //                if (!excelColumnConfig.ContainsKey(col.ColumnName)) continue;

            //                var columnConfig = excelColumnConfig[col.ColumnName];

            //                ws.Cells[columnConfig.ColumnLabel + startRow].Value = row[col];
            //                //ws.Cells[columnConfig.ColumnLabel + startRow].AutoFitColumns();
            //                ws.Cells[columnConfig.ColumnLabel + startRow].Style.WrapText = true;
            //                ws.Cells[columnConfig.ColumnLabel + startRow].Style.Border.Top.Style = ExcelBorderStyle.Thin;
            //                ws.Cells[columnConfig.ColumnLabel + startRow].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            //                ws.Cells[columnConfig.ColumnLabel + startRow].Style.Border.Left.Style = ExcelBorderStyle.Thin;
            //                ws.Cells[columnConfig.ColumnLabel + startRow].Style.Border.Right.Style = ExcelBorderStyle.Thin;

            //                if (columnConfig.Width > 0)
            //                {
            //                    ws.Column(i).Width = columnConfig.Width.Value;
            //                }
            //                else
            //                {
            //                    ws.Cells[columnConfig.ColumnLabel + startRow].AutoFitColumns();
            //                }
            //            }
            //            startRow++;
            //        }
            //        using (var ms = new MemoryStream())
            //        {
            //            package.SaveAs(ms);
            //            streams.Add(new ContentEntry(configs.FirstOrDefault(x => x.Name == dt.TableName).SourceFile, ms.ToArray()));
            //        }

            //    }

            //}
            #endregion

            if (configs == null || configs.Count() == 0) return null;

            var streams = new List<ZipEntry>();

            foreach (DataTable dt in set.Tables)
            {
                if (!configs.Any(x => x.Name == dt.TableName) || dt.Rows.Count == 0) continue;

                var config = configs.FirstOrDefault(x => x.Name == dt.TableName);
                var columns = config.ExcelColumnConfig.ToList();
                var excelColumnConfig = columns.ToDictionary(x => x.Field);
                var startRow = config.StartRow <= 1 ? 2 : config.StartRow;

                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    var ws = package.Workbook.Worksheets.Add(config.SheetName);

                    foreach (var col in columns)
                    {
                        var cell = ws.Cells[col.ColumnLabel + "1"];
                        cell.Value = col.Field;
                    }

                    foreach (DataRow row in dt.Rows)
                    {
                        foreach (DataColumn col in dt.Columns)
                        {
                            if (!excelColumnConfig.ContainsKey(col.ColumnName)) continue;

                            var columnConfig = excelColumnConfig[col.ColumnName];
                            //ws.Cells[columnConfig.ColumnLabel + startRow].Value = row[col];
                            ws.Cells[columnConfig.ColumnLabel + startRow].Value =System.Web.HttpUtility.HtmlEncode(row[col]?.ToString());
                        }
                        startRow++;
                    }

                    using (var ms = new MemoryStream())
                    {
                        package.SaveAs(ms);
                        streams.Add(new ZipEntry(config.SourceFile, ms.ToArray()));
                    }
                }
            }

            return streams;
        }

        

        public static DataTable ExcelToDataTable(string fileName, string sheetName = "", bool firstRowIsColumn = true)
        {
            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                var dt = new DataTable();
                using (var stream = File.OpenRead(fileName))
                {
                    package.Load(stream);
                }

                var ws = string.IsNullOrEmpty(sheetName) ? package.Workbook.Worksheets.First() : package.Workbook.Worksheets[sheetName];
                foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                {
                    dt.Columns.Add(firstRowIsColumn ? firstRowCell.Text : string.Format("Column {0}", firstRowCell.Start.Column));
                }

                var startRow = firstRowIsColumn ? 2 : 1;
                for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                    var row = dt.Rows.Add();
                    foreach (var cell in wsRow)
                    {
                        row[cell.Start.Column - 1] = cell.Text;
                    }
                }

                return dt;
            }
        }

    }
}

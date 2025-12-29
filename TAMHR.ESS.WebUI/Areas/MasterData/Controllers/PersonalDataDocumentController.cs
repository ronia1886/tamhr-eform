using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    /// <summary>
    /// Personal Data Document API Manager
    /// </summary>
    [Route("api/master-data/personal-data-document")]
    [Permission(PermissionKey.ManagePersonalDataDocument)]
    public class PersonalDataDocumentApiController : GenericApiControllerBase<PersonalDataDocumentService, PersonalDataDocument>
    {
        protected override string[] ComparerKeys => new[] { "NoReg", "DocumentTypeCode" };

        [HttpPost("gets")]
        public override async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            // 1. Ambil dokumen
            var documents = CommonService.GetDocuments();

            // 2. Ambil user aktif dari service yang sama
            var users = CommonService.GetActiveUsers();
            var userDict = users.ToDictionary(u => u.NoReg, u => u.Name);

            // 3. Join di memory untuk isi Name
            var dataWithName = documents
                .Select(doc =>
                {
                    doc.Name = userDict.TryGetValue(doc.NoReg, out var name) ? name : "-";
                    return doc;
                })
                .OrderBy(doc => doc.Name)
                .ToList();

            return dataWithName.ToDataSourceResult(request);
        }

        [HttpGet("download-report")]
        public IActionResult Download()
        {
            // 1. Ambil dokumen dari service
            var documents = CommonService.GetDocuments();

            // 2. Ambil user aktif
            var users = CommonService.GetActiveUsers();
            var userDict = users.ToDictionary(u => u.NoReg, u => u.Name);

            // 3. Join data dokumen + user name
            var dataWithName = documents
                .Select(doc =>
                {
                    doc.Name = userDict.TryGetValue(doc.NoReg, out var name) ? name : "-";
                    return doc;
                })
                .OrderBy(doc => doc.Name)
                .ToList();

            string title = "PersonalDataDocument";
            var fileName = $"{title}_Report.xlsx";

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    int rowIndex = 2;
                    var sheet = package.Workbook.Worksheets.Add("PersonalDataDocument");

                    // header kolom
                    var cols = new[]
                    {
                        "NoReg",
                        "Employee Name",
                        "Document Type Code",
                        "Document Name",
                        "Start Date",
                        "End Date"
                    };

                    for (var i = 1; i <= cols.Length; i++)
                    {
                        sheet.Cells[1, i].Value = cols[i - 1];
                        sheet.Cells[1, i].Style.Font.Bold = true;
                        sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        sheet.Cells[1, i].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        sheet.Cells[1, i].Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#b4c6e7"));
                        sheet.Column(i).Width = 25; // set default width
                    }

                    // isi data
                    foreach (var doc in dataWithName)
                    {
                        sheet.Cells[rowIndex, 1].Value = doc.NoReg;
                        sheet.Cells[rowIndex, 2].Value = doc.Name;
                        sheet.Cells[rowIndex, 3].Value = doc.DocumentTypeCode;
                        sheet.Cells[rowIndex, 4].Value = doc.DocumentValue;
                        sheet.Cells[rowIndex, 5].Value = doc.StartDate;
                        sheet.Cells[rowIndex, 5].Style.Numberformat.Format = "dd-MM-yyyy";
                        sheet.Cells[rowIndex, 6].Value = doc.EndDate;
                        sheet.Cells[rowIndex, 6].Style.Numberformat.Format = "dd-MM-yyyy";

                        for (var i = 1; i <= cols.Length; i++)
                        {
                            sheet.Cells[rowIndex, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }

                        rowIndex++;
                    }

                    package.SaveAs(ms);
                }

                // bisa return file langsung
                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);

                // atau return base64 biar bisa dipakai di frontend (kayak contohmu)
                //return Ok(Convert.ToBase64String(ms.ToArray()));
            }
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Personal data document page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewPersonalDataDocument)]
    public class PersonalDataDocumentController : GenericMvcControllerBase<PersonalDataDocumentService, PersonalDataDocument>
    {
        [HttpGet]
        public IActionResult PersonalDocumentType()
        {
            var data = new[]
            {
             new { Value = "bpjskesehatan", Text = "BPJS Kesehatan" },
             new { Value = "bpjsketenagakerjaan", Text = "BPJS Ketenagakerjaan" },
             new { Value = "danapensiunastra", Text = "Dana Pensiun Astra" }
        };

            return Json(data);
        }

        [HttpPost]
        public override IActionResult Load(Guid id)
        {
            PersonalDataDocument commonData;

            if (id == Guid.Empty)
            {
                commonData = new PersonalDataDocument();
            }
            else
            {
                commonData = CommonService.GetById(id);

                if (commonData != null && !string.IsNullOrEmpty(commonData.NoReg))
                {
                    var user = CommonService.GetUserByNoReg(commonData.NoReg);
                    if (user != null)
                    {
                        commonData.Name = user.Name;
                    }
                }
            }

            return GetViewData(commonData);
        }
    }
    #endregion
}
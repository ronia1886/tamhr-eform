using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using Kendo.Mvc.UI;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace TAMHR.ESS.WebUI.Areas.ClaimBenefit.Controllers
{
    #region API Controller
    /// <summary>
    /// COP fuel allowance API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.CopFuelAllowance)]
    public class CopFuelAllowanceApiController : FormApiControllerBase<CopFuelAllowanceViewModel>
    {
        #region Domain Services
        protected ApprovalService approvalService => ServiceProxy.GetService<ApprovalService>();
        protected MdmService mdmService => ServiceProxy.GetService<MdmService>();
        protected ClaimBenefitService claimBenefitService => ServiceProxy.GetService<ClaimBenefitService>();
        #endregion

        protected override void ValidateOnCreate(string formKey)
        {
            base.ValidateOnCreate(formKey);

            claimBenefitService.PreValidateCopFuelAllowance(ServiceProxy.UserClaim.NoReg, ServiceProxy.UserClaim.Name);
        }

        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<CopFuelAllowanceViewModel> copFuelAllowanceViewModel)
        {
            base.ValidateOnPostCreate(copFuelAllowanceViewModel);

            if (copFuelAllowanceViewModel.Object.data == null || copFuelAllowanceViewModel.Object.data.Count == 0) throw new Exception("Cannot create request because request is empty");
        }

        protected override void ValidateOnPostUpdate(DocumentRequestDetailViewModel<CopFuelAllowanceViewModel> copFuelAllowanceViewModel)
        {
            base.ValidateOnPostUpdate(copFuelAllowanceViewModel);

            if (copFuelAllowanceViewModel.Object.data == null || copFuelAllowanceViewModel.Object.data.Count == 0) throw new Exception("Cannot create request because request is empty");
        }

        public System.IO.MemoryStream GenerateExcel(Guid id)
        {
            try
            {
                ExcelPackage pck = new ExcelPackage();
                // Results Output
                System.IO.MemoryStream output = new System.IO.MemoryStream();
                //data
                var docDetail = approvalService.GetDocumentRequestDetailByApprovalId(id);
                var actualOrganizationStructure = mdmService.GetActualOrganizationStructure(docDetail.CreatedBy);
                CopFuelAllowanceViewModel model = JsonConvert.DeserializeObject<CopFuelAllowanceViewModel>(docDetail.ObjectValue);
                //new worksheet
                ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Klaim Bensin");
                //header
                ws.Cells["A2:G2"].Value = "Car Ownership Programme ( COP )";
                ws.Cells["A2:G2"].Merge = true; ws.Cells["A2:G2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; ws.Cells["A2:G2"].Style.Font.Bold = true;
                ws.Cells["A3:G3"].Value = "Formulir Klaim Bensin (Perjalanan Dinas Bulanan)";
                ws.Cells["A3:G3"].Merge = true; ws.Cells["A3:G3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; ws.Cells["A3:G3"].Style.Font.Bold = true;
                //header table
                ws.Cells[5, 1].Value = "Noreg";
                ws.Cells[5, 2].Value = " : " + actualOrganizationStructure.NoReg;
                ws.Cells[6, 1].Value = "Nama";
                ws.Cells[6, 2].Value = " : " + actualOrganizationStructure.Name;
                ws.Cells[7, 1].Value = "Divisi";
                ws.Cells[7, 2].Value = " : " + actualOrganizationStructure.OrgName;
                ws.Cells["A9:A10"].Value = "No"; ws.Cells["A9:A10"].Merge = true; ws.Cells["A9:A10"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["B9:B10"].Value = "Tanggal"; ws.Cells["B9:B10"].Merge = true; ws.Cells["B9:B10"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["C9:C10"].Value = "Tujuan"; ws.Cells["C9:C10"].Merge = true; ws.Cells["C9:C10"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["D9:E9"].Value = "Posisi KM"; ws.Cells["D9:E9"].Merge = true; ws.Cells["D9:E9"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[10, 4].Value = "Berangkaat"; ws.Cells[10, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells[10, 5].Value = "Kembali"; ws.Cells[10, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["F9:F10"].Value = "Total Pemakaian"; ws.Cells["F9:F10"].Merge = true; ws.Cells["F9:F10"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ws.Cells["G9:G10"].Value = "Keperluan"; ws.Cells["G9:G10"].Merge = true; ws.Cells["G9:G10"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                int idx = 1;
                int currRow = 10;
                int totalPemakaian = 0;
                foreach (var item in model.data)
                {
                    ws.Cells[10 + idx, 1].Value = idx;
                    ws.Cells[10 + idx, 2].Value = item.Date.Value.ToShortDateString();
                    ws.Cells[10 + idx, 3].Value = item.Destination;
                    ws.Cells[10 + idx, 4].Value = item.Start;
                    ws.Cells[10 + idx, 5].Value = item.Back;
                    ws.Cells[10 + idx, 6].Value = item.Back - item.Start;
                    ws.Cells[10 + idx, 7].Value = item.Necessity;

                    currRow += idx;
                    idx++;
                    totalPemakaian += item.Back - item.Start;
                }

                ws.SelectedRange[9, 1, currRow - 1, 7].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.SelectedRange[9, 1, currRow - 1, 7].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.SelectedRange[9, 1, currRow - 1, 7].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.SelectedRange[9, 1, currRow - 1, 7].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                ws.Cells[currRow, 4].Value = "TOTAL KM"; ws.Cells[currRow, 4, currRow, 5].Merge = true;
                ws.Cells[currRow, 6].Value = totalPemakaian;
                ws.SelectedRange[currRow, 4, currRow, 6].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.SelectedRange[currRow, 4, currRow, 6].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.SelectedRange[currRow, 4, currRow, 6].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.SelectedRange[currRow, 4, currRow, 6].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                ws.Cells[currRow + 1, 1].Value = "Note :";
                ws.Cells[currRow + 2, 1].Value = "1. Dibayar bersamaan Gaji Bulanaan dengan periode pembayaran tanggal 1 s/d 30 bulan sebelum nya";
                ws.Cells[currRow + 3, 1].Value = "2. Formulir ini harus di serahkan ke HRD-HRIS & Payroll Section paling lambat tanggal 5 setiap bulannya";

                ws.Cells[currRow + 4, 6, currRow + 4, 7].Value = "Jakarta, "; ws.Cells[currRow + 4, 6, currRow + 4, 7].Merge = true;
                ws.Cells[currRow + 5, 6].Value = "Disetujui";
                ws.Cells[currRow + 6, 6, currRow + 8, 6].Merge = true;
                ws.Cells[currRow + 5, 7].Value = "Dibuat Oleh";
                ws.Cells[currRow + 6, 7, currRow + 8, 7].Merge = true;
                ws.Cells[currRow + 9, 6].Value = "Kepala Divisi";
                ws.Cells[currRow + 9, 7].Value = "Karyawan";
                ws.SelectedRange[currRow + 4, 6, currRow + 9, 7].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.SelectedRange[currRow + 4, 6, currRow + 9, 7].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.SelectedRange[currRow + 4, 6, currRow + 9, 7].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.SelectedRange[currRow + 4, 6, currRow + 9, 7].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                //ws.Cells.AutoFitColumns();

                pck.SaveAs(output);

                return output;
            }
            catch
            {
                // Log Exception
                return null;
            }
        }

        [HttpPost("download")]
        public IActionResult Download()
        {
            Guid id = Guid.Parse(Request.Form["id"]);
            System.IO.MemoryStream documentKlaimBensin = GenerateExcel(id);
            // Make Sure Document is Loaded
            if (documentKlaimBensin != null && documentKlaimBensin.Length > 0)
            {
                // Generate dynamic name for Attachment document
                string documentName = string.Format("{0}-{1}.xlsx", "Kalim Bensin ", DateTime.Now.ToString("ddMMyyyyHHmm"));
                documentKlaimBensin.Position = 0;
                return Ok(Convert.ToBase64String(documentKlaimBensin.ToArray()));
            }

            // If something fails or somebody calls invalid URI, throw error.
            return NotFound();
        }

        //[HttpPost("download2")]
        public FileResult Download2(Guid id)
        {
            ExcelPackage pck = new ExcelPackage();
            //data
            var docDetail = approvalService.GetDocumentRequestDetailByApprovalId(id);
            var actualOrganizationStructure = mdmService.GetActualOrganizationStructure(docDetail.CreatedBy);
            CopFuelAllowanceViewModel model = JsonConvert.DeserializeObject<CopFuelAllowanceViewModel>(docDetail.ObjectValue);
            //new worksheet
            ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Klaim Bensin");
            //header
            ws.Cells[2, 1].Value = "Car Ownership Programme ( COP )";
            ws.Cells["A2:G2"].Merge = true;
            ws.Cells[3, 1].Value = "Formulir Klaim Bensin (Perjalanan Dinas Bulanan)";
            ws.Cells["A3:G3"].Merge = true;
            //header table
            ws.Cells[5, 1].Value = "Noreg";
            ws.Cells[5, 2].Value = " : " + actualOrganizationStructure.NoReg;
            ws.Cells[6, 1].Value = "Nama";
            ws.Cells[6, 2].Value = " : " + actualOrganizationStructure.Name;
            ws.Cells[7, 1].Value = "Divisi";
            ws.Cells[7, 2].Value = " : " + actualOrganizationStructure.OrgName;
            ws.Cells[9, 1].Value = "No"; ws.Cells["A9:A10"].Merge = true;
            ws.Cells[9, 2].Value = "Tanggal"; ws.Cells["B9:B10"].Merge = true;
            ws.Cells[9, 3].Value = "Tujuan"; ws.Cells["C9:C10"].Merge = true;
            ws.Cells[9, 4].Value = "Posisi KM"; ws.Cells["D9:E10"].Merge = true;
            ws.Cells[10, 4].Value = "Berangkaat";
            ws.Cells[10, 5].Value = "Kembali";
            ws.Cells[9, 6].Value = "Total Pemakaian"; ws.Cells["F9:F10"].Merge = true;
            ws.Cells[9, 7].Value = "Keperluan"; ws.Cells["G9:G10"].Merge = true;
            ws.SelectedRange[2, 1, 10, 7].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            ws.SelectedRange[2, 1, 10, 7].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Double;
            ws.SelectedRange[2, 1, 10, 7].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            ws.SelectedRange[2, 1, 10, 7].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

            int idx = 1;
            int currRow = 10;
            int totalPemakaian = 0;
            foreach (var item in model.data)
            {
                ws.Cells[10 + idx, 1].Value = idx;
                ws.Cells[10 + idx, 2].Value = item.Date.Value.ToShortDateString();
                ws.Cells[10 + idx, 3].Value = item.Destination;
                ws.Cells[10 + idx, 4].Value = item.Start;
                ws.Cells[10 + idx, 5].Value = item.Back;
                ws.Cells[10 + idx, 6].Value = item.Back - item.Start;
                ws.Cells[10 + idx, 7].Value = item.Necessity;

                currRow += idx;
                idx++;
                totalPemakaian += item.Back - item.Start;
            }

            ws.SelectedRange[10, 1, currRow - 1, 7].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            ws.SelectedRange[10, 1, currRow - 1, 7].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            ws.SelectedRange[10, 1, currRow - 1, 7].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            ws.SelectedRange[10, 1, currRow - 1, 7].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

            ws.Cells[currRow, 4].Value = "TOTAL KM"; ws.Cells[currRow, 4, currRow, 5].Merge = true;
            ws.Cells[currRow, 6].Value = totalPemakaian;
            ws.SelectedRange[10, 1, currRow - 1, 7].Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            ws.SelectedRange[10, 1, currRow - 1, 7].Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            ws.SelectedRange[10, 1, currRow - 1, 7].Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            ws.SelectedRange[10, 1, currRow - 1, 7].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

            ws.Cells[currRow + 1, 1].Value = "Note :";
            ws.Cells[currRow + 2, 1].Value = "1. Dibayar bersamaan Gaji Bulanaan dengan periode pembayaran tanggal 1 s/d 30 bulan sebelum nya";
            ws.Cells[currRow + 3, 1].Value = "2. Formulir ini harus di serahkan ke HRD-HRIS & Payroll Section paling lambat tanggal 5 setiap bulannya";
            ws.Cells.AutoFitColumns();

            var fsr = new FileContentResult(pck.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            fsr.FileDownloadName = "Klaim Bensin " + docDetail.CreatedBy + DateTime.Now.ToString("ddMMyyyy") + ".xls";

            return fsr;
        }

        private DataSourceResult GetDetailsReport(DataSourceRequest request, DateTime startDate, DateTime endDate)
        {
            return ServiceProxy.GetQueryDataSourceResult<CopFuel>(@"SELECT i.*, u.Name, u.PostName FROM dbo.TB_M_BENEFIT_CLAIM_COP_FUEL i LEFT JOIN dbo.VW_USER_POSITION u ON u.NoReg = i.NoReg WHERE i.COPFuelDate BETWEEN @startDate AND @endDate", request, new { startDate, endDate });
        }

        [HttpPost("details-report")]
        public DataSourceResult GetDetailsReport([FromForm] DateTime startDate, [FromForm] DateTime endDate, [DataSourceRequest]DataSourceRequest request)
        {
            return GetDetailsReport(request, startDate, endDate);
        }

        /// <summary>
        /// Download reimbursement with date parameter
        /// </summary>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        [HttpGet("download")]
        public IActionResult Download(DateTime startDate, DateTime endDate)
        {
            var request = new DataSourceRequest();

            //var output = GetDetailsReport(request, startDate, endDate).Data.Cast<CopFuel>();
            var dataSourceResult = claimBenefitService.GetCopFuelDetailsReport(request, startDate, endDate);
            var output = dataSourceResult.Data.OfType<CopFuel>();

            var fileName = string.Format("COP FUEL REPORT {0:ddMMyyyy}-{1:ddMMyyyy}.xlsx", startDate, endDate);

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    int rowIndex = 2;
                    var sheet = package.Workbook.Worksheets.Add("Output");
                    var format = "dd/MM/yyyy";

                    var cols = new[] { "No Reg.", "Name", "Position Name", "Date", "Destination", "Total KM" };

                    for (var i = 1; i <= cols.Length; i++)
                    {
                        sheet.Cells[1, i].Value = cols[i - 1];
                        sheet.Cells[1, i].Style.Font.Bold = true;
                        sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }

                    foreach (var data in output)
                    {
                        sheet.Cells[rowIndex, 1].Value = int.Parse(data.NoReg);
                        sheet.Cells[rowIndex, 2].Value = data.Name;
                        sheet.Cells[rowIndex, 3].Value = data.PostName;
                        sheet.Cells[rowIndex, 4].Value = data.COPFuelDate.ToString(format);
                        sheet.Cells[rowIndex, 4].Style.QuotePrefix = true;
                        sheet.Cells[rowIndex, 5].Value = data.DestinationStart;
                        sheet.Cells[rowIndex, 6].Value = data.KMTotal;

                        for (var i = 1; i <= cols.Length; i++)
                        {
                            sheet.Cells[rowIndex, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }

                        rowIndex++;
                    }

                    sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

                    package.SaveAs(ms);
                }

                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    }
    #endregion
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Runtime.Providers;
using Agit.Common;
using OfficeOpenXml;
using Newtonsoft.Json;

namespace TAMHR.ESS.WebUI.Areas.ClaimBenefit.Controllers
{
    #region API Controller
    /// <summary>
    /// Company loan for 3-6 class API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.CompanyLoan36)]
    [ApiController]
    public class Loan36ApiController : FormApiControllerBase<Loan36ViewModel>
    {
        public ClaimBenefitService ClaimBenefitService => ServiceProxy.GetService<ClaimBenefitService>();

        protected override void ValidateOnCreate(string formKey)
        {
            base.ValidateOnCreate(formKey);
            ClaimBenefitService.PreValidateCompanyLoan36(ServiceProxy.UserClaim.NoReg, ServiceProxy.UserClaim.Name);
        }

        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<Loan36ViewModel> requestDetailViewModel)
        {
            Validate(requestDetailViewModel);
        }

        protected override void ValidateOnPostUpdate(DocumentRequestDetailViewModel<Loan36ViewModel> requestDetailViewModel)
        {
            base.ValidateOnPostUpdate(requestDetailViewModel);

            Validate(requestDetailViewModel);
        }

        private void Validate(DocumentRequestDetailViewModel<Loan36ViewModel> requestDetailViewModel)
        {
            var maxAmmount = int.Parse(requestDetailViewModel.Object.CalculationLoan) * requestDetailViewModel.Object.BasicSalary;

            Assert.ThrowIf(maxAmmount < requestDetailViewModel.Object.LoanAmount, "Cannot input loan amount more than max amount " + maxAmmount);
        }

        [HttpPost("verified")]
        public async Task<IActionResult> Verified(string eventName, [FromBody]DocumentRequestDetailViewModel<Loan36ViewModel> documentRequestDetail)
        {
            //update
            var username = ServiceProxy.UserClaim.Username;
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var docApp = ApprovalService.GetDocumentApprovalById(documentRequestDetail.DocumentApprovalId);

            ClaimBenefitService.InsertAllowanceSeq36(docApp.CreatedBy, documentRequestDetail.Object);
            documentRequestDetail.ObjectValue = JsonConvert.SerializeObject(documentRequestDetail.Object);

            ApprovalService.DocumentUpdated += ApprovalService_DocumentUpdated;
            ApprovalService.UpdateDocumentRequestDetail(documentRequestDetail);

            //workflow
            if (!ApprovalService.HasApprovalMatrix(documentRequestDetail.DocumentApprovalId))
            {
                throw new Exception("Approval matrix for this form is not defined. Please contact administrator for more information.");
            }

            var actualOrganizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);

            await ApprovalService.PostAsync(username, actualOrganizationStructure, eventName, documentRequestDetail.DocumentApprovalId);

            return NoContent();
        }

        [HttpGet]
        [Route("download-rincian-pembayaran")]
        public IActionResult DownloadAgreement(Guid docId = new Guid())
        {
            //ClaimBenefitService.
            using (var documentStreamPayment = GeneratePaymentSchedule(docId))
            {
                // Make Sure Document is Loaded
                if (documentStreamPayment != null && documentStreamPayment.Length > 0)
                {
                    // Generate dynamic name for Attachment document "Loan Agreement-XXXXXX.XLSX"
                    string documentName = string.Format("{0}-{1}.xlsx", "Rincian Pembayaran", DateTime.Now.ToString("ddMMyyyyHHmm"));
                    documentStreamPayment.Position = 0;

                    return File(documentStreamPayment.ToArray(), "application/vnd.ms-excel", documentName);
                }
            }

            return CreatedAtAction("", "OK");
        }

        public System.IO.MemoryStream GeneratePaymentSchedule(Guid id)
        {
            try
            {
                var app = ApprovalService.GetDocumentRequestDetailViewModel<Loan36ViewModel>(id, "");
                var getNP = ClaimBenefitService.GetInfo(app.Requester).FirstOrDefault();
                var NP = getNP.GetType().GetProperty("NP").GetValue(getNP).ToString();
                PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));
                var orgObj = ServiceProxy.GetService<MdmService>().GetEmployeeOrganizationObjects(app.Requester);

                decimal? loanAmount = app.Object.LoanAmount;
                double loanRate = (float.Parse(app.Object.Interest.ToString()) / 100);

                var periodLoan = ClaimBenefitService.GetLoan("CalculationLoan", "PeriodLoan", Convert.ToInt32(NP)).FirstOrDefault();
                var AmmountperiodLoan = periodLoan.GetType().GetProperty("Ammount").GetValue(periodLoan);
                int loanPeriod = Convert.ToInt32(AmmountperiodLoan) / 12;

                int totalMonth = Convert.ToInt32(AmmountperiodLoan); //(loanPeriod * 12);
                string Division = "";
                string Name = "";
                foreach (var aa in orgObj)
                {
                    if (aa.ObjectDescription == "Division")
                    {
                        Division = aa.ObjectText;
                        Name = aa.Name;

                    }
                }

                var bunga = 5;
                var pinjaman = app.Object.LoanAmount;
                var jumlahbunga = pinjaman * bunga / 100;
                var jumlah = pinjaman + jumlahbunga;

                DateTime startDate = new DateTime(app.Object.Period.Value.Year, app.Object.Period.Value.Month, 1);
                //get Loan Description
                var getLoanDescription = ClaimBenefitService.GetDescriptionLoan(app.Object.LoanType).FirstOrDefault();
                var LoanDescription = getLoanDescription.GetType().GetProperty("Description").GetValue(getLoanDescription).ToString();

                //get tanggal cutoff
                var getDateCutOff = ClaimBenefitService.GetCutOff().FirstOrDefault();
                int dateCutOff = Convert.ToInt16(getDateCutOff.GetType().GetProperty("ConfigValue").GetValue(getDateCutOff).ToString());
                int datePeriode = Convert.ToInt16(app.Object.Period?.ToString("dd"));

                //get tanggal full approve
                var getDateFullApprove = ClaimBenefitService.GetDateFullApproved(new Guid(id.ToString())).FirstOrDefault();
                DateTime dateFullApprove = Convert.ToDateTime(getDateFullApprove.GetType().GetProperty("ModifiedOn").GetValue(getDateFullApprove));
                int date = Convert.ToInt16(dateFullApprove.ToString("dd"));

                DateTime? mulaicicil = null;

                if (date <= dateCutOff)
                {
                    mulaicicil = dateFullApprove;
                }
                else
                {
                    mulaicicil = dateFullApprove.AddMonths(1);
                }

                string tahun = mulaicicil?.ToString("yyyy");
                string bulan = mulaicicil?.ToString("MM");
                string tanggal = mulaicicil?.ToString("dd");
                string format = "=DATE(" + tahun + "," + bulan + "," + tanggal + ")";
                // Template File
                string templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\loan-agreement-templatev2.xlsx");

                // Results Output
                System.IO.MemoryStream output = new System.IO.MemoryStream();

                // Read Template
                using (System.IO.FileStream templateDocumentStream = System.IO.File.OpenRead(templateDocument))
                {
                    // Create Excel EPPlus Package based on template stream
                    using (ExcelPackage package = new ExcelPackage(templateDocumentStream))
                    {

                        ExcelWorksheet sheet = package.Workbook.Worksheets[1];
                        sheet.Cells["C3"].Value = app.Requester;
                        sheet.Cells["C4"].Value = Name;
                        sheet.Cells["C5"].Value = LoanDescription;
                        sheet.Cells["C6"].Value = Division;
                        sheet.Cells["C7"].Value = loanAmount;
                        sheet.Cells["C8"].Value = loanRate;
                        sheet.Cells["C9"].Value = loanPeriod;
                        sheet.Cells["C10"].Formula = "=PMT(C8/12,C9*12,-(C7))";
                        sheet.Cells["D8"].Formula = "D9-C7";
                        sheet.Cells["D9"].Formula = "C10*(C9*12)";

                        sheet.Cells["E8"].Value = "( Total Bunga selama " + loanPeriod + " Tahun )";
                        sheet.Cells["E9"].Value = "( Total Peminjaman + Bunga )";

                        sheet.DeleteRow(14, 84);

                        sheet.InsertRow(14, totalMonth);

                        sheet.Cells["B14"].Style.Numberformat.Format = "MMM-yy";
                        sheet.Cells["B14"].Formula = format;
                        sheet.Cells["C14"].Formula = "C7";
                        sheet.Cells["D14"].Formula = "$C$10 - E14";
                        sheet.Cells["E14"].Formula = "C14 * $C$8 / 12";
                        sheet.Cells["F14"].Formula = "D14 + E14";
                        sheet.Cells["G14"].Formula = "C14 - D14";

                        sheet.Cells["C14:G14"].Style.Numberformat.Format = "#,##0;-#,##0";
                        sheet.Cells["A14:G14"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dashed);


                        int num = 1;
                        for (int i = 0; i <= totalMonth; i++)
                        {
                            startDate = startDate.AddMonths(1);
                            var numDays = DateTime.DaysInMonth(startDate.Year, startDate.Month);

                            if (i < totalMonth)
                            {
                                sheet.Cells[$"A{i + 14}"].Value = num;
                                sheet.Cells[$"B{i + 15}"].Formula = $"=B{i + 14} + {31}";
                                sheet.Cells[$"C{i + 15}"].Formula = $"=C{i + 14}-D{i + 14}";
                                sheet.Cells[$"D{i + 15}"].Formula = $"$C$10 - E{i + 15}";
                                sheet.Cells[$"E{i + 15}"].Formula = $"C{i + 15} * $C$8 / 12";
                                sheet.Cells[$"F{i + 15}"].Formula = $"D{i + 15} + E{i + 15}";
                                sheet.Cells[$"G{i + 15}"].Formula = $"C{i + 15} - D{i + 15}";

                                sheet.Cells[$"A{i + 15}:G{i + 15}"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dashed);
                                sheet.Cells[$"B{i + 15}"].Style.Numberformat.Format = "MMM-yy";
                                sheet.Cells[$"C{i + 15}:G{i + 15}"].Style.Numberformat.Format = "#,##0;-#,##0";
                            }
                            else
                            {
                                sheet.Cells[$"D{i + 14}"].Formula = $"SUM(D14:D{i + 13})";
                                sheet.Cells[$"E{i + 14}"].Formula = $"SUM(E14:E{i + 13})";
                                sheet.Cells[$"F{i + 14}"].Formula = $"D{i + 14} + E{i + 14}";
                                sheet.Cells[$"G{i + 14}"].Formula = "";
                                sheet.Cells[$"G{i + 14}"].Value = "";
                            }

                            num++;

                        }

                        package.SaveAs(output);
                    }
                    return output;
                }
            }
            catch
            {
                // Log Exception
                return null;
            }
        }

        public System.IO.MemoryStream GenerateAgreementCar(Guid id)
        {
            try
            {
                var app = ApprovalService.GetDocumentRequestDetailViewModel<Loan36ViewModel>(id, "");


                PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));

                //DateTime startDate = new DateTime(app.Object.Period.Value.Year, app.Object.Period.Value.Month, 1);
                int calculation = Convert.ToInt16(app.Object.CalculationLoan);
                DateTime Period = app.Object.Period.Value;
                DateTime enddate = Period.AddMonths(calculation); //new DateTime(app.Object.Period.Value.Year , app.Object.Period.Value.Month, calculation);
                // Template File
                string templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\agreement-car.xlsx");

                // Results Output
                System.IO.MemoryStream output = new System.IO.MemoryStream();

                // Read Template
                using (System.IO.FileStream templateDocumentStream = System.IO.File.OpenRead(templateDocument))
                {
                    // Create Excel EPPlus Package based on template stream
                    using (ExcelPackage package = new ExcelPackage(templateDocumentStream))
                    {

                        ExcelWorksheet sheet = package.Workbook.Worksheets[1];

                        sheet.Cells["D24"].Value = ServiceProxy.UserClaim.NoReg;
                        sheet.Cells["D25"].Value = ServiceProxy.UserClaim.Name;
                        sheet.Cells["D26"].Value = "";
                        sheet.Cells["E50"].Style.Font.Bold = true;
                        sheet.Cells["E50"].Value = app.Object.Period?.ToString("dd/MM/yyyy");
                        sheet.Cells["E52"].Style.Font.Bold = true;
                        sheet.Cells["E52"].Value = enddate.ToString("dd/MM/yyyy");
                        sheet.Cells["D53"].Value = "";
                        sheet.Cells["F60"].Style.Font.Bold = true;
                        sheet.Cells["F60"].Value = app.Object.Period?.ToString("dd/MM/yyyy");
                        sheet.Cells["F63"].Value = "";
                        sheet.Cells["E73"].Value = app.Object.Brand;
                        sheet.Cells["E74"].Value = "";
                        sheet.Cells["E75"].Value = "";
                        sheet.Cells["C105"].Style.Font.Bold = true;
                        sheet.Cells["C105"].Value = string.Format("{0:N}", app.Object.LoanAmount);
                        sheet.Cells["G62"].Value = app.Object.CalculationLoan;
                        sheet.Cells["G62"].Style.Font.Bold = true;
                        sheet.Cells["D84"].Value = ServiceProxy.UserClaim.Name;
                        sheet.Cells["D85"].Value = ServiceProxy.UserClaim.NoReg;
                        sheet.Cells["D86"].Value = "";
                        sheet.Cells["D87"].Value = "";
                        sheet.Cells["D91"].Value = "";
                        sheet.Cells["D92"].Value = "";
                        sheet.Cells["B98"].Style.Font.Bold = true;
                        sheet.Cells["B98"].Value = app.Object.Period?.ToString("dd/MM/yyyy");

                        package.SaveAs(output);
                    }
                    return output;
                }
            }
            catch
            {
                // Log Exception
                return null;
            }
        }

        public System.IO.MemoryStream GenerateAgreement36(Guid id)
        {
            try
            {
                var app = ApprovalService.GetDocumentRequestDetailViewModel<Loan36ViewModel>(id, "");


                PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));

                //DateTime startDate = new DateTime(app.Object.Period.Value.Year, app.Object.Period.Value.Month, 1);
                int calculation = Convert.ToInt16(app.Object.CalculationLoan);
                DateTime Period = app.Object.Period.Value;
                DateTime enddate = Period.AddMonths(calculation); //new DateTime(app.Object.Period.Value.Year , app.Object.Period.Value.Month, calculation);
                // Template File
                string templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\agreement-kelas3-6.xlsx");

                // Results Output
                System.IO.MemoryStream output = new System.IO.MemoryStream();

                // Read Template
                using (System.IO.FileStream templateDocumentStream = System.IO.File.OpenRead(templateDocument))
                {
                    // Create Excel EPPlus Package based on template stream
                    using (ExcelPackage package = new ExcelPackage(templateDocumentStream))
                    {

                        ExcelWorksheet sheet = package.Workbook.Worksheets[1];

                        sheet.Cells["C7"].Value = ServiceProxy.UserClaim.Name;
                        sheet.Cells["C8"].Value = ServiceProxy.UserClaim.NoReg;
                        sheet.Cells["C9"].Value = "";
                        sheet.Cells["C12"].Value = "";
                        sheet.Cells["C13"].Value = "";
                        sheet.Cells["G17"].Style.Font.Bold = true;
                        sheet.Cells["G17"].Value = string.Format("{0:N}", app.Object.LoanAmount);
                        sheet.Cells["H24"].Style.Font.Bold = true;
                        sheet.Cells["H24"].Value = string.Format("{0:N}", app.Object.LoanAmount);
                        sheet.Cells["D34"].Style.Font.Bold = true;
                        sheet.Cells["D34"].Value = app.Object.Period?.ToString("MMMM yyyy");
                        sheet.Cells["B35"].Style.Font.Bold = true;
                        sheet.Cells["B35"].Value = enddate.ToString("MMMM yyyy");
                        sheet.Cells["E40"].Style.Font.Bold = true;
                        sheet.Cells["E40"].Value = app.Object.CalculationLoan;
                        sheet.Cells["F62"].Style.Font.Bold = true;
                        sheet.Cells["F62"].Value = ServiceProxy.UserClaim.Name;

                        package.SaveAs(output);
                    }
                    return output;
                }
            }
            catch
            {
                // Log Exception
                return null;
            }
        }

        public System.IO.MemoryStream GenerateAgreement7up(Guid id)
        {
            try
            {
                var app = ApprovalService.GetDocumentRequestDetailViewModel<Loan36ViewModel>(id, "");
                var bunga = 5;
                var pinjaman = app.Object.LoanAmount;
                var jumahbunga = pinjaman * bunga / 100;
                var jumlah = pinjaman + jumahbunga;

                PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));

                //DateTime startDate = new DateTime(app.Object.Period.Value.Year, app.Object.Period.Value.Month, 1);
                int calculation = Convert.ToInt16(app.Object.CalculationLoan);
                DateTime Period = app.Object.Period.Value;
                DateTime enddate = Period.AddMonths(calculation); //new DateTime(app.Object.Period.Value.Year , app.Object.Period.Value.Month, calculation);
                // Template File
                string templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\agreement-kelas7up.xlsx");

                // Results Output
                System.IO.MemoryStream output = new System.IO.MemoryStream();

                // Read Template
                using (System.IO.FileStream templateDocumentStream = System.IO.File.OpenRead(templateDocument))
                {
                    // Create Excel EPPlus Package based on template stream
                    using (ExcelPackage package = new ExcelPackage(templateDocumentStream))
                    {

                        ExcelWorksheet sheet = package.Workbook.Worksheets[1];

                        sheet.Cells["D32"].Value = ServiceProxy.UserClaim.NoReg;
                        sheet.Cells["D33"].Value = ServiceProxy.UserClaim.Name;
                        sheet.Cells["D34"].Value = "";
                        sheet.Cells["D48"].Value = ServiceProxy.UserClaim.Name;
                        sheet.Cells["D49"].Value = ServiceProxy.UserClaim.NoReg;
                        sheet.Cells["D50"].Value = "";
                        sheet.Cells["D51"].Value = "";
                        sheet.Cells["D55"].Value = "";
                        sheet.Cells["D56"].Value = "";
                        sheet.Cells["D60"].Value = DateTime.Now.ToString("dd MMMM yyyy");
                        sheet.Cells["B66"].Value = string.Format("{0:N}", app.Object.LoanAmount);
                        sheet.Cells["I73"].Value = string.Format("{0:N}", jumlah);
                        sheet.Cells["E85"].Value = app.Object.Period?.ToString("dd/MM/yyyy");
                        sheet.Cells["E87"].Value = enddate.ToString("dd/MM/yyyy");
                        sheet.Cells["B93"].Value = app.Object.CalculationLoan;
                        sheet.Cells["F116"].Value = "";
                        sheet.Cells["F117"].Value = "";
                        sheet.Cells["F118"].Value = "";
                        sheet.Cells["F119"].Value = "";
                        sheet.Cells["H161"].Value = ServiceProxy.UserClaim.Name;

                        sheet.Cells["D60"].Style.Font.Bold = true;
                        sheet.Cells["B66"].Style.Font.Bold = true;
                        sheet.Cells["I73"].Style.Font.Bold = true;
                        sheet.Cells["E85"].Style.Font.Bold = true;
                        sheet.Cells["E87"].Style.Font.Bold = true;
                        sheet.Cells["B93"].Style.Font.Bold = true;
                        sheet.Cells["F116"].Style.Font.Bold = true;
                        sheet.Cells["F117"].Style.Font.Bold = true;
                        sheet.Cells["F118"].Style.Font.Bold = true;
                        sheet.Cells["F119"].Style.Font.Bold = true;
                        sheet.Cells["H161"].Style.Font.Bold = true;

                        package.SaveAs(output);
                    }
                    return output;
                }
            }
            catch
            {
                // Log Exception
                return null;
            }
        }

        public class RequestDownload
        {
            public Guid DocIds { get; set; }
            public int NP { get; set; }
            public string Type { get; set; }
        }

        [HttpPost("download")]
        public IActionResult Download(RequestDownload data)
        {
            var body = data;

            if (data.NP >= 3 && data.NP <= 6)
            {
                if (data.Type == "renovasirumah" || data.Type == "kontrakrumah" || data.Type == "pembelianrumah" || data.Type == "mendirikanrumah" || data.Type == "pembelianmotor")
                {
                    using (var document36 = GenerateAgreement36(data.DocIds))
                    {
                        if (document36 != null && document36.Length > 0)
                        {
                            string documentName = string.Format("{0}-{1}.xlsx", "Loan Agreement", DateTime.Now.ToString("ddMMyyyyHHmm"));
                            document36.Position = 0;
                            return Ok(Convert.ToBase64String(document36.ToArray()));
                        }
                    }
                }
            }
            else if (data.NP >= 7)
            {
                if (data.Type == "buyinghouse7up" || data.Type == "buyingland7up" || data.Type == "buildhouse7up" || data.Type == "renovationhouse7up")
                {
                    using (var document7up = GenerateAgreement7up(data.DocIds))
                    {
                        if (document7up != null && document7up.Length > 0)
                        {
                            string documentName = string.Format("{0}-{1}.xlsx", "Loan Agreement Home", DateTime.Now.ToString("ddMMyyyyHHmm"));
                            document7up.Position = 0;
                            return Ok(Convert.ToBase64String(document7up.ToArray()));
                        }
                    }
                }
                else if (data.Type == "buyingvehicle7up")
                {
                    using (var documentCar = GenerateAgreementCar(data.DocIds))
                    {
                        if (documentCar != null && documentCar.Length > 0)
                        {
                            string documentName = string.Format("{0}-{1}.xlsx", "Loan Agreement Car", DateTime.Now.ToString("ddMMyyyyHHmm"));
                            documentCar.Position = 0;
                            return Ok(Convert.ToBase64String(documentCar.ToArray()));
                        }
                    }
                }
            }

            return NotFound();
        }

        public System.IO.MemoryStream GenerateRincianBiaya(Guid id)
        {
            try
            {
                var app = ApprovalService.GetDocumentRequestDetailViewModel<Loan36ViewModel>(id, "");
                PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));
                decimal loanAmount = app.Object.LoanAmount.Value;
                double loanRate = (float.Parse(app.Object.Interest.ToString()) / 100);
                int loanPeriod = app.Object.LoanPeriod / 12;
                int totalMonth = app.Object.LoanPeriod; //(loanPeriod * 12);
                DateTime startDate = new DateTime(app.Object.Period.Value.Year, app.Object.Period.Value.Month, 1);
                // Template File
                string templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\loan-agreement-template.xlsx");

                // Results Output
                System.IO.MemoryStream output = new System.IO.MemoryStream();

                // Read Template
                using (System.IO.FileStream templateDocumentStream = System.IO.File.OpenRead(templateDocument))
                {
                    // Create Excel EPPlus Package based on template stream
                    using (ExcelPackage package = new ExcelPackage(templateDocumentStream))
                    {

                        ExcelWorksheet sheet = package.Workbook.Worksheets[1];

                        sheet.Cells["C7"].Value = loanAmount;
                        sheet.Cells["C8"].Value = loanRate;
                        sheet.Cells["C9"].Value = loanPeriod;

                        sheet.DeleteRow(14, 84);

                        sheet.InsertRow(14, totalMonth);

                        sheet.Cells["B14"].Style.Numberformat.Format = "MM/YY";
                        sheet.Cells["B14"].Formula = "=DATE(2018,11,1)";

                        sheet.Cells["C14"].Formula = "C7";
                        sheet.Cells["D14"].Formula = "$C$10 - E14";
                        sheet.Cells["E14"].Formula = "C14 * $C$8 / 12";
                        sheet.Cells["F14"].Formula = "D14 + E14";
                        sheet.Cells["G14"].Formula = "C14 - D14";

                        sheet.Cells["C14:G14"].Style.Numberformat.Format = "#,##0;-#,##0";
                        sheet.Cells["A14:G14"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dashed);


                        int num = 1;
                        for (int i = 0; i <= totalMonth; i++)
                        {
                            startDate = startDate.AddMonths(1);
                            var numDays = DateTime.DaysInMonth(startDate.Year, startDate.Month);

                            if (i < totalMonth)
                            {
                                sheet.Cells[$"A{i + 14}"].Value = num;
                                sheet.Cells[$"B{i + 15}"].Formula = $"=B{i + 14} + {30}";
                                sheet.Cells[$"C{i + 15}"].Formula = $"=C{i + 14}-D{i + 14}";
                                sheet.Cells[$"D{i + 15}"].Formula = $"$C$10 - E{i + 15}";
                                sheet.Cells[$"E{i + 15}"].Formula = $"C{i + 15} * $C$8 / 12";
                                sheet.Cells[$"F{i + 15}"].Formula = $"D{i + 15} + E{i + 15}";
                                sheet.Cells[$"G{i + 15}"].Formula = $"C{i + 15} - D{i + 15}";

                                sheet.Cells[$"A{i + 15}:G{i + 15}"].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dashed);
                                sheet.Cells[$"B{i + 15}"].Style.Numberformat.Format = "MM/YY";
                                sheet.Cells[$"C{i + 15}:G{i + 15}"].Style.Numberformat.Format = "#,##0;-#,##0";
                            }
                            else
                            {
                                sheet.Cells[$"D{i + 14}"].Formula = $"SUM(D14:D{i + 13})";
                                sheet.Cells[$"E{i + 14}"].Formula = $"SUM(E14:E{i + 13})";
                                sheet.Cells[$"F{i + 14}"].Formula = $"D{i + 14} + E{i + 14}";
                                sheet.Cells[$"G{i + 14}"].Formula = "";
                                sheet.Cells[$"G{i + 14}"].Value = "";
                            }

                            num++;

                        }

                        package.SaveAs(output);
                    }
                    return output;
                }
            }
            catch
            {
                // Log Exception
                return null;
            }
        }
    }
    #endregion
}
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using Agit.Common;
using Agit.Common.Utility;
using Agit.Common.Archieve;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using OfficeOpenXml;

namespace TAMHR.ESS.WebUI.Areas.ClaimBenefit.Controllers
{
    #region API Controller
    /// <summary>
    /// Letter of guarantee API controller.
    /// </summary>
    [Route("api/" + ApplicationForm.LetterOfGuarantee)]
    public class LetterOfGuaranteeApiController : FormApiControllerBase<LetterOfGuaranteeViewModel>
    {
        #region Domain Services
        /// <summary>
        /// Claim benefit service
        /// </summary>
        public ClaimBenefitService ClaimBenefitService => ServiceProxy.GetService<ClaimBenefitService>();

        /// <summary>
        /// Personal data service
        /// </summary>
        public PersonalDataService PersonalDataService => ServiceProxy.GetService<PersonalDataService>();
        #endregion

        /// <summary>
        /// Validate on post create object
        /// </summary>
        /// <param name="letterOfGuaranteeViewModel">Letter of Guarantee Object</param>
        protected override void ValidateOnPostCreate(DocumentRequestDetailViewModel<LetterOfGuaranteeViewModel> letterOfGuaranteeViewModel)
        {
            base.ValidateOnPostCreate(letterOfGuaranteeViewModel);

            ValidateInput(letterOfGuaranteeViewModel);
        }

        /// <summary>
        /// Validate on post update object
        /// </summary>
        /// <param name="letterOfGuaranteeViewModel">Letter of Guarantee Object</param>
        protected override void ValidateOnPostUpdate(DocumentRequestDetailViewModel<LetterOfGuaranteeViewModel> letterOfGuaranteeViewModel)
        {
            base.ValidateOnPostUpdate(letterOfGuaranteeViewModel);

            ValidateInput(letterOfGuaranteeViewModel);
        }

        private void ValidateInput(DocumentRequestDetailViewModel<LetterOfGuaranteeViewModel> letterOfGuaranteeViewModel)
        {
            Assert.ThrowIf(letterOfGuaranteeViewModel.Object == null, "Cannot create request because request is empty");
            Assert.ThrowIf(letterOfGuaranteeViewModel.Object.EndDateOfCare < letterOfGuaranteeViewModel.Object.StartDateOfCare, "Tanggal Keluar tidak boleh lebih kecil dari Tanggal Masuk");
        }

        protected override DocumentRequestDetailViewModel<LetterOfGuaranteeViewModel> ValidateViewModel(DocumentRequestDetailViewModel<LetterOfGuaranteeViewModel> letterOfGuaranteeViewModel)
        {
            var documentApproval = GetDocumentApproval(letterOfGuaranteeViewModel.DocumentApprovalId);
            var creator = documentApproval != null ? documentApproval.CreatedBy : ServiceProxy.UserClaim.NoReg;
            var now = DateTime.Now.Date;
            var configClassification = MdmService.GetConfigClassification(creator, now);

            letterOfGuaranteeViewModel.Object.BenefitClassification = configClassification.Value;

            letterOfGuaranteeViewModel.Refresh();

            return letterOfGuaranteeViewModel;
        }

        /// <summary>
        /// Get details report
        /// </summary>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        /// <param name="request">Data Source Request Object</param>
        [HttpPost("details-report")]
        public async Task<DataSourceResult> GetDetailsReport([FromForm] DateTime startDate, [FromForm] DateTime endDate, [DataSourceRequest]DataSourceRequest request)
        {
            var letterOfGuarantee = ClaimBenefitService.GetDocumentRequestDetails<LetterOfGuaranteeViewModel>("letter-of-guarantee", startDate, endDate);

            return await letterOfGuarantee.ToDataSourceResultAsync(request);
        }

        ///// <summary>
        ///// Download reimbursement with date parameter
        ///// </summary>
        ///// <param name="startDate">Start Date</param>
        ///// <param name="endDate">End Date</param>
        //[HttpGet("download")]
        //public IActionResult Download(DateTime startDate, DateTime endDate)
        //{
        //    var fileName = string.Format("LETTER OF GUARANTEE REPORT {0:ddMMyyyy}-{1:ddMMyyyy}.xlsx", startDate, endDate);

        //    return File(GetDownloadData(startDate, endDate), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        //}

        /// <summary>
        /// Download reimbursement with date parameter
        /// </summary>
        /// <param name="startDate">Start Date</param>
        /// <param name="endDate">End Date</param>
        [HttpGet("download")]
        public async Task<IActionResult> DownloadMailMerge(DateTime startDate, DateTime endDate)
        {
            var archieveManager = new ZipManager();
            var excelEntry = new ZipEntry("Data Source.xlsx", GetDownloadData(startDate, endDate));

            using (var ms = new MemoryStream())
            {
                var sourceFilePath = ServiceProxy.GetPathProvider().ContentPath("uploads\\word-template\\Surat Jaminan.docm");
                await System.IO.File.OpenRead(sourceFilePath).CopyToAsync(ms);
                archieveManager.AttachEntry(new ZipEntry(System.IO.Path.GetFileName(sourceFilePath), ms.ToArray()));
            }

            archieveManager.AttachEntry(excelEntry);

            var fileName = string.Format("LETTER_OF_GUARANTEE_{0:ddMMyyyy}_{1:ddMMyyyy}.zip", startDate, endDate);
            var attachmentHeader = string.Format(@"attachment; filename=""{0}""", fileName);

            Response.Headers.Clear();
            Response.ContentType = "application/zip";
            Response.Headers.Add("content-disposition", attachmentHeader);
            Response.Headers.Add("fileName", fileName);

            return new FileContentResult(archieveManager.ToZip(), "application/zip");
        }

        private byte[] GetDownloadData(DateTime startDate, DateTime endDate)
        {
            var benefitClassifications = ConfigService.GetConfigClassifications("EmployeeSubgroup").ToDictionary(x => x.Code);

            var reimbursements = ClaimBenefitService.GetDocumentRequestDetails<LetterOfGuaranteeViewModel>("letter-of-guarantee", startDate, endDate).Where(x => !ObjectHelper.IsIn(x.DocumentStatusCode, DocumentStatus.Cancelled, DocumentStatus.Rejected));
            var childCriteria = new[] { "rsanak", "Anak" };
            var spouseCriteria = new[] { "rspasangan", "Suami / Istri" };

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    int rowIndex = 2;
                    var sheet = package.Workbook.Worksheets.Add("Output");
                    var uiCulture = System.Globalization.CultureInfo.CurrentUICulture;
                    var format = "dd MMMM yyyy";

                    var cols = new[] { "No Reg.", "Name", "Class", "Division", "Document Number", "Document Status", "Progress", "Family Relationship", "Patient Name", "Birth Date", "Entry Date", "Out Date", "Benefit Classification", "Hospital", "Hospital Address", "Hospital City", "Criteria Control", "Diagnose", "Control Date", "Control Number", "Submit Date", "Approve Date" };

                    for (var i = 1; i <= cols.Length; i++)
                    {
                        sheet.Cells[1, i].Value = cols[i - 1];
                        sheet.Cells[1, i].Style.Font.Bold = true;
                        sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }

                    foreach (var data in reimbursements)
                    {
                        var item = data.Object;
                        var gender = data.Gender;

                        sheet.Cells[rowIndex, 1].Value = int.Parse(data.Requester);
                        sheet.Cells[rowIndex, 2].Value = data.RequesterName;
                        sheet.Cells[rowIndex, 3].Value = data.EmployeeSubgroupText;
                        sheet.Cells[rowIndex, 4].Value = data.Division;
                        sheet.Cells[rowIndex, 5].Value = data.DocumentNumber;
                        sheet.Cells[rowIndex, 6].Value = data.DocumentStatusCode;
                        sheet.Cells[rowIndex, 7].Value = data.Progress;
                        sheet.Cells[rowIndex, 8].Value = ObjectHelper.IsIn(item.FamilyRelationship, childCriteria) ? "Anak Karyawan" : (ObjectHelper.IsIn(item.FamilyRelationship, spouseCriteria) ? ((gender == "Male" ? "Istri" : "Suami") + " Karyawan") : "Karyawan Ybs");
                        sheet.Cells[rowIndex, 9].Value = ObjectHelper.IsIn(item.FamilyRelationship, childCriteria) ? item.PatientChildName : item.PatientName;

                        sheet.Cells[rowIndex, 10].Value = item.DateOfBirth.HasValue ? item.DateOfBirth.Value.ToString(format, uiCulture) : string.Empty;
                        sheet.Cells[rowIndex, 10].Style.QuotePrefix = item.DateOfBirth.HasValue;

                        sheet.Cells[rowIndex, 11].Value = item.StartDateOfCare.HasValue ? item.StartDateOfCare.Value.ToString(format, uiCulture) : string.Empty;
                        sheet.Cells[rowIndex, 11].Style.QuotePrefix = item.StartDateOfCare.HasValue;

                        sheet.Cells[rowIndex, 12].Value = item.EndDateOfCare.HasValue ? item.EndDateOfCare.Value.ToString(format, uiCulture) : string.Empty;
                        sheet.Cells[rowIndex, 12].Style.QuotePrefix = item.EndDateOfCare.HasValue;

                        sheet.Cells[rowIndex, 13].Value = string.IsNullOrEmpty(item.BenefitClassification) && benefitClassifications.ContainsKey(data.EmployeeSubgroup) ? benefitClassifications[data.EmployeeSubgroup].Value : item.BenefitClassification;
                        sheet.Cells[rowIndex, 14].Value = item.Hospital;
                        sheet.Cells[rowIndex, 15].Value = item.HospitalAddress;
                        sheet.Cells[rowIndex, 16].Value = item.HospitalCity;
                        sheet.Cells[rowIndex, 17].Value = item.CriteriaControl;
                        sheet.Cells[rowIndex, 18].Value = item.CriteriaControl == "" ? item.Diagnosa : item.DiagnosaRawatInap;

                        sheet.Cells[rowIndex, 19].Value = item.ControlDate.HasValue ? item.ControlDate.Value.ToString(format, uiCulture) : string.Empty;
                        sheet.Cells[rowIndex, 19].Style.QuotePrefix = item.ControlDate.HasValue;

                        sheet.Cells[rowIndex, 20].Value = int.Parse(item.CheckUpCount) > 3
                            ? string.Format("Khusus ({0})", item.CheckUpCount)
                            : string.Format("Kontrol Ke {0} ({1})", item.CheckUpCount, item.CheckUpCount == "1" ? "Pertama" : (item.CheckUpCount == "2" ? "Kedua" : item.CheckUpCount == "3" ? "Ketiga" : item.CheckUpCount));

                        sheet.Cells[rowIndex, 21].Value = data.SubmitOn.HasValue ? data.SubmitOn.Value.ToString(format, uiCulture) : string.Empty;
                        sheet.Cells[rowIndex, 21].Style.QuotePrefix = data.SubmitOn.HasValue;

                        sheet.Cells[rowIndex, 22].Value = data.LastApprovedOn.HasValue && data.DocumentStatusCode == DocumentStatus.Completed ? data.LastApprovedOn.Value.ToString(format) : string.Empty;
                        sheet.Cells[rowIndex, 22].Style.QuotePrefix = data.LastApprovedOn.HasValue && data.DocumentStatusCode == DocumentStatus.Completed;

                        for (var i = 1; i <= cols.Length; i++)
                        {
                            sheet.Cells[rowIndex, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }

                        rowIndex++;
                    }

                    sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

                    package.SaveAs(ms);
                }

                return ms.ToArray();
            }
        }
    }
    #endregion
}
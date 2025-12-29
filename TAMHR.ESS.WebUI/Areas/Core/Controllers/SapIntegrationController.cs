using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Agit.Common;
using Agit.Common.Archieve;
using Agit.Common.Extensions;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using OfficeOpenXml;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using System.Reflection;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    [Route("api/sapintegration")]
    [Permission(PermissionKey.ViewSapIntegration)]
    public class SapIntegrationApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// SAP integration service object
        /// </summary>
        protected SapIntegrationService SapIntegrationService => ServiceProxy.GetService<SapIntegrationService>();

        /// <summary>
        /// Approval service object
        /// </summary>
        protected ApprovalService ApprovalService => ServiceProxy.GetService<ApprovalService>();
        protected LogService logService => ServiceProxy.GetService<LogService>();
        #endregion

        protected Dictionary<string, Func<List<object>, ZipEntry>> _handlers = new Dictionary<string, Func<List<object>, ZipEntry>>();

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public SapIntegrationApiController()
            : base()
        {
            _handlers.Add("marriage-status", (data) => GenerateExcellMariageStatus(data));
            _handlers.Add("family-registration", (data) => GenerateExcelFamilyRegistration(data));
            _handlers.Add("divorce", (data) => GenerateExcelDivorce(data));
            _handlers.Add("condolance", (data) => GenerateExcelDismemberment(data));
            // _handlers.Add("condolance-allowance", (data) => GenerateExcelDismemberment(data));
            _handlers.Add("address", (data) => GenerateExcelAddress(data));
            _handlers.Add("education", (data) => GenerateExcelEducation(data));
            _handlers.Add("bank-account", (data) => GenerateExcelBankAccount(data));
            _handlers.Add("tax-status", (data) => GenerateExcelTaxStatus(data));
            _handlers.Add("claim-benefit", (data) => GenerateExcelBenefitClaim(data));
            _handlers.Add("pta-allowance", (data) => GenerateExcelPtaAllowance(data));
            _handlers.Add("company-loan", (data) => GenerateExcelCompanyLoan(data));
            _handlers.Add("vacation-allowance", (data) => GenerateExcelVacation(data));
            _handlers.Add("meal-allowance", (data) => GenerateExcelMeal(data));
            _handlers.Add("shift-meal-allowance", (data) => GenerateExcelShiftMeal(data));
            _handlers.Add("spkl-overtime", (data) => GenerateExcelSPKL(data));
            _handlers.Add("hiring-employee", (data) => GenerateExcelHiringEmployee(data));
            _handlers.Add("bpjs-tk", (data) => GenerateExcelBPJSTK(data));
        }
        #endregion

        [HttpPost("get-view-count")]
        public async Task<DataSourceResult> GetViewCount([FromForm] Guid id, [DataSourceRequest]DataSourceRequest request)
        {
            return await SapIntegrationService.GetHistories(id).ToDataSourceResultAsync(request);
        }

        [HttpPost("export")]
        public IActionResult Export([FromBody]DownloadRequest data)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var archieveManager = new ZipManager();
            var terminationArchieveManager = new ZipManager();
            var documentApprovalIds = data.documentApprovalIds;
            var listExcell = new List<string>();

            SapIntegrationService.UpdateHitCount(noreg, documentApprovalIds);

            

            var dataTerminationNumber = SapIntegrationService.GetDataTermination(documentApprovalIds);
            var dataEnroll = SapIntegrationService.GetDataUpdateContract(documentApprovalIds);

            if (dataTerminationNumber.Length > 0)
            {
                var streams = SapIntegrationService.GenerateExcelTermination(dataTerminationNumber).ToArray();


                terminationArchieveManager.AttachEntries(streams);
                var fileNames = string.Format("OUTPUT_{0:ddMMyyyy}.zip", DateTime.Now);
                var attachmentHeaders = string.Format(@"attachment; filename=""{0}""", fileNames);

                Response.Headers.Clear();
                Response.ContentType = "application/zip";
                Response.Headers.Add("content-disposition", attachmentHeaders);
                Response.Headers.Add("fileName", fileNames);

                //var counters = 1;
                //terminationArchieveManager.Entries.ForEach(x =>
                //{
                //    x.FileName = (counters++).ToString("D3") + "_" + x.FileName;
                //});

                return new FileContentResult(terminationArchieveManager.ToZip(), "application/zip");
            }
            else if(dataEnroll.Length > 0)
            {
                var streams = SapIntegrationService.GenerateExcelUpdateContract(dataEnroll).ToArray();


                terminationArchieveManager.AttachEntries(streams);
                var fileNames = string.Format("OUTPUT_{0:ddMMyyyy}.zip", DateTime.Now);
                var attachmentHeaders = string.Format(@"attachment; filename=""{0}""", fileNames);

                Response.Headers.Clear();
                Response.ContentType = "application/zip";
                Response.Headers.Add("content-disposition", attachmentHeaders);
                Response.Headers.Add("fileName", fileNames);

                //var counters = 1;
                //terminationArchieveManager.Entries.ForEach(x =>
                //{
                //    x.FileName = (counters++).ToString("D3") + "_" + x.FileName;
                //});

                return new FileContentResult(terminationArchieveManager.ToZip(), "application/zip");
            }
            else
            {
                var dataExcel = SapIntegrationService.GetListData(documentApprovalIds);

                foreach (var item in dataExcel)
                {
                    if (!_handlers.ContainsKey(item.Key)) continue;

                    var zipEntry = _handlers[item.Key](item.Value);

                    if (zipEntry != null)
                    {
                        archieveManager.AttachEntry(zipEntry);
                    }
                }
            }



            

            var fileName = string.Format("OUTPUT_{0:ddMMyyyy}.zip", DateTime.Now);
            var attachmentHeader = string.Format(@"attachment; filename=""{0}""", fileName);

            Response.Headers.Clear();
            Response.ContentType = "application/zip";
            Response.Headers.Add("content-disposition", attachmentHeader);
            Response.Headers.Add("fileName", fileName);

            var counter = 1;
            archieveManager.Entries.ForEach(x =>
            {
                if (!x.FileName.Contains("JAM"))
                {
                    x.FileName = (counter++).ToString("D3") + "_" + x.FileName;
                }              
            });

            return new FileContentResult(archieveManager.ToZip(), "application/zip");
        }

        private ZipEntry GenerateExcellMariageStatus(List<dynamic> data)
        {
            try
            {
                using (var pck = new ExcelPackage())
                {
                    using (var output = new MemoryStream())
                    {
                        var ws = pck.Workbook.Worksheets.Add("Pernikahan");

                        ws.Cells[1, 1].Value = "Pers. No";
                        ws.Cells[1, 2].Value = "Family Member";
                        ws.Cells[1, 3].Value = "Start Date";
                        ws.Cells[1, 4].Value = "End Date";
                        ws.Cells[1, 5].Value = "Name";
                        ws.Cells[1, 6].Value = "Nationality";
                        ws.Cells[1, 7].Value = "Birthplace";
                        ws.Cells[1, 8].Value = "Date of Birth";
                        ws.Cells[1, 9].Value = "Gender";
                        ws.Cells[1, 10].Value = "Employer";
                        ws.Cells[1, 11].Value = "Marital Status";

                        var currRow = 2;

                        foreach (var selectedData in data)
                        {
                            ws.Cells[currRow, 1].Value = selectedData.GetType().GetProperty("NoReg").GetValue(selectedData, null);
                            ws.Cells[currRow, 2].Value = selectedData.GetType().GetProperty("FamilyTypeCode").GetValue(selectedData, null);
                            ws.Cells[currRow, 3].Value = selectedData.GetType().GetProperty("StartDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 4].Value = selectedData.GetType().GetProperty("EndDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 5].Value = selectedData.GetType().GetProperty("Name").GetValue(selectedData, null);
                            ws.Cells[currRow, 6].Value = selectedData.GetType().GetProperty("NationalityCode").GetValue(selectedData, null);
                            ws.Cells[currRow, 7].Value = selectedData.GetType().GetProperty("BirthPlace").GetValue(selectedData, null);
                            ws.Cells[currRow, 8].Value = selectedData.GetType().GetProperty("BirthDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 9].Value = selectedData.GetType().GetProperty("GenderCode").GetValue(selectedData, null);
                            ws.Cells[currRow, 10].Value = selectedData.GetType().GetProperty("Occupation").GetValue(selectedData, null);
                            ws.Cells[currRow, 11].Value = selectedData.GetType().GetProperty("MartialStatusCode").GetValue(selectedData, null);

                            currRow = currRow + 1;
                        }

                        pck.SaveAs(output);

                        var byteArray = output.ToArray();

                        if (byteArray == null || byteArray.Length == 0) return null;

                        return new ZipEntry(string.Format("{0}{1}.xlsx", "FAM_", DateTime.Now.ToString("yyyyMMdd")), byteArray);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private ZipEntry GenerateExcelFamilyRegistration(List<dynamic> data)
        {
            try
            {
                using (var pck = new ExcelPackage())
                {
                    using (var output = new System.IO.MemoryStream())
                    {
                        var ws = pck.Workbook.Worksheets.Add("Kelahiran Anak");

                        ws.Cells[1, 1].Value = "Pers. No";
                        ws.Cells[1, 2].Value = "Family Member";
                        ws.Cells[1, 3].Value = "Start Date";
                        ws.Cells[1, 4].Value = "End Date";
                        ws.Cells[1, 5].Value = "Name";
                        ws.Cells[1, 6].Value = "Nationality";
                        ws.Cells[1, 7].Value = "Birthplace";
                        ws.Cells[1, 8].Value = "Date of Birth";
                        ws.Cells[1, 9].Value = "Gender";

                        var currRow = 2;

                        foreach (var selectedData in data)
                        {
                            ws.Cells[currRow, 1].Value = selectedData.GetType().GetProperty("NoReg").GetValue(selectedData, null);
                            ws.Cells[currRow, 2].Value = selectedData.GetType().GetProperty("FamilyTypeCode").GetValue(selectedData, null);
                            ws.Cells[currRow, 3].Value = selectedData.GetType().GetProperty("StartDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 4].Value = selectedData.GetType().GetProperty("EndDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 5].Value = selectedData.GetType().GetProperty("Name").GetValue(selectedData, null);
                            ws.Cells[currRow, 6].Value = selectedData.GetType().GetProperty("NationalityCode").GetValue(selectedData, null);
                            ws.Cells[currRow, 7].Value = selectedData.GetType().GetProperty("BirthPlace").GetValue(selectedData, null);
                            ws.Cells[currRow, 8].Value = selectedData.GetType().GetProperty("BirthDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 9].Value = selectedData.GetType().GetProperty("GenderCode").GetValue(selectedData, null);
                            currRow = currRow + 1;
                        }

                        pck.SaveAs(output);

                        var byteArray = output.ToArray();

                        if (byteArray == null || byteArray.Length == 0) return null;

                        return new ZipEntry(string.Format("{0}{1}.xlsx", "CHD_", DateTime.Now.ToString("yyyyMMdd")), byteArray);
                    }
                }
            }
            catch
            {
                // Log Exception
                return null;
            }
        }

        private ZipEntry GenerateExcelDivorce(List<dynamic> data)
        {
            try
            {
                using (var pck = new ExcelPackage())
                {
                    using (var output = new System.IO.MemoryStream())
                    {
                        var ws = pck.Workbook.Worksheets.Add("Perceraian");

                        ws.Cells[1, 1].Value = "Pers. No";
                        ws.Cells[1, 2].Value = "Family Member";
                        ws.Cells[1, 3].Value = "Start Date";
                        ws.Cells[1, 4].Value = "End Date";
                        ws.Cells[1, 5].Value = "Name";
                        ws.Cells[1, 6].Value = "Nationality";
                        ws.Cells[1, 7].Value = "Birthplace";
                        ws.Cells[1, 8].Value = "Date of Birth";
                        ws.Cells[1, 9].Value = "Gender";
                        ws.Cells[1, 10].Value = "Employer";
                        ws.Cells[1, 11].Value = "Marital Status";

                        int currRow = 2;

                        foreach (var selectedData in data)
                        {

                            ws.Cells[currRow, 1].Value = selectedData.GetType().GetProperty("NoReg").GetValue(selectedData, null);
                            ws.Cells[currRow, 2].Value = selectedData.GetType().GetProperty("FamilyTypeCode").GetValue(selectedData, null);
                            ws.Cells[currRow, 3].Value = selectedData.GetType().GetProperty("StartDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 4].Value = selectedData.GetType().GetProperty("EndDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 5].Value = selectedData.GetType().GetProperty("Name").GetValue(selectedData, null);
                            ws.Cells[currRow, 6].Value = selectedData.GetType().GetProperty("NationalityCode").GetValue(selectedData, null);
                            ws.Cells[currRow, 7].Value = selectedData.GetType().GetProperty("BirthPlace").GetValue(selectedData, null);
                            ws.Cells[currRow, 8].Value = selectedData.GetType().GetProperty("BirthDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 9].Value = selectedData.GetType().GetProperty("GenderCode").GetValue(selectedData, null);
                            ws.Cells[currRow, 10].Value = selectedData.GetType().GetProperty("Occupation").GetValue(selectedData, null);
                            ws.Cells[currRow, 11].Value = selectedData.GetType().GetProperty("MartialStatusCode").GetValue(selectedData, null);
                            currRow = currRow + 1;

                        }

                        pck.SaveAs(output);

                        var byteArray = output.ToArray();

                        if (byteArray == null || byteArray.Length == 0) return null;

                        return new ZipEntry(string.Format("{0}{1}.xlsx", "DIV_", DateTime.Now.ToString("yyyyMMdd")), byteArray);
                    }
                }
            }
            catch
            {
                // Log Exception
                return null;
            }
        }

        private ZipEntry GenerateExcelDismemberment(List<dynamic> data)
        {
            try
            {
                using (var pck = new ExcelPackage())
                {
                    using (var output = new System.IO.MemoryStream())
                    {
                        var ws = pck.Workbook.Worksheets.Add("Kematian");

                        ws.Cells[1, 1].Value = "Pers. No";
                        ws.Cells[1, 2].Value = "Family Member";
                        ws.Cells[1, 3].Value = "Start Date";
                        ws.Cells[1, 4].Value = "End Date";
                        ws.Cells[1, 5].Value = "Name";
                        ws.Cells[1, 6].Value = "Nationality";
                        ws.Cells[1, 7].Value = "Birthplace";
                        ws.Cells[1, 8].Value = "Date of Birth";
                        ws.Cells[1, 9].Value = "Gender";
                        ws.Cells[1, 10].Value = "Employer";
                        ws.Cells[1, 11].Value = "Marital Status";

                        int currRow = 2;

                        foreach (var selectedData in data)
                        {

                            ws.Cells[currRow, 1].Value = selectedData.GetType().GetProperty("NoReg").GetValue(selectedData, null);
                            ws.Cells[currRow, 2].Value = selectedData.GetType().GetProperty("FamilyTypeCode").GetValue(selectedData, null);
                            ws.Cells[currRow, 3].Value = selectedData.GetType().GetProperty("StartDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 4].Value = selectedData.GetType().GetProperty("EndDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 5].Value = selectedData.GetType().GetProperty("Name").GetValue(selectedData, null);
                            ws.Cells[currRow, 6].Value = selectedData.GetType().GetProperty("NationalityCode").GetValue(selectedData, null);
                            ws.Cells[currRow, 7].Value = selectedData.GetType().GetProperty("BirthPlace").GetValue(selectedData, null);
                            ws.Cells[currRow, 8].Value = selectedData.GetType().GetProperty("BirthDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 9].Value = selectedData.GetType().GetProperty("GenderCode").GetValue(selectedData, null);
                            ws.Cells[currRow, 10].Value = selectedData.GetType().GetProperty("Occupation").GetValue(selectedData, null);
                            ws.Cells[currRow, 11].Value = selectedData.GetType().GetProperty("MartialStatusCode").GetValue(selectedData, null);
                            currRow = currRow + 1;

                        }

                        pck.SaveAs(output);

                        var byteArray = output.ToArray();

                        if (byteArray == null || byteArray.Length == 0) return null;

                        return new ZipEntry(string.Format("{0}{1}.xlsx", "DIV_", DateTime.Now.ToString("yyyyMMdd")), byteArray);
                    }
                }
            }
            catch
            {
                // Log Exception
                return null;
            }
        }

        private ZipEntry GenerateExcelAddress(List<dynamic> data)
        {
            try
            {
                using (var pck = new ExcelPackage())
                {
                    using (var output = new System.IO.MemoryStream())
                    {
                        var ws = pck.Workbook.Worksheets.Add("Tempat Tinggal");

                        ws.Cells[1, 1].Value = "Pers. No";
                        ws.Cells[1, 2].Value = "ID Card Address";
                        ws.Cells[1, 3].Value = "Start Date";
                        ws.Cells[1, 4].Value = "End Date";
                        ws.Cells[1, 5].Value = "Region";
                        ws.Cells[1, 6].Value = "City";
                        ws.Cells[1, 7].Value = "District";
                        ws.Cells[1, 8].Value = "Postal Code";
                        ws.Cells[1, 9].Value = "Street and House No.";
                        ws.Cells[1, 10].Value = "RT";
                        ws.Cells[1, 11].Value = "RW";
                        ws.Cells[1, 12].Value = "Country";

                        int currRow = 2;

                        foreach (var selectedData in data)
                        {

                            ws.Cells[currRow, 1].Value = selectedData.GetType().GetProperty("NoReg").GetValue(selectedData, null);
                            ws.Cells[currRow, 2].Value = selectedData.GetType().GetProperty("IDCardAddress").GetValue(selectedData, null);
                            ws.Cells[currRow, 3].Value = selectedData.GetType().GetProperty("StartDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 4].Value = selectedData.GetType().GetProperty("EndDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 5].Value = selectedData.GetType().GetProperty("RegionCode").GetValue(selectedData, null);
                            ws.Cells[currRow, 6].Value = selectedData.GetType().GetProperty("CityCode").GetValue(selectedData, null);
                            ws.Cells[currRow, 7].Value = selectedData.GetType().GetProperty("DistrictCode").GetValue(selectedData, null);
                            ws.Cells[currRow, 8].Value = selectedData.GetType().GetProperty("PostalCode").GetValue(selectedData, null);
                            ws.Cells[currRow, 9].Value = selectedData.GetType().GetProperty("Address").GetValue(selectedData, null);
                            ws.Cells[currRow, 10].Value = "";
                            ws.Cells[currRow, 11].Value = "";
                            ws.Cells[currRow, 12].Value = selectedData.GetType().GetProperty("Country").GetValue(selectedData, null);
                            currRow = currRow + 1;

                        }

                        pck.SaveAs(output);

                        var byteArray = output.ToArray();

                        if (byteArray == null || byteArray.Length == 0) return null;

                        return new ZipEntry(string.Format("{0}{1}.xlsx", "ADD_", DateTime.Now.ToString("yyyyMMdd")), byteArray);
                    }
                }
            }
            catch
            {
                // Log Exception
                return null;
            }
        }

        private ZipEntry GenerateExcelEducation(List<dynamic> data)
        {
            try
            {
                using (var pck = new ExcelPackage())
                {
                    using (var output = new System.IO.MemoryStream())
                    {
                        var ws = pck.Workbook.Worksheets.Add("Pendidikan");

                        ws.Cells[1, 1].Value = "Pers. No";
                        ws.Cells[1, 2].Value = "Educational est.";
                        ws.Cells[1, 3].Value = "Start Date";
                        ws.Cells[1, 4].Value = "End Date";
                        ws.Cells[1, 5].Value = "Education/training";
                        ws.Cells[1, 6].Value = "Institute/location";
                        ws.Cells[1, 7].Value = "Country Key";
                        ws.Cells[1, 8].Value = "Certificate";
                        ws.Cells[1, 9].Value = "Branch of Study";
                        ws.Cells[1, 10].Value = "Tanggal Akhir Pendidikan";
                        ws.Cells[1, 11].Value = "Final Grade";

                        int currRow = 2;

                        foreach (var selectedData in data)
                        {

                            ws.Cells[currRow, 1].Value = selectedData.GetType().GetProperty("NoReg").GetValue(selectedData, null);
                            ws.Cells[currRow, 2].Value = selectedData.GetType().GetProperty("EducationTypeCode").GetValue(selectedData, null);
                            ws.Cells[currRow, 3].Value = selectedData.GetType().GetProperty("StartDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 4].Value = selectedData.GetType().GetProperty("EndDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 5].Value = selectedData.GetType().GetProperty("Education").GetValue(selectedData, null);
                            ws.Cells[currRow, 6].Value = selectedData.GetType().GetProperty("Institute").GetValue(selectedData, null);
                            ws.Cells[currRow, 7].Value = selectedData.GetType().GetProperty("CountryCode").GetValue(selectedData, null);
                            ws.Cells[currRow, 8].Value = selectedData.GetType().GetProperty("Certificate").GetValue(selectedData, null);
                            ws.Cells[currRow, 9].Value = selectedData.GetType().GetProperty("Major").GetValue(selectedData, null);
                            ws.Cells[currRow, 10].Value = selectedData.GetType().GetProperty("GraduationDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 11].Value = selectedData.GetType().GetProperty("FinalGrade").GetValue(selectedData, null);
                            currRow = currRow + 1;

                        }

                        pck.SaveAs(output);

                        var byteArray = output.ToArray();

                        if (byteArray == null || byteArray.Length == 0) return null;

                        return new ZipEntry(string.Format("{0}{1}.xlsx", "EDU_", DateTime.Now.ToString("yyyyMMdd")), byteArray);
                    }
                }
            }
            catch
            {
                // Log Exception
                return null;
            }
        }

        private ZipEntry GenerateExcelBankAccount(List<dynamic> data)
        {
            try
            {
                using (var pck = new ExcelPackage())
                {
                    using (var output = new System.IO.MemoryStream())
                    {
                        var ws = pck.Workbook.Worksheets.Add("Bank");

                        ws.Cells[1, 1].Value = "Pers. No";
                        ws.Cells[1, 2].Value = "Start Date";
                        ws.Cells[1, 3].Value = "End Date";
                        ws.Cells[1, 4].Value = "Bank Details Type";
                        ws.Cells[1, 5].Value = "Bank Name";
                        ws.Cells[1, 6].Value = "Bank Branch";
                        ws.Cells[1, 7].Value = "Bank Address";
                        ws.Cells[1, 8].Value = "Bank Key";
                        ws.Cells[1, 9].Value = "Bank Account";
                        ws.Cells[1, 10].Value = "Payee";
                        ws.Cells[1, 11].Value = "Payment Method";
                        ws.Cells[1, 12].Value = "Payment Currency";

                        int currRow = 2;

                        foreach (var selectedData in data)
                        {

                            ws.Cells[currRow, 1].Value = selectedData.GetType().GetProperty("NoReg").GetValue(selectedData, null);
                            ws.Cells[currRow, 2].Value = selectedData.GetType().GetProperty("StartDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 3].Value = selectedData.GetType().GetProperty("EndDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 4].Value = selectedData.GetType().GetProperty("BanksDetailType").GetValue(selectedData, null);
                            ws.Cells[currRow, 5].Value = "";
                            ws.Cells[currRow, 6].Value = "";
                            ws.Cells[currRow, 7].Value = "";
                            ws.Cells[currRow, 8].Value = selectedData.GetType().GetProperty("BankCode").GetValue(selectedData, null);
                            ws.Cells[currRow, 9].Value = selectedData.GetType().GetProperty("AccountNumber").GetValue(selectedData, null);
                            ws.Cells[currRow, 10].Value = selectedData.GetType().GetProperty("Name").GetValue(selectedData, null);
                            ws.Cells[currRow, 11].Value = selectedData.GetType().GetProperty("PaymentMethod").GetValue(selectedData, null);
                            ws.Cells[currRow, 12].Value = selectedData.GetType().GetProperty("PaymentCurrency").GetValue(selectedData, null);
                            currRow = currRow + 1;

                        }

                        pck.SaveAs(output);

                        var byteArray = output.ToArray();

                        if (byteArray == null || byteArray.Length == 0) return null;

                        return new ZipEntry(string.Format("{0}{1}.xlsx", "ACC_", DateTime.Now.ToString("yyyyMMdd")), byteArray);
                    }
                }
            }
            catch
            {
                // Log Exception
                return null;
            }
        }

        private ZipEntry GenerateExcelTaxStatus(List<dynamic> data)
        {
            try
            {
                using (var pck = new ExcelPackage())
                {
                    using (var output = new System.IO.MemoryStream())
                    {
                        var ws = pck.Workbook.Worksheets.Add("Tax");

                        ws.Cells[1, 1].Value = "Pers. No";
                        ws.Cells[1, 2].Value = "Start Date";
                        ws.Cells[1, 3].Value = "End Date";
                        ws.Cells[1, 4].Value = "TAX ID 1";
                        ws.Cells[1, 5].Value = "TAX ID 2";
                        ws.Cells[1, 6].Value = "TAX ID 3";
                        ws.Cells[1, 7].Value = "Number of Dependents";
                        ws.Cells[1, 8].Value = "Married for Tax Purposes";
                        ws.Cells[1, 9].Value = "Spouse Benefit";

                        int currRow = 2;

                        foreach (var selectedData in data)
                        {

                            ws.Cells[currRow, 1].Value = selectedData.GetType().GetProperty("NoReg").GetValue(selectedData, null);
                            ws.Cells[currRow, 2].Value = selectedData.GetType().GetProperty("StartDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 3].Value = selectedData.GetType().GetProperty("EndDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 4].Value = selectedData.GetType().GetProperty("NPWP1").GetValue(selectedData, null);
                            ws.Cells[currRow, 5].Value = selectedData.GetType().GetProperty("NPWP2").GetValue(selectedData, null);
                            ws.Cells[currRow, 6].Value = selectedData.GetType().GetProperty("NPWP3").GetValue(selectedData, null);
                            ws.Cells[currRow, 7].Value = selectedData.GetType().GetProperty("Dependants").GetValue(selectedData, null);
                            ws.Cells[currRow, 8].Value = selectedData.GetType().GetProperty("Married").GetValue(selectedData, null);
                            ws.Cells[currRow, 9].Value = selectedData.GetType().GetProperty("Spouse").GetValue(selectedData, null);
                            currRow = currRow + 1;

                        }

                        pck.SaveAs(output);

                        var byteArray = output.ToArray();

                        if (byteArray == null && byteArray.Length == 0) return null;

                        return new ZipEntry(string.Format("{0}{1}.xlsx", "TAX_", DateTime.Now.ToString("yyyyMMdd")), byteArray);
                    }
                }
            }
            catch
            {
                // Log Exception
                return null;
            }
        }


        private ZipEntry GenerateExcelBenefitClaim(List<dynamic> data)
        {
            try
            {
                using (var pck = new ExcelPackage())
                {
                    using (var output = new MemoryStream())
                    {
                        var ws = pck.Workbook.Worksheets.Add("Tunjangan");
                        //header
                        ws.Cells[1, 1].Value = "Pers. No";
                        ws.Cells[1, 2].Value = "Infotype";
                        ws.Cells[1, 3].Value = "Wage Type";
                        ws.Cells[1, 4].Value = "Amount";
                        ws.Cells[1, 5].Value = "Currency";
                        ws.Cells[1, 6].Value = "Date of Origin";

                        var currRow = 2;

                        var aa = data[0];

                        foreach (var selectedData in data)
                        {
                            ws.Cells[currRow, 1].Value = selectedData.GetType().GetProperty("GeneralNoReg").GetValue(selectedData, null);
                            ws.Cells[currRow, 2].Value = selectedData.GetType().GetProperty("GeneralInfotype").GetValue(selectedData, null);
                            ws.Cells[currRow, 3].Value = selectedData.GetType().GetProperty("GeneralWageType").GetValue(selectedData, null);
                            ws.Cells[currRow, 4].Value = selectedData.GetType().GetProperty("GeneralAmount").GetValue(selectedData, null);
                            ws.Cells[currRow, 5].Value = selectedData.GetType().GetProperty("GeneralCurrency").GetValue(selectedData, null);
                            ws.Cells[currRow, 6].Value = selectedData.GetType().GetProperty("GeneralDateofOrigin").GetValue(selectedData, null);
                            currRow = currRow + 1;
                        }

                        pck.SaveAs(output);

                        var byteArray = output.ToArray();

                        if (byteArray == null || byteArray.Length == 0) return null;

                        return new ZipEntry(string.Format("{0}{1}.xlsx", "ADP_", DateTime.Now.ToString("yyyyMMdd")), byteArray);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private ZipEntry GenerateExcelPtaAllowance(List<dynamic> data)
        {
            try
            {
                using (var pck = new ExcelPackage())
                {
                    using (var output = new MemoryStream())
                    {
                        var ws = pck.Workbook.Worksheets.Add("Tunjangan");
                        //header
                        ws.Cells[1, 1].Value = "Pers. No";
                        ws.Cells[1, 2].Value = "Infotype";
                        ws.Cells[1, 3].Value = "Wage Type";
                        ws.Cells[1, 4].Value = "Amount";
                        ws.Cells[1, 5].Value = "Currency";
                        ws.Cells[1, 6].Value = "Date of Origin";

                        var currRow = 2;

                        foreach (var selectedData in data)
                        {
                            foreach (var aa in selectedData)
                            {
                                ws.Cells[currRow, 1].Value = aa.GetType().GetProperty("GeneralNoReg").GetValue(aa, null);
                                ws.Cells[currRow, 2].Value = aa.GetType().GetProperty("GeneralInfotype").GetValue(aa, null);
                                ws.Cells[currRow, 3].Value = aa.GetType().GetProperty("GeneralWageType").GetValue(aa, null);
                                ws.Cells[currRow, 4].Value = aa.GetType().GetProperty("GeneralAmount").GetValue(aa, null);
                                ws.Cells[currRow, 5].Value = aa.GetType().GetProperty("GeneralCurrency").GetValue(aa, null);
                                ws.Cells[currRow, 6].Value = aa.GetType().GetProperty("GeneralDateofOrigin").GetValue(aa, null);
                                currRow = currRow + 1;
                            }
                        }

                        pck.SaveAs(output);

                        var byteArray = output.ToArray();

                        if (byteArray == null || byteArray.Length == 0) return null;

                        return new ZipEntry(string.Format("{0}{1}.xlsx", "ADP_", DateTime.Now.ToString("yyyyMMdd")), byteArray);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private ZipEntry GenerateExcelCompanyLoan(List<dynamic> data)
        {
            try
            {
                using (var pck = new ExcelPackage())
                {
                    using (var output = new System.IO.MemoryStream())
                    {
                        var ws = pck.Workbook.Worksheets.Add("Pinjaman Perusahaan");

                        ws.Cells[1, 1].Value = "Pers. No";
                        ws.Cells[1, 2].Value = "Infotype";
                        ws.Cells[1, 3].Value = "Loan Type";
                        ws.Cells[1, 4].Value = "Sequence Number";
                        ws.Cells[1, 5].Value = "Start Date";
                        ws.Cells[1, 6].Value = "Approval Date";
                        ws.Cells[1, 7].Value = "Loan Amount Granted";
                        ws.Cells[1, 8].Value = "Currency";
                        ws.Cells[1, 9].Value = "Loan Conditions";
                        ws.Cells[1, 10].Value = "Indiv. Interest rate";
                        ws.Cells[1, 11].Value = "Indiv. Ref. Interest";
                        ws.Cells[1, 12].Value = "Repayment Start";
                        ws.Cells[1, 13].Value = "Annuity Instal.";
                        ws.Cells[1, 14].Value = "Currency";
                        ws.Cells[1, 15].Value = "Payment Type";

                        int currRow = 2;

                        foreach (var selectedData in data)
                        {

                            ws.Cells[currRow, 1].Value = selectedData.GetType().GetProperty("NoReg").GetValue(selectedData, null);
                            ws.Cells[currRow, 2].Value = selectedData.GetType().GetProperty("InfoType").GetValue(selectedData, null);
                            ws.Cells[currRow, 3].Value = selectedData.GetType().GetProperty("LoanType").GetValue(selectedData, null);
                            ws.Cells[currRow, 4].Value = selectedData.GetType().GetProperty("SequenceNumber").GetValue(selectedData, null);
                            ws.Cells[currRow, 5].Value = selectedData.GetType().GetProperty("StartDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 6].Value = selectedData.GetType().GetProperty("ApprovalDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 7].Value = selectedData.GetType().GetProperty("LoanAmount").GetValue(selectedData, null);
                            ws.Cells[currRow, 8].Value = selectedData.GetType().GetProperty("CurrencyKey").GetValue(selectedData, null);
                            ws.Cells[currRow, 9].Value = selectedData.GetType().GetProperty("LoanConditions").GetValue(selectedData, null);
                            ws.Cells[currRow, 10].Value = selectedData.GetType().GetProperty("InterestRate").GetValue(selectedData, null);
                            ws.Cells[currRow, 11].Value = selectedData.GetType().GetProperty("RefInterest").GetValue(selectedData, null);
                            ws.Cells[currRow, 12].Value = selectedData.GetType().GetProperty("RepaymentStart").GetValue(selectedData, null);
                            ws.Cells[currRow, 13].Value = selectedData.GetType().GetProperty("Annuity").GetValue(selectedData, null);
                            ws.Cells[currRow, 14].Value = selectedData.GetType().GetProperty("CurrencyKey").GetValue(selectedData, null);
                            ws.Cells[currRow, 15].Value = selectedData.GetType().GetProperty("PaymentType").GetValue(selectedData, null);
                            currRow = currRow + 1;

                        }

                        pck.SaveAs(output);

                        var byteArray = output.ToArray();

                        if (byteArray == null || byteArray.Length == 0) return null;

                        return new ZipEntry(string.Format("{0}{1}.xlsx", "LON_", DateTime.Now.ToString("yyyyMMdd")), byteArray);
                    }
                }
            }
            catch
            {
                // Log Exception
                return null;
            }
        }

        private ZipEntry GenerateExcelVacation(List<dynamic> data)
        {
            try
            {
                using (var pck = new ExcelPackage())
                {
                    using (var output = new System.IO.MemoryStream())
                    {
                        var ws = pck.Workbook.Worksheets.Add("Rekreasi");

                        ws.Cells[1, 1].Value = "Pers. No";
                        ws.Cells[1, 2].Value = "Infotype";
                        ws.Cells[1, 3].Value = "Wage Type";
                        ws.Cells[1, 4].Value = "Amount";
                        ws.Cells[1, 5].Value = "Currency";
                        ws.Cells[1, 6].Value = "Date Of Origin";

                        int currRow = 2;

                        foreach (var selectedData in data)
                        {
                            foreach (var dat in selectedData)
                            {
                                ws.Cells[currRow, 1].Value = dat.GetType().GetProperty("GeneralNoReg").GetValue(dat, null);
                                ws.Cells[currRow, 2].Value = dat.GetType().GetProperty("GeneralInfotype").GetValue(dat, null);
                                ws.Cells[currRow, 3].Value = dat.GetType().GetProperty("GeneralWageType").GetValue(dat, null);
                                ws.Cells[currRow, 4].Value = dat.GetType().GetProperty("GeneralAmount").GetValue(dat, null);
                                ws.Cells[currRow, 5].Value = dat.GetType().GetProperty("GeneralCurrency").GetValue(dat, null);
                                ws.Cells[currRow, 6].Value = dat.GetType().GetProperty("GeneralDateofOrigin").GetValue(dat, null);
                                currRow = currRow + 1;
                            }
                            //ws.Cells[currRow, 1].Value = selectedData.GetType().GetProperty("NoReg").GetValue(selectedData, null);
                            //ws.Cells[currRow, 2].Value = selectedData.GetType().GetProperty("InfoType").GetValue(selectedData, null);
                            //ws.Cells[currRow, 3].Value = selectedData.GetType().GetProperty("LoanType").GetValue(selectedData, null);
                            //ws.Cells[currRow, 4].Value = selectedData.GetType().GetProperty("SequenceNumber").GetValue(selectedData, null);
                            //ws.Cells[currRow, 5].Value = selectedData.GetType().GetProperty("StartDate").GetValue(selectedData, null);
                            //ws.Cells[currRow, 6].Value = selectedData.GetType().GetProperty("ApprovalDate").GetValue(selectedData, null);
                            //ws.Cells[currRow, 7].Value = selectedData.GetType().GetProperty("LoanAmount").GetValue(selectedData, null);
                            //ws.Cells[currRow, 8].Value = selectedData.GetType().GetProperty("CurrencyKey").GetValue(selectedData, null);
                            //ws.Cells[currRow, 9].Value = selectedData.GetType().GetProperty("LoanConditions").GetValue(selectedData, null);
                            //ws.Cells[currRow, 10].Value = selectedData.GetType().GetProperty("RepaymentStart").GetValue(selectedData, null);
                            //ws.Cells[currRow, 11].Value = selectedData.GetType().GetProperty("Annuity").GetValue(selectedData, null);
                            //ws.Cells[currRow, 12].Value = selectedData.GetType().GetProperty("PaymentDate").GetValue(selectedData, null);
                            //ws.Cells[currRow, 13].Value = selectedData.GetType().GetProperty("PaymentType").GetValue(selectedData, null);
                            //ws.Cells[currRow, 14].Value = selectedData.GetType().GetProperty("Amount").GetValue(selectedData, null);


                        }

                        pck.SaveAs(output);

                        var byteArray = output.ToArray();

                        if (byteArray == null || byteArray.Length == 0) return null;

                        return new ZipEntry(string.Format("{0}{1}.xlsx", "ADP_", DateTime.Now.ToString("yyyyMMdd")), byteArray);
                    }
                }
            }
            catch
            {
                // Log Exception
                return null;
            }
        }

        private ZipEntry GenerateExcelShiftMeal(List<dynamic> data)
        {
            try
            {
                using (var pck = new ExcelPackage())
                {
                    using (var output = new MemoryStream())
                    {
                        var ws = pck.Workbook.Worksheets.Add("Pengganti Uang Makan dan Uang Makan Shift");
                        //header
                        ws.Cells[1, 1].Value = "Pers. No";
                        ws.Cells[1, 2].Value = "Infotype";
                        ws.Cells[1, 3].Value = "Date";
                        ws.Cells[1, 4].Value = "Wage Type";
                        ws.Cells[1, 5].Value = "Number Of Hours";
                        ws.Cells[1, 6].Value = "Number";
                        ws.Cells[1, 7].Value = "Unit";

                        var currRow = 2;
                        foreach (var selectedData in data)
                        {
                            foreach (var dat in selectedData)
                            {
                                ws.Cells[currRow, 1].Value = dat.GetType().GetProperty("NoReg").GetValue(dat, null);
                                ws.Cells[currRow, 2].Value = dat.GetType().GetProperty("Infotype").GetValue(dat, null);
                                ws.Cells[currRow, 3].Value = dat.GetType().GetProperty("Date").GetValue(dat, null);
                                ws.Cells[currRow, 4].Value = dat.GetType().GetProperty("WageType").GetValue(dat, null);
                                ws.Cells[currRow, 5].Value = dat.GetType().GetProperty("NumberOfHours").GetValue(dat, null);
                                ws.Cells[currRow, 6].Value = dat.GetType().GetProperty("Number").GetValue(dat, null);
                                ws.Cells[currRow, 7].Value = dat.GetType().GetProperty("Unit").GetValue(dat, null);
                                currRow = currRow + 1;
                            }
                            //ws.Cells[currRow, 1].Value = selectedData.GetType().GetProperty("NoReg").GetValue(selectedData, null);
                            //ws.Cells[currRow, 2].Value = selectedData.GetType().GetProperty("Infotype").GetValue(selectedData, null);
                            //ws.Cells[currRow, 3].Value = selectedData.GetType().GetProperty("Date").GetValue(selectedData, null);
                            //ws.Cells[currRow, 4].Value = selectedData.GetType().GetProperty("WageType").GetValue(selectedData, null);
                            //ws.Cells[currRow, 5].Value = selectedData.GetType().GetProperty("NumberOfHours").GetValue(selectedData, null);
                            //ws.Cells[currRow, 6].Value = selectedData.GetType().GetProperty("Number").GetValue(selectedData, null);
                            //ws.Cells[currRow, 7].Value = selectedData.GetType().GetProperty("Unit").GetValue(selectedData, null);
                            //currRow = currRow + 1;
                        }

                        pck.SaveAs(output);

                        var byteArray = output.ToArray();

                        if (byteArray == null || byteArray.Length == 0) return null;

                        return new ZipEntry(string.Format("{0}{1}.xlsx", "ERI_", DateTime.Now.ToString("yyyyMMdd")), byteArray);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private ZipEntry GenerateExcelMeal(List<dynamic> data)
        {
            try
            {
                using (var pck = new ExcelPackage())
                {
                    using (var output = new MemoryStream())
                    {
                        var ws = pck.Workbook.Worksheets.Add("Pengganti Uang Makan dan Uang Makan Shift");
                        //header
                        ws.Cells[1, 1].Value = "Pers. No";
                        ws.Cells[1, 2].Value = "Infotype";
                        ws.Cells[1, 3].Value = "Date";
                        ws.Cells[1, 4].Value = "Wage Type";
                        ws.Cells[1, 5].Value = "Number Of Hours";
                        ws.Cells[1, 6].Value = "Number";
                        ws.Cells[1, 7].Value = "Unit";

                        var currRow = 2;
                        var groupedData = data.GroupBy(x => new { x.NoReg, x.InfoType, x.Date, x.WageType, x.NumberOfHours, x.Unit });

                        foreach (var selectedData in groupedData)
                        {
                            var key = selectedData.Key;

                            ws.Cells[currRow, 1].Value = key.NoReg;
                            ws.Cells[currRow, 2].Value = key.InfoType;
                            ws.Cells[currRow, 3].Value = key.Date;
                            ws.Cells[currRow, 4].Value = key.WageType;
                            ws.Cells[currRow, 5].Value = key.NumberOfHours;
                            ws.Cells[currRow, 6].Value = selectedData.Sum(x => x.Number);
                            ws.Cells[currRow, 7].Value = key.Unit;

                            currRow = currRow + 1;
                        }

                        pck.SaveAs(output);

                        var byteArray = output.ToArray();

                        if (byteArray == null || byteArray.Length == 0) return null;

                        return new ZipEntry(string.Format("{0}{1}.xlsx", "ERI_", DateTime.Now.ToString("yyyyMMdd")), byteArray);
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        private ZipEntry GenerateExcelSPKL(List<dynamic> data)
        {
            try
            {
                using (var pck = new ExcelPackage())
                {
                    using (var output = new System.IO.MemoryStream())
                    {
                        var ws = pck.Workbook.Worksheets.Add("SPKL");

                        ws.Cells[1, 1].Value = "Pers. No";
                        ws.Cells[1, 2].Value = "Start Date";
                        ws.Cells[1, 3].Value = "End Date";
                        ws.Cells[1, 4].Value = "Type";
                        ws.Cells[1, 5].Value = "Time Start";
                        ws.Cells[1, 6].Value = "Time End";
                        //ws.Cells[1, 7].Value = "Quota Number";
                        //ws.Cells[1, 8].Value = "Overtime Comp. Type";

                        int currRow = 2;

                        foreach (var selectedData in data)
                        {

                            ws.Cells[currRow, 1].Value = selectedData.GetType().GetProperty("NoReg").GetValue(selectedData, null);
                            ws.Cells[currRow, 2].Value = selectedData.GetType().GetProperty("StartDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 3].Value = selectedData.GetType().GetProperty("EndDate").GetValue(selectedData, null);
                            ws.Cells[currRow, 4].Value = selectedData.GetType().GetProperty("Type").GetValue(selectedData, null);
                            ws.Cells[currRow, 5].Value = selectedData.GetType().GetProperty("TimeStart").GetValue(selectedData, null);
                            ws.Cells[currRow, 6].Value = selectedData.GetType().GetProperty("TimeEnd").GetValue(selectedData, null);
                            //ws.Cells[currRow, 7].Value = selectedData.GetType().GetProperty("QuotaNumber").GetValue(selectedData, null);
                            //ws.Cells[currRow, 8].Value = selectedData.GetType().GetProperty("OvertimeCompType").GetValue(selectedData, null);
                            currRow = currRow + 1;

                        }

                        pck.SaveAs(output);

                        var byteArray = output.ToArray();

                        if (byteArray == null || byteArray.Length == 0) return null;

                        return new ZipEntry(string.Format("{0}{1}.xlsx", "2007_", DateTime.Now.ToString("yyyyMMdd")), byteArray);
                    }
                }
            }
            catch
            {
                // Log Exception
                return null;
            }
        }

        private ZipEntry GenerateExcelHiringEmployee(List<dynamic> data)
        {
            try
            {
                using (var pck = new ExcelPackage())
                {
                    using (var output = new MemoryStream())
                    {
                        var ws = pck.Workbook.Worksheets.Add("Hiring Employee");

                        ws.Cells[1, 1].Value = "NPK";
                        ws.Cells[1, 2].Value = "Placement Date";
                        ws.Cells[1, 3].Value = "Action Type";
                        ws.Cells[1, 4].Value = "Reason For Action";
                        ws.Cells[1, 5].Value = "Position";
                        ws.Cells[1, 6].Value = "Personnel Area";
                        ws.Cells[1, 7].Value = "Employee Group";
                        ws.Cells[1, 8].Value = "Employee SubGroup";
                        ws.Cells[1, 9].Value = "SubArea";
                        ws.Cells[1, 10].Value = "Work Contract";
                        ws.Cells[1, 11].Value = "Effective Date";
                        ws.Cells[1, 12].Value = "SubClass";
                        ws.Cells[1, 13].Value = "Labour Type";
                        ws.Cells[1, 14].Value = "Nama Sesuai KTP";
                        ws.Cells[1, 15].Value = "Nama Lengkap";
                        ws.Cells[1, 16].Value = "Tempat Lahir";
                        ws.Cells[1, 17].Value = "Tanggal Lahir";
                        ws.Cells[1, 18].Value = "Gender";
                        ws.Cells[1, 19].Value = "Agama";
                        ws.Cells[1, 20].Value = "Golongan Darah";
                        ws.Cells[1, 21].Value = "Kewarganegaraan";
                        ws.Cells[1, 22].Value = "NIK";
                        ws.Cells[1, 23].Value = "No. Paspor";
                        ws.Cells[1, 24].Value = "No. KK";
                        ws.Cells[1, 25].Value = "No. BPJS KetenagaKerjaan";
                        ws.Cells[1, 26].Value = "No. Handphone";
                        ws.Cells[1, 27].Value = "Email Pribadi";
                        ws.Cells[1, 28].Value = "NPWP";
                        ws.Cells[1, 29].Value = "Tax Status";
                        ws.Cells[1, 30].Value = "Dependence";
                        ws.Cells[1, 31].Value = "Status Pernikahan";
                        ws.Cells[1, 32].Value = "No. Akta Nikah";
                        ws.Cells[1, 33].Value = "Tanggal Pernikahan";
                        ws.Cells[1, 34].Value = "SIM A Number";
                        ws.Cells[1, 35].Value = "SIM C Number";
                        ws.Cells[1, 36].Value = "Alamat Sesuai KTP";
                        ws.Cells[1, 37].Value = "RT";
                        ws.Cells[1, 38].Value = "RW";
                        ws.Cells[1, 39].Value = "Kelurahan";
                        ws.Cells[1, 40].Value = "Kecamatan";
                        ws.Cells[1, 41].Value = "Kota";
                        ws.Cells[1, 42].Value = "Provinsi";
                        ws.Cells[1, 43].Value = "Kode Pos";
                        ws.Cells[1, 44].Value = "Alamat Sesuai Domisili";
                        ws.Cells[1, 45].Value = "RT";
                        ws.Cells[1, 46].Value = "RW";
                        ws.Cells[1, 47].Value = "Kelurahan";
                        ws.Cells[1, 48].Value = "Kecamatan";
                        ws.Cells[1, 49].Value = "Kota";
                        ws.Cells[1, 50].Value = "Provinsi";
                        ws.Cells[1, 51].Value = "Kode Pos";
                        ws.Cells[1, 52].Value = "Account Number";
                        ws.Cells[1, 53].Value = "Nama Rekening";
                        ws.Cells[1, 54].Value = "Bank Key";
                        ws.Cells[1, 55].Value = "Payment Type";
                        ws.Cells[1, 56].Value = "Nama Ayah Kandung";
                        ws.Cells[1, 57].Value = "Tempat Lahir";
                        ws.Cells[1, 58].Value = "Tanggal Lahir";
                        ws.Cells[1, 59].Value = "Gender";
                        ws.Cells[1, 60].Value = "Nama Ibu Kandung";
                        ws.Cells[1, 61].Value = "Tempat Lahir";
                        ws.Cells[1, 62].Value = "Tanggal Lahir";
                        ws.Cells[1, 63].Value = "Gender";
                        ws.Cells[1, 64].Value = "Nama Saudara Kandung";
                        ws.Cells[1, 65].Value = "Tempat Lahir";
                        ws.Cells[1, 66].Value = "Tanggal Lahir";
                        ws.Cells[1, 67].Value = "Gender";
                        ws.Cells[1, 68].Value = "Nama Suami/Istri";
                        ws.Cells[1, 69].Value = "Tempat Lahir";
                        ws.Cells[1, 70].Value = "Tanggal Lahir";
                        ws.Cells[1, 71].Value = "Gender";
                        ws.Cells[1, 72].Value = "Nama Anak1";
                        ws.Cells[1, 73].Value = "Anak Ke";
                        ws.Cells[1, 74].Value = "Status Anak";
                        ws.Cells[1, 75].Value = "Tempat Lahir";
                        ws.Cells[1, 76].Value = "Tanggal Lahir";
                        ws.Cells[1, 77].Value = "Gender";
                        ws.Cells[1, 78].Value = "Nama Anak2";
                        ws.Cells[1, 79].Value = "Anak Ke";
                        ws.Cells[1, 80].Value = "Status Anak";
                        ws.Cells[1, 81].Value = "Tempat Lahir";
                        ws.Cells[1, 82].Value = "Tanggal Lahir";
                        ws.Cells[1, 83].Value = "Gender";
                        ws.Cells[1, 84].Value = "Nama Anak3";
                        ws.Cells[1, 85].Value = "Anak Ke";
                        ws.Cells[1, 86].Value = "Status Anak";
                        ws.Cells[1, 87].Value = "Tempat Lahir";
                        ws.Cells[1, 88].Value = "Tanggal Lahir";
                        ws.Cells[1, 89].Value = "Gender";
                        ws.Cells[1, 90].Value = "Nama Ayah Mertua";
                        ws.Cells[1, 91].Value = "Tempat Lahir";
                        ws.Cells[1, 92].Value = "Tanggal Lahir";
                        ws.Cells[1, 93].Value = "Gender";
                        ws.Cells[1, 94].Value = "Nama Ibu Mertua";
                        ws.Cells[1, 95].Value = "Tempat Lahir";
                        ws.Cells[1, 96].Value = "Tanggal Lahir";
                        ws.Cells[1, 97].Value = "Gender";
                        ws.Cells[1, 98].Value = "Nama Kakek dari Ayah";
                        ws.Cells[1, 99].Value = "Tanggal Lahir";
                        ws.Cells[1, 100].Value = "Gender";
                        ws.Cells[1, 101].Value = "Nama Kakek dari Ibu";
                        ws.Cells[1, 102].Value = "Tanggal Lahir";
                        ws.Cells[1, 103].Value = "Gender";
                        ws.Cells[1, 104].Value = "Nama Nenek dari Ayah";
                        ws.Cells[1, 105].Value = "Tanggal Lahir";
                        ws.Cells[1, 106].Value = "Gender";
                        ws.Cells[1, 107].Value = "Nama Nenek dari Ibu";
                        ws.Cells[1, 108].Value = "Tanggal Lahir";
                        ws.Cells[1, 109].Value = "Gender";
                        ws.Cells[1, 110].Value = "Tingkat Pendidikan";
                        ws.Cells[1, 111].Value = "Nama Lembaga Pendidikan";
                        ws.Cells[1, 112].Value = "Jurusan";
                        ws.Cells[1, 113].Value = "Negara";
                        ws.Cells[1, 114].Value = "Tanggal Awal Pendidikan";
                        ws.Cells[1, 115].Value = "Tanggal Akhir Pendidikan";
                        ws.Cells[1, 116].Value = "Nilai";
                        ws.Cells[1, 117].Value = "Sertifikat";
                        ws.Cells[1, 118].Value = "Tanggal Join Astra";
                        ws.Cells[1, 119].Value = "Work Schedule Rule";
                        ws.Cells[1, 120].Value = "Time Management Status";
                        ws.Cells[1, 121].Value = "Time Recording Info";
                        ws.Cells[1, 122].Value = "ID Version";

                        var currRow = 2;

                        foreach (var selectedData in data)
                        {
                            var emp = selectedData[0];
                            //ws.Cells[currRow, 1].Value = selectedData.GetType().GetProperty("NoReg").GetValue(selectedData, null);
                            //ws.Cells[currRow, 2].Value = selectedData.GetType().GetProperty("FamilyTypeCode").GetValue(selectedData, null);
                            //ws.Cells[currRow, 3].Value = selectedData.GetType().GetProperty("StartDate").GetValue(selectedData, null);
                            //ws.Cells[currRow, 4].Value = selectedData.GetType().GetProperty("EndDate").GetValue(selectedData, null);
                            //ws.Cells[currRow, 5].Value = selectedData.GetType().GetProperty("Name").GetValue(selectedData, null);
                            //ws.Cells[currRow, 6].Value = selectedData.GetType().GetProperty("NationalityCode").GetValue(selectedData, null);
                            //ws.Cells[currRow, 7].Value = selectedData.GetType().GetProperty("BirthPlace").GetValue(selectedData, null);
                            //ws.Cells[currRow, 8].Value = selectedData.GetType().GetProperty("BirthDate").GetValue(selectedData, null);
                            //ws.Cells[currRow, 9].Value = selectedData.GetType().GetProperty("GenderCode").GetValue(selectedData, null);
                            //ws.Cells[currRow, 10].Value = selectedData.GetType().GetProperty("Occupation").GetValue(selectedData, null);
                            //ws.Cells[currRow, 11].Value = selectedData.GetType().GetProperty("MartialStatusCode").GetValue(selectedData, null);

                            ws.Cells[currRow, 1].Value = emp.GetType().GetProperty("NoReg").GetValue(emp, null);
                            ws.Cells[currRow, 2].Value = emp.GetType().GetProperty("PlacementDate").GetValue(emp, null);
                            ws.Cells[currRow, 3].Value = emp.GetType().GetProperty("ActionType").GetValue(emp, null);
                            ws.Cells[currRow, 4].Value = emp.GetType().GetProperty("ReasonForAction").GetValue(emp, null);
                            ws.Cells[currRow, 5].Value = emp.GetType().GetProperty("Position").GetValue(emp, null);
                            ws.Cells[currRow, 6].Value = emp.GetType().GetProperty("PersonelArea").GetValue(emp, null);
                            ws.Cells[currRow, 7].Value = emp.GetType().GetProperty("EmployeeGroup").GetValue(emp, null);
                            ws.Cells[currRow, 8].Value = emp.GetType().GetProperty("EmployeeSubGroup").GetValue(emp, null);
                            ws.Cells[currRow, 9].Value = emp.GetType().GetProperty("SubArea").GetValue(emp, null);
                            ws.Cells[currRow, 10].Value = emp.GetType().GetProperty("WorkContract").GetValue(emp, null);
                            ws.Cells[currRow, 11].Value = emp.GetType().GetProperty("EffectiveDate").GetValue(emp, null);
                            ws.Cells[currRow, 12].Value = emp.GetType().GetProperty("SubClass").GetValue(emp, null);
                            ws.Cells[currRow, 13].Value = emp.GetType().GetProperty("LabourType").GetValue(emp, null);
                            ws.Cells[currRow, 14].Value = emp.GetType().GetProperty("Name").GetValue(emp, null);
                            ws.Cells[currRow, 15].Value = emp.GetType().GetProperty("FullName").GetValue(emp, null);
                            ws.Cells[currRow, 16].Value = emp.GetType().GetProperty("BirthPlace").GetValue(emp, null);
                            ws.Cells[currRow, 17].Value = emp.GetType().GetProperty("BirthDate").GetValue(emp, null);
                            ws.Cells[currRow, 18].Value = emp.GetType().GetProperty("Gender").GetValue(emp, null);
                            ws.Cells[currRow, 19].Value = emp.GetType().GetProperty("Religion").GetValue(emp, null);
                            ws.Cells[currRow, 20].Value = emp.GetType().GetProperty("BloodType").GetValue(emp, null);
                            ws.Cells[currRow, 21].Value = emp.GetType().GetProperty("Nationality").GetValue(emp, null);
                            ws.Cells[currRow, 22].Value = emp.GetType().GetProperty("NIK").GetValue(emp, null);
                            ws.Cells[currRow, 23].Value = emp.GetType().GetProperty("Passport").GetValue(emp, null);
                            ws.Cells[currRow, 24].Value = emp.GetType().GetProperty("KK").GetValue(emp, null);
                            ws.Cells[currRow, 25].Value = emp.GetType().GetProperty("BPJSNumber").GetValue(emp, null);
                            ws.Cells[currRow, 26].Value = emp.GetType().GetProperty("PhoneNumber").GetValue(emp, null);
                            ws.Cells[currRow, 27].Value = emp.GetType().GetProperty("email").GetValue(emp, null);
                            ws.Cells[currRow, 28].Value = emp.GetType().GetProperty("NPWP").GetValue(emp, null);
                            ws.Cells[currRow, 29].Value = emp.GetType().GetProperty("TaxStatus").GetValue(emp, null);
                            ws.Cells[currRow, 30].Value = emp.GetType().GetProperty("Dependence").GetValue(emp, null);
                            ws.Cells[currRow, 31].Value = emp.GetType().GetProperty("MaritalStatus").GetValue(emp, null);
                            ws.Cells[currRow, 32].Value = emp.GetType().GetProperty("MaritalNumber").GetValue(emp, null);
                            ws.Cells[currRow, 33].Value = emp.GetType().GetProperty("MaritalDate").GetValue(emp, null);
                            ws.Cells[currRow, 34].Value = emp.GetType().GetProperty("SimANumber").GetValue(emp, null);
                            ws.Cells[currRow, 35].Value = emp.GetType().GetProperty("SimCNumber").GetValue(emp, null);
                            ws.Cells[currRow, 36].Value = emp.GetType().GetProperty("PrimaryAddress").GetValue(emp, null);
                            ws.Cells[currRow, 37].Value = emp.GetType().GetProperty("PrimaryRT").GetValue(emp, null);
                            ws.Cells[currRow, 38].Value = emp.GetType().GetProperty("PrimaryRW").GetValue(emp, null);
                            ws.Cells[currRow, 39].Value = emp.GetType().GetProperty("PrimaryUrbanVillage").GetValue(emp, null);
                            ws.Cells[currRow, 40].Value = emp.GetType().GetProperty("PrimarySubDistrict").GetValue(emp, null);
                            ws.Cells[currRow, 41].Value = emp.GetType().GetProperty("PrimaryDistrict").GetValue(emp, null);
                            ws.Cells[currRow, 42].Value = emp.GetType().GetProperty("PrimaryProvince").GetValue(emp, null);
                            ws.Cells[currRow, 43].Value = emp.GetType().GetProperty("PrimaryPostalCode").GetValue(emp, null);
                            ws.Cells[currRow, 44].Value = emp.GetType().GetProperty("HomeAddress").GetValue(emp, null);
                            ws.Cells[currRow, 45].Value = emp.GetType().GetProperty("HomeRT").GetValue(emp, null);
                            ws.Cells[currRow, 46].Value = emp.GetType().GetProperty("HomeRW").GetValue(emp, null);
                            ws.Cells[currRow, 47].Value = emp.GetType().GetProperty("HomeUrbanVillage").GetValue(emp, null);
                            ws.Cells[currRow, 48].Value = emp.GetType().GetProperty("HomeSubDistrict").GetValue(emp, null);
                            ws.Cells[currRow, 49].Value = emp.GetType().GetProperty("HomeDistrict").GetValue(emp, null);
                            ws.Cells[currRow, 50].Value = emp.GetType().GetProperty("HomeProvince").GetValue(emp, null);
                            ws.Cells[currRow, 51].Value = emp.GetType().GetProperty("HomePostalCode").GetValue(emp, null);
                            ws.Cells[currRow, 52].Value = emp.GetType().GetProperty("BankAccountNumber").GetValue(emp, null);
                            ws.Cells[currRow, 53].Value = emp.GetType().GetProperty("BankAccountName").GetValue(emp, null);
                            ws.Cells[currRow, 54].Value = emp.GetType().GetProperty("BankAccountKey").GetValue(emp, null);
                            ws.Cells[currRow, 55].Value = emp.GetType().GetProperty("PaymentType").GetValue(emp, null);
                            ws.Cells[currRow, 56].Value = emp.GetType().GetProperty("AyahKandung").GetValue(emp, null);
                            ws.Cells[currRow, 57].Value = emp.GetType().GetProperty("AyahKandungBirthPlace").GetValue(emp, null);
                            ws.Cells[currRow, 58].Value = emp.GetType().GetProperty("AyahKandungBirthDate").GetValue(emp, null);
                            ws.Cells[currRow, 59].Value = emp.GetType().GetProperty("AyahKandungGender").GetValue(emp, null);
                            ws.Cells[currRow, 60].Value = emp.GetType().GetProperty("IbuKandung").GetValue(emp, null);
                            ws.Cells[currRow, 61].Value = emp.GetType().GetProperty("IbuKandungBirthPlace").GetValue(emp, null);
                            ws.Cells[currRow, 62].Value = emp.GetType().GetProperty("IbuKandungBirthDate").GetValue(emp, null);
                            ws.Cells[currRow, 63].Value = emp.GetType().GetProperty("IbuKandungGender").GetValue(emp, null);
                            ws.Cells[currRow, 64].Value = emp.GetType().GetProperty("SaudaraKandung").GetValue(emp, null);
                            ws.Cells[currRow, 65].Value = emp.GetType().GetProperty("SaudaraKandungBirthPlace").GetValue(emp, null);
                            ws.Cells[currRow, 66].Value = emp.GetType().GetProperty("SaudaraKandungBirthDate").GetValue(emp, null);
                            ws.Cells[currRow, 67].Value = emp.GetType().GetProperty("SaudaraKandungGender").GetValue(emp, null);
                            ws.Cells[currRow, 68].Value = emp.GetType().GetProperty("Pasangan").GetValue(emp, null);
                            ws.Cells[currRow, 69].Value = emp.GetType().GetProperty("PasanganBirthPlace").GetValue(emp, null);
                            ws.Cells[currRow, 70].Value = emp.GetType().GetProperty("PasanganBirthDate").GetValue(emp, null);
                            ws.Cells[currRow, 71].Value = emp.GetType().GetProperty("PasanganGender").GetValue(emp, null);
                            ws.Cells[currRow, 72].Value = emp.GetType().GetProperty("FirstChild").GetValue(emp, null);
                            ws.Cells[currRow, 73].Value = emp.GetType().GetProperty("FirstChildNumber").GetValue(emp, null);
                            ws.Cells[currRow, 74].Value = emp.GetType().GetProperty("FirstChildStatus").GetValue(emp, null);
                            ws.Cells[currRow, 75].Value = emp.GetType().GetProperty("FirstChildBirthPlace").GetValue(emp, null);
                            ws.Cells[currRow, 76].Value = emp.GetType().GetProperty("FirstChildBirthDate").GetValue(emp, null);
                            ws.Cells[currRow, 77].Value = emp.GetType().GetProperty("FirstChildGender").GetValue(emp, null);
                            ws.Cells[currRow, 78].Value = emp.GetType().GetProperty("SecondChild").GetValue(emp, null);
                            ws.Cells[currRow, 79].Value = emp.GetType().GetProperty("SecondChildNumber").GetValue(emp, null);
                            ws.Cells[currRow, 80].Value = emp.GetType().GetProperty("SecondChildStatus").GetValue(emp, null);
                            ws.Cells[currRow, 81].Value = emp.GetType().GetProperty("SecondChildBirthPlace").GetValue(emp, null);
                            ws.Cells[currRow, 82].Value = emp.GetType().GetProperty("SecondChildBirthDate").GetValue(emp, null);
                            ws.Cells[currRow, 83].Value = emp.GetType().GetProperty("SecondChildGender").GetValue(emp, null);
                            ws.Cells[currRow, 84].Value = emp.GetType().GetProperty("ThirdChild").GetValue(emp, null);
                            ws.Cells[currRow, 85].Value = emp.GetType().GetProperty("ThirdChildNumber").GetValue(emp, null);
                            ws.Cells[currRow, 86].Value = emp.GetType().GetProperty("ThirdChildStatus").GetValue(emp, null);
                            ws.Cells[currRow, 87].Value = emp.GetType().GetProperty("ThirdChildBirthPlace").GetValue(emp, null);
                            ws.Cells[currRow, 88].Value = emp.GetType().GetProperty("ThirdChildBirthDate").GetValue(emp, null);
                            ws.Cells[currRow, 89].Value = emp.GetType().GetProperty("ThirdChildGender").GetValue(emp, null);
                            ws.Cells[currRow, 90].Value = emp.GetType().GetProperty("AyahMertua").GetValue(emp, null);
                            ws.Cells[currRow, 91].Value = emp.GetType().GetProperty("AyahMertuaBirthPlace").GetValue(emp, null);
                            ws.Cells[currRow, 92].Value = emp.GetType().GetProperty("AyahMertuaBirthDate").GetValue(emp, null);
                            ws.Cells[currRow, 93].Value = emp.GetType().GetProperty("AyahMertuaGender").GetValue(emp, null);
                            ws.Cells[currRow, 94].Value = emp.GetType().GetProperty("IbuMertua").GetValue(emp, null);
                            ws.Cells[currRow, 95].Value = emp.GetType().GetProperty("IbuMertuaBirthPlace").GetValue(emp, null);
                            ws.Cells[currRow, 96].Value = emp.GetType().GetProperty("IbuMertuaBirthDate").GetValue(emp, null);
                            ws.Cells[currRow, 97].Value = emp.GetType().GetProperty("IbuMertuaGender").GetValue(emp, null);
                            ws.Cells[currRow, 98].Value = emp.GetType().GetProperty("KakekDariAyah").GetValue(emp, null);
                            ws.Cells[currRow, 99].Value = emp.GetType().GetProperty("KakekDariAyahBirthDate").GetValue(emp, null);
                            ws.Cells[currRow, 100].Value = emp.GetType().GetProperty("KakekDariAyahGender").GetValue(emp, null);
                            ws.Cells[currRow, 101].Value = emp.GetType().GetProperty("KakekDariIbu").GetValue(emp, null);
                            ws.Cells[currRow, 102].Value = emp.GetType().GetProperty("KakekDariIbuBirthDate").GetValue(emp, null);
                            ws.Cells[currRow, 103].Value = emp.GetType().GetProperty("KakekDariIbuGender").GetValue(emp, null);
                            ws.Cells[currRow, 104].Value = emp.GetType().GetProperty("NenekDariAyah").GetValue(emp, null);
                            ws.Cells[currRow, 105].Value = emp.GetType().GetProperty("NenekDariAyahBirthDate").GetValue(emp, null);
                            ws.Cells[currRow, 106].Value = emp.GetType().GetProperty("NenekDariAyahGender").GetValue(emp, null);
                            ws.Cells[currRow, 107].Value = emp.GetType().GetProperty("NenekDariIbu").GetValue(emp, null);
                            ws.Cells[currRow, 108].Value = emp.GetType().GetProperty("NenekDariIbuBirthDate").GetValue(emp, null);
                            ws.Cells[currRow, 109].Value = emp.GetType().GetProperty("NenekDariIbuGender").GetValue(emp, null);
                            ws.Cells[currRow, 110].Value = emp.GetType().GetProperty("EducationType").GetValue(emp, null);
                            ws.Cells[currRow, 111].Value = emp.GetType().GetProperty("Institute").GetValue(emp, null);
                            ws.Cells[currRow, 112].Value = emp.GetType().GetProperty("Major").GetValue(emp, null);
                            ws.Cells[currRow, 113].Value = emp.GetType().GetProperty("NationInstitute").GetValue(emp, null);
                            ws.Cells[currRow, 114].Value = emp.GetType().GetProperty("StartDateEducation").GetValue(emp, null);
                            ws.Cells[currRow, 115].Value = emp.GetType().GetProperty("GraduationDate").GetValue(emp, null);
                            ws.Cells[currRow, 116].Value = emp.GetType().GetProperty("FinalGrade").GetValue(emp, null);
                            ws.Cells[currRow, 117].Value = emp.GetType().GetProperty("Certificate").GetValue(emp, null);
                            ws.Cells[currRow, 118].Value = emp.GetType().GetProperty("JoinAstraDate").GetValue(emp, null);
                            ws.Cells[currRow, 119].Value = emp.GetType().GetProperty("WorkScheduleRule").GetValue(emp, null);
                            ws.Cells[currRow, 120].Value = emp.GetType().GetProperty("TimeManagementStatus").GetValue(emp, null);
                            ws.Cells[currRow, 121].Value = emp.GetType().GetProperty("TimeRecord").GetValue(emp, null);
                            ws.Cells[currRow, 122].Value = emp.GetType().GetProperty("IdVersion").GetValue(emp, null);





                            currRow = currRow + 1;
                        }

                        pck.SaveAs(output);

                        var byteArray = output.ToArray();

                        if (byteArray == null || byteArray.Length == 0) return null;

                        return new ZipEntry(string.Format("{0}{1}.xlsx", "EMP_", DateTime.Now.ToString("yyyyMMdd")), byteArray);
                    }
                }
            }
            catch(Exception e)
            {
                logService.InsertLog("Generate Excel", "Get", "SAP Integration", "false", e.ToString(), ServiceProxy.UserClaim.NoReg);
                return null;
            }
        }

        private ZipEntry GenerateExcelBPJSTK(List<dynamic> data)
        {
            try
            {
                using (var pck = new ExcelPackage())
                {
                    using (var output = new System.IO.MemoryStream())
                    {
                        var ws = pck.Workbook.Worksheets.Add("BPJSTK");

                        ws.Cells[1, 1].Value = "Pers. No";
                        ws.Cells[1, 2].Value = "Start Date";
                        ws.Cells[1, 3].Value = "End Date";
                        ws.Cells[1, 4].Value = "BPJS TK";
                        //ws.Cells[1, 7].Value = "Quota Number";
                        //ws.Cells[1, 8].Value = "Overtime Comp. Type";

                        int currRow = 2;

                        foreach (var selectedData in data)
                        {
                            var emp = selectedData[0];

                            ws.Cells[currRow, 1].Value = emp.GetType().GetProperty("NoReg").GetValue(emp, null);
                            ws.Cells[currRow, 2].Value = emp.GetType().GetProperty("StartDate").GetValue(emp, null);
                            ws.Cells[currRow, 3].Value = emp.GetType().GetProperty("EndDate").GetValue(emp, null);
                            ws.Cells[currRow, 4].Value = emp.GetType().GetProperty("BPJSTK").GetValue(emp, null);
                            //ws.Cells[currRow, 7].Value = selectedData.GetType().GetProperty("QuotaNumber").GetValue(selectedData, null);
                            //ws.Cells[currRow, 8].Value = selectedData.GetType().GetProperty("OvertimeCompType").GetValue(selectedData, null);
                            currRow = currRow + 1;

                        }

                        pck.SaveAs(output);

                        var byteArray = output.ToArray();

                        if (byteArray == null || byteArray.Length == 0) return null;
                        var now = DateTime.Now;
                        return new ZipEntry(string.Format("011_JAM_{0:yyyyMMdd}.xlsx",now), byteArray);
                    }
                }
            }
            catch
            {
                // Log Exception
                return null;
            }
        }



        public class DownloadRequest
        {
            public Guid[] documentApprovalIds { get; set; }
        }

        [HttpPost("completed")]
        public DataSourceResult GetCompletedTasks([DataSourceRequest] DataSourceRequest request)
        {
            return ServiceProxy.GetDataSourceResult<DocumentApprovalSAPView>(request, new { RowStatus = true, VisibleInHistory = true, IntegrationDownload = true/*, DocumentStatusCode = DocumentStatus.Completed*/ });
        }
    }
    #endregion

    #region MVC Controller
    [Area("Core")]
    [Permission(PermissionKey.ViewSapIntegration)]
    public class SapIntegrationController : MvcControllerBase
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ViewCount(Guid id)
        {
            ViewBag.Id = id;

            return PartialView("_ViewCount");
        }
    }
    #endregion
}
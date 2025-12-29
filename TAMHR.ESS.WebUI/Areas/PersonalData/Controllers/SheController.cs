using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Runtime.Providers;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using OfficeOpenXml;

namespace TAMHR.ESS.WebUI.Areas.PersonalData.Controllers
{
    #region API Controller
    /// <summary>
    /// SHE API Manager
    /// </summary>
    [Route("api/she")]
    public class SheApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Personal data service object.
        /// </summary>
        protected PersonalDataService PersonalDataService => ServiceProxy.GetService<PersonalDataService>();

        /// <summary>
        /// MDM service object.
        /// </summary>
        protected MdmService MdmService => ServiceProxy.GetService<MdmService>();

        /// <summary>
        /// Email service object.
        /// </summary>
        protected EmailService EmailService => ServiceProxy.GetService<EmailService>();
        #endregion

        [HttpPost("getlistshe")]
        public async Task<DataSourceResult> GetList([DataSourceRequest] DataSourceRequest request)
        {
            var listdata = new List<SHEViewModel>();

            var dataBpjs = PersonalDataService.GetDataBpjs();

            foreach (var item in dataBpjs.Where(x => x.FamilyMemberId != null))
            {
                listdata.Add(new SHEViewModel()
                {
                    CommonnId = item.Id,
                    Noreg = item.NoReg,
                    Name = item.EmployeeName,
                    FamilyRelation = item.FamilyRelation,
                    Type = "BPJS",
                    ActionType = item.ActionType,
                    Status = item.CompleteStatus ? "Complete" : "Pending"
                });
            }

            var dataInsurance = PersonalDataService.GetDataInsurance();
            
            foreach (var item in dataInsurance.Where(x => x.FamilyMemberId != null))
            {
                listdata.Add(new SHEViewModel()
                {
                    CommonnId = item.Id,
                    Noreg = item.NoReg,
                    Name = item.EmployeeName,
                    FamilyRelation = item.FamilyRelation,
                    Type = "ASURANSI",
                    ActionType = item.ActionType,
                    Status = item.CompleteStatus ? "Complete" : "Pending"
                });
            }

            if (request.Filters.Count == 0)
            {
                listdata = listdata.Where(x => x.Status == "Pending").ToList();
            }

            return await listdata.ToDataSourceResultAsync(request);
        }

        [HttpPost("complete")]
        public async Task<IActionResult> Complete(dynamic data)
        {
            List<Guid> listIdBpjs = data.listIdBpjs.ToObject<List<Guid>>();
            List<Guid> listIdInsurance = data.listIdInsurance.ToObject<List<Guid>>();
            //data bpjs
            var listDataBpjs = PersonalDataService.GetPersonalDataBpjsByListId(listIdBpjs);
            //data asuransi
            var listDataAsuransi = PersonalDataService.GetPersonalDataInsurancesByListId(listIdInsurance);

            listDataBpjs.Each(x => x.CompleteStatus = true);
            listDataAsuransi.Each(x => x.CompleteStatus = true);

            PersonalDataService.UpsertMultiplePersonalDataBpjs(listDataBpjs.ToList());
            PersonalDataService.UpsertMultiplePersonalDataInsurance(listDataAsuransi.ToList());

            List<Domain.Notification> listNotif = new List<Domain.Notification>();
            var dataApprover = MdmService.GetActualOrganizationStructure(ServiceProxy.UserClaim.NoReg);
            foreach (var item in listDataBpjs)
            {
                var familyDetail = item.FamilyMemberId.HasValue
                            ? PersonalDataService.GetFamilyMemberDetail(item.FamilyMemberId.Value)
                            : null;
                listNotif.Add(new Domain.Notification
                {
                    FromNoReg = dataApprover.NoReg,
                    ToNoReg = item.NoReg,
                    //Message = $"Document Category BPJS for { PersonalDataService.GetFamilyMemberDetail(item.FamilyMemberId).Name } has been completed { (item.ActionType == "pendaftaran" ? "register" : "deactived") } by { dataApprover.Name }",
                    Message = $"Document Category BPJS for {(familyDetail != null ? familyDetail.Name : "-")} " + $"has been completed {(item.ActionType == "pendaftaran" ? "register" : "deactived")} " + $"by {dataApprover.Name}",
                    NotificationTypeCode = "notice",
                });
            }

            foreach (var item in listDataAsuransi)
            {
                var familyName = item.FamilyMemberId.HasValue? PersonalDataService.GetFamilyMemberDetail(item.FamilyMemberId.Value)?.Name ?? "-" : "-";
                listNotif.Add(new Domain.Notification
                {
                    FromNoReg = ServiceProxy.UserClaim.NoReg,
                    ToNoReg = item.NoReg,
                    //Message = $"Document Category Asuransi for { PersonalDataService.GetFamilyMemberDetail(item.FamilyMemberId.Value).Name } has been completed { (item.ActionType == "pendaftaran" ? "register" : "deactived") } by { dataApprover.Name }",
                    Message = $"Document Category Asuransi for {familyName} " + $"has been completed {(item.ActionType == "pendaftaran" ? "register" : "deactived")} " + $"by {dataApprover.Name}",
                    NotificationTypeCode = "notice"
                });
            }

            CoreService.CreateNotifications(listNotif);

            await EmailService.SendSheNotifAsync(listDataBpjs.ToList(), listDataAsuransi.ToList());

            return NoContent();
        }

        private System.IO.MemoryStream GenerateBpjs(List<Guid> id)
        {
            try
            {
                PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));

                // Template File
                string templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\bpjs-template.xlsx");

                // Results Output
                System.IO.MemoryStream output = new System.IO.MemoryStream();

                // Read Template
                using (System.IO.FileStream templateDocumentStream = System.IO.File.OpenRead(templateDocument))
                {
                    // Create Excel EPPlus Package based on template stream
                    using (ExcelPackage package = new ExcelPackage(templateDocumentStream))
                    {

                        // Grab BPJS data from database
                        var dataBpjs = PersonalDataService.GetDataBpjs().Where(x => id.Contains(x.Id));

                        // Grab the sheet with the template, sheet name is "BPJS".
                        ExcelWorksheet sheet = package.Workbook.Worksheets["BPJS"];

                        // Start Row for Detail Rows
                        int rowIndex = 5;
                        int rowNumber = 1;

                        // Set BPJS Field
                        foreach (var item in dataBpjs)
                        {
                            sheet.Cells[rowIndex, 1].Value = rowNumber;
                            sheet.Cells[rowIndex, 2].Value = item.KKNumber;
                            sheet.Cells[rowIndex, 3].Value = item.Nik;
                            sheet.Cells[rowIndex, 4].Value = item.FamilyName;
                            sheet.Cells[rowIndex, 5].Value = item.FamilyRelationNum;//.FamilyTypeCode;
                            sheet.Cells[rowIndex, 6].Value = item.BirthPlace;
                            sheet.Cells[rowIndex, 7].Value = item.BirthDate.ToString("dd/MM/yyyy");
                            sheet.Cells[rowIndex, 8].Value = item.FamilyGenderCode.StartsWith("l", StringComparison.OrdinalIgnoreCase) ? 1 : 2;
                            sheet.Cells[rowIndex, 9].Value = item.FamilyTypeCode.Contains("anak", StringComparison.OrdinalIgnoreCase) ? 1 : (item.FamilyTypeCode.Contains("suamiistri", StringComparison.OrdinalIgnoreCase) ? 2 : 0);
                            sheet.Cells[rowIndex, 10].Value = item.Address;
                            sheet.Cells[rowIndex, 11].Value = item.Rt;
                            sheet.Cells[rowIndex, 12].Value = item.Rw;
                            sheet.Cells[rowIndex, 13].Value = item.PostalCode;
                            sheet.Cells[rowIndex, 14].Value = item.DistrictName;
                            sheet.Cells[rowIndex, 15].Value = item.SubDistrictCode;
                            sheet.Cells[rowIndex, 16].Value = item.SubDistrictName;
                            sheet.Cells[rowIndex, 17].Value = "";
                            sheet.Cells[rowIndex, 18].Value = "";
                            sheet.Cells[rowIndex, 19].Value = item.FaskesCode;
                            sheet.Cells[rowIndex, 20].Value = item.FaskesName;
                            sheet.Cells[rowIndex, 21].Value = "";
                            sheet.Cells[rowIndex, 22].Value = "";
                            sheet.Cells[rowIndex, 23].Value = item.Telephone;
                            sheet.Cells[rowIndex, 24].Value = item.Email;
                            sheet.Cells[rowIndex, 25].Value = item.Nik;
                            sheet.Cells[rowIndex, 26].Value = "";
                            sheet.Cells[rowIndex, 27].Value = "";
                            sheet.Cells[rowIndex, 28].Value = "";
                            sheet.Cells[rowIndex, 29].Value = item.NationalityCode;

                            rowIndex++;
                            rowNumber++;
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

        private System.IO.MemoryStream GenerateBpjsV2(List<Guid> id)
        {
            try
            {
                PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));

                // Template File
                string templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\bpjs-template-v2.xlsx");

                // Results Output
                System.IO.MemoryStream output = new System.IO.MemoryStream();

                // Read Template
                using (System.IO.FileStream templateDocumentStream = System.IO.File.OpenRead(templateDocument))
                {
                    // Create Excel EPPlus Package based on template stream
                    using (ExcelPackage package = new ExcelPackage(templateDocumentStream))
                    {

                        // Grab BPJS data from database
                        var dataBpjs = PersonalDataService.GetDataBpjs().Where(x => id.Contains(x.Id));

                        // Grab the sheet with the template, sheet name is "BPJS".
                        ExcelWorksheet sheet = package.Workbook.Worksheets["BPJS"];

                        // Start Row for Detail Rows
                        int rowIndex = 5;
                        int rowNumber = 1;


                        // Set BPJS Field
                        foreach (var item in dataBpjs)
                        {
                            sheet.Cells[rowIndex, 1].Value = rowNumber;
                            sheet.Cells[rowIndex, 2].Value = item.BpjsNumber;
                            sheet.Cells[rowIndex, 3].Value = "";//Dikosongkan
                            sheet.Cells[rowIndex, 4].Value = "";//Dikosongkan
                            sheet.Cells[rowIndex, 5].Value = item.KKNumber;
                            sheet.Cells[rowIndex, 6].Value = item.Nik;
                            sheet.Cells[rowIndex, 7].Value = item.FamilyName;
                            sheet.Cells[rowIndex, 8].Value = item.FamilyRelationNum;
                            sheet.Cells[rowIndex, 9].Value = item.BirthPlace;
                            sheet.Cells[rowIndex, 10].Value = item.BirthDate.ToString("dd/MM/yyyy");
                            sheet.Cells[rowIndex, 11].Value = item.FamilyGenderCode.StartsWith("l", StringComparison.OrdinalIgnoreCase) ? 1 : 2;
                            sheet.Cells[rowIndex, 12].Value = item.FamilyTypeCode.Contains("anak", StringComparison.OrdinalIgnoreCase) ? 1 : (item.FamilyTypeCode.Contains("suamiistri", StringComparison.OrdinalIgnoreCase) ? 2 : 0);
                            sheet.Cells[rowIndex, 13].Value = item.Address;
                            sheet.Cells[rowIndex, 14].Value = item.Rt;
                            sheet.Cells[rowIndex, 15].Value = item.Rw;
                            sheet.Cells[rowIndex, 16].Value = item.PostalCode;
                            sheet.Cells[rowIndex, 17].Value = item.DistrictCode;
                            sheet.Cells[rowIndex, 18].Value = item.DistrictName;
                            sheet.Cells[rowIndex, 19].Value = "";//Kode Desa (Diisi Petugas)
                            sheet.Cells[rowIndex, 20].Value = "";//Nama Desa
                            sheet.Cells[rowIndex, 21].Value = item.FaskesCode;
                            sheet.Cells[rowIndex, 22].Value = item.FaskesName;
                            sheet.Cells[rowIndex, 23].Value = "";//Kode Faskes Dokter Gigi
                            sheet.Cells[rowIndex, 24].Value = "";//Nama Faskes Dokter Gigi
                            sheet.Cells[rowIndex, 25].Value = item.Telephone;
                            sheet.Cells[rowIndex, 26].Value = item.Email;
                            sheet.Cells[rowIndex, 27].Value = item.Nik;
                            sheet.Cells[rowIndex, 28].Value = "";//Jabatan
                            sheet.Cells[rowIndex, 29].Value = "";//Status
                            sheet.Cells[rowIndex, 30].Value = "";//Kelas Rawat
                            sheet.Cells[rowIndex, 31].Value = "";//TMT Kerja Karyawan Aktif
                            sheet.Cells[rowIndex, 32].Value = "";//Gapok + Tunjangan
                            sheet.Cells[rowIndex, 33].Value = item.NationalityCode;
                            sheet.Cells[rowIndex, 34].Value = "";//No. Kartu Asuransi
                            sheet.Cells[rowIndex, 35].Value = "";//Nama Asuransi
                            sheet.Cells[rowIndex, 36].Value = "";//NPWP
                            sheet.Cells[rowIndex, 37].Value = item.PassportNumber;

                            rowIndex++;
                            rowNumber++;
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

        private System.IO.MemoryStream GenerateAsuransi(List<Guid> id)
        {
            try
            {
                PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));

                // Template File
                string templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\asuransi-template.xlsx");

                // Results Output
                System.IO.MemoryStream output = new System.IO.MemoryStream();

                // Read Template
                using (System.IO.FileStream templateDocumentStream = System.IO.File.OpenRead(templateDocument))
                {
                    // Create Excel EPPlus Package based on template stream
                    using (ExcelPackage package = new ExcelPackage(templateDocumentStream))
                    {

                        // Grab BPJS data from database
                        var dataBpjs = PersonalDataService.GetDataInsurance().Where(x => id.Contains(x.Id));

                        // Grab the sheet with the template, sheet name is "ASURANSI".
                        ExcelWorksheet sheet = package.Workbook.Worksheets["ASURANSI"];

                        // Start Row for Detail Rows
                        int rowIndex = 4;
                        int rowNumber = 1;

                        // Set ASURANSI Field
                        foreach (var item in dataBpjs)
                        {
                            var jenkel = "";
                            var Division = "";
                            var Kelas = "";
                            var Event = "";
                            var Klasifikasi = "";

                            if (item.GenderCode.ToLower() == "perempuan")
                            { jenkel = "F"; }
                            else
                            { jenkel = "M"; }

                            var orgObj = ServiceProxy.GetService<MdmService>().GetEmployeeOrganizationObjects(item.NoReg);
                            foreach (var aa in orgObj)
                            {
                                if (aa.ObjectDescription == "Division")
                                {
                                    Division = aa.ObjectText;
                                    Kelas = aa.Kelas;
                                    if (int.Parse(aa.NP) >= 3 && int.Parse(aa.NP) <= 6)
                                    {
                                        Klasifikasi = "Kelas II";
                                    }
                                    else if (int.Parse(aa.NP) >= 7 && int.Parse(aa.NP) <= 8)
                                    {
                                        Klasifikasi = "Kelas I";
                                    }
                                    else if (int.Parse(aa.NP) >= 9)
                                    {
                                        Klasifikasi = "VIP";
                                    }
                                }
                            }

                            if (item.EventType == "family-registration")
                            { Event = "Kelahiran"; }
                            else if (item.EventType == "marriage-status")
                            { Event = "Pernikahan"; }
                            else if (item.EventType == "condolance")
                            { Event = "Kedukaan"; }
                            else if (item.EventType == "divorce")
                            { Event = "Perceraian"; }


                            sheet.Cells[rowIndex, 1].Value = rowNumber;
                            sheet.Cells[rowIndex, 2].Value = item.FamilyMemberName;
                            sheet.Cells[rowIndex, 3].Value = item.FamilyRelation;
                            sheet.Cells[rowIndex, 4].Value = item.BirthDate.ToString("dd/MM/yyyy");
                            sheet.Cells[rowIndex, 5].Value = jenkel;
                            sheet.Cells[rowIndex, 6].Value = item.BenefitClassification;
                            sheet.Cells[rowIndex, 7].Value = item.StartDate.ToString("dd/MM/yyyy");
                            sheet.Cells[rowIndex, 8].Value = item.EmployeeName;
                            sheet.Cells[rowIndex, 9].Value = item.MemberNumber;
                            sheet.Cells[rowIndex, 10].Value = item.NoReg;
                            sheet.Cells[rowIndex, 11].Value = Kelas;
                            sheet.Cells[rowIndex, 12].Value = Division;
                            sheet.Cells[rowIndex, 13].Value = Event;
                            sheet.Cells[rowIndex, 14].Value = Klasifikasi;

                            sheet.Cells[rowIndex, 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dashed);
                            sheet.Cells[rowIndex, 2].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dashed);
                            sheet.Cells[rowIndex, 3].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dashed);
                            sheet.Cells[rowIndex, 4].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dashed);
                            sheet.Cells[rowIndex, 5].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dashed);
                            sheet.Cells[rowIndex, 6].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dashed);
                            sheet.Cells[rowIndex, 7].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dashed);
                            sheet.Cells[rowIndex, 8].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dashed);
                            sheet.Cells[rowIndex, 9].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dashed);
                            sheet.Cells[rowIndex, 10].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dashed);
                            sheet.Cells[rowIndex, 11].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dashed);
                            sheet.Cells[rowIndex, 12].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dashed);
                            sheet.Cells[rowIndex, 13].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dashed);
                            sheet.Cells[rowIndex, 14].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Dashed);
                            rowIndex++;
                            rowNumber++;
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

        [HttpPost("download")]
        public IActionResult Download([FromBody] DownloadRequest data)
        {
            List<Guid> listIdBpjs = data.Type == "BPJS" ? data.DocIds : null;
            List<Guid> listIdInsurance = data.Type == "INSURANCE" ? data.DocIds : null;

            if (listIdBpjs != null && listIdBpjs.Count > 0)
            {
                using (var documentStreamBpjs = GenerateBpjsV2(listIdBpjs))
                {
                    // Make Sure Document is Loaded
                    if (documentStreamBpjs != null && documentStreamBpjs.Length > 0)
                    {
                        // Generate dynamic name for Attachment document "BPJS-XXXXXX.XLSX"
                        string documentName = string.Format("{0}-{1}.xlsx", "BPJS", DateTime.Now.ToString("ddMMyyyyHHmm"));
                        documentStreamBpjs.Position = 0;

                        return Ok(Convert.ToBase64String(documentStreamBpjs.ToArray()));// File(documentStreamBpjs, contentType, documentName);
                    }
                }
            }

            if (listIdInsurance != null && listIdInsurance.Count > 0)
            {
                using (var documentStreamAsuransi = GenerateAsuransi(listIdInsurance))
                {
                    // Make Sure Document is Loaded
                    if (documentStreamAsuransi != null && documentStreamAsuransi.Length > 0)
                    {
                        // Generate dynamic name for Attachment document "INSURANCE-XXXXXX.XLSX"
                        string documentName = string.Format("{0}-{1}.xlsx", "INSURANCE", DateTime.Now.ToString("ddMMyyyyHHmm"));
                        documentStreamAsuransi.Position = 0;

                        return Ok(Convert.ToBase64String(documentStreamAsuransi.ToArray())); //File(documentStreamAsuransi, contentType, documentName);
                    }
                }
            }

            // If something fails or somebody calls invalid URI, throw error.
            return NotFound();
        }

        public class DownloadRequest
        {
            public List<Guid> DocIds { get; set; }
            public string Type { get; set; }
        }
    }
    #endregion
}
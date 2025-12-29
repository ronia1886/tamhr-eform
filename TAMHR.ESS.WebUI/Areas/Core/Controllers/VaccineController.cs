using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Helpers;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    [Route("api/vaccine-report")]
    [Permission(PermissionKey.ViewVaccineReport)]
    public class VaccineReportApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// MDM service object.
        /// </summary>
        protected MdmService MdmService => ServiceProxy.GetService<MdmService>();
        protected VaccineService vaccineService => ServiceProxy.GetService<VaccineService>();
        protected FormService FormService => ServiceProxy.GetService<FormService>();
        protected VaccineScheduleService vaccineScheduleService => ServiceProxy.GetService<VaccineScheduleService>();

        #endregion
        [HttpGet("download")]
        public IActionResult Download(string whereClause)
        {
            var Class = JsonConvert.DeserializeObject<VaccinReportEmployeeView>(whereClause);
            var request = new DataSourceRequest();
            var div = "";
            if (!string.IsNullOrEmpty(Class.Division))
                div = Class.Division.Replace("xtx", "&");
            var output = vaccineService.GetReportEmployee(Class.VaccineDate1, Class.VaccineDate2)
                .Where(x => x.VaccineType.Contains(Class.EmployeeName) || x.NoReg.Contains(Class.EmployeeName) || x.EmployeeName.Contains(Class.EmployeeName) || x.Name.Contains(Class.EmployeeName) || x.Status.Contains(Class.EmployeeName)
                || x.FamilyStatus.Contains(Class.EmployeeName) || x.Class.Contains(Class.EmployeeName) || x.JobName.Contains(Class.EmployeeName) || x.Department.Contains(Class.EmployeeName)
                || x.Division.Contains(Class.EmployeeName) || x.Section.Contains(Class.EmployeeName) || x.Email.Contains(Class.EmployeeName) || x.PhoneNumber.Contains(Class.EmployeeName)
                || x.Age.Contains(Class.EmployeeName) || x.SideEffects1.Contains(Class.EmployeeName) || x.SideEffects2.Contains(Class.EmployeeName) || x.RiwayatPenyakit.Contains(Class.EmployeeName) || x.MonitoringNotes.Contains(Class.EmployeeName));
            if (!string.IsNullOrEmpty(Class.Age))
                output = output.Where(x => x.Age == Class.Age);
            if (!string.IsNullOrEmpty(Class.VaccineType))
                output = output.Where(x => x.VaccineType == Class.VaccineType);
            if (!string.IsNullOrEmpty(Class.Status))
                output = output.Where(x => x.StatusCode == Class.Status);
            if (Class.VaccineDate1End.HasValue)
                output = output.Where(x => x.VaccineDate1.Value <= Class.VaccineDate1End.Value);
            if (Class.VaccineDate2End.HasValue)
                output = output.Where(x => x.VaccineDate2.Value <= Class.VaccineDate2End.Value);
            if (!string.IsNullOrEmpty(Class.Division))
                output = output.Where(x => x.Division == div);


            var fileName = string.Format("Vaccine REPORT {0:ddMMyyyy}-{1:ddMMyyyy}.xlsx", Class.VaccineDate1, Class.VaccineDate2);

            var vaccineQuestion = vaccineService.GeVaccineQuestion().ToList();

            using (var ms = new MemoryStream())
            {
                using (var package = new ExcelPackage())
                {
                    int rowIndex = 2;
                    var sheet = package.Workbook.Worksheets.Add("Vaccine Report");
                    var format = "dd/MM/yyyy";

                    var cols = new[] { "Employee Noreg", "Employee Name", "Name", "Hubungan", "Class", "Job", "Division", "Department", "Section", "Email", "Phone Number", "Umur", "Eligible", "Riwayat Penyakit", "VaccineAgreement", "Vaccine Date 1", "Efek Samping Vaksin 1", "Hospital Vaccine 1", "Kartu Vaksin 1", "Vaccine Date 2", "Efek Samping Vaksin 2", "Hospital Vaccine 2", "Kartu Vaksin 2", "Status", "Monitoring Notes" };

                    for (var i = 1; i <= cols.Length; i++)
                    {
                        sheet.Cells[1, i].Value = cols[i - 1];
                        sheet.Cells[1, i].Style.Font.Bold = true;
                        sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }
                    int no = 1;

                    foreach (var data in output)
                    {
                        no = 1;
                        sheet.Cells[rowIndex, no++].Value = data.NoReg;
                        sheet.Cells[rowIndex, no++].Value = data.EmployeeName;
                        sheet.Cells[rowIndex, no++].Value = data.Name;
                        sheet.Cells[rowIndex, no++].Value = data.FamilyStatus;
                        sheet.Cells[rowIndex, no++].Value = data.Class;
                        sheet.Cells[rowIndex, no++].Value = data.JobName;
                        sheet.Cells[rowIndex, no++].Value = data.Division;
                        sheet.Cells[rowIndex, no++].Value = data.Department;
                        sheet.Cells[rowIndex, no++].Value = data.Section;
                        sheet.Cells[rowIndex, no++].Value = data.Email;
                        sheet.Cells[rowIndex, no++].Value = data.PhoneNumber;
                        sheet.Cells[rowIndex, no++].Value = data.Age;
                        sheet.Cells[rowIndex, no++].Value = data.Eligible.HasValue ? (data.Eligible.Value ? "Siap Vaksin": "Tidak Siap Vaksin") : "";
                        sheet.Cells[rowIndex, no++].Value = ConvertListToStringValueQuestion(vaccineQuestion.Where(x => x.VaccineId == data.VaccineId).ToList());
                        sheet.Cells[rowIndex, no++].Value = data.TAMVaccineAgreement;
                        sheet.Cells[rowIndex, no++].Value = data.VaccineDate1.HasValue ? data.VaccineDate1.Value.ToString(format) : "";
                        sheet.Cells[rowIndex, no++].Value = data.SideEffects1;
                        sheet.Cells[rowIndex, no++].Value = data.VaccineHospital1;
                        sheet.Cells[rowIndex, no++].Value = data.StatusUploadVaccineCard1;
                        sheet.Cells[rowIndex, no++].Value = data.VaccineDate2.HasValue ? data.VaccineDate2.Value.ToString(format) : "";
                        sheet.Cells[rowIndex, no++].Value = data.SideEffects2;
                        sheet.Cells[rowIndex, no++].Value = data.VaccineHospital2;
                        sheet.Cells[rowIndex, no++].Value = data.StatusUploadVaccineCard2;
                        sheet.Cells[rowIndex, no++].Value = string.IsNullOrEmpty(data.Status) ? "Not Submit" : data.Status;
                        sheet.Cells[rowIndex, no++].Value = data.MonitoringNotes;
                        sheet.Cells[rowIndex, no].Style.QuotePrefix = true;

                        for (var i = 1; i <= cols.Length; i++)
                        {
                            sheet.Cells[rowIndex, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }

                        rowIndex++;
                    }

                    sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
                    RawData(package, output.ToList(), vaccineQuestion);

                    package.SaveAs(ms);
                }

                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        [HttpPost("upload-autofill")]
        public async Task<IActionResult> UploadAutofill()
        {
            var actor = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var file = Request.Form.Files[0];
            
            var now = DateTime.Now.Date;
            var totalSuccess = 0;
            var totalUpload = 0;
            var messages = new List<string>();

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);

                using (var package = new ExcelPackage(ms))
                {
                    using (var workSheet = package.Workbook.Worksheets.FirstOrDefault())
                    {
                        if (workSheet == null) return NoContent();

                        var totalRows = workSheet.Dimension.Rows;
                        var initialStart = 2;
                        var rowStart = initialStart;
                        var dt = new DataTable();

                        dt.Columns.AddRange(new[] {
                            new DataColumn("NoReg", typeof(string)),
                            new DataColumn("Name", typeof(string)),
                            new DataColumn("TAMVaccineAgreement", typeof(string)),
                        });

                        while (!string.IsNullOrEmpty(workSheet.Cells[rowStart, 1].Text))
                        {
                            


                            var NoReg = workSheet.Cells[rowStart, 1].Text;
                            var Name = workSheet.Cells[rowStart, 2].Text;
                            if (string.IsNullOrEmpty(NoReg) || string.IsNullOrEmpty(Name))
                            {
                                messages.Add(string.Format("Row {0}: NoReg atau Name kosong", rowStart));
                                rowStart++;
                                continue;
                            }
                            else
                            {
                                var checkNoRegNameExists = vaccineService.GetVaccineQuery(NoReg, Name);
                                if (checkNoRegNameExists == null)
                                {
                                    messages.Add(string.Format("Row {0}: NoReg [{1}] dengan Nama [{2}] tidak ditemukan", rowStart, NoReg, Name));
                                    rowStart++;
                                    continue;
                                }
                                else
                                {
                                    var checkVaccineExists = vaccineService.GetVaccine(NoReg, Name);
                                    if (checkVaccineExists != null)
                                    {
                                        if (checkVaccineExists.Id != Guid.Parse("00000000-0000-0000-0000-000000000000")) {
                                            messages.Add(string.Format("Row {0}: NoReg [{1}] dengan Nama [{2}] telah terdaftar", rowStart, NoReg, Name));
                                            rowStart++;
                                            continue;
                                        }
                                        
                                    }
                                }

                                
                            }

                            var TAMVaccineAgreement = workSheet.Cells[rowStart, 3].Text;
                            if (string.IsNullOrEmpty(TAMVaccineAgreement))
                            {
                                messages.Add(string.Format("Row {0}: Persetujuan vaksin kosong", rowStart));
                                rowStart++;
                                continue;
                            }

                            var row = dt.NewRow();
                            row.ItemArray = new object[] {
                               NoReg,
                               Name,
                               TAMVaccineAgreement
                            };

                            dt.Rows.Add(row);

                            rowStart++;
                            totalSuccess++;
                        }

                        totalUpload = rowStart - initialStart;

                        vaccineService.UploadVaccineAutofill(actor, dt);
                    }
                }
            }

            return Ok(new { TotalUpload = totalUpload, TotalSuccess = totalSuccess, TotalFailed = totalUpload - totalSuccess, Messages = messages });
        }

        [HttpGet("download/download-template")]
        public IActionResult DownloadVaccineAutofill(string orgCode)
        {
            var keyDate = DateTime.Now.Date;
            var pathProvider = ServiceProxy.GetPathProvider();
            var templatePath = pathProvider.ContentPath("uploads\\excel-template\\vaccineautofill-template.xlsx");
            var fileName = string.Format("VACCINE-AUTOFILL-TEMPLATE-{0:ddMMyyyy}.xlsx", keyDate);

            using (var ms = new MemoryStream())
            {
                using (var stream = System.IO.File.OpenRead(templatePath))
                {
                    using (ExcelPackage package = new ExcelPackage(stream))
                    {
                        var sheet = package.Workbook.Worksheets[0];
                        var now = DateTime.Now;
                        var defaultDate = now.ToString("dd/MM/yyyy");
                        

                        package.SaveAs(ms);
                    }

                    return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

        [HttpPost("upload-schedule-backdate")]
        public async Task<IActionResult> UploadScheduleBackdate()
        {
            var actor = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var file = Request.Form.Files[0];

            var now = DateTime.Now.Date;
            var totalSuccess = 0;
            var totalUpload = 0;
            var messages = new List<string>();

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);

                using (var package = new ExcelPackage(ms))
                {
                    using (var workSheet = package.Workbook.Worksheets.FirstOrDefault())
                    {
                        if (workSheet == null) return NoContent();

                        var totalRows = workSheet.Dimension.Rows;
                        var initialStart = 2;
                        var rowStart = initialStart;
                        var dt = new DataTable();

                        dt.Columns.AddRange(new[] {
                            new DataColumn("NoReg", typeof(string)),
                            new DataColumn("Name", typeof(string)),
                            new DataColumn("VaccineDate1", typeof(DateTime)),
                            new DataColumn("VaccineHospital1", typeof(string)),
                            new DataColumn("VaccineDate2", typeof(DateTime)),
                            new DataColumn("VaccineHospital2", typeof(string))
                        });

                        while (!string.IsNullOrEmpty(workSheet.Cells[rowStart, 1].Text))
                        {
                            var NoReg = workSheet.Cells[rowStart, 1].Text;
                            var Name = workSheet.Cells[rowStart, 2].Text;
                            if (string.IsNullOrEmpty(NoReg) || string.IsNullOrEmpty(Name))
                            {
                                messages.Add(string.Format("Row {0}: NoReg or Name is empty", rowStart));
                                rowStart++;
                                continue;
                            }
                            else
                            {
                                var checkNoRegNameExists = vaccineService.GetVaccineQuery(NoReg, Name);
                                if (checkNoRegNameExists == null)
                                {
                                    messages.Add(string.Format("Row {0}: NoReg [{1}] with Name [{2}] is not found", rowStart, NoReg, Name));
                                    rowStart++;
                                    continue;
                                }
                                else
                                {
                                    var checkVaccineExists = vaccineService.GetVaccine(NoReg, Name);
                                    if (checkVaccineExists != null)
                                    {
                                        if (checkVaccineExists.Id == Guid.Parse("00000000-0000-0000-0000-000000000000"))
                                        {
                                            messages.Add(string.Format("Row {0}: NoReg [{1}] with Name [{2}] is not fill health assessment yet", rowStart, NoReg, Name));
                                            rowStart++;
                                            continue;
                                        }

                                    }
                                }


                            }

                            DateTime VaccineDate1;
                            string VaccineHospital1 = workSheet.Cells[rowStart, 4].Text;
                            try
                            {
                                VaccineDate1 = DateTime.ParseExact(workSheet.Cells[rowStart, 3].Text, new[] { "d/M/yyyy", "dd/MM/yyyy" }, CultureInfo.CurrentCulture);
                                var checkExists = vaccineScheduleService.GetVaccineScheduleByDateHospitalName(VaccineDate1, VaccineHospital1);
                                if (checkExists == null)
                                {
                                    messages.Add(string.Format("Row {0}: Vaccine Schedule with date [{1}] and hospital [{2}] is not found", rowStart, VaccineDate1.ToString("yyyy-MM-dd"), VaccineHospital1));
                                    rowStart++;
                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                messages.Add(string.Format("Row {0}: {1}", rowStart, ex.Message));
                                rowStart++;
                                continue;
                            }

                            DateTime? VaccineDate2 = null;
                            string VaccineHospital2 = workSheet.Cells[rowStart, 6].Text;
                            if (workSheet.Cells[rowStart, 5].Text != "" || workSheet.Cells[rowStart, 6].Text != "") {
                                try
                                {
                                    VaccineDate2 = DateTime.ParseExact(workSheet.Cells[rowStart, 5].Text, new[] { "d/M/yyyy", "dd/MM/yyyy" }, CultureInfo.CurrentCulture);
                                    var checkExists = vaccineScheduleService.GetVaccineScheduleByDateHospitalName(VaccineDate2, VaccineHospital2);
                                    if (checkExists == null)
                                    {
                                        messages.Add(string.Format("Row {0}: Vaccine Schedule with date [{1}] and hospital [{2}] is not found", rowStart, VaccineDate2.Value.ToString("yyyy-MM-dd"), VaccineHospital2));
                                        rowStart++;
                                        continue;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    messages.Add(string.Format("Row {0}: {1}", rowStart, ex.Message));
                                    rowStart++;
                                    continue;
                                }
                            }

                            var row = dt.NewRow();
                            row.ItemArray = new object[] {
                               NoReg,
                               Name,
                               VaccineDate1,
                               VaccineHospital1,
                               VaccineDate2,
                               VaccineHospital2
                            };

                            dt.Rows.Add(row);

                            rowStart++;
                            totalSuccess++;
                        }

                        totalUpload = rowStart - initialStart;

                        vaccineService.UploadVaccineScheduleBackdate(actor, dt);
                    }
                }
            }

            return Ok(new { TotalUpload = totalUpload, TotalSuccess = totalSuccess, TotalFailed = totalUpload - totalSuccess, Messages = messages });
        }

        [HttpGet("download/download-template2")]
        public IActionResult DownloadVaccineScheduleBackdate(string orgCode)
        {
            var keyDate = DateTime.Now.Date;
            var pathProvider = ServiceProxy.GetPathProvider();
            var templatePath = pathProvider.ContentPath("uploads\\excel-template\\vaccineschedulebackdate-template.xlsx");
            var fileName = string.Format("VACCINE-SCHEDULE-BACKDATE-TEMPLATE-{0:ddMMyyyy}.xlsx", keyDate);

            using (var ms = new MemoryStream())
            {
                using (var stream = System.IO.File.OpenRead(templatePath))
                {
                    using (ExcelPackage package = new ExcelPackage(stream))
                    {
                        var sheet = package.Workbook.Worksheets[0];
                        var now = DateTime.Now;
                        var defaultDate = now.ToString("dd/MM/yyyy");


                        package.SaveAs(ms);
                    }

                    return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }
        private void RawData(ExcelPackage package, List<VaccinReportEmployeeView> output, List<VaccineQuestionView> vaccineQuestion)
        {
            var questionVaccine = FormService.GetFormQuestions("vaccine", true).Where(x => x.RowStatus == true).ToList();
            var Typequestion = questionVaccine.Where(x => x.ParentFormQuestionId == null).ToList();
            var generateQuestion = new List<FormQuestion>();
            int seqQuestion = 1;
            foreach (var item in Typequestion.OrderBy(x => x.OrderSequence))
            {
                var qs = questionVaccine.Where(x => x.ParentFormQuestionId == item.Id).ToList();

                for (int i = 0; i < qs.Count; i++)
                {
                    qs[i].OrderSequence = seqQuestion++;
                    generateQuestion.Add(qs[i]);

                    var obj = new FormQuestion();
                    obj.OrderSequence = seqQuestion++;
                    obj.Id = qs[i].Id;
                    obj.Title = "Deskripsi";
                    generateQuestion.Add(obj);
                }
            }


            int rowIndex = 2;
            var sheet = package.Workbook.Worksheets.Add("Raw Data");
            var format = "dd/MM/yyyy";
            List<string> AuthorList = new List<string>();
            var bf = new[] { "Employee Noreg", "Employee Name","Name", "Hubungan","Tanggal Lahir", "Umur", "Phone Number",
                                "Alamat","Domisili","Kelurahan","Kecamatan","Kota","No KTP","Foto KTP         .","VaccineAgreement;"};
            var aft = new[] { "Tanggal Vaccine 1","RS Vaccine 1", "Kartu Vaksin 1"," Apakah Anda mengalami Kejadian Ikutan Pasca Vaksinasi (KIPI) / efek samping?", "Mohon menuliskan efek samping yang Anda alami",
                                "Tanggal Vaccine 2","RS Vaccine 2", "Kartu Vaksin 2"," Apakah Anda mengalami Kejadian Ikutan Pasca Vaksinasi (KIPI) / efek samping?", "Mohon menuliskan efek samping yang Anda alami"};

            AuthorList.AddRange(bf);
            AuthorList.AddRange(generateQuestion.Select(x => x.Title).ToList());
            AuthorList.AddRange(aft);

            for (var i = 1; i <= AuthorList.Count(); i++)
            {
                sheet.Cells[1, i].Value = AuthorList[i - 1];
                sheet.Cells[1, i].Style.Font.Bold = true;
                sheet.Cells[1, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            }
            int no = 1;
            var pathProvider = ServiceProxy.GetPathProvider();
            var pathFile = pathProvider.ContentPath("");
            var tempPath = pathProvider.ContentPath("temps");
            var answerdata = vaccineService.GetVaccineAnswerAll().ToList();
            var vaccineQuestionAll = vaccineService.GetVaccineQuestionsAll().ToList();


            foreach (var data in output)
            {
                no = 1;
                sheet.Cells[rowIndex, no++].Value = data.NoReg;
                sheet.Cells[rowIndex, no++].Value = data.Employee;
                sheet.Cells[rowIndex, no++].Value = data.Name;
                sheet.Cells[rowIndex, no++].Value = data.FamilyStatus;
                sheet.Cells[rowIndex, no++].Value = data.BirthDate.HasValue ? data.BirthDate.Value.ToString(format) : "";
                sheet.Cells[rowIndex, no++].Value = data.Age;
                sheet.Cells[rowIndex, no++].Value = data.PhoneNumber;
                sheet.Cells[rowIndex, no++].Value = data.Address;
                sheet.Cells[rowIndex, no++].Value = data.Domicile;
                sheet.Cells[rowIndex, no++].Value = data.SubDistrict;
                sheet.Cells[rowIndex, no++].Value = data.District;
                sheet.Cells[rowIndex, no++].Value = data.City;
                sheet.Cells[rowIndex, no++].Value = data.IdentityId;

                if (!string.IsNullOrEmpty(data.IdentityImage) && false)
                {
                    //var path = Url.Content(data.IdentityImage);
                    var filePath = pathFile+ data.IdentityImage.Replace("~","");
                    
                    var col = no++;
                    try
                    {
                        Image img = Image.FromFile(filePath);
                        //ExcelPicture pic = sheet.Drawings.AddPicture(data.IdentityImage, img);
                        ExcelPicture pic = sheet.Drawings.AddPicture(data.IdentityImage, filePath);
                        pic.SetPosition(rowIndex - 1, 4, col - 1, 4);
                        sheet.Row(rowIndex).Height = 50;
                        sheet.Column(col - 1).Width = 100;
                        pic.SetSize(93, 58);
                    }
                    catch (Exception e)
                    {
                        
                        sheet.Cells[rowIndex, col].Value = e.Message;
                    }

                }
                else
                {
                    sheet.Cells[rowIndex, no++].Value = "";
                }


                //sheet.Cells[rowIndex, 14].Value = data.FotoKTP;
                sheet.Cells[rowIndex, no++].Value = data.TAMVaccineAgreement;
                #region Question
                var vqs = new List<VaccineQuestion>();
                    vqs = vaccineQuestionAll.Where(x => x.VaccineId == data.VaccineId).ToList();// (data.VaccineId);
                foreach (var qs in generateQuestion)
                {
                    var hasAnswer = vqs.Where(x => x.FormQuestionId == qs.Id).Select(x => x.Answer).FirstOrDefault() == "true" ? "Yes" : "No";
                    if (qs.Title == "Deskripsi")
                    {
                        if (vqs != null && hasAnswer == "Yes")
                        {
                            var ans = vqs.Where(x => x.FormQuestionId == qs.Id).FirstOrDefault();
                            var listAnswer = answerdata.Where(x => x.VaccineId == data.VaccineId && x.Id == (ans != null ? ans.Id : Guid.NewGuid()));// vaccineService.GetVaccineAnswer(data.VaccineId, ans.Id);
                            var deskripsi = "";
                            foreach (var answer in listAnswer.OrderBy(x => x.SubQuestionOrderSequence))
                            {
                                deskripsi += "-" + answer.SubQuestion + " : " + (answer.SubQuestionType == "date" ? (DateTime.Parse(answer.Answer).ToString(format)) : answer.Answer) + System.Environment.NewLine;
                            }

                            sheet.Cells[rowIndex, no++].Value = deskripsi;
                        }
                        else
                        {
                            sheet.Cells[rowIndex, no++].Value = "";
                        }

                    }
                    else
                    {
                        if (vqs != null)
                        {
                            sheet.Cells[rowIndex, no++].Value = hasAnswer;
                        }
                        else
                        {
                            sheet.Cells[rowIndex, no++].Value = "";
                        }

                    }
                }
                #endregion

                sheet.Cells[rowIndex, no++].Value = data.VaccineDate1.HasValue ? data.VaccineDate1.Value.ToString(format) : "";
                sheet.Cells[rowIndex, no++].Value = data.VaccineHospital1;

                if (!string.IsNullOrEmpty(data.VaccineCard1) && false)
                {
                    var col = no++;
                    try
                    {
                        //var path = Url.Content(data.VaccineCard1);
                        var filePath = pathFile + data.VaccineCard1.Replace("~", ""); ;
                        Image img = Image.FromFile(filePath);
                        //ExcelPicture pic = sheet.Drawings.AddPicture(data.VaccineCard1, img);
                        ExcelPicture pic = sheet.Drawings.AddPicture(data.VaccineCard1, filePath);
                        pic.SetPosition(rowIndex - 1, 4, col - 1, 4);
                        sheet.Row(rowIndex).Height = 50;
                        pic.SetSize(93, 58);
                    }
                    catch (Exception)
                    {

                        sheet.Cells[rowIndex, col].Value = "";
                    }

                }
                else
                {
                    sheet.Cells[rowIndex, no++].Value = "";
                }

                sheet.Cells[rowIndex, no++].Value = data.IsSideEffects1.HasValue ? (data.IsSideEffects1.Value ? "Yes" : "No") : "";
                sheet.Cells[rowIndex, no++].Value = data.SideEffects1;
                sheet.Cells[rowIndex, no++].Value = data.VaccineDate2.HasValue ? data.VaccineDate2.Value.ToString(format) : "";
                sheet.Cells[rowIndex, no++].Value = data.VaccineHospital2;

                if (!string.IsNullOrEmpty(data.VaccineCard2) && false)
                {
                    var col = no++;
                    try
                    {
                        //var path = Url.Content(data.VaccineCard2);
                        var filePath = pathFile + data.VaccineCard2.Replace("~", ""); ;
                        Image img = Image.FromFile(filePath);
                        //ExcelPicture pic = sheet.Drawings.AddPicture(data.VaccineCard2, img);
                        ExcelPicture pic = sheet.Drawings.AddPicture(data.VaccineCard2, filePath);
                        pic.SetPosition(rowIndex - 1, 4, col - 1, 4);
                        sheet.Row(rowIndex).Height = 50;
                        pic.SetSize(93, 58);
                    }
                    catch (Exception)
                    {
                        sheet.Cells[rowIndex, col].Value = "";
                    }

                }
                else
                {
                    sheet.Cells[rowIndex, no++].Value = "";
                }

                sheet.Cells[rowIndex, no++].Value = data.IsSideEffects2.HasValue ? (data.IsSideEffects2.Value ? "Yes" : "No") : "";
                sheet.Cells[rowIndex, no++].Value = data.SideEffects2;
                sheet.Cells[rowIndex, no].Style.QuotePrefix = true;

                for (var i = 1; i <= AuthorList.Count; i++)
                {
                    sheet.Cells[rowIndex, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                }

                rowIndex++;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
        }

        private void GetImage()
        {

        }

        public string ConvertListToStringValueQuestion(List<VaccineQuestionView> list)
        {
            //var result = "";
            //foreach (var item in list)
            //{
            //    result = result + item.Title + ", ";
            //}
            //result = result.Substring(0, result.Length > 0 ? result.Length - 2 : 0);
            //return result;
            if (list == null || list.Count == 0)
                return string.Empty;

            var result = new StringBuilder();

            foreach (var item in list)
            {
                var title = item.Title ?? string.Empty;
                title = title.Trim();

                if (title.StartsWith("=") || title.StartsWith("+") ||
                    title.StartsWith("-") || title.StartsWith("@"))
                {
                    title = "'" + title;
                }

                title = title
                    .Replace("<", string.Empty)
                    .Replace(">", string.Empty)
                    .Replace("\"", string.Empty)
                    .Replace("'", string.Empty);

                result.Append(title);
                result.Append(", ");
            }

            if (result.Length > 2)
                result.Length -= 2;

            return result.ToString();
        }

        [HttpPost("get-report-employee")]
        public async Task<DataSourceResult> GetReportSumary([FromForm] DateTime? vaccineDate1, [FromForm] DateTime? vaccineDate2, [FromForm] DateTime? vaccineDate1End, [FromForm] DateTime? vaccineDate2End, [DataSourceRequest] DataSourceRequest request)
        {
            var output = vaccineService.GetReportEmployee(vaccineDate1, vaccineDate2);
            if (vaccineDate1End.HasValue)
                output = output.Where(x => x.VaccineDate1 <= vaccineDate1End);
            if (vaccineDate2End.HasValue)
                output = output.Where(x => x.VaccineDate2 <= vaccineDate2End);
            return await output.ToDataSourceResultAsync(request);
        }

        [HttpPost("get-division")]
        public async Task<DataSourceResult> GetDivision()
        {
            return await vaccineService.GetDivisions().ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("get-class")]
        public async Task<DataSourceResult> GetClass()
        {
            return await vaccineService.GetClass().ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("get-position")]
        public async Task<DataSourceResult> GetPosition()
        {
            return await vaccineService.GetPosition().ToDataSourceResultAsync(new DataSourceRequest());
        }


        ///// <summary>
        ///// Get report summary (sick and non-sick) by submission date.
        ///// </summary>
        ///// <param name="submissionDate">This submission date.</param>
        ///// <returns>This list of dynamic object.</returns>
        //[HttpGet("summary")]
        //public async Task<IEnumerable<dynamic>> GetSummary()
        //{
        //    // Get and set noreg from current user session.
        //    var noreg = ServiceProxy.UserClaim.NoReg;

        //    // Get and set position code from current user session.
        //    var postCode = ServiceProxy.UserClaim.PostCode;

        //    var canViewAll = AclHelper.HasPermission("Core.ViewAllVaccineReport");

        //    var canViewDivisionReport = AclHelper.HasPermission("Core.ViewVaccineDivisionReport");

        //    // Get organization structure object from given current user session noreg and position code.
        //    var organizationStructure = MdmService.GetOrganizationLevel(noreg, postCode, canViewDivisionReport ? "Division" : null);

        //    // Create new anonymous parameters.
        //    var parameters = new
        //    {
        //        // Get and set actor.
        //        actor = noreg,
        //        // Get and set submission date.

        //        // Get and set organization code.
        //        orgCode = canViewAll ? "*" : organizationStructure.OrgCode,
        //        // Get and set organization level.
        //        orgLevel = canViewAll ? 0 : organizationStructure.OrgLevel
        //    };

        //    // Create dynamic summary query from given parameters and filter.
        //    // The field WorkTypeCode will be compare and map with code in general category with "WorkingType" as category.
        //    return await Task.FromResult(ServiceProxy.GetTableValuedSummary<VaccineReportStoredEntity>("HealthTypeCode", "HealthType", parameters));
        //}

        /// <summary>
        /// Update or insert form log.
        /// </summary>
        /// <param name="formLog">This <see cref="FormLog"/> object.</param>
        [HttpPost("monitoring-log")]
        public IActionResult Upsert([FromBody] VaccineMonitoringLog Log)
        {
            // Get current user session noreg.
            var actor = ServiceProxy.UserClaim.NoReg;

            var output = vaccineService.Upsertentity(actor, Log);

            return Ok(output);
        }

        /// <summary>
        /// Get list of closed form logs.
        /// </summary>
        /// <param name="noreg">This target noreg.</param>
        /// <param name="request">This <see cref="DataSourceRequest"/> object.</param>
        /// <returns>This <see cref="DataSourceResult"/> object.</returns>
        [HttpPost("get-closed")]
        public async Task<DataSourceResult> GetClosed([FromForm] string noreg, [FromForm] Guid? vaccineId, [DataSourceRequest] DataSourceRequest request)
        {
            // Get current user session noreg.
            var actor = ServiceProxy.UserClaim.NoReg;

            return await vaccineService.GetMonitoringLogs()
                .Where(x => x.Closed && x.NoReg == noreg && x.VaccineId == vaccineId)
                .ToDataSourceResultAsync(request);
        }

        [HttpPost("update-schedule-backdate")]
        public IActionResult UpdateScheduleBackdate([FromBody] VaccineScheduleBackdateStoredEntity vaccineModel)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            vaccineModel.ModifiedBy = noreg;
            if (vaccineModel.VaccineCard1 != null)
            {
                vaccineModel.VaccineCard1 = MoveVaccineFile("vaccine", vaccineModel.VaccineCard1, "VaccineCard1-" + vaccineModel.NoReg.SanitizeFileName(string.Empty) + "-" + vaccineModel.Name.SanitizeFileName(string.Empty));
            }
            if (vaccineModel.VaccineCard2 != null)
            {
                vaccineModel.VaccineCard2 = MoveVaccineFile("vaccine", vaccineModel.VaccineCard2, "VaccineCard2-" + vaccineModel.NoReg.SanitizeFileName(string.Empty) + "-" + vaccineModel.Name.SanitizeFileName(string.Empty));
            }
            var output = vaccineService.UpsertVaccineScheduleBackdate(vaccineModel);

            return Ok(output);
        }
        protected string MoveVaccineFile(string path, string fileUrl, string newFileName = null)
        {
            var pathProvider = ServiceProxy.GetPathProvider();
            var assetsPath = pathProvider.ContentPath(path);
            var tempPath = pathProvider.ContentPath("temps");
            var extension = Path.GetExtension(fileUrl);
            var fileName = string.IsNullOrEmpty(newFileName) ? Path.GetFileName(fileUrl) : Path.GetFileName(newFileName);
            //var fileName = string.IsNullOrEmpty(newFileName) ? Path.GetFileName(fileUrl) : Path.GetFileName(newFileName + extension);
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
                if (System.IO.File.Exists(targetFilePath))
                {
                    System.IO.File.Delete(targetFilePath);
                }

                System.IO.File.Move(tempFilePath, targetFilePath);

                return $"~/{path}/{fileName}";
            }

            return fileUrl;
        }
    }

    #region API Controller
    /// <summary>
    /// Health declaration API controller.
    /// </summary>
    [Route("api/vaccine")]
    public class VaccineApiController : ApiControllerBase
    {
        protected VaccineService VaccineService { get { return ServiceProxy.GetService<VaccineService>(); } }
        protected VaccineScheduleService VaccineScheduleService { get { return ServiceProxy.GetService<VaccineScheduleService>(); } }

        [HttpPost("gets")]
        public DataSourceResult GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            string name = null;
            var result = ServiceProxy.GetTableValuedDataSourceResult<VaccineSummaryStoredEntity>(request, new { noreg, name });
            return result;
        }

        public void UpsertVaccine(Vaccine vaccineModel)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            vaccineModel.IdentityImage = MoveVaccineFile("vaccine", vaccineModel.IdentityImage, "KTP-" + vaccineModel.NoReg.SanitizeFileName(string.Empty) + "-" + vaccineModel.Name.SanitizeFileName(string.Empty));
            if (vaccineModel.VaccineCard1 != null)
            {
                vaccineModel.VaccineCard1 = MoveVaccineFile("vaccine", vaccineModel.VaccineCard1, "VaccineCard1-" + vaccineModel.NoReg.SanitizeFileName(string.Empty) + "-" + vaccineModel.Name.SanitizeFileName(string.Empty));
            }
            if (vaccineModel.VaccineCard2 != null)
            {
                vaccineModel.VaccineCard2 = MoveVaccineFile("vaccine", vaccineModel.VaccineCard2, "VaccineCard2-" + vaccineModel.NoReg.SanitizeFileName(string.Empty) + "-" + vaccineModel.Name.SanitizeFileName(string.Empty));
            }

            vaccineModel.CreatedBy = noreg;
            vaccineModel.ModifiedBy = noreg;
            VaccineService.UpsertVaccine(vaccineModel);
        }
        [HttpPost("create")]
        public IActionResult Create([FromBody] Vaccine vaccineModel)
        {
            UpsertVaccine(vaccineModel);

            return NoContent();
        }

        [HttpPost("update")]
        public IActionResult Update([FromBody] Vaccine vaccineModel)
        {
            UpsertVaccine(vaccineModel);

            return NoContent();
        }

        [HttpGet("download")]
        public async Task<IActionResult> Download(Guid Id)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var vaccineData = VaccineService.GetById(Id);
            var fullPath = Url.Content(vaccineData.IdentityImage);

            return await DownloadFromPath(fullPath);
        }

        [HttpGet("download-image")]
        public async Task<IActionResult> DownloadImage(Guid Id, string ImageType)
        {
            //var vaccineData = VaccineService.GetVaccineById(Id);
            //var filePath = "";
            //if (ImageType == "IdentityId")
            //{
            //    filePath = vaccineData.IdentityImage;
            //}
            //else if (ImageType == "VaccineCard1")
            //{
            //    filePath = vaccineData.VaccineCard1;
            //}
            //else if (ImageType == "VaccineCard2")
            //{
            //    filePath = vaccineData.VaccineCard2;
            //}
            ////var Path2 = Url.Content(filePath);
            //var pathProvider = ServiceProxy.GetPathProvider();
            //var fullPath = pathProvider.ContentPath(filePath).Replace("~/", "");
            //if (System.IO.File.Exists(fullPath))
            //{
            //    using (var memory = new MemoryStream())
            //    {
            //        using (var stream = new FileStream(fullPath, FileMode.Open))
            //        {
            //            stream.CopyToAsync(memory);
            //        }

            //        memory.Position = 0;

            //        return File(memory.ToArray(), GetContentType(fullPath), Path.GetFileName(fullPath));
            //    }
            //}
            //else
            //{
            //    return NoContent();
            //}
            var vaccineData = VaccineService.GetVaccineById(Id);
            string filePath = ImageType switch
            {
                "IdentityId" => vaccineData.IdentityImage,
                "VaccineCard1" => vaccineData.VaccineCard1,
                "VaccineCard2" => vaccineData.VaccineCard2,
                _ => null
            };

            if (string.IsNullOrWhiteSpace(filePath))
                return BadRequest("Invalid image type");

            var pathProvider = ServiceProxy.GetPathProvider();

            // Base folder tetap (FIXED)
            var baseFolder = pathProvider.ContentPath("vaccine");

            // Sanitasi filename
            var fileName = Path.GetFileName(filePath);

            // Gabungkan path dengan aman
            var combinedPath = Path.Combine(baseFolder, fileName);

            // Normalize path, lalu cek traversal
            var normalizedFullPath = Path.GetFullPath(combinedPath);
            var normalizedBaseFolder = Path.GetFullPath(baseFolder);

            if (!normalizedFullPath.StartsWith(normalizedBaseFolder))
            {
                return BadRequest("Invalid file path.");
            }

            return await DownloadFromPath(normalizedFullPath);
        }

        [HttpPost]
        [Route("upload-public")]
        public async Task<IActionResult> UploadPublicFile()
        {
            var pathProvider = ServiceProxy.GetPathProvider();
            var file = Request.Form.Files.FirstOrDefault();
            var tempPath = pathProvider.ContentPath("temps");

            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(tempPath, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return Ok(new { status = "success", url = "~/temps/" + Path.GetFileName(filePath), oldFileName = file.FileName });
        }

        /// <summary>
        /// Move file from given url to specified path.
        /// </summary>
        /// <param name="path">This source path.</param>
        /// <param name="fileUrl">This source url.</param>
        /// <param name="newFileName">This new filename if any.</param>
        /// <returns>This source url.</returns>
        protected string MoveVaccineFile(string path, string fileUrl, string newFileName = null)
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
                if (System.IO.File.Exists(targetFilePath))
                {
                    System.IO.File.Delete(targetFilePath);
                }

                System.IO.File.Move(tempFilePath, targetFilePath);

                return $"~/{path}/{fileName}";
            }

            return fileUrl;
        }

        [HttpGet("get-schedule-limit")]
        public int GetScheduleLimit(DateTime vaccineDate, Guid vaccineHospitalId)
        {
            int limit = 0;
            var data = VaccineScheduleService.GetScheduleLimit(vaccineDate, vaccineHospitalId);
            if (data != null)
            {
                limit = data.Qty.Value;
            }
            return limit;
        }

        [HttpGet("get-available-schedule")]
        public IActionResult GetAvailableSchedule(DateTime vaccineDate)
        {
            var data = VaccineScheduleService.GetAvailableVaccine(vaccineDate);

            return Ok(data);
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Bank page controller
    /// </summary>
    [Area("Core")]
    public class VaccineController : MvcControllerBase
    {
        protected VaccineService VaccineService { get { return ServiceProxy.GetService<VaccineService>(); } }

        public IActionResult Index()
        {
            return View();
        }

        [Permission(PermissionKey.ManageVaccine)]
        public IActionResult Schedule()
        {
            return PartialView("_vaccineSchedule");
        }

        [HttpPost]
        [Permission(PermissionKey.ManageVaccine)]
        public IActionResult Load(string NoReg, string Name)
        {
            var data = VaccineService.GetVaccine(NoReg, Name);
            ViewBag.ActivateDate = VaccineService.ActiveDate();

            return PartialView("_vaccineForm", data);
        }

        public IActionResult LoadScheduleBackdate(string NoReg, string Name)
        {
            var data = VaccineService.GetVaccine(NoReg, Name);
            ViewBag.ActivateDate = VaccineService.ActiveDate();

            return PartialView("_vaccineReportSchedule", data);
        }

        public IActionResult Report()
        {
            // Return the respected view.
            return View();
        }

        [HttpPost]
        [Permission(PermissionKey.ManageVaccine)]
        public IActionResult VaccineReportDetail(string NoReg, string Name)
        {
            var data = VaccineService.GetReportEmployeeDetail(NoReg, Name).FirstOrDefault();
            return PartialView("_VaccineReportDetail", data);
        }
    }


    #endregion
}
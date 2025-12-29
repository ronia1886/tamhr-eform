using Agit.Common;
using Agit.Common.Extensions;
using Kendo.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.Infrastructure.Implementation;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart.ChartEx;
using OfficeOpenXml.Drawing.Style.Coloring;
using OfficeOpenXml.Style;
using Rotativa.AspNetCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Helpers;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using TAMHR.ESS.Infrastructure.Web.Runtime.Providers;
using Telerik.SvgIcons;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Net.WebRequestMethods;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;

namespace TAMHR.ESS.WebUI.Areas.OHS.Controllers
{
    #region API Controller
    /// <summary>
    /// ohs medicalrecord API Manager
    /// </summary>
    [Route("api/ohs/medical-record")]
    //[Permission(PermissionKey.ViewAyoSekolahReport)]
    public class MedicalRecordApiController : ApiControllerBase
    {
        #region Domain Services
        protected MdmService MdmService => ServiceProxy.GetService<MdmService>();
        public MedicalRecordService MedicalRecordService => ServiceProxy.GetService<MedicalRecordService>();
        public McuService McuService => ServiceProxy.GetService<McuService>();
        public UpkkService UpkkService => ServiceProxy.GetService<UpkkService>();
        public PatientService PatientService => ServiceProxy.GetService<PatientService>();
        public KategoriPenyakitService KategoriPenyakitService => ServiceProxy.GetService<KategoriPenyakitService>();


        #endregion

        [HttpPost("document-status-summary")]
        public IEnumerable<dynamic> GetDocumentStatusSummary([FromForm] DateTime startDate, [FromForm] DateTime endDate)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var username = ServiceProxy.UserClaim.Username;
            var organizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);
            var orgCode = organizationStructure?.OrgCode;
            var filter = string.Format("CreatedOn BETWEEN '{0}' AND '{1}'", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

            return ServiceProxy.GetTableValuedSummary<AyoSekolahStoredEntity>("DocumentStatusCode", "DocumentStatus", new { noreg, username, orgCode }, filter);
        }


        [HttpPost("class-summary")]
        public IEnumerable<dynamic> GetClassSummary([FromForm] DateTime startDate, [FromForm] DateTime endDate)
        {
            var noreg = ServiceProxy.UserClaim.NoReg;
            var postCode = ServiceProxy.UserClaim.PostCode;
            var username = ServiceProxy.UserClaim.Username;
            var organizationStructure = MdmService.GetActualOrganizationStructure(noreg, postCode);
            var orgCode = organizationStructure?.OrgCode;
            var filter = string.Format("CreatedOn BETWEEN '{0}' AND '{1}'", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

            return ServiceProxy.GetTableValuedSummary<AyoSekolahStoredEntity>("ClassRange", "ClassRange", new { noreg, username, orgCode }, filter);
        }


        #region Grafik
        [HttpPost("gets-summary-MR")]
        public IActionResult GetsummaryMR([DataSourceRequest] DataSourceRequest request, [FromForm] List<string> NoReg, [FromForm] List<string> NamakaryawanTam, [FromForm] List<string> Divisi)
        {
            string startDate = this.Request.Form["startDate"].ToString();
            string endDate = this.Request.Form["endDate"].ToString();
            string lahirDate = this.Request.Form["LahirDate"].ToString();
            string namaKaryawanVendor = this.Request.Form["NamaKaryawanVendor"].ToString();
           

            DateTime? startDateDT = !string.IsNullOrEmpty(startDate) ? Convert.ToDateTime(startDate) : (DateTime?)null;
            DateTime? endDateDT = !string.IsNullOrEmpty(endDate) ? Convert.ToDateTime(endDate) : (DateTime?)null;
            DateTime? lahirDateDT = !string.IsNullOrEmpty(lahirDate) ? Convert.ToDateTime(lahirDate) : (DateTime?)null;

            // 🔹 Gabungkan NoReg,Divisi menjadi string dengan koma untuk dikirim ke stored procedure
            string noRegCsv = NoReg != null && NoReg.Any() ? string.Join(",", NoReg) : null;
            string noNamaKaryawanTamCsv = NamakaryawanTam != null && NamakaryawanTam.Any() ? string.Join(",", NamakaryawanTam) : null;
            string noDivCsv = Divisi != null && Divisi.Any() ? string.Join(",", Divisi) : null;

            var getData = MedicalRecordService.GetsummaryMR(startDateDT, endDateDT, lahirDateDT, namaKaryawanVendor, noRegCsv, noNamaKaryawanTamCsv, noDivCsv).ToList();

            return new JsonResult(getData); // 🔹 Gunakan JsonResult
            //return getData.ToDataSourceResult(request);

        }
        [HttpPost("gets-summary-UPKK-MR")]
        public IActionResult GetsummaryUpkkMR([DataSourceRequest] DataSourceRequest request, [FromForm] List<string> NoReg, [FromForm] List<string> NamakaryawanTam, [FromForm] List<string> Divisi)
        {
            string startDate = this.Request.Form["startDate"].ToString();
            string endDate = this.Request.Form["endDate"].ToString();
            string lahirDate = this.Request.Form["LahirDate"].ToString();
            string namaKaryawanVendor = this.Request.Form["NamaKaryawanVendor"].ToString();


            //DateTime startDateDT = startDate != "" ? Convert.ToDateTime(startDate) : string.Empty;
            //DateTime endDateDT = endDate != "" ? Convert.ToDateTime(endDate) : string.Empty;

            DateTime? startDateDT = !string.IsNullOrEmpty(startDate) ? Convert.ToDateTime(startDate) : (DateTime?)null;
            DateTime? endDateDT = !string.IsNullOrEmpty(endDate) ? Convert.ToDateTime(endDate) : (DateTime?)null;
            DateTime? lahirDateDT = !string.IsNullOrEmpty(lahirDate) ? Convert.ToDateTime(lahirDate) : (DateTime?)null;

            // 🔹 Gabungkan NoReg,Divisi menjadi string dengan koma untuk dikirim ke stored procedure
            string noRegCsv = NoReg != null && NoReg.Any() ? string.Join(",", NoReg) : null;
            string noNamaKaryawanTamCsv = NamakaryawanTam != null && NamakaryawanTam.Any() ? string.Join(",", NamakaryawanTam) : null;
            string noDivCsv = Divisi != null && Divisi.Any() ? string.Join(",", Divisi) : null;

            var getData = MedicalRecordService.GetsummaryUpkkMR(startDateDT, endDateDT, lahirDateDT, namaKaryawanVendor, noRegCsv, noNamaKaryawanTamCsv, noDivCsv).ToList();

            return new JsonResult(getData); // 🔹 Gunakan JsonResult
            //return getData.ToDataSourceResult(request);

        }
        [HttpPost("gets-summary-SickLeave-MR")]
        public IActionResult GetsummarySickLeaveMR([DataSourceRequest] DataSourceRequest request, [FromForm] List<string> NoReg, [FromForm] List<string> NamakaryawanTam, [FromForm] List<string> Divisi)
        {
            string startDate = this.Request.Form["startDate"].ToString();
            string endDate = this.Request.Form["endDate"].ToString();
            string lahirDate = this.Request.Form["LahirDate"].ToString();
            string namaKaryawanVendor = this.Request.Form["NamaKaryawanVendor"].ToString();


            //DateTime startDateDT = startDate != "" ? Convert.ToDateTime(startDate) : string.Empty;
            //DateTime endDateDT = endDate != "" ? Convert.ToDateTime(endDate) : string.Empty;

            DateTime? startDateDT = !string.IsNullOrEmpty(startDate) ? Convert.ToDateTime(startDate) : (DateTime?)null;
            DateTime? endDateDT = !string.IsNullOrEmpty(endDate) ? Convert.ToDateTime(endDate) : (DateTime?)null;
            DateTime? lahirDateDT = !string.IsNullOrEmpty(lahirDate) ? Convert.ToDateTime(lahirDate) : (DateTime?)null;
            // 🔹 Gabungkan NoReg,Divisi menjadi string dengan koma untuk dikirim ke stored procedure
            string noRegCsv = NoReg != null && NoReg.Any() ? string.Join(",", NoReg) : null;
            string noNamaKaryawanTamCsv = NamakaryawanTam != null && NamakaryawanTam.Any() ? string.Join(",", NamakaryawanTam) : null;
            string noDivCsv = Divisi != null && Divisi.Any() ? string.Join(",", Divisi) : null;

            var getData = MedicalRecordService.GetsummarySickLeaveMR(startDateDT, endDateDT, lahirDateDT, namaKaryawanVendor, noRegCsv, noNamaKaryawanTamCsv,noDivCsv).ToList();

            return new JsonResult(getData); // 🔹 Gunakan JsonResult
            //return getData.ToDataSourceResult(request);

        }

        [HttpPost("gets-summary-SickNessRate-MR")]
        public IActionResult GetsummarySickNessRateMR([DataSourceRequest] DataSourceRequest request, [FromForm] List<string> NoReg, [FromForm] List<string> NamakaryawanTam, [FromForm] List<string> Divisi)
        {
            string startDate = this.Request.Form["startDate"].ToString();
            string endDate = this.Request.Form["endDate"].ToString();
            string lahirDate = this.Request.Form["LahirDate"].ToString();
            string namaKaryawanVendor = this.Request.Form["NamaKaryawanVendor"].ToString();


            //DateTime startDateDT = startDate != "" ? Convert.ToDateTime(startDate) : string.Empty;
            //DateTime endDateDT = endDate != "" ? Convert.ToDateTime(endDate) : string.Empty;

            DateTime? startDateDT = !string.IsNullOrEmpty(startDate) ? Convert.ToDateTime(startDate) : (DateTime?)null;
            DateTime? endDateDT = !string.IsNullOrEmpty(endDate) ? Convert.ToDateTime(endDate) : (DateTime?)null;
            DateTime? lahirDateDT = !string.IsNullOrEmpty(lahirDate) ? Convert.ToDateTime(lahirDate) : (DateTime?)null;
            // 🔹 Gabungkan NoReg,Divisi menjadi string dengan koma untuk dikirim ke stored procedure
            string noRegCsv = NoReg != null && NoReg.Any() ? string.Join(",", NoReg) : null;
            string noNamaKaryawanTamCsv = NamakaryawanTam != null && NamakaryawanTam.Any() ? string.Join(",", NamakaryawanTam) : null;
            string noDivCsv = Divisi != null && Divisi.Any() ? string.Join(",", Divisi) : null;

            var getData = MedicalRecordService.GetsummarySickNessRateMR(startDateDT, endDateDT, lahirDateDT, namaKaryawanVendor, noRegCsv, noNamaKaryawanTamCsv, noDivCsv).ToList();

            return new JsonResult(getData); // 🔹 Gunakan JsonResult
            //return getData.ToDataSourceResult(request);

        }


        [HttpPost("GetTotalKunjunganRawat")]
        public IActionResult GetTotalKunjunganRawat([FromForm] string startDate, [FromForm] string endDate, [FromForm] string lahirDate, [FromForm] string namaKaryawanVendor, [FromForm] List<string> NoReg, [FromForm] List<string> NamakaryawanTam, [FromForm] List<string> Divisi)
        {

            DateTime? startDateDT = !string.IsNullOrEmpty(startDate) ? Convert.ToDateTime(startDate) : (DateTime?)null;
            DateTime? endDateDT = !string.IsNullOrEmpty(endDate) ? Convert.ToDateTime(endDate) : (DateTime?)null;
            DateTime? lahirDateDT = !string.IsNullOrEmpty(lahirDate) ? Convert.ToDateTime(lahirDate) : (DateTime?)null;

            string noRegCsv = NoReg != null && NoReg.Any() ? string.Join(",", NoReg) : null;
            string noNamaKaryawanTamCsv = NamakaryawanTam != null && NamakaryawanTam.Any() ? string.Join(",", NamakaryawanTam) : null;
            string noDivCsv = Divisi != null && Divisi.Any() ? string.Join(",", Divisi) : null;


            var getData = MedicalRecordService.GetsummaryTotalPatientMR(startDateDT, endDateDT, lahirDateDT, namaKaryawanVendor, noRegCsv, noNamaKaryawanTamCsv, noDivCsv).ToList();

           
            return Ok(new { total = getData[0].Total });

        }

        [HttpPost("GetTotalKunjunganSickLeave")]
        public IActionResult GetTotalKunjunganSickLeave([FromForm] string startDate, [FromForm] string endDate, [FromForm] string lahirDate, [FromForm] string namaKaryawanVendor, [FromForm] List<string> NoReg, [FromForm] List<string> NamakaryawanTam, [FromForm] List<string> Divisi)
        {

            DateTime? startDateDT = !string.IsNullOrEmpty(startDate) ? Convert.ToDateTime(startDate) : (DateTime?)null;
            DateTime? endDateDT = !string.IsNullOrEmpty(endDate) ? Convert.ToDateTime(endDate) : (DateTime?)null;
            DateTime? lahirDateDT = !string.IsNullOrEmpty(lahirDate) ? Convert.ToDateTime(lahirDate) : (DateTime?)null;

            string noRegCsv = NoReg != null && NoReg.Any() ? string.Join(",", NoReg) : null;
            string noNamaKaryawanTamCsv = NamakaryawanTam != null && NamakaryawanTam.Any() ? string.Join(",", NamakaryawanTam) : null;
            string noDivCsv = Divisi != null && Divisi.Any() ? string.Join(",", Divisi) : null;

            var getData = MedicalRecordService.GetsummaryTotalSickLeaveMR(startDateDT, endDateDT, lahirDateDT, namaKaryawanVendor, noRegCsv, noNamaKaryawanTamCsv, noDivCsv).ToList();


            return Ok(new { total = getData[0].Total });

        }

        [HttpPost("GetTotalKunjunganSickNessRate")]
        public IActionResult GetTotalKunjunganSickNessRate([FromForm] string startDate, [FromForm] string endDate, [FromForm] string lahirDate, [FromForm] string namaKaryawanVendor, [FromForm] List<string> NoReg, [FromForm] List<string> NamakaryawanTam, [FromForm] List<string> Divisi)
        {

            DateTime? startDateDT = !string.IsNullOrEmpty(startDate) ? Convert.ToDateTime(startDate) : (DateTime?)null;
            DateTime? endDateDT = !string.IsNullOrEmpty(endDate) ? Convert.ToDateTime(endDate) : (DateTime?)null;
            DateTime? lahirDateDT = !string.IsNullOrEmpty(lahirDate) ? Convert.ToDateTime(lahirDate) : (DateTime?)null;

            string noRegCsv = NoReg != null && NoReg.Any() ? string.Join(",", NoReg) : null;
            string noNamaKaryawanTamCsv = NamakaryawanTam != null && NamakaryawanTam.Any() ? string.Join(",", NamakaryawanTam) : null;
            string noDivCsv = Divisi != null && Divisi.Any() ? string.Join(",", Divisi) : null;

            var getData = MedicalRecordService.GetsummaryTotalSickNessRateMR(startDateDT, endDateDT, lahirDateDT, namaKaryawanVendor, noRegCsv, noNamaKaryawanTamCsv, noDivCsv).ToList();


            return Ok(new { total = getData[0].Total });

        }



        [HttpPost("GetTotalKunjunganUpkk")]
        public IActionResult GetTotalKunjunganUpkk([FromForm] string startDate, [FromForm] string endDate, [FromForm] string lahirDate, [FromForm] string namaKaryawanVendor, [FromForm] List<string> NoReg, [FromForm] List<string> NamakaryawanTam, [FromForm] List<string> Divisi)
        {

            DateTime? startDateDT = !string.IsNullOrEmpty(startDate) ? Convert.ToDateTime(startDate) : (DateTime?)null;
            DateTime? endDateDT = !string.IsNullOrEmpty(endDate) ? Convert.ToDateTime(endDate) : (DateTime?)null;
            DateTime? lahirDateDT = !string.IsNullOrEmpty(lahirDate) ? Convert.ToDateTime(lahirDate) : (DateTime?)null;

            string noRegCsv = NoReg != null && NoReg.Any() ? string.Join(",", NoReg) : null;
            string noNamaKaryawanTamCsv = NamakaryawanTam != null && NamakaryawanTam.Any() ? string.Join(",", NamakaryawanTam) : null;
            string noDivCsv = Divisi != null && Divisi.Any() ? string.Join(",", Divisi) : null;

            var getData = MedicalRecordService.GetsummaryTotalUpkkMR(startDateDT, endDateDT, lahirDateDT, namaKaryawanVendor, noRegCsv, noNamaKaryawanTamCsv, noDivCsv).ToList();


            return Ok(new { total = getData[0].Total });

        }

        //[HttpPost("gets-summary-MR")]
        //public IActionResult GetsummaryMR([FromForm] DataSourceRequest request)
        //{
        //    string startDate = this.Request.Form["startDate"].ToString();
        //    string endDate = this.Request.Form["endDate"].ToString();
        //    string lahirDate = this.Request.Form["LahirDate"].ToString();
        //    string namaKaryawanVendor = this.Request.Form["NamaKaryawanVendor"].ToString();


        //    //DateTime startDateDT = startDate != "" ? Convert.ToDateTime(startDate) : string.Empty;
        //    //DateTime endDateDT = endDate != "" ? Convert.ToDateTime(endDate) : string.Empty;

        //    DateTime? startDateDT = !string.IsNullOrEmpty(startDate) ? Convert.ToDateTime(startDate) : (DateTime?)null;
        //    DateTime? endDateDT = !string.IsNullOrEmpty(endDate) ? Convert.ToDateTime(endDate) : (DateTime?)null;
        //    DateTime? lahirDateDT = !string.IsNullOrEmpty(lahirDate) ? Convert.ToDateTime(lahirDate) : (DateTime?)null;


        //    var getData = MedicalRecordService.GetsummaryMR(startDateDT, endDateDT, lahirDateDT, namaKaryawanVendor).ToList();

        //   return new JsonResult(getData); // 🔹 Gunakan JsonResult
        //    //return getData.ToDataSourceResult(request);

        //}
        //[HttpPost("gets-summary-MR")]
        //public IActionResult GetsummaryMR([FromBody] RequestModel request)
        //{
        //    DateTime? startDateDT = !string.IsNullOrEmpty(request.StartDate) ? Convert.ToDateTime(request.StartDate) : (DateTime?)null;
        //    DateTime? endDateDT = !string.IsNullOrEmpty(request.EndDate) ? Convert.ToDateTime(request.EndDate) : (DateTime?)null;
        //    DateTime? lahirDateDT = !string.IsNullOrEmpty(request.LahirDate) ? Convert.ToDateTime(request.LahirDate) : (DateTime?)null;

        //    var data = MedicalRecordService.GetsummaryMR(startDateDT, endDateDT, lahirDateDT, request.NamaKaryawanVendor).ToList();

        //    return JsonResult(data); // 🔹 Mengembalikan JSON langsung ke UI
        //}
        #endregion


        [HttpPost("gets-header")]
        public DataSourceResult GetHeader([DataSourceRequest] DataSourceRequest request)
        {
            string startDate = this.Request.Form["startDate"].ToString();
            string endDate = this.Request.Form["endDate"].ToString();
            string lahirDate = this.Request.Form["LahirDate"].ToString();
            string namaKaryawanVendor = this.Request.Form["NamaKaryawanVendor"].ToString();


            //DateTime startDateDT = startDate != "" ? Convert.ToDateTime(startDate) : string.Empty;
            //DateTime endDateDT = endDate != "" ? Convert.ToDateTime(endDate) : string.Empty;

            DateTime? startDateDT = !string.IsNullOrEmpty(startDate) ? Convert.ToDateTime(startDate) : (DateTime?)null;
            DateTime? endDateDT = !string.IsNullOrEmpty(endDate) ? Convert.ToDateTime(endDate) : (DateTime?)null;
            DateTime? lahirDateDT = !string.IsNullOrEmpty(lahirDate) ? Convert.ToDateTime(lahirDate) : (DateTime?)null;
            string[] role = ServiceProxy.UserClaim.Roles;
            string targetRole = role.FirstOrDefault(r => r == "OHS_ADMIN");

            var getData = MedicalRecordService.GetMedicalHeader(startDateDT, endDateDT, lahirDateDT, namaKaryawanVendor, targetRole).ToList();


            return getData.ToDataSourceResult(request);
        }

        #region grid detail
        [HttpPost("gets-mcu-detail")]
        public async Task<DataSourceResult> GetDetail([FromForm] string noreg, [FromForm] string startDate, [FromForm] string endDate, [FromForm] string namekaryawan, [DataSourceRequest] DataSourceRequest request)
        {
            //string startDate = this.Request.Form["startDate"].ToString();
            //string endDate = this.Request.Form["endDate"].ToString();


            //DateTime startDateDT = startDate != "" ? Convert.ToDateTime(startDate) : DateTime.Today;
            //DateTime endDateDT = endDate != "" ? Convert.ToDateTime(endDate) : DateTime.Today;
            DateTime? startDateDT = !string.IsNullOrEmpty(startDate) ? Convert.ToDateTime(startDate) : (DateTime?)null;
            DateTime? endDateDT = !string.IsNullOrEmpty(endDate) ? Convert.ToDateTime(endDate) : (DateTime?)null;

            if (noreg == null)
                noreg = "";

            var getData = McuService.GetMcuDetail(noreg, startDateDT, endDateDT).ToList();

            return await getData.ToDataSourceResultAsync(request);
        }

        [HttpPost("gets-upkk-detail")]
        public DataSourceResult GetUpkkDetail([FromForm] string noreg, [FromForm] string startDate, [FromForm] string endDate, [FromForm] string namekaryawan, [DataSourceRequest] DataSourceRequest request)
        {
            //string startDate = this.Request.Form["startDate"].ToString();
            //string endDate = this.Request.Form["endDate"].ToString();


            DateTime? startDateDT = !string.IsNullOrEmpty(startDate) ? Convert.ToDateTime(startDate) : (DateTime?)null;
            DateTime? endDateDT = !string.IsNullOrEmpty(endDate) ? Convert.ToDateTime(endDate) : (DateTime?)null;
            if (noreg == null)
                noreg = "";

            var getData = UpkkService.GetUpkkDetail(noreg, startDateDT, endDateDT).ToList();


            return getData.ToDataSourceResult(request);
        }
        [HttpPost("gets-patient-detail")]
        public DataSourceResult GetPatientDetail([FromForm] string noreg, [FromForm] string startDate, [FromForm] string endDate, [FromForm] string namekaryawan, [DataSourceRequest] DataSourceRequest request)
        {
            //string startDate = this.Request.Form["startDate"].ToString();
            //string endDate = this.Request.Form["endDate"].ToString();


            DateTime? startDateDT = !string.IsNullOrEmpty(startDate) ? Convert.ToDateTime(startDate) : (DateTime?)null;
            DateTime? endDateDT = !string.IsNullOrEmpty(endDate) ? Convert.ToDateTime(endDate) : (DateTime?)null;
            if (noreg == null)
                noreg = "";

            var getData = PatientService.GetPatientDetail(noreg, startDateDT, endDateDT).ToList();


            return getData.ToDataSourceResult(request);
        }
        [HttpPost("gets-sickleave-detail")]
        public DataSourceResult GetSickLeaveDetail([FromForm] string noreg, [FromForm] string startDate, [FromForm] string endDate, [FromForm] string namekaryawan, [DataSourceRequest] DataSourceRequest request)
        {
            //string startDate = this.Request.Form["startDate"].ToString();
            //string endDate = this.Request.Form["endDate"].ToString();


            DateTime? startDateDT = !string.IsNullOrEmpty(startDate) ? Convert.ToDateTime(startDate) : (DateTime?)null;
            DateTime? endDateDT = !string.IsNullOrEmpty(endDate) ? Convert.ToDateTime(endDate) : (DateTime?)null;


            if (noreg == null)
                noreg = "";

            var getData = MedicalRecordService.GetSickLeaveDetail(noreg, startDateDT, endDateDT).ToList();


            return getData.ToDataSourceResult(request);
        }

        [HttpPost("gets-tanyaohs-detail")]
        public DataSourceResult GetTanyaOhsDetail([FromForm] string noreg, [FromForm] string startDate, [FromForm] string endDate, [FromForm] string namekaryawan, [DataSourceRequest] DataSourceRequest request)
        {
            //string startDate = this.Request.Form["startDate"].ToString();
            //string endDate = this.Request.Form["endDate"].ToString();



            DateTime? startDateDT = !string.IsNullOrEmpty(startDate) ? Convert.ToDateTime(startDate) : (DateTime?)null;
            DateTime? endDateDT = !string.IsNullOrEmpty(endDate) ? Convert.ToDateTime(endDate) : (DateTime?)null;
            if (noreg == null)
                noreg = "";

            var getData = PatientService.GetTanyaOhsDetail(noreg, startDateDT, endDateDT).ToList();


            return getData.ToDataSourceResult(request);
        }
        #endregion

        [HttpPost("gets-Noreg")]//combobox
        public async Task<DataSourceResult> GetNoReg()
        {
            return await McuService.Getnoreg().ToDataSourceResultAsync(new DataSourceRequest());
        }
        [HttpPost("get-name-bynoreg")]
        public async Task<DataSourceResult> NameByNoreg([FromBody] string entity, [DataSourceRequest] DataSourceRequest request)
        {
            return await MedicalRecordService.GetByNoreg(entity).ToDataSourceResultAsync(request);
        }
        [HttpPost("gets-UpkkDropDown")]//combobox
        public async Task<DataSourceResult> GetUpkkDropDown()
        {
            return await MedicalRecordService.GetUpkkDropDown().ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("gets-kp")]//combobox
        public async Task<DataSourceResult> GetKpDropDown()
        {
            return await MedicalRecordService.GetKpDropDown().ToDataSourceResultAsync(new DataSourceRequest());
        }
        [HttpPost("get-SpesifikPenyakit")]
        public async Task<DataSourceResult> GetByMappingQuery([DataSourceRequest] DataSourceRequest request)
        {
            string code = Request.Form["code"].ToString();

            return await MedicalRecordService.GetMappingQuery(code).ToDataSourceResultAsync(request);
        }
        #region insert update

        [HttpPost("insertMcu")]
        public async Task<IActionResult> CreateMcu([FromBody] PersonalDataMedicalHistory Param)
        {
            if (Param == null)
            {
                return BadRequest("Invalid data received.");
            }

            //int yearperiod = DateTime.Now.Year;
            //bool dataExists = false;
            //dataExists = McuService.GetDataMcu(Param.NoregMcu, yearperiod);
            //Assert.ThrowIf(dataExists == true, "Data Alredy Exists");
            
            Assert.ThrowIf(!Param.YearPeriod.HasValue, "Tahun MCU Must Fill");
            Assert.ThrowIf(Param.LokasiMCU == "", "Lokasi Mcu Must Fill");
            Assert.ThrowIf(Param.HealthEmployeeStatus == "", "Satus Hasil MCU Must Fill");

            //Param.YearPeriod = yearperiod;
            Param.NoReg = Param.NoregMcu;
            McuService.Upsert(Param);

            return CreatedAtAction("Get", new { id = Param.Id });

        }
        [HttpPut("updateMcu")]
        public IActionResult UpdateMcu([FromBody] PersonalDataMedicalHistory param)
        {
            param.NoReg = param.NoregMcu;
            McuService.Upsert(param);

            return NoContent();
        }
        [HttpPost("insertUpkk")]
        public async Task<IActionResult> CreateUpkk([FromBody] UPKK Param)
        {
            //int yearperiod = DateTime.Now.Year;
            //bool dataExists = false;
            //dataExists = McuService.GetDataMcu(Param.Noreg, yearperiod);
            //Assert.ThrowIf(dataExists == true, "Data Alredy Exists");
            //Param.YearPeriod = yearperiod;

            Assert.ThrowIf(Param.LokasiUPKK == "", "Lokasi Upkk Must Fill");
            Assert.ThrowIf(Param.TanggalKunjungan == null, "Tanggal Kunjungan Must Fill");
            Assert.ThrowIf(Param.KategoriKunjungan == "", "Kategori Kunjungan Must Fill");
            Assert.ThrowIf(Param.KategoriPenyakit == "", "Kategori Penyakit Must Fill");
            Assert.ThrowIf(Param.HasilAkhir == "", "Hasil Akhir Must Fill");
            Param.Noreg = Param.NoregUpkk;
            Param.Divisi = Param.DivisiUpkk;
            UpkkService.Upsert(Param);
            //return NoContent();
            return CreatedAtAction("Get", new { id = Param.Id });

        }
        [HttpPut("updateUpkk")]
        public IActionResult UpdateUpkk([FromBody] UPKK param)
        {
            param.Noreg = param.NoregUpkk;
            param.Divisi = param.DivisiUpkk;
            UpkkService.Upsert(param);

            return NoContent();
        }
        [HttpPost("insertPatient")]
        public async Task<IActionResult> CreatePatient([FromBody] Patient Param)
        {
            Assert.ThrowIf(Param.AdmissionDate == null, "Admission Date Must Fill");
            Assert.ThrowIf(Param.Provider == "", "Provider Must Fill");

            Param.Noreg = Param.NoregPatient;
            PatientService.Upsert(Param);
            //return NoContent();
            return CreatedAtAction("Get", new { id = Param.Id });

        }
        [HttpPut("updatePatient")]
        public IActionResult UpdatePatient([FromBody] Patient param)
        {
            param.Noreg = param.NoregPatient;
            PatientService.Upsert(param);

            return NoContent();
        }
        #endregion

        #region Softdelete

        [HttpDelete("deleteMcu")]
        public IActionResult deleteMcu([FromForm] Guid id)
        {
            McuService.Delete(id);

            return NoContent();
        }

        [HttpDelete("deleteUpkk")]
        public IActionResult deleteUpkk([FromForm] Guid id)
        {
            UpkkService.Delete(id);

            return NoContent();
        }

        [HttpDelete("deletePatient")]
        public IActionResult deletePatient([FromForm] Guid id)
        {
            PatientService.Delete(id);

            return NoContent();
        }
        #endregion

        #region download template upload
        [HttpGet("mcu/download-template")]
        public IActionResult DownloadTemplateMcu()
        {
            PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));

            string templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\Upload Medical Checkup.xlsx");

            // Results Output
            System.IO.MemoryStream output = new System.IO.MemoryStream();

            // Read Template
            using (System.IO.FileStream templateDocumentStream = System.IO.File.OpenRead(templateDocument))
            {
                // Create Excel EPPlus Package based on template stream
                using (ExcelPackage package = new ExcelPackage(templateDocumentStream))
                {
                    package.SaveAs(output);
                }
            }

            var fileName = "Upload Medical Checkup.xlsx";
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(output.ToArray(), contentType, fileName);


        }

        [HttpGet("upkkTam/download-template")]
        public IActionResult DownloadTemplateUpkkTam()
        {
            PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));

            string templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\UPLOAD UPKK VISIT-TAM Employee.xlsx");

            // Results Output
            System.IO.MemoryStream output = new System.IO.MemoryStream();

            // Read Template
            using (System.IO.FileStream templateDocumentStream = System.IO.File.OpenRead(templateDocument))
            {
                // Create Excel EPPlus Package based on template stream
                using (ExcelPackage package = new ExcelPackage(templateDocumentStream))
                {
                    package.SaveAs(output);
                }
            }

            var fileName = "UPLOAD UPKK VISIT_TAM Employee.xlsx";
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(output.ToArray(), contentType, fileName);


        }

        [HttpGet("upkkVendor/download-template")]
        public IActionResult DownloadTemplateUpkkVendor()
        {
            PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));

            string templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\UPLOAD UPKK VISIT-Vendor Employee.xlsx");

            // Results Output
            System.IO.MemoryStream output = new System.IO.MemoryStream();

            // Read Template
            using (System.IO.FileStream templateDocumentStream = System.IO.File.OpenRead(templateDocument))
            {
                // Create Excel EPPlus Package based on template stream
                using (ExcelPackage package = new ExcelPackage(templateDocumentStream))
                {
                    package.SaveAs(output);
                }
            }

            var fileName = "UPLOAD UPKK VISIT_Vendor Employee.xlsx";
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(output.ToArray(), contentType, fileName);


        }

        [HttpGet("Patient/download-template")]
        public IActionResult DownloadTemplatePatient()
        {
            PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));

            string templateDocument = pathProvider.FilePath("wwwroot\\uploads\\excel-template\\UPLOAD IN & OUT PATIENT.xlsx");

            // Results Output
            System.IO.MemoryStream output = new System.IO.MemoryStream();

            // Read Template
            using (System.IO.FileStream templateDocumentStream = System.IO.File.OpenRead(templateDocument))
            {
                // Create Excel EPPlus Package based on template stream
                using (ExcelPackage package = new ExcelPackage(templateDocumentStream))
                {
                    package.SaveAs(output);
                }
            }

            var fileName = "UPLOAD IN & OUT PATIENT.xlsx";
            var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return File(output.ToArray(), contentType, fileName);

        }
        #endregion

        #region Upload
        [HttpPost("mcu/merge")]
        public async Task<IActionResult> mergemcu()
        {
            string[] ComparerKeysmcu = new[] { "NoReg", "YearPeriod", "CreatedOn" };
            var dicts = new Dictionary<string, Type>
            {
                { "YearPeriod", typeof(int) },
                { "Paket", typeof(string) },
                { "LokasiMCU", typeof(string) },
                { "NoReg", typeof(string) },
                { "Usia", typeof(int) },
                { "PenyakitPernahDiderita", typeof(string) },
                { "Perokok", typeof(string) },
                { "JumlahBatangPerHari", typeof(int) },
                { "Miras", typeof(string) },
                { "Olahraga", typeof(string) },
                { "FrequencyPerMinggu", typeof(int) },
                { "Kebisingan", typeof(string) },
                { "SuhuExtremePanasAtauDingin", typeof(string) },
                { "Radiasi", typeof(string) },
                { "GetaranLokal", typeof(string) },
                { "GetaranSeluruhTubuh", typeof(string) },
                { "LainnyaFisika", typeof(string) },
                { "Debu", typeof(string) },
                { "Asap", typeof(string) },
                { "LimbahB3", typeof(string) },
                { "LainnyaKimia", typeof(string) },
                { "BakteriAtaVirusAtauJamurAtauParasit", typeof(string) },
                { "LainnyaBiologi", typeof(string) },
                { "GerakanBerulangDenganTangan", typeof(string) },
                { "AngkatBerat", typeof(string) },
                { "DudukLama", typeof(string) },
                { "BerdiriLama", typeof(string) },
                { "PosisiTubuhTidakErgonomis", typeof(string) },
                { "PencahayaanTidakSesuai", typeof(string) },
                { "BekerjaDepanLayar", typeof(string) },
                { "LainnyaErgonomis", typeof(string) },
                { "RiwayatHipertensi", typeof(string) },
                { "RiwayatDiabetes", typeof(string) },
                { "RiwayatPenyakitJantung", typeof(string) },
                { "RiwayatPenyakitGinjal", typeof(string) },
                { "RiwayatGangguanMental", typeof(string) },
                { "RiwayatPenyakitLain", typeof(string) },
                { "PenyakitSaatIni", typeof(string) },
                { "SedangBerobat", typeof(string) },
                { "ObatYangDiberikan", typeof(string) },
                { "Tinggi", typeof(decimal) },
                { "Berat", typeof(decimal) },
                { "StatusBMI", typeof(decimal) },
                { "TekananDarahSistol", typeof(int) },
                { "TekananDarahDiastol", typeof(int) },
                { "TekananDarah", typeof(string) },
                { "HasilEKG", typeof(string) },
                { "KesimpulanEKG", typeof(string) },
                { "KesimpulanTreadmill", typeof(string) },
                { "KesanPhotoRontgen", typeof(string) },
                { "HasilRontgen", typeof(string) },
                { "KesimpulanUsgAbdomen", typeof(string) },
                { "HasilUsgMammae", typeof(string) },
                { "KesimpulanUsgMammae", typeof(string) },
                { "KesimpulanFisik", typeof(string) },
                { "KesimpulanButaWarna", typeof(string) },
                { "KesimpulanPemVisusMata", typeof(string) },
                { "HasilPapsmear", typeof(string) },
                { "KesimpulanPamsmear", typeof(string) },
                { "Hemoglobine", typeof(decimal) },
                { "Hematocrit", typeof(int) },
                { "Leucocyte", typeof(int) },
                { "TotalPlatelets", typeof(decimal) },
                { "Eryrocyte", typeof(decimal) },
                { "MCV", typeof(decimal) },
                { "MCH", typeof(int) },
                { "MCHC", typeof(int) },
                { "ESR", typeof(int) },
                { "GlukosaPuasa", typeof(int) },
                { "HbA1c", typeof(decimal) },
                { "SGOT", typeof(int) },
                { "SGPT", typeof(int) },
                { "GammaGT", typeof(int) },
                { "HBsAg", typeof(string) },
                { "Ureum", typeof(int) },
                { "Kreatinin", typeof(decimal) },
                { "AsamUrat", typeof(decimal) },
                { "GFR", typeof(decimal) },
                { "KolesterolTotal", typeof(int) },
                { "HDL", typeof(int) },
                { "LDL", typeof(int) },
                { "Triglyceride", typeof(int) },
                { "PlateletAggregation", typeof(string) },
                { "Fibrinogen", typeof(int) },
                { "CEA", typeof(decimal) },
                { "PSA", typeof(string) },
                { "Ca125", typeof(int) },
                { "VitD25OH", typeof(decimal) },
                { "UrinDarah", typeof(string) },
                { "UrinBakteri", typeof(string) },
                { "UrinKristal", typeof(string) },
                { "UrinLeukosit", typeof(string) },
                { "ScoreAmbiguity", typeof(int) },
                { "KesimpulanAmbiguity", typeof(string) },
                { "ScoreConflict", typeof(int) },
                { "KesimpulanConflict", typeof(string) },
                { "ScoreQuantitative", typeof(int) },
                { "KesimpulanQuantitative", typeof(string) },
                { "ScoreQualitative", typeof(int) },
                { "KesimpulanQualitative", typeof(string) },
                { "ScoreCareerDevelopment", typeof(int) },
                { "KesimpulanCareerDevelopment", typeof(string) },
                { "ScoreResponsibilityforPeople", typeof(int) },
                { "KesimpulanResponsibilityforPeople", typeof(string) },
                { "KesimpulanLab", typeof(string) },
                { "Saran", typeof(string) },
                { "HealthEmployeeStatus", typeof(string) },
                { "CreatedOn", typeof(DateTime) }

            };

            //string[] excludes = { "No", "NAMA", "DIVISI", "TANGGAL LAHIR\n(dd/mm/yy)", "JENIS KELAMIN" };
            string[] excludes = { "No" };

            IFormFile file = Request.Form.Files.FirstOrDefault();

            Assert.ThrowIf(file == null, "Upload failed. Please choose a file");

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);

                using (var package = new ExcelPackage(ms))
                {
                    var workSheet = package.Workbook.Worksheets.FirstOrDefault();

                    if (workSheet == null) Assert.ThrowIf(file == null, "Cannot read uploaded file as excel");
                    int counter = 3;
                    //var rowCount = workSheet.Dimension.Rows;

                    //if (rowCount <= 3)
                    //{
                    //    Assert.ThrowIf(rowCount <= 3, "No data Row");
                    //}
                    //else
                    //{
                    //    McuService.UploadAndMergeAsync<PersonalDataMedicalHistory>(ServiceProxy.UserClaim.NoReg, workSheet, dicts, ComparerKeys, excludes);

                    //}

                    do
                    {
                        var LokasiMcu = workSheet.Cells[counter, 4].Text;
                        var Noreg = workSheet.Cells[counter, 5].Text;
                        var Hasil = workSheet.Cells[counter, 108].Text;

                        if (string.IsNullOrEmpty(Noreg))
                        {
                            break;
                        }

                        Assert.ThrowIf(LokasiMcu == "", "Row " + counter + ": cannot insert. Lokasi Mcu Must Fill");
                        Assert.ThrowIf(Noreg == "", "Row " + counter + ": cannot insert. NoReg Must Fill");
                        Assert.ThrowIf(Hasil == "", "Row " + counter + ": cannot insert. Hasil MCU Must Fill");



                        counter++;
                    } while (true);




                    McuService.UploadAndMergeAsync<PersonalDataMedicalHistory>(ServiceProxy.UserClaim.NoReg, workSheet, dicts, ComparerKeysmcu, excludes);
                    //McuService.Merge(ServiceProxy.UserClaim.NoReg, dt);


                }
            }

            return NoContent();
        }

        [HttpPost("upkkTam/merge")]
        public async Task<IActionResult> mergeupkkTam()
        {
            string[] ComparerKeysUPKK = new[] { "Noreg", "CreatedOn" };
            var dicts = new Dictionary<string, Type>
            {
                { "LokasiUPKK", typeof(string) },
                { "KategoriKunjungan", typeof(string) },
                { "TanggalKunjungan", typeof(DateTime) },
                { "Noreg", typeof(string) },
                { "Usia", typeof(int) },
                { "JenisPekerjaan", typeof(string) },
                { "LokasiKerja", typeof(string) },
                { "Keluhan", typeof(string) },
                { "TDSistole", typeof(string) },
                { "TDDiastole", typeof(string) },
                { "Nadi", typeof(string) },
                { "Respirasi", typeof(string) },
                { "Suhu", typeof(string) },
                { "Diagnosa", typeof(string) },
                { "KategoriPenyakit", typeof(string) },
                { "SpesifikPenyakit", typeof(string) },
                { "JenisKasus", typeof(string) },
                { "Treatment", typeof(string) },
                { "Pemeriksa", typeof(string) },
                { "NamaPemeriksa", typeof(string) },
                { "HasilAkhir", typeof(string) },
                { "Company", typeof(string) },
                { "Divisi", typeof(string) },
                { "CreatedOn", typeof(DateTime) }

            };

            string[] excludes = { "" };

            IFormFile file = Request.Form.Files.FirstOrDefault();

            Assert.ThrowIf(file == null, "Upload failed. Please choose a file");

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);

                using (var package = new ExcelPackage(ms))
                {
                    var workSheet = package.Workbook.Worksheets.FirstOrDefault();

                    if (workSheet == null) Assert.ThrowIf(file == null, "Cannot read uploaded file as excel");
                    int counterRow = 3;

                    do
                    {
                        //var Company = workSheet.Cells[counterRow, 4].Text;
                        //var Divisi = workSheet.Cells[counterRow, 5].Text;
                        var LokasiUpkk = workSheet.Cells[counterRow, 1].Text;
                        var Kategorikunjungan = workSheet.Cells[counterRow, 2].Text;
                        var TanggalKunjungan = workSheet.Cells[counterRow, 3].Text;
                        var Noreg = workSheet.Cells[counterRow, 4].Text;
                        var KategoriPenyakit = workSheet.Cells[counterRow, 15].Text;
                        var Hasil = workSheet.Cells[counterRow, 21].Text;


                        if (string.IsNullOrEmpty(Noreg) && string.IsNullOrEmpty(TanggalKunjungan))
                        {
                            break;
                        }


                        // Assert.ThrowIf(Company == "", "Row " + counterRow + ": cannot insert. Company Must Fill");
                        // Assert.ThrowIf(Divisi == "", "Row " + counterRow + ": cannot insert. Divisi Must Fill");
                        Assert.ThrowIf(LokasiUpkk == "", "Row " + counterRow + ": cannot insert. Lokasi Upkk Must Fill");
                        Assert.ThrowIf(Kategorikunjungan == "", "Row " + counterRow + ": cannot insert. Kategori kunjungan Must Fill");
                        Assert.ThrowIf(TanggalKunjungan == "", "Row " + counterRow + ": cannot insert. Tanggal Kunjungan Must Fill");
                        Assert.ThrowIf(Noreg == "", "Row " + counterRow + ": cannot insert. Noreg Must Fill");
                        Assert.ThrowIf(KategoriPenyakit == "", "Row " + counterRow + ": cannot insert. Kategori Penyakit Must Fill");
                        Assert.ThrowIf(Hasil == "", "Row " + counterRow + ": cannot insert. Hasil akhir Must Fill");



                        //find company divisi exists in db

                        //var exists = UpkkService.GetDataUpkkTam(Noreg);
                        //Assert.ThrowIf(exists == true, "Row " + counterRow + ": cannot insert. Data already Exists");

                        counterRow++;

                    } while (true);

                    UpkkService.UploadAndMergeTamAsync<UPKK>(ServiceProxy.UserClaim.NoReg, workSheet, dicts, ComparerKeysUPKK, excludes);

                }
            }

            return NoContent();
        }


        [HttpPost("upkkvendor/merge")]
        public async Task<IActionResult> mergeupkkVendor()
        {
            string[] ComparerKeysUPKK = new[] { "Company", "Divisi", "NamaEmployeeVendor", "TanggalLahirVendor", "JenisKelaminEmployeeVendor", "CreatedOn" };
            var dicts = new Dictionary<string, Type>
            {
                { "LokasiUPKK", typeof(string) },
                { "KategoriKunjungan", typeof(string) },
                { "TanggalKunjungan", typeof(DateTime) },
                { "Company", typeof(string) },
                { "Divisi", typeof(string) },
                { "Noreg", typeof(string) },
                { "NamaEmployeeVendor", typeof(string) },
                { "TanggalLahirVendor", typeof(DateTime) },
                { "Usia", typeof(int) },
                { "JenisKelaminEmployeeVendor", typeof(string) },
                { "JenisPekerjaan", typeof(string) },
                { "LokasiKerja", typeof(string) },
                { "Keluhan", typeof(string) },
                { "TDSistole", typeof(string) },
                { "TDDiastole", typeof(string) },
                { "Nadi", typeof(string) },
                { "Respirasi", typeof(string) },
                { "Suhu", typeof(string) },
                { "Diagnosa", typeof(string) },
                { "KategoriPenyakit", typeof(string) },
                { "SpesifikPenyakit", typeof(string) },
                { "JenisKasus", typeof(string) },
                { "Treatment", typeof(string) },
                { "Pemeriksa", typeof(string) },
                { "NamaPemeriksa", typeof(string) },
                { "HasilAkhir", typeof(string) },
                { "CreatedOn", typeof(string) }


            };

            string[] excludes = { "" };

            IFormFile file = Request.Form.Files.FirstOrDefault();

            Assert.ThrowIf(file == null, "Upload failed. Please choose a file");

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);

                using (var package = new ExcelPackage(ms))
                {
                    var workSheet = package.Workbook.Worksheets.FirstOrDefault();

                    if (workSheet == null) Assert.ThrowIf(file == null, "Cannot read uploaded file as excel");
                    int counterRow = 3;

                    do
                    {
                        var LokasiUpkk = workSheet.Cells[counterRow, 1].Text;
                        var KategoriKunjungan = workSheet.Cells[counterRow, 2].Text;
                        var TanggalKunjungan = workSheet.Cells[counterRow, 3].Text;
                        var Company = workSheet.Cells[counterRow, 4].Text;
                        var Divisi = workSheet.Cells[counterRow, 5].Text;
                        var Nama = workSheet.Cells[counterRow, 6].Text;
                        var TgllahirString = workSheet.Cells[counterRow, 7].Text;
                        var JenisKelamin = workSheet.Cells[counterRow, 9].Text;
                        var KategoriPenyakit = workSheet.Cells[counterRow, 19].Text;
                        var Hasil = workSheet.Cells[counterRow, 25].Text;
                        //DateTime? Tgllahir = null;


                        if (string.IsNullOrEmpty(Company) && string.IsNullOrEmpty(Divisi) && string.IsNullOrEmpty(Nama) && string.IsNullOrEmpty(TgllahirString) && string.IsNullOrEmpty(JenisKelamin))
                        {
                            break;
                        }

                        // Coba parsing string ke DateTime
                        //if (DateTime.TryParseExact(TgllahirString, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                        //{
                        //    Tgllahir = parsedDate;
                        //}

                        Assert.ThrowIf(LokasiUpkk == "", "Row " + counterRow + ": cannot insert. Lokasi Upkk Must Fill");
                        Assert.ThrowIf(KategoriKunjungan == "", "Row " + counterRow + ": cannot insert. Kategori kunjungan Must Fill");
                        Assert.ThrowIf(TanggalKunjungan == "", "Row " + counterRow + ": cannot insert. TanggalKunjungan Must Fill");
                        Assert.ThrowIf(Company == "", "Row " + counterRow + ": cannot insert. Company Must Fill");
                        Assert.ThrowIf(Divisi == "", "Row " + counterRow + ": cannot insert. Divisi Must Fill");
                        Assert.ThrowIf(Nama == "", "Row " + counterRow + ": cannot insert. Nama Must Fill");
                        Assert.ThrowIf(TgllahirString == "", "Row " + counterRow + ": cannot insert. Tanggal Lahir Must Fill");
                        Assert.ThrowIf(JenisKelamin == "", "Row " + counterRow + ": cannot insert. Tanggal Lahir Must Fill");
                        Assert.ThrowIf(KategoriPenyakit == "", "Row " + counterRow + ": cannot insert. Kategori Penyakit Must Fill");
                        Assert.ThrowIf(Hasil == "", "Row " + counterRow + ": cannot insert. Hasil akhir Must Fill");

                        //find company divisi nama tgllahir jenis kelamin exists in db

                        //var exists = UpkkService.GetDataUpkkVendor(Company, Divisi, Nama, Tgllahir, JenisKelamin);
                        //Assert.ThrowIf(exists == true, "Row " + counterRow + ": cannot insert. Data already Exists");


                        counterRow++;

                    } while (true);

                    UpkkService.UploadAndMergeUpkkVendorAsync<UPKK>(ServiceProxy.UserClaim.NoReg, workSheet, dicts, ComparerKeysUPKK, excludes);

                }
            }

            return NoContent();
        }


        [HttpPost("Patient/merge")]
        public async Task<IActionResult> mergepatient()
        {
            string[] ComparerKeysPatient = new[] { "Noreg", "CreatedOn" };
            var dicts = new Dictionary<string, Type>
            {
                { "Noreg", typeof(string) },
                { "Provider", typeof(string) },
                { "AdmissionDate", typeof(DateTime) },
                { "DisChargeAbleDate", typeof(DateTime) },
                { "DiagnosisDesc", typeof(string) },
                { "CreatedOn", typeof(DateTime) }


        };

            string[] excludes = { "No" };

            IFormFile file = Request.Form.Files.FirstOrDefault();

            Assert.ThrowIf(file == null, "Upload failed. Please choose a file");

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);

                using (var package = new ExcelPackage(ms))
                {
                    var workSheet = package.Workbook.Worksheets.FirstOrDefault();

                    if (workSheet == null) Assert.ThrowIf(file == null, "Cannot read uploaded file as excel");
                    int counterRow = 2;

                    do
                    {
                        var Noreg = workSheet.Cells[counterRow, 2].Text;
                        var Provider = workSheet.Cells[counterRow, 3].Text;
                        var Admissiondate = workSheet.Cells[counterRow, 4].Text;

                        if (string.IsNullOrEmpty(Noreg) && string.IsNullOrEmpty(Provider) && string.IsNullOrEmpty(Admissiondate))
                        {
                            break;
                        }

                        Assert.ThrowIf(Noreg == "", "Row " + counterRow + ": cannot insert. NoReg Must Fill");
                        Assert.ThrowIf(Provider == "", "Row " + counterRow + ": cannot insert. Provider Must Fill");
                        Assert.ThrowIf(Admissiondate == "", "Row " + counterRow + ": cannot insert. Admission date Must Fill");


                        counterRow++;

                    } while (true);



                    PatientService.UploadAndMergeAsync<Patient>(ServiceProxy.UserClaim.NoReg, workSheet, dicts, ComparerKeysPatient, excludes);

                }
            }

            return NoContent();
        }
        #endregion

        #region REQUEST DOWNLOAD dan validasi

        [HttpPost("CekValidasi")]
        public async Task<IActionResult> CekValidasiReqDownload([FromBody] ReqDownloadMedical Param)
        {

            bool dataExists = false;
            bool dataApprove = false;
            dataExists = MedicalRecordService.GetDataReq(Param.Requestor);
            Assert.ThrowIf(dataExists == true, "You Request Waiting Approve");


            dataApprove = MedicalRecordService.GetDataApprove(Param.Requestor);

            //MedicalRecordService.UpsertPost(Param);
            return Ok(new { datarequest = dataApprove });


        }


        [HttpPost("InsertNewRequest")]
        public async Task<IActionResult> CreateReqDownload([FromBody] ReqDownloadMedical Param)
        {

            Param.Requestor = Param.Requestor;
            Param.Approver = "";
            Param.StatusRequest = "Waiting Approve";
            MedicalRecordService.UpsertPost(Param);
            return NoContent();


        }
        #endregion

        private string SanitizeInput(string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            string sanitized = input
                .Replace("<", string.Empty)
                .Replace(">", string.Empty)
                .Replace("'", string.Empty)
                .Trim();

            return sanitized.Length > 200 ? sanitized[..200] : sanitized;
        }

        #region Download File excel medical record
        [HttpGet("downloadMedicalRecord")]
        public IActionResult downloadMedicalRecord(DateTime startDate, DateTime endDate, DateTime? lahirDate, string namaKaryawanVendor, string Noreg,string NamakaryawanTam, string Divisi)
        {
            string[] role = ServiceProxy.UserClaim.Roles;
            string targetRole = role.FirstOrDefault(r => r == "OHS_ADMIN");

            namaKaryawanVendor = SanitizeInput(namaKaryawanVendor);
            Noreg = SanitizeInput(Noreg);
            NamakaryawanTam = SanitizeInput(NamakaryawanTam);
            Divisi = SanitizeInput(Divisi);

            if (targetRole == "OHS_ADMIN")
            {
                #region role ohs admin
                using (var package = new ExcelPackage())
                {

                    #region sheet mcu

                    var worksheet = package.Workbook.Worksheets.Add("MCU");

                    // Header Style Warna Biru
                    using (var range = worksheet.Cells["A1:H1"])
                    {
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                        range.Style.Font.Color.SetColor(Color.White);
                        range.Style.Font.Bold = true;
                    }

                    // Header Style Warna Merah
                    using (var range = worksheet.Cells["I1:Y1"])
                    {
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.Red);
                        range.Style.Font.Color.SetColor(Color.White);
                        range.Style.Font.Bold = true;
                    }

                    #region set header
                    // Set Header 
                    worksheet.Cells["A1"].Value = "No";
                    worksheet.Cells["A1:A2"].Merge = true;
                    worksheet.Cells["B1"].Value = "Paket";
                    worksheet.Cells["B1:B2"].Merge = true;
                    worksheet.Cells["C1"].Value = "Lokasi";
                    worksheet.Cells["C1:C2"].Merge = true;
                    worksheet.Cells["D1"].Value = "Nomor Register Karyawan";
                    worksheet.Cells["D1:D2"].Merge = true;
                    worksheet.Cells["E1"].Value = "Nama";
                    worksheet.Cells["E1:E2"].Merge = true;
                    worksheet.Cells["F1"].Value = "Divisi";
                    worksheet.Cells["F1:F2"].Merge = true;
                    worksheet.Cells["G1"].Value = "Tanggal Lahir";
                    worksheet.Cells["G1:G2"].Merge = true;
                    worksheet.Cells["G1:G2"].Merge = true;
                    worksheet.Cells["H1"].Value = "Usia";
                    worksheet.Cells["H1:H2"].Merge = true;
                    worksheet.Cells["I1"].Value = "Jenis Kelamin";
                    worksheet.Cells["I1:I2"].Merge = true;
                    worksheet.Cells["J1"].Value = "Penyakit Yang Pernah Diderita";
                    worksheet.Cells["J1:J2"].Merge = true;
                    worksheet.Cells["K1"].Value = "Kebiasaan Merokok";
                    worksheet.Cells["K1:L1"].Merge = true;
                    worksheet.Cells["K2"].Value = "Kebiasaan Merokok";
                    worksheet.Cells["L2"].Value = "Jumlah Batang/Hari";
                    worksheet.Cells["M1"].Value = "Kebiasaan Alkohol";
                    worksheet.Cells["M1:M2"].Merge = true;
                    worksheet.Cells["N1"].Value = "Kebiasaan Olahraga";
                    worksheet.Cells["N1:O1"].Merge = true;
                    worksheet.Cells["N2"].Value = "Kebiasaan Olahraga";
                    worksheet.Cells["O2"].Value = "Frekuensi/Minggu";
                    worksheet.Cells["P1:u1"].Merge = true;
                    worksheet.Cells["P1"].Value = "Pajanan Fisik";
                    worksheet.Cells["P2"].Value = "Kebisingan";
                    worksheet.Cells["Q2"].Value = "Suhu Extreme Panas/dingin";
                    worksheet.Cells["R2"].Value = "Radiasi";
                    worksheet.Cells["S2"].Value = "Getaran Lokal";
                    worksheet.Cells["T2"].Value = "Getaran Seluruh Tubuh";
                    worksheet.Cells["U2"].Value = "Lain-Lain";
                    worksheet.Cells["V1:Y1"].Merge = true;
                    worksheet.Cells["V1"].Value = "Pajanan Kimia";
                    worksheet.Cells["V2"].Value = "Debu";
                    worksheet.Cells["W2"].Value = "Asap";
                    worksheet.Cells["X2"].Value = "Limbah B3";
                    worksheet.Cells["Y2"].Value = "Bahan Kimia Berbahaya Lain";
                    worksheet.Cells["Z1:AA1"].Merge = true;
                    worksheet.Cells["Z1"].Value = "PAJANAN BIOLOGI";
                    worksheet.Cells["Z2"].Value = "Bakteri / Virus / Jamur / Parasit";
                    worksheet.Cells["AA2"].Value = "Lain-lain";
                    worksheet.Cells["AB1:AI1"].Merge = true;
                    worksheet.Cells["AB1"].Value = "PAJANAN ERGONOMIS";
                    worksheet.Cells["AB2"].Value = "Gerakan berulang dengan tangan";
                    worksheet.Cells["AC2"].Value = "Angkat / angkut berat";
                    worksheet.Cells["AD2"].Value = "Duduk lama > 4 jam terus menerus";
                    worksheet.Cells["AE2"].Value = "Berdiri lama > 4 jam terus menerus";
                    worksheet.Cells["AF2"].Value = "Posisi tubuh tidak ergonomis";
                    worksheet.Cells["AG2"].Value = "Pencahayaan tidak sesuai";
                    worksheet.Cells["AH2"].Value = "Bekerja dengan layar / monitor ≥ 4 jam/ hari";
                    worksheet.Cells["AI2"].Value = "Lain-lain";
                    worksheet.Cells["AJ1:AJ2"].Merge = true;
                    worksheet.Cells["AJ1"].Value = "RIWAYAT HIPERTENSI";
                    worksheet.Cells["AK1:AK2"].Merge = true;
                    worksheet.Cells["AK1"].Value = "RIWAYAT DIABETES";
                    worksheet.Cells["AL1:AL2"].Merge = true;
                    worksheet.Cells["AL1"].Value = "RIWAYAT PENYAKIT JANTUNG";
                    worksheet.Cells["AM1:AM2"].Merge = true;
                    worksheet.Cells["AM1"].Value = "RIWAYAT PENYAKIT GINJAL";
                    worksheet.Cells["AN1:AN2"].Merge = true;
                    worksheet.Cells["AN1"].Value = "RIWAYAT GANGGUAN MENTAL";
                    worksheet.Cells["AO1:AO2"].Merge = true;
                    worksheet.Cells["AO1"].Value = "RIWAYAT PENYAKIT LAIN";
                    worksheet.Cells["AP1:AP2"].Merge = true;
                    worksheet.Cells["AP1"].Value = "PENYAKIT YANG DIALAMI SAAT INI";
                    worksheet.Cells["AQ1:AQ2"].Merge = true;
                    worksheet.Cells["AQ1"].Value = "SEDANG BEROBAT";
                    worksheet.Cells["AR1:AR2"].Merge = true;
                    worksheet.Cells["AR1"].Value = "OBAT YANG DIBERIKAN";
                    worksheet.Cells["AS1:AS2"].Merge = true;
                    worksheet.Cells["AS1"].Value = "TINGGI BADAN  (cm)";
                    worksheet.Cells["AT1:AT2"].Merge = true;
                    worksheet.Cells["AT1"].Value = "BERAT BADAN (kg)";
                    worksheet.Cells["AU1:AU2"].Merge = true;
                    worksheet.Cells["AU1"].Value = "BMI";
                    worksheet.Cells["AV1:AV2"].Merge = true;
                    worksheet.Cells["AV1"].Value = "TEKANAN DARAH SISTOL";
                    worksheet.Cells["AW1:AW2"].Merge = true;
                    worksheet.Cells["AW1"].Value = "TEKANAN DARAH DIASTOL";
                    worksheet.Cells["AX1:AX2"].Merge = true;
                    worksheet.Cells["AX1"].Value = "KATEGORI TEKANAN DARAH";
                    worksheet.Cells["AY1:AY2"].Merge = true;
                    worksheet.Cells["AY1"].Value = "HASIL EKG";
                    worksheet.Cells["AZ1:AZ2"].Merge = true;
                    worksheet.Cells["AZ1"].Value = "KESIMPULAN EKG";

                    worksheet.Cells["BA1:BA2"].Merge = true;
                    worksheet.Cells["BA1"].Value = "KESIMPULAN TREADMILL";
                    worksheet.Cells["BB1:BB2"].Merge = true;
                    worksheet.Cells["BB1"].Value = "KESAN PHOTO RONTGEN";
                    worksheet.Cells["BC1:BC2"].Merge = true;
                    worksheet.Cells["BC1"].Value = "KESIMPULAN RONTGEN";
                    worksheet.Cells["BD1:BD2"].Merge = true;
                    worksheet.Cells["BD1"].Value = "KESIMPULAN USG ABDOMEN";
                    worksheet.Cells["BE1:BE2"].Merge = true;
                    worksheet.Cells["BE1"].Value = "HASIL USG MAMMAE";
                    worksheet.Cells["BF1:BF2"].Merge = true;
                    worksheet.Cells["BF1"].Value = "KESIMPULAN USG MAMMAE";
                    worksheet.Cells["BG1:BG2"].Merge = true;
                    worksheet.Cells["BG1"].Value = "KESIMPULAN PEMERIKSAAN FISIK";
                    worksheet.Cells["BH1:BH2"].Merge = true;
                    worksheet.Cells["BH1"].Value = "KESIMPULAN BUTA WARNA";
                    worksheet.Cells["BI1:BI2"].Merge = true;
                    worksheet.Cells["BI1"].Value = "KESIMPULAN PEM. VISUS MATA";
                    worksheet.Cells["BJ1:BJ2"].Merge = true;
                    worksheet.Cells["BJ1"].Value = "HASIL PAPSMEAR";
                    worksheet.Cells["BK1:BK2"].Merge = true;
                    worksheet.Cells["BK1"].Value = "KESIMPULAN PAPSMEAR";
                    worksheet.Cells["BL1:BL2"].Merge = true;
                    worksheet.Cells["BL1"].Value = "Hemoglobine (Male : N ….. g/dl; Female : …...g/dl)";
                    worksheet.Cells["BM1:BM2"].Merge = true;
                    worksheet.Cells["BM1"].Value = "Hematocrit \r\n(Male : N ….. g/dl; Female : …...g/dl)";
                    worksheet.Cells["BN1:BN2"].Merge = true;
                    worksheet.Cells["BN1"].Value = "Leucocyte Count/ WBC \r\n(Male : N ….. g/dl; Female : …...g/dl)";
                    worksheet.Cells["BO1:BO2"].Merge = true;
                    worksheet.Cells["BO1"].Value = "Total Platelets Count \r\nNilai normal : ... K/uL";
                    worksheet.Cells["BP1:BP2"].Merge = true;
                    worksheet.Cells["BP1"].Value = "Eryrocyte Count/ RBC Nilai normal : ...  M/uL";
                    worksheet.Cells["BQ1:BQ2"].Merge = true;
                    worksheet.Cells["BQ1"].Value = "MCV \r\n(range normal)";
                    worksheet.Cells["BR1:BR2"].Merge = true;
                    worksheet.Cells["BR1"].Value = "MCH \r\n(range normal)";
                    worksheet.Cells["BS1:BS2"].Merge = true;
                    worksheet.Cells["BS1"].Value = "MCHC \r\n(range normal)";
                    worksheet.Cells["BT1:BT2"].Merge = true;
                    worksheet.Cells["BT1"].Value = "ESR/\r\nEryrocyte Sedimen\r\n(range normal) ";
                    worksheet.Cells["BU1:BU2"].Merge = true;
                    worksheet.Cells["BU1"].Value = "Glucose Fasting(range normal)L";
                    worksheet.Cells["BV1:BV2"].Merge = true;
                    worksheet.Cells["BV1"].Value = "HbA1c\r\n(range normal)";
                    worksheet.Cells["BW1:BW2"].Merge = true;
                    worksheet.Cells["BW1"].Value = "SGOT (range normal)";
                    worksheet.Cells["BX1:BX2"].Merge = true;
                    worksheet.Cells["BX1"].Value = "SGPT \r\n(range normal)";
                    worksheet.Cells["BY1:BY2"].Merge = true;
                    worksheet.Cells["BY1"].Value = "Gamma GT\r\n(range normal)";
                    worksheet.Cells["BZ1:BZ2"].Merge = true;
                    worksheet.Cells["BZ1"].Value = "HbsAg";
                    worksheet.Cells["CA1:CA2"].Merge = true;
                    worksheet.Cells["CA1"].Value = "Ureum\r\n(range normal)";
                    worksheet.Cells["CB1:CB2"].Merge = true;
                    worksheet.Cells["CB1"].Value = "Creatinin\r\n(range normal)";
                    worksheet.Cells["CC1:CC2"].Merge = true;
                    worksheet.Cells["CC1"].Value = "Uric Acid\r\n(range normal) ";
                    worksheet.Cells["CD1:CD2"].Merge = true;
                    worksheet.Cells["CD1"].Value = "GFR";
                    worksheet.Cells["CE1:CE2"].Merge = true;
                    worksheet.Cells["CE1"].Value = "Cholesterol Total \r\n(range normal)";
                    worksheet.Cells["CF1:CF2"].Merge = true;
                    worksheet.Cells["CF1"].Value = "HDL \r\n(range normal)";
                    worksheet.Cells["CG1:CG2"].Merge = true;
                    worksheet.Cells["CG1"].Value = "LDL \r\n(range normal)";
                    worksheet.Cells["CH1:CH2"].Merge = true;
                    worksheet.Cells["CH1"].Value = "Triglyceride \r\n(range normal)";
                    worksheet.Cells["CI1:CI2"].Merge = true;
                    worksheet.Cells["CI1"].Value = "PLATELET AGGREGATION\r\n(range normal)";
                    worksheet.Cells["CJ1:CJ2"].Merge = true;
                    worksheet.Cells["CJ1"].Value = "Fibrinogen\r\n(range normal)";
                    worksheet.Cells["CK1:CK2"].Merge = true;
                    worksheet.Cells["CK1"].Value = "CEA\r\n(range normal)";
                    worksheet.Cells["CL1:CL2"].Merge = true;
                    worksheet.Cells["CL1"].Value = "PSA\r\n(range normal)";
                    worksheet.Cells["CM1:CM2"].Merge = true;
                    worksheet.Cells["CM1"].Value = "Ca-125\r\n(range normal)";
                    worksheet.Cells["CN1:CN2"].Merge = true;
                    worksheet.Cells["CN1"].Value = "Vit D25-OH\r\n(range normal)";
                    worksheet.Cells["CN1:CN2"].Merge = true;
                    worksheet.Cells["CN1"].Value = "Vit D25-OH\r\n(range normal)";
                    worksheet.Cells["CO1:CR1"].Merge = true;
                    worksheet.Cells["CO1"].Value = "Kesimpulan urinalisis";
                    worksheet.Cells["CO2"].Value = "Darah (Makro / Mikro)";
                    worksheet.Cells["CP2"].Value = "Bakteri";
                    worksheet.Cells["CQ2"].Value = "Kristal";
                    worksheet.Cells["CR2"].Value = "Leukosit";
                    worksheet.Cells["CS1:CS2"].Merge = true;
                    worksheet.Cells["CS1"].Value = "Score Role Ambiguity";

                    worksheet.Cells["CT1:CT2"].Merge = true;
                    worksheet.Cells["CT1"].Value = "Kesimpulan Role Ambiguity";
                    worksheet.Cells["CU1:CU2"].Merge = true;
                    worksheet.Cells["CU1"].Value = "Score Role Conflict";

                    worksheet.Cells["CV1:CV2"].Merge = true;
                    worksheet.Cells["CV1"].Value = "Kesimpulan Role Conflict";

                    worksheet.Cells["CW1:CW2"].Merge = true;
                    worksheet.Cells["CW1"].Value = "Score Role Overload Quantitative";

                    worksheet.Cells["CX1:CX2"].Merge = true;
                    worksheet.Cells["CX1"].Value = "Kesimpulan Role Overload Quantitative";

                    worksheet.Cells["CY1:CY2"].Merge = true;
                    worksheet.Cells["CY1"].Value = "Score Role Overload Qualitative";

                    worksheet.Cells["CZ1:CZ2"].Merge = true;
                    worksheet.Cells["CZ1"].Value = "Kesimpulan Role Overload Qualitative";

                    worksheet.Cells["DA1:DA2"].Merge = true;
                    worksheet.Cells["DA1"].Value = "Score Career Development";

                    worksheet.Cells["DB1:DB2"].Merge = true;
                    worksheet.Cells["DB1"].Value = "Kesimpulan Career Development";

                    worksheet.Cells["DC1:DC2"].Merge = true;
                    worksheet.Cells["DC1"].Value = "Score Responsibility for People";

                    worksheet.Cells["DD1:DD2"].Merge = true;
                    worksheet.Cells["DD1"].Value = "Kesimpulan Responsibility for People";

                    worksheet.Cells["DE1:DE2"].Merge = true;
                    worksheet.Cells["DE1"].Value = "KESIMPULAN LABORATORIUM";

                    worksheet.Cells["DF1:DF2"].Merge = true;
                    worksheet.Cells["DF1"].Value = "SARAN/ANJURAN";

                    worksheet.Cells["DG1:DG2"].Merge = true;
                    worksheet.Cells["DG1"].Value = "STATUS HASIL MCU/ FITNESS TO WORK";

                    #endregion

                    #region styling header
                    // Styling
                    worksheet.Cells["A1:DG2"].Style.Fill.PatternType = ExcelFillStyle.Solid;

                    worksheet.Cells["B1"].Style.Fill.BackgroundColor.SetColor(Color.Red);
                    worksheet.Cells["I1"].Style.Fill.BackgroundColor.SetColor(Color.Red);
                    worksheet.Cells["AQ1"].Style.Fill.BackgroundColor.SetColor(Color.Red);
                    worksheet.Cells["K1:AN2"].Style.Fill.BackgroundColor.SetColor(Color.Red);
                    worksheet.Cells["AX1"].Style.Fill.BackgroundColor.SetColor(Color.Red);
                    worksheet.Cells["AZ1"].Style.Fill.BackgroundColor.SetColor(Color.Red);
                    worksheet.Cells["BC1"].Style.Fill.BackgroundColor.SetColor(Color.Red);
                    worksheet.Cells["BF1"].Style.Fill.BackgroundColor.SetColor(Color.Red);
                    worksheet.Cells["BH1:BI2"].Style.Fill.BackgroundColor.SetColor(Color.Red);
                    worksheet.Cells["BK1"].Style.Fill.BackgroundColor.SetColor(Color.Red);
                    worksheet.Cells["BZ1"].Style.Fill.BackgroundColor.SetColor(Color.Red);
                    worksheet.Cells["CO1:CR2"].Style.Fill.BackgroundColor.SetColor(Color.Red);
                    worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                    worksheet.Cells["J1"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                    worksheet.Cells["C1:H2"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                    worksheet.Cells["AO1:AP2"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                    worksheet.Cells["AR1:AW2"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                    worksheet.Cells["BA1:BB2"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                    worksheet.Cells["BD1:BE2"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                    worksheet.Cells["BG1"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                    worksheet.Cells["BJ1"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                    worksheet.Cells["BL1:BY2"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                    worksheet.Cells["CA1:CN2"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                    worksheet.Cells["CS1:DG2"].Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);

                    worksheet.Cells["A1:DG2"].Style.Font.Color.SetColor(Color.White);
                    worksheet.Cells["A1:DG2"].Style.Font.Bold = true;
                    worksheet.Cells["A1:DG2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells["A1:DG2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    var borderx = worksheet.Cells["A1:DG2"];
                    borderx.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    borderx.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    borderx.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    borderx.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    borderx.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    borderx.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    borderx.Style.Font.Bold = true;
                    #endregion

                    #region data

                    var request = new DataSourceRequest();

                    var output = MedicalRecordService.GetdataMcu(startDate, endDate, null, "", Noreg, NamakaryawanTam, Divisi);
                    int rowIndex = 3;
                    int normerurut = 1;
                    foreach (var data in output)
                    {
                        worksheet.Cells[rowIndex, 1].Value = normerurut;
                        worksheet.Cells[rowIndex, 2].Value = data.Paket;
                        worksheet.Cells[rowIndex, 3].Value = data.LokasiMCU;
                        worksheet.Cells[rowIndex, 4].Value = data.NoReg;
                        worksheet.Cells[rowIndex, 5].Value = data.Name;
                        worksheet.Cells[rowIndex, 6].Value = data.Divisi;
                        worksheet.Cells[rowIndex, 7].Value = data.Tanggal_Lahir?.ToString("dd/MM/yyyy");
                        worksheet.Cells[rowIndex, 8].Value = data.Usia;
                        worksheet.Cells[rowIndex, 9].Value = data.Jenis_Kelamin;
                        worksheet.Cells[rowIndex, 10].Value = data.PenyakitPernahDiderita;

                        worksheet.Cells[rowIndex, 11].Value = data.Perokok;
                        worksheet.Cells[rowIndex, 12].Value = data.JumlahBatangPerHari;
                        worksheet.Cells[rowIndex, 13].Value = data.Miras;
                        worksheet.Cells[rowIndex, 14].Value = data.Olahraga;
                        worksheet.Cells[rowIndex, 15].Value = data.FrequencyPerMinggu;
                        worksheet.Cells[rowIndex, 16].Value = data.Kebisingan;
                        worksheet.Cells[rowIndex, 17].Value = data.SuhuExtremePanasAtauDingin;
                        worksheet.Cells[rowIndex, 18].Value = data.Radiasi;
                        worksheet.Cells[rowIndex, 19].Value = data.GetaranLokal;
                        worksheet.Cells[rowIndex, 20].Value = data.GetaranSeluruhTubuh;

                        worksheet.Cells[rowIndex, 21].Value = data.LainnyaFisika;
                        worksheet.Cells[rowIndex, 22].Value = data.Debu;
                        worksheet.Cells[rowIndex, 23].Value = data.Asap;
                        worksheet.Cells[rowIndex, 24].Value = data.LimbahB3;
                        worksheet.Cells[rowIndex, 25].Value = data.LainnyaKimia;
                        worksheet.Cells[rowIndex, 26].Value = data.BakteriAtaVirusAtauJamurAtauParasit;
                        worksheet.Cells[rowIndex, 27].Value = data.LainnyaBiologi;
                        worksheet.Cells[rowIndex, 28].Value = data.GerakanBerulangDenganTangan;
                        worksheet.Cells[rowIndex, 29].Value = data.AngkatBerat;
                        worksheet.Cells[rowIndex, 30].Value = data.DudukLama;

                        worksheet.Cells[rowIndex, 31].Value = data.BerdiriLama;
                        worksheet.Cells[rowIndex, 32].Value = data.PosisiTubuhTidakErgonomis;
                        worksheet.Cells[rowIndex, 33].Value = data.PencahayaanTidakSesuai;
                        worksheet.Cells[rowIndex, 34].Value = data.BekerjaDepanLayar;
                        worksheet.Cells[rowIndex, 35].Value = data.LainnyaErgonomis;
                        worksheet.Cells[rowIndex, 36].Value = data.RiwayatHipertensi;
                        worksheet.Cells[rowIndex, 37].Value = data.RiwayatDiabetes;
                        worksheet.Cells[rowIndex, 38].Value = data.RiwayatPenyakitJantung;
                        worksheet.Cells[rowIndex, 39].Value = data.RiwayatPenyakitGinjal;
                        worksheet.Cells[rowIndex, 40].Value = data.RiwayatGangguanMental;

                        worksheet.Cells[rowIndex, 41].Value = data.RiwayatPenyakitLain;
                        worksheet.Cells[rowIndex, 42].Value = data.PenyakitSaatIni;
                        worksheet.Cells[rowIndex, 43].Value = data.SedangBerobat;
                        worksheet.Cells[rowIndex, 44].Value = data.ObatYangDiberikan;
                        worksheet.Cells[rowIndex, 45].Value = data.Tinggi;
                        worksheet.Cells[rowIndex, 46].Value = data.Berat;
                        worksheet.Cells[rowIndex, 47].Value = data.StatusBMI;
                        worksheet.Cells[rowIndex, 48].Value = data.TekananDarahSistol;
                        worksheet.Cells[rowIndex, 49].Value = data.TekananDarahDiastol;
                        worksheet.Cells[rowIndex, 50].Value = data.TekananDarah;

                        worksheet.Cells[rowIndex, 51].Value = data.HasilEKG;
                        worksheet.Cells[rowIndex, 52].Value = data.KesimpulanEKG;
                        worksheet.Cells[rowIndex, 53].Value = data.KesimpulanTreadmill;
                        worksheet.Cells[rowIndex, 54].Value = data.KesanPhotoRontgen;
                        worksheet.Cells[rowIndex, 55].Value = data.HasilRontgen;
                        worksheet.Cells[rowIndex, 56].Value = data.KesimpulanUsgAbdomen;
                        worksheet.Cells[rowIndex, 57].Value = data.HasilUsgMammae;
                        worksheet.Cells[rowIndex, 58].Value = data.KesimpulanUsgMammae;
                        worksheet.Cells[rowIndex, 59].Value = data.KesimpulanFisik;
                        worksheet.Cells[rowIndex, 60].Value = data.KesimpulanButaWarna;

                        worksheet.Cells[rowIndex, 61].Value = data.KesimpulanPemVisusMata;
                        worksheet.Cells[rowIndex, 62].Value = data.HasilPapsmear;
                        worksheet.Cells[rowIndex, 63].Value = data.KesimpulanPamsmear;
                        worksheet.Cells[rowIndex, 64].Value = data.Hemoglobine;
                        worksheet.Cells[rowIndex, 65].Value = data.Hematocrit;
                        worksheet.Cells[rowIndex, 66].Value = data.Leucocyte;
                        worksheet.Cells[rowIndex, 67].Value = data.TotalPlatelets;
                        worksheet.Cells[rowIndex, 68].Value = data.Eryrocyte;
                        worksheet.Cells[rowIndex, 69].Value = data.MCV;
                        worksheet.Cells[rowIndex, 70].Value = data.MCH;

                        worksheet.Cells[rowIndex, 71].Value = data.MCHC;
                        worksheet.Cells[rowIndex, 72].Value = data.ESR;
                        worksheet.Cells[rowIndex, 73].Value = data.GlukosaPuasa;
                        worksheet.Cells[rowIndex, 74].Value = data.HbA1c;
                        worksheet.Cells[rowIndex, 75].Value = data.SGOT;
                        worksheet.Cells[rowIndex, 76].Value = data.SGPT;
                        worksheet.Cells[rowIndex, 77].Value = data.GammaGT;
                        worksheet.Cells[rowIndex, 78].Value = data.HBsAg;
                        worksheet.Cells[rowIndex, 79].Value = data.Ureum;
                        worksheet.Cells[rowIndex, 80].Value = data.Kreatinin;
                        worksheet.Cells[rowIndex, 81].Value = data.AsamUrat;

                        worksheet.Cells[rowIndex, 82].Value = data.GFR;
                        worksheet.Cells[rowIndex, 83].Value = data.KolesterolTotal;
                        worksheet.Cells[rowIndex, 84].Value = data.HDL;
                        worksheet.Cells[rowIndex, 85].Value = data.LDL;
                        worksheet.Cells[rowIndex, 86].Value = data.Triglyceride;
                        worksheet.Cells[rowIndex, 87].Value = data.PlateletAggregation;
                        worksheet.Cells[rowIndex, 88].Value = data.Fibrinogen;
                        worksheet.Cells[rowIndex, 89].Value = data.CEA;
                        worksheet.Cells[rowIndex, 90].Value = data.PSA;
                        worksheet.Cells[rowIndex, 91].Value = data.Ca125;

                        worksheet.Cells[rowIndex, 92].Value = data.VitD25OH;
                        worksheet.Cells[rowIndex, 93].Value = data.UrinDarah;
                        worksheet.Cells[rowIndex, 94].Value = data.UrinBakteri;
                        worksheet.Cells[rowIndex, 95].Value = data.UrinKristal;
                        worksheet.Cells[rowIndex, 96].Value = data.UrinLeukosit;
                        worksheet.Cells[rowIndex, 97].Value = data.ScoreAmbiguity;
                        worksheet.Cells[rowIndex, 98].Value = data.KesimpulanAmbiguity;
                        worksheet.Cells[rowIndex, 99].Value = data.ScoreConflict;
                        worksheet.Cells[rowIndex, 100].Value = data.KesimpulanConflict;
                        worksheet.Cells[rowIndex, 101].Value = data.ScoreQuantitative;
                        worksheet.Cells[rowIndex, 102].Value = data.KesimpulanQuantitative;

                        worksheet.Cells[rowIndex, 103].Value = data.ScoreQualitative;
                        worksheet.Cells[rowIndex, 104].Value = data.KesimpulanQualitative;
                        worksheet.Cells[rowIndex, 105].Value = data.ScoreCareerDevelopment;
                        worksheet.Cells[rowIndex, 106].Value = data.KesimpulanCareerDevelopment;
                        worksheet.Cells[rowIndex, 107].Value = data.ScoreResponsibilityforPeople;
                        worksheet.Cells[rowIndex, 108].Value = data.KesimpulanResponsibilityforPeople;
                        worksheet.Cells[rowIndex, 109].Value = data.KesimpulanLab;
                        worksheet.Cells[rowIndex, 110].Value = data.Saran;
                        worksheet.Cells[rowIndex, 111].Value = data.HealthEmployeeStatus;



                        for (var i = 1; i <= 111; i++)
                        {
                            worksheet.Cells[rowIndex, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }

                        rowIndex++;
                        normerurut++;
                    }

                    #endregion

                    // Auto Fit Kolom
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                    #endregion

                    #region sheet sickleave
                    var worksheetSick = package.Workbook.Worksheets.Add("Sick Leave");
                    #region set header

                    worksheetSick.Cells["A1"].Value = "No";
                    worksheetSick.Cells["B1"].Value = "NoReg";
                    worksheetSick.Cells["C1"].Value = "Name";
                    worksheetSick.Cells["D1"].Value = "Start Date";
                    worksheetSick.Cells["E1"].Value = "End Date";
                    worksheetSick.Cells["F1"].Value = "Type";
                    worksheetSick.Cells["G1"].Value = "Kategori Penyakt";
                    worksheetSick.Cells["H1"].Value = "Penyakit";
                    worksheetSick.Cells["I1"].Value = "Total Absen";
                    worksheetSick.Cells["J1"].Value = "Diagnosis";

                    // Styling Header 
                    using (var range = worksheetSick.Cells["A1:J1"])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }

                    #region data
                    var outputSick = MedicalRecordService.GetdataSick(startDate, endDate, null, "", Noreg, NamakaryawanTam, Divisi);
                    int rowIndexSick = 2;
                    int normerurutSick = 1;
                    foreach (var data in outputSick)
                    {
                        worksheetSick.Cells[rowIndexSick, 1].Value = normerurutSick;
                        worksheetSick.Cells[rowIndexSick, 2].Value = data.NoReg;
                        worksheetSick.Cells[rowIndexSick, 3].Value = data.Name;
                        worksheetSick.Cells[rowIndexSick, 4].Value = data.StartDate.ToString("dd/MM/yyyy");
                        worksheetSick.Cells[rowIndexSick, 5].Value = data.EndDate.ToString("dd/MM/yyyy"); ;
                        worksheetSick.Cells[rowIndexSick, 6].Value = data.ReasonCode;
                        worksheetSick.Cells[rowIndexSick, 7].Value = data.KategoriPenyakit;
                        worksheetSick.Cells[rowIndexSick, 8].Value = data.SpesifikPenyakit;
                        worksheetSick.Cells[rowIndexSick, 9].Value = data.AbsentDuration;
                        worksheetSick.Cells[rowIndexSick, 10].Value = data.Description;
                        for (var i = 1; i <= 10; i++)
                        {
                            worksheetSick.Cells[rowIndexSick, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }

                        rowIndexSick++;
                        normerurutSick++;

                    }
                    #endregion

                    worksheetSick.Cells[worksheetSick.Dimension.Address].AutoFitColumns();
                    #endregion
                    #endregion

                    #region Sheet UPKK
                    var sheetUPKK = package.Workbook.Worksheets.Add("UPKK Visit");

                    #region tabel all
                    // Merge Header tabel ALL COLOM
                    sheetUPKK.Cells["A1:A2"].Merge = true;
                    sheetUPKK.Cells["B1:B2"].Merge = true;
                    sheetUPKK.Cells["C1:C2"].Merge = true;
                    sheetUPKK.Cells["D1:D2"].Merge = true;
                    sheetUPKK.Cells["E1:E2"].Merge = true;
                    sheetUPKK.Cells["F1:F2"].Merge = true;
                    sheetUPKK.Cells["G1:G2"].Merge = true;
                    sheetUPKK.Cells["H1:H2"].Merge = true;
                    sheetUPKK.Cells["I1:I2"].Merge = true;
                    sheetUPKK.Cells["J1:J2"].Merge = true;
                    sheetUPKK.Cells["K1:K2"].Merge = true;
                    sheetUPKK.Cells["L1:L2"].Merge = true;
                    sheetUPKK.Cells["M1:Q1"].Merge = true;
                    sheetUPKK.Cells["R1:R2"].Merge = true;
                    sheetUPKK.Cells["S1:S2"].Merge = true;
                    sheetUPKK.Cells["T1:T2"].Merge = true;
                    sheetUPKK.Cells["U1:U2"].Merge = true;
                    sheetUPKK.Cells["V1:V2"].Merge = true;
                    sheetUPKK.Cells["W1:W2"].Merge = true;
                    sheetUPKK.Cells["X1:X2"].Merge = true;
                    sheetUPKK.Cells["Y1:Y2"].Merge = true;

                    // Set Header Text tabel ALL COLOM

                    sheetUPKK.Cells["A1"].Value = "Lokasi UPKK";
                    sheetUPKK.Cells["B1"].Value = "Kategori Kunjungan \r\n(Berobat / Pemeriksaan Kesehatan)";
                    sheetUPKK.Cells["C1"].Value = "Tanggal Kunjungan";
                    sheetUPKK.Cells["D1"].Value = "Company";
                    sheetUPKK.Cells["E1"].Value = "Divisi";
                    sheetUPKK.Cells["F1"].Value = "Noreg";
                    sheetUPKK.Cells["G1"].Value = "Nama";
                    sheetUPKK.Cells["H1"].Value = "Umur";
                    sheetUPKK.Cells["I1"].Value = "Jenis Kelamin";
                    sheetUPKK.Cells["J1"].Value = "Jenis Pekerjaan";
                    sheetUPKK.Cells["K1"].Value = "Lokasi Kerja";
                    sheetUPKK.Cells["L1"].Value = "Keluhan";
                    sheetUPKK.Cells["M1"].Value = "Pemeriksaan";
                    sheetUPKK.Cells["M2"].Value = "TD Sistole";
                    sheetUPKK.Cells["N2"].Value = "TD Diastole";
                    sheetUPKK.Cells["O2"].Value = "Nadi";
                    sheetUPKK.Cells["P2"].Value = "Respirasi";
                    sheetUPKK.Cells["Q2"].Value = "Suhu";
                    sheetUPKK.Cells["R1"].Value = "Diagnosa";
                    sheetUPKK.Cells["S1"].Value = "Kategori Penyakit";
                    sheetUPKK.Cells["T1"].Value = "Spesifik Penyakit";
                    sheetUPKK.Cells["U1"].Value = "Jenis Kasus";
                    sheetUPKK.Cells["V1"].Value = "Treatment (Nama Obat)";
                    sheetUPKK.Cells["W1"].Value = "Pemeriksa\r\n(Nurse / Doctor)";
                    sheetUPKK.Cells["X1"].Value = "Nama Pemeriksa\r\n(Nurse / Doctor)";
                    sheetUPKK.Cells["Y1"].Value = "Hasil Akhir";

                    // Styling Header 
                    using (var range = sheetUPKK.Cells["A1:Y2"])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#D9EAD3"));
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }
                    #region data
                    var outputupkkAll = MedicalRecordService.GetdataUpkkAll(startDate, endDate, lahirDate, namaKaryawanVendor, Noreg, NamakaryawanTam, Divisi);
                    int rowIndexUpkkALL = 3;

                    foreach (var data in outputupkkAll)
                    {
                        sheetUPKK.Cells[rowIndexUpkkALL, 1].Value = data.LokasiUPKK;
                        sheetUPKK.Cells[rowIndexUpkkALL, 2].Value = data.KategoriKunjungan;
                        sheetUPKK.Cells[rowIndexUpkkALL, 3].Value = data.TanggalKunjungan?.ToString("dd/MM/yyyy");
                        sheetUPKK.Cells[rowIndexUpkkALL, 4].Value = data.Company;
                        sheetUPKK.Cells[rowIndexUpkkALL, 5].Value = data.Divisi;
                        sheetUPKK.Cells[rowIndexUpkkALL, 6].Value = data.Noreg;
                        sheetUPKK.Cells[rowIndexUpkkALL, 7].Value = data.NamaEmployeeVendor;
                        sheetUPKK.Cells[rowIndexUpkkALL, 8].Value = data.Usia;
                        sheetUPKK.Cells[rowIndexUpkkALL, 9].Value = data.JenisKelaminEmployeeVendor;
                        sheetUPKK.Cells[rowIndexUpkkALL, 11].Value = data.LokasiKerja;
                        sheetUPKK.Cells[rowIndexUpkkALL, 12].Value = data.Keluhan;
                        sheetUPKK.Cells[rowIndexUpkkALL, 13].Value = data.TDSistole;
                        sheetUPKK.Cells[rowIndexUpkkALL, 14].Value = data.TDDiastole;
                        sheetUPKK.Cells[rowIndexUpkkALL, 15].Value = data.Nadi;
                        sheetUPKK.Cells[rowIndexUpkkALL, 16].Value = data.Respirasi;
                        sheetUPKK.Cells[rowIndexUpkkALL, 17].Value = data.Suhu;
                        sheetUPKK.Cells[rowIndexUpkkALL, 18].Value = data.Diagnosa;
                        sheetUPKK.Cells[rowIndexUpkkALL, 19].Value = data.KategoriPenyakit;
                        sheetUPKK.Cells[rowIndexUpkkALL, 20].Value = data.SpesifikPenyakit;
                        sheetUPKK.Cells[rowIndexUpkkALL, 21].Value = data.JenisKasus;
                        sheetUPKK.Cells[rowIndexUpkkALL, 22].Value = data.Treatment;
                        sheetUPKK.Cells[rowIndexUpkkALL, 23].Value = data.Pemeriksa;
                        sheetUPKK.Cells[rowIndexUpkkALL, 24].Value = data.NamaPemeriksa;
                        sheetUPKK.Cells[rowIndexUpkkALL, 25].Value = data.HasilAkhir;

                        for (var i = 1; i <= 25; i++)
                        {
                            sheetUPKK.Cells[rowIndexUpkkALL, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }

                        rowIndexUpkkALL++;


                    }
                    #endregion

                    #endregion

                    #region table 1
                    // Merge Header tabel 1
                    sheetUPKK.Cells[rowIndexUpkkALL + 4, 3, rowIndexUpkkALL + 4, 24].Merge = true; // KLINIK/UPKK
                    sheetUPKK.Cells[rowIndexUpkkALL + 4, 1, rowIndexUpkkALL + 4 + 2, 1].Merge = true; // UPKK VISIT
                    sheetUPKK.Cells[rowIndexUpkkALL + 4, 2, rowIndexUpkkALL + 4 + 2, 2].Merge = true; // TOTAL
                    sheetUPKK.Cells[rowIndexUpkkALL + 5, 3, rowIndexUpkkALL + 5, 4].Merge = true; // UPKK Sunter 2 - TSD
                    sheetUPKK.Cells[rowIndexUpkkALL + 5, 5, rowIndexUpkkALL + 5, 6].Merge = true; // UPKK SUNTER 2 - PPDD
                    sheetUPKK.Cells[rowIndexUpkkALL + 5, 7, rowIndexUpkkALL + 5, 8].Merge = true; // UPKK SUNTER 2 HRGA
                    sheetUPKK.Cells[rowIndexUpkkALL + 5, 9, rowIndexUpkkALL + 5, 10].Merge = true; // UPKK LEXUS - LNTC
                    sheetUPKK.Cells[rowIndexUpkkALL + 5, 11, rowIndexUpkkALL + 5, 12].Merge = true; // UPKK LEXUS - MENTENG
                    sheetUPKK.Cells[rowIndexUpkkALL + 5, 13, rowIndexUpkkALL + 5, 14].Merge = true; // UPKK SPLD
                    sheetUPKK.Cells[rowIndexUpkkALL + 5, 15, rowIndexUpkkALL + 5, 16].Merge = true; // UPKK TTC
                    sheetUPKK.Cells[rowIndexUpkkALL + 5, 17, rowIndexUpkkALL + 5, 18].Merge = true; // UPKK Sunter 3
                    sheetUPKK.Cells[rowIndexUpkkALL + 5, 19, rowIndexUpkkALL + 5, 20].Merge = true; // UPKK CCY
                    sheetUPKK.Cells[rowIndexUpkkALL + 5, 21, rowIndexUpkkALL + 5, 22].Merge = true; // UPKK Karawang
                    sheetUPKK.Cells[rowIndexUpkkALL + 5, 23, rowIndexUpkkALL + 5, 24].Merge = true; // UPKK Ngoro


                    int startColumn = 3; // Kolom C di Excel dimulai dari indeks 3
                    int rowTAM = rowIndexUpkkALL + 6; // Sesuaikan baris sesuai struktur tabel

                    for (int i = 0; i < 22; i += 2) // 22 kolom TAM & Outsource
                    {
                        sheetUPKK.Cells[rowTAM, startColumn + i].Value = "TAM";
                        sheetUPKK.Cells[rowTAM, startColumn + i + 1].Value = "Outsource";
                    }



                    // Set Header Text tabel 1
                    sheetUPKK.Cells[rowIndexUpkkALL + 4, 3].Value = "KLINIK / UPKK";
                    sheetUPKK.Cells[rowIndexUpkkALL + 4, 1].Value = "UPKK VISIT";
                    sheetUPKK.Cells[rowIndexUpkkALL + 4, 2].Value = "TOTAL";
                    sheetUPKK.Cells[rowIndexUpkkALL + 5, 3].Value = "UPKK Sunter 2 - TSD";
                    sheetUPKK.Cells[rowIndexUpkkALL + 5, 5].Value = "UPKK SUNTER 2 - PPDD";
                    sheetUPKK.Cells[rowIndexUpkkALL + 5, 7].Value = "UPKK SUNTER 2 HRGA";
                    sheetUPKK.Cells[rowIndexUpkkALL + 5, 9].Value = "UPKK LEXUS - LNTC";
                    sheetUPKK.Cells[rowIndexUpkkALL + 5, 11].Value = "UPKK LEXUS - MENTENG";
                    sheetUPKK.Cells[rowIndexUpkkALL + 5, 13].Value = "UPKK SPLD";
                    sheetUPKK.Cells[rowIndexUpkkALL + 5, 15].Value = "UPKK TTC";
                    sheetUPKK.Cells[rowIndexUpkkALL + 5, 17].Value = "UPKK Sunter 3";
                    sheetUPKK.Cells[rowIndexUpkkALL + 5, 19].Value = "UPKK CCY";
                    sheetUPKK.Cells[rowIndexUpkkALL + 5, 21].Value = "UPKK Karawang";
                    sheetUPKK.Cells[rowIndexUpkkALL + 5, 23].Value = "UPKK Ngoro";

                    // Data Sub-header tabel 1
                    sheetUPKK.Cells[rowIndexUpkkALL + 7, 1].Value = "Visit Number";
                    sheetUPKK.Cells[rowIndexUpkkALL + 8, 1].Value = "Total Visitor";

                    // Styling Header tabel 1
                    using (var range = sheetUPKK.Cells[rowIndexUpkkALL + 4, 1, rowIndexUpkkALL + 6, 24])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                        //range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        //range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        //range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        //range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }

                    //// Border untuk tabel 1
                    using (var range = sheetUPKK.Cells[rowIndexUpkkALL + 4, 1, rowIndexUpkkALL + 8, 24])
                    {
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }
                    #region REMARK data CARA LINQ
                    //#region data tabel 1
                    ////DATA 1
                    //var outputUpkk1 = MedicalRecordService.GetdataUpkk1(startDate, endDate, Noreg);
                    //int rowIndexUPKK = rowIndexUpkkALL + 7;
                    //foreach (var data in outputUpkk1)
                    //{
                    //    sheetUPKK.Cells[rowIndexUPKK, 3].Value = data.TAM;
                    //    sheetUPKK.Cells[rowIndexUPKK, 4].Value = data.Outsource;
                    //    rowIndexUPKK++;
                    //}
                    ////DATA 2
                    //var outputUpkk2 = MedicalRecordService.GetdataUpkk2(startDate, endDate, Noreg);
                    //rowIndexUPKK = rowIndexUpkkALL + 7;
                    //foreach (var data in outputUpkk2)
                    //{
                    //    sheetUPKK.Cells[rowIndexUPKK, 5].Value = data.TAM;
                    //    sheetUPKK.Cells[rowIndexUPKK, 6].Value = data.Outsource;
                    //    rowIndexUPKK++;
                    //}
                    ////DATA 3
                    //var outputUpkk3 = MedicalRecordService.GetdataUpkk3(startDate, endDate, Noreg);
                    //rowIndexUPKK = rowIndexUpkkALL + 7;
                    //foreach (var data in outputUpkk3)
                    //{
                    //    sheetUPKK.Cells[rowIndexUPKK, 7].Value = data.TAM;
                    //    sheetUPKK.Cells[rowIndexUPKK, 8].Value = data.Outsource;
                    //    rowIndexUPKK++;
                    //}
                    ////DATA 4
                    //var outputUpkk4 = MedicalRecordService.GetdataUpkk4(startDate, endDate, Noreg);
                    //rowIndexUPKK = rowIndexUpkkALL + 7;
                    //foreach (var data in outputUpkk4)
                    //{
                    //    sheetUPKK.Cells[rowIndexUPKK, 9].Value = data.TAM;
                    //    sheetUPKK.Cells[rowIndexUPKK, 10].Value = data.Outsource;
                    //    rowIndexUPKK++;
                    //}
                    ////DATA 5
                    //var outputUpkk5 = MedicalRecordService.GetdataUpkk5(startDate, endDate, Noreg);
                    //rowIndexUPKK = rowIndexUpkkALL + 7;
                    //foreach (var data in outputUpkk5)
                    //{
                    //    sheetUPKK.Cells[rowIndexUPKK, 11].Value = data.TAM;
                    //    sheetUPKK.Cells[rowIndexUPKK, 12].Value = data.Outsource;
                    //    rowIndexUPKK++;
                    //}
                    ////DATA 6
                    //var outputUpkk6 = MedicalRecordService.GetdataUpkk6(startDate, endDate, Noreg);
                    //rowIndexUPKK = rowIndexUpkkALL + 7;
                    //foreach (var data in outputUpkk6)
                    //{
                    //    sheetUPKK.Cells[rowIndexUPKK, 13].Value = data.TAM;
                    //    sheetUPKK.Cells[rowIndexUPKK, 14].Value = data.Outsource;
                    //    rowIndexUPKK++;
                    //}
                    ////DATA 7
                    //var outputUpkk7 = MedicalRecordService.GetdataUpkk7(startDate, endDate, Noreg);
                    //rowIndexUPKK = rowIndexUpkkALL + 7;
                    //foreach (var data in outputUpkk7)
                    //{
                    //    sheetUPKK.Cells[rowIndexUPKK, 15].Value = data.TAM;
                    //    sheetUPKK.Cells[rowIndexUPKK, 16].Value = data.Outsource;
                    //    rowIndexUPKK++;
                    //}
                    ////DATA 8
                    //var outputUpkk8 = MedicalRecordService.GetdataUpkk8(startDate, endDate, Noreg);
                    //rowIndexUPKK = rowIndexUpkkALL + 7;
                    //foreach (var data in outputUpkk8)
                    //{
                    //    sheetUPKK.Cells[rowIndexUPKK, 17].Value = data.TAM;
                    //    sheetUPKK.Cells[rowIndexUPKK, 18].Value = data.Outsource;
                    //    rowIndexUPKK++;
                    //}
                    ////DATA 9
                    //var outputUpkk9 = MedicalRecordService.GetdataUpkk9(startDate, endDate, Noreg);
                    //rowIndexUPKK = rowIndexUpkkALL + 7;
                    //foreach (var data in outputUpkk9)
                    //{
                    //    sheetUPKK.Cells[rowIndexUPKK, 19].Value = data.TAM;
                    //    sheetUPKK.Cells[rowIndexUPKK, 20].Value = data.Outsource;
                    //    rowIndexUPKK++;
                    //}
                    ////DATA 10
                    //var outputUpkk10 = MedicalRecordService.GetdataUpkk10(startDate, endDate, Noreg);
                    //rowIndexUPKK = rowIndexUpkkALL + 7;
                    //foreach (var data in outputUpkk10)
                    //{
                    //    sheetUPKK.Cells[rowIndexUPKK, 21].Value = data.TAM;
                    //    sheetUPKK.Cells[rowIndexUPKK, 22].Value = data.Outsource;
                    //    rowIndexUPKK++;
                    //}
                    ////DATA 11
                    //var outputUpkk11 = MedicalRecordService.GetdataUpkk11(startDate, endDate, Noreg);
                    //rowIndexUPKK = rowIndexUpkkALL + 7;
                    //foreach (var data in outputUpkk11)
                    //{
                    //    sheetUPKK.Cells[rowIndexUPKK, 23].Value = data.TAM;
                    //    sheetUPKK.Cells[rowIndexUPKK, 24].Value = data.Outsource;
                    //    rowIndexUPKK++;
                    //}


                    //int ColumnAwal = 3;
                    //int endColumn = 24;   // Kolom terakhir yang ingin dijumlahkan
                    //int row = rowIndexUpkkALL + 7;         // Baris "Visit Number"

                    //int totalVisitNumber = 0;

                    //for (int col = ColumnAwal; col <= endColumn; col++)
                    //{
                    //    if (sheetUPKK.Cells[row, col].Value != null)
                    //    {
                    //        totalVisitNumber += Convert.ToInt32(sheetUPKK.Cells[row, col].Value);
                    //    }
                    //}

                    //int totalVisitor = 0;
                    //row = rowIndexUpkkALL + 8;
                    //for (int col = ColumnAwal; col <= endColumn; col++)
                    //{
                    //    if (sheetUPKK.Cells[row, col].Value != null)
                    //    {
                    //        totalVisitor += Convert.ToInt32(sheetUPKK.Cells[row, col].Value);
                    //    }
                    //}
                    //sheetUPKK.Cells[rowIndexUpkkALL + 7, 2].Value = totalVisitNumber;
                    //sheetUPKK.Cells[rowIndexUpkkALL + 8, 2].Value = totalVisitor;
                    //#endregion
                    #endregion

                    #region data tabel 1
                    //DATA 1
                    var outputUpkk1 = MedicalRecordService.GetdataUpkk1(startDate, endDate, lahirDate, namaKaryawanVendor, Noreg, NamakaryawanTam, Divisi);
                    int rowIndexUPKK = rowIndexUpkkALL + 7;
                    foreach (var data in outputUpkk1)
                    {
                        sheetUPKK.Cells[rowIndexUPKK, 2].Value = data.Total;
                        sheetUPKK.Cells[rowIndexUPKK, 3].Value = data.TAM_TSD;
                        sheetUPKK.Cells[rowIndexUPKK, 4].Value = data.Outsource_TSD;
                        sheetUPKK.Cells[rowIndexUPKK, 5].Value = data.TAM_PPDD;
                        sheetUPKK.Cells[rowIndexUPKK, 6].Value = data.Outsource_PPDD;
                        sheetUPKK.Cells[rowIndexUPKK, 7].Value = data.TAM_HRGA;
                        sheetUPKK.Cells[rowIndexUPKK, 8].Value = data.Outsource_HRGA;
                        sheetUPKK.Cells[rowIndexUPKK, 9].Value = data.TAM_LNTC;
                        sheetUPKK.Cells[rowIndexUPKK, 10].Value = data.Outsource_LNTC;
                        sheetUPKK.Cells[rowIndexUPKK, 11].Value = data.TAM_MENTENG;
                        sheetUPKK.Cells[rowIndexUPKK, 12].Value = data.Outsource_MENTENG;
                        sheetUPKK.Cells[rowIndexUPKK, 13].Value = data.TAM_SPLD;
                        sheetUPKK.Cells[rowIndexUPKK, 14].Value = data.Outsource_SPLD;
                        sheetUPKK.Cells[rowIndexUPKK, 15].Value = data.TAM_TTC;
                        sheetUPKK.Cells[rowIndexUPKK, 16].Value = data.Outsource_TTC;
                        sheetUPKK.Cells[rowIndexUPKK, 17].Value = data.TAM_SUNTER3;
                        sheetUPKK.Cells[rowIndexUPKK, 18].Value = data.Outsource_SUNTER3;
                        sheetUPKK.Cells[rowIndexUPKK, 19].Value = data.TAM_CCY;
                        sheetUPKK.Cells[rowIndexUPKK, 20].Value = data.Outsource_CCY;
                        sheetUPKK.Cells[rowIndexUPKK, 21].Value = data.TAM_KARAWANG;
                        sheetUPKK.Cells[rowIndexUPKK, 22].Value = data.Outsource_KARAWANG;
                        sheetUPKK.Cells[rowIndexUPKK, 23].Value = data.TAM_NGORO;
                        sheetUPKK.Cells[rowIndexUPKK, 24].Value = data.Outsource_NGORO;
                        rowIndexUPKK++;
                    }



                    //int ColumnAwal = 3;
                    //int endColumn = 24;   // Kolom terakhir yang ingin dijumlahkan
                    //int row = rowIndexUpkkALL + 7;         // Baris "Visit Number"

                    //int totalVisitNumber = 0;

                    //for (int col = ColumnAwal; col <= endColumn; col++)
                    //{
                    //    if (sheetUPKK.Cells[row, col].Value != null)
                    //    {
                    //        totalVisitNumber += Convert.ToInt32(sheetUPKK.Cells[row, col].Value);
                    //    }
                    //}

                    //int totalVisitor = 0;
                    //row = rowIndexUpkkALL + 8;
                    //for (int col = ColumnAwal; col <= endColumn; col++)
                    //{
                    //    if (sheetUPKK.Cells[row, col].Value != null)
                    //    {
                    //        totalVisitor += Convert.ToInt32(sheetUPKK.Cells[row, col].Value);
                    //    }
                    //}
                    //sheetUPKK.Cells[rowIndexUpkkALL + 7, 2].Value = totalVisitNumber;
                    //sheetUPKK.Cells[rowIndexUpkkALL + 8, 2].Value = totalVisitor;
                    #endregion
                    #endregion

                    #region table 2
                    //HEADER tabel 2
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 1].Value = "Kategori Penyakit";
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 2].Value = "TAM";
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 3].Value = "OutSource";
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 4].Value = "TOTAL";

                    using (var range = sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 1, rowIndexUpkkALL + 8 + 5, 4])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }

                    #region data tabel 2
                    var outputUpkk12 = MedicalRecordService.GetdataUpkkTab2(startDate, endDate, lahirDate, namaKaryawanVendor, Noreg, NamakaryawanTam, Divisi);
                    rowIndexUPKK = rowIndexUpkkALL + 8 + 6;
                    foreach (var data in outputUpkk12)
                    {
                        sheetUPKK.Cells[rowIndexUPKK, 1].Value = data.KategoriPenyakit;
                        sheetUPKK.Cells[rowIndexUPKK, 2].Value = data.TAM;
                        sheetUPKK.Cells[rowIndexUPKK, 3].Value = data.Outsource;
                        sheetUPKK.Cells[rowIndexUPKK, 4].Value = data.Total;

                        for (var i = 1; i <= 4; i++)
                        {
                            sheetUPKK.Cells[rowIndexUPKK, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }
                        rowIndexUPKK++;
                    }
                    #endregion
                    #endregion

                    #region table 3
                    //HEADER tabel 3
                    // Merge Header tabel 3
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 7, rowIndexUpkkALL + 8 + 6, 7].Merge = true; // KATEGORI PENYAKIT
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 8, rowIndexUpkkALL + 8 + 5, 9].Merge = true; // UPKK Sunter 2 - TSD
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 10, rowIndexUpkkALL + 8 + 5, 11].Merge = true; // UPKK SUNTER 2 - PPDD
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 12, rowIndexUpkkALL + 8 + 5, 13].Merge = true; // UPKK SUNTER 2 HRGA
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 14, rowIndexUpkkALL + 8 + 5, 15].Merge = true; // UPKK LEXUS - LNTC
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 16, rowIndexUpkkALL + 8 + 5, 17].Merge = true; // UPKK LEXUS - MENTENG
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 18, rowIndexUpkkALL + 8 + 5, 19].Merge = true; // UPKK SPLD
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 20, rowIndexUpkkALL + 8 + 5, 21].Merge = true; // UPKK TTC
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 22, rowIndexUpkkALL + 8 + 5, 23].Merge = true; // UPKK Sunter 3
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 24, rowIndexUpkkALL + 8 + 5, 25].Merge = true; // UPKK CCY
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 26, rowIndexUpkkALL + 8 + 5, 27].Merge = true; // UPKK Karawang
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 28, rowIndexUpkkALL + 8 + 5, 29].Merge = true; // UPKK Ngoro

                    // Menulis Judul Header (Kategori Penyakit & UPKK)
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 7].Value = "Kategori Penyakit";
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 8].Value = "UPKK Sunter 2 - TSD";
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 10].Value = "UPKK SUNTER 2 - PPDD";
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 12].Value = "UPKK SUNTER 2 HRGA";
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 14].Value = "UPKK LEXUS - LNTC";
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 16].Value = "UPKK LEXUS - MENTENG";
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 18].Value = "UPKK SPLD";
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 20].Value = "UPKK TTC";
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 22].Value = "UPKK Sunter 3";
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 24].Value = "UPKK CCY";
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 26].Value = "UPKK Karawang";
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 28].Value = "UPKK Ngoro";

                    int startColumnIndex = 8; // 'H' adalah kolom ke-8 dalam Excel (1-based index)

                    for (int i = 0; i < 22; i += 2) // 22 kolom TAM & Outsource
                    {
                        sheetUPKK.Cells[rowIndexUpkkALL + 8 + 6, startColumnIndex + i].Value = "TAM";
                        sheetUPKK.Cells[rowIndexUpkkALL + 8 + 6, startColumnIndex + i + 1].Value = "Outsource";
                    }


                    using (var range = sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 7, rowIndexUpkkALL + 8 + 6, 29])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }


                    #region data tab3
                    var outputUpkk13 = MedicalRecordService.GetdataUpkkTab3(startDate, endDate, lahirDate, namaKaryawanVendor, Noreg, NamakaryawanTam, Divisi);
                    rowIndexUPKK = rowIndexUpkkALL + 8 + 7;
                    foreach (var data in outputUpkk13)
                    {
                        sheetUPKK.Cells[rowIndexUPKK, 7].Value = data.KategoriPenyakit;
                        sheetUPKK.Cells[rowIndexUPKK, 8].Value = data.TAM_TSD;
                        sheetUPKK.Cells[rowIndexUPKK, 9].Value = data.Outsource_TSD;
                        sheetUPKK.Cells[rowIndexUPKK, 10].Value = data.TAM_PPDD;
                        sheetUPKK.Cells[rowIndexUPKK, 11].Value = data.Outsource_PPDD;
                        sheetUPKK.Cells[rowIndexUPKK, 12].Value = data.TAM_HRGA;
                        sheetUPKK.Cells[rowIndexUPKK, 13].Value = data.Outsource_HRGA;
                        sheetUPKK.Cells[rowIndexUPKK, 14].Value = data.TAM_LNTC;
                        sheetUPKK.Cells[rowIndexUPKK, 15].Value = data.Outsource_LNTC;
                        sheetUPKK.Cells[rowIndexUPKK, 16].Value = data.TAM_MENTENG;
                        sheetUPKK.Cells[rowIndexUPKK, 17].Value = data.Outsource_MENTENG;
                        sheetUPKK.Cells[rowIndexUPKK, 18].Value = data.TAM_SPLD;
                        sheetUPKK.Cells[rowIndexUPKK, 19].Value = data.Outsource_SPLD;
                        sheetUPKK.Cells[rowIndexUPKK, 20].Value = data.TAM_TTC;
                        sheetUPKK.Cells[rowIndexUPKK, 21].Value = data.Outsource_TTC;
                        sheetUPKK.Cells[rowIndexUPKK, 22].Value = data.TAM_SUNTER3;
                        sheetUPKK.Cells[rowIndexUPKK, 23].Value = data.Outsource_SUNTER3;
                        sheetUPKK.Cells[rowIndexUPKK, 24].Value = data.TAM_CCY;
                        sheetUPKK.Cells[rowIndexUPKK, 25].Value = data.Outsource_CCY;
                        sheetUPKK.Cells[rowIndexUPKK, 26].Value = data.TAM_KARAWANG;
                        sheetUPKK.Cells[rowIndexUPKK, 27].Value = data.Outsource_KARAWANG;
                        sheetUPKK.Cells[rowIndexUPKK, 28].Value = data.TAM_NGORO;
                        sheetUPKK.Cells[rowIndexUPKK, 29].Value = data.Outsource_NGORO;


                        for (var i = 7; i <= 29; i++)
                        {
                            sheetUPKK.Cells[rowIndexUPKK, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }
                        rowIndexUPKK++;
                    }
                    #endregion

                    #endregion


                    // AutoFit Columns
                    //sheetUPKK.Cells["A1:X5"].AutoFitColumns();
                    //sheetUPKK.Cells["A10:D10"].AutoFitColumns();
                    //sheetUPKK.Cells["G10:G11"].AutoFitColumns();
                    sheetUPKK.Cells[sheetUPKK.Dimension.Address].AutoFitColumns();

                    #endregion

                    #region In&Out Patient
                    var worksheetPatient = package.Workbook.Worksheets.Add("In & Out Patient");
                    #region set header

                    worksheetPatient.Cells["A1"].Value = "No";
                    worksheetPatient.Cells["B1"].Value = "PATIENT NAME";
                    worksheetPatient.Cells["C1"].Value = "NOREG";
                    worksheetPatient.Cells["D1"].Value = "DOB";
                    worksheetPatient.Cells["E1"].Value = "GENDER";
                    worksheetPatient.Cells["F1"].Value = "PROVIDER";
                    worksheetPatient.Cells["G1"].Value = "ADMISSION DATE";
                    worksheetPatient.Cells["H1"].Value = "DISCHARGEABLE DATE";
                    worksheetPatient.Cells["I1"].Value = "DIAGNOSIS DESC";
                    // Styling Header 
                    using (var range = worksheetPatient.Cells["A1:I1"])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }
                    #region data
                    var outputPatient = MedicalRecordService.GetdataPatient(startDate, endDate, null, "", Noreg, NamakaryawanTam, Divisi);
                    int rowIndexPatient = 2;
                    int normerurutPatient = 1;
                    foreach (var data in outputPatient)
                    {
                        worksheetPatient.Cells[rowIndexPatient, 1].Value = normerurutPatient;
                        worksheetPatient.Cells[rowIndexPatient, 2].Value = data.Name;
                        worksheetPatient.Cells[rowIndexPatient, 3].Value = data.Noreg;
                        worksheetPatient.Cells[rowIndexPatient, 4].Value = data.Tanggal_Lahir?.ToString("dd/MM/yyyy");
                        worksheetPatient.Cells[rowIndexPatient, 5].Value = data.Jenis_Kelamin;
                        worksheetPatient.Cells[rowIndexPatient, 6].Value = data.Provider;
                        worksheetPatient.Cells[rowIndexPatient, 7].Value = data.AdmissionDate?.ToString("dd/MM/yyyy");
                        worksheetPatient.Cells[rowIndexPatient, 8].Value = data.DisChargeAbleDate?.ToString("dd/MM/yyyy");
                        worksheetPatient.Cells[rowIndexPatient, 9].Value = data.DiagnosisDesc;

                        for (var i = 1; i <= 9; i++)
                        {
                            worksheetPatient.Cells[rowIndexPatient, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }

                        rowIndexPatient++;
                        normerurutPatient++;

                    }
                    #endregion

                    worksheetPatient.Cells[worksheetPatient.Dimension.Address].AutoFitColumns();
                    #endregion
                    #endregion

                    #region Tanya Ohs
                    var worksheetTanyaohs = package.Workbook.Worksheets.Add("Tanya Ohs");
                    #region set header

                    worksheetTanyaohs.Cells["A1"].Value = "No";
                    worksheetTanyaohs.Cells["B1"].Value = "TANGGAL MULAI";
                    worksheetTanyaohs.Cells["C1"].Value = "TANGGAL SELESAI";
                    worksheetTanyaohs.Cells["D1"].Value = "NOREG";
                    worksheetTanyaohs.Cells["E1"].Value = "NAME";
                    worksheetTanyaohs.Cells["F1"].Value = "KELUHAN";
                    worksheetTanyaohs.Cells["G1"].Value = "CATEGORY";
                    worksheetTanyaohs.Cells["H1"].Value = "STATUS";
                    worksheetTanyaohs.Cells["I1"].Value = "RATING";
                    // Styling Header 
                    using (var range = worksheetTanyaohs.Cells["A1:I1"])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }

                    #region data
                    var outputTANYAohs = MedicalRecordService.GetdataTanyaOhs(startDate, endDate, null, "", Noreg, NamakaryawanTam,Divisi);
                    int rowIndexTanyaOhs = 2;
                    int normerurutTanyaOhs = 1;
                    foreach (var data in outputTANYAohs)
                    {
                        worksheetTanyaohs.Cells[rowIndexTanyaOhs, 1].Value = normerurutTanyaOhs;
                        worksheetTanyaohs.Cells[rowIndexTanyaOhs, 2].Value = data.CreatedOn.ToString("dd/MM/yyyy HH:mm");
                        worksheetTanyaohs.Cells[rowIndexTanyaOhs, 3].Value = data.ModifiedOn?.ToString("dd/MM/yyyy HH:mm");
                        worksheetTanyaohs.Cells[rowIndexTanyaOhs, 4].Value = data.NoReg;
                        worksheetTanyaohs.Cells[rowIndexTanyaOhs, 5].Value = data.Name;
                        worksheetTanyaohs.Cells[rowIndexTanyaOhs, 6].Value = data.Keluhan;
                        worksheetTanyaohs.Cells[rowIndexTanyaOhs, 7].Value = data.KategoriLayanan;
                        worksheetTanyaohs.Cells[rowIndexTanyaOhs, 8].Value = data.Status;
                        worksheetTanyaohs.Cells[rowIndexTanyaOhs, 9].Value = data.Rating;


                        for (var i = 1; i <= 9; i++)
                        {
                            worksheetTanyaohs.Cells[rowIndexTanyaOhs, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                        }

                        rowIndexTanyaOhs++;
                        normerurutTanyaOhs++;

                    }
                    #endregion

                    worksheetTanyaohs.Cells[worksheetTanyaohs.Dimension.Address].AutoFitColumns();
                    #endregion
                    #endregion

                    // Simpan ke MemoryStream
                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MedicalRecord.xlsx");
                }
                #endregion

            }

            #region role ohs klinik
            using (var package = new ExcelPackage())
            {

                #region Sheet UPKK
                var sheetUPKK = package.Workbook.Worksheets.Add("UPKK Visit");

                #region tabel all
                // Merge Header tabel ALL COLOM
                sheetUPKK.Cells["A1:A2"].Merge = true;
                sheetUPKK.Cells["B1:B2"].Merge = true;
                sheetUPKK.Cells["C1:C2"].Merge = true;
                sheetUPKK.Cells["D1:D2"].Merge = true;
                sheetUPKK.Cells["E1:E2"].Merge = true;
                sheetUPKK.Cells["F1:F2"].Merge = true;
                sheetUPKK.Cells["G1:G2"].Merge = true;
                sheetUPKK.Cells["H1:H2"].Merge = true;
                sheetUPKK.Cells["I1:I2"].Merge = true;
                sheetUPKK.Cells["J1:J2"].Merge = true;
                sheetUPKK.Cells["K1:K2"].Merge = true;
                sheetUPKK.Cells["L1:L2"].Merge = true;
                sheetUPKK.Cells["M1:Q1"].Merge = true;
                sheetUPKK.Cells["R1:R2"].Merge = true;
                sheetUPKK.Cells["S1:S2"].Merge = true;
                sheetUPKK.Cells["T1:T2"].Merge = true;
                sheetUPKK.Cells["U1:U2"].Merge = true;
                sheetUPKK.Cells["V1:V2"].Merge = true;
                sheetUPKK.Cells["W1:W2"].Merge = true;
                sheetUPKK.Cells["X1:X2"].Merge = true;
                sheetUPKK.Cells["Y1:Y2"].Merge = true;

                // Set Header Text tabel ALL COLOM

                sheetUPKK.Cells["A1"].Value = "Lokasi UPKK";
                sheetUPKK.Cells["B1"].Value = "Kategori Kunjungan \r\n(Berobat / Pemeriksaan Kesehatan)";
                sheetUPKK.Cells["C1"].Value = "Tanggal Kunjungan";
                sheetUPKK.Cells["D1"].Value = "Company";
                sheetUPKK.Cells["E1"].Value = "Divisi";
                sheetUPKK.Cells["F1"].Value = "Noreg";
                sheetUPKK.Cells["G1"].Value = "Nama";
                sheetUPKK.Cells["H1"].Value = "Umur";
                sheetUPKK.Cells["I1"].Value = "Jenis Kelamin";
                sheetUPKK.Cells["J1"].Value = "Jenis Pekerjaan";
                sheetUPKK.Cells["K1"].Value = "Lokasi Kerja";
                sheetUPKK.Cells["L1"].Value = "Keluhan";
                sheetUPKK.Cells["M1"].Value = "Pemeriksaan";
                sheetUPKK.Cells["M2"].Value = "TD Sistole";
                sheetUPKK.Cells["N2"].Value = "TD Diastole";
                sheetUPKK.Cells["O2"].Value = "Nadi";
                sheetUPKK.Cells["P2"].Value = "Respirasi";
                sheetUPKK.Cells["Q2"].Value = "Suhu";
                sheetUPKK.Cells["R1"].Value = "Diagnosa";
                sheetUPKK.Cells["S1"].Value = "Kategori Penyakit";
                sheetUPKK.Cells["T1"].Value = "Spesifik Penyakit";
                sheetUPKK.Cells["U1"].Value = "Jenis Kasus";
                sheetUPKK.Cells["V1"].Value = "Treatment (Nama Obat)";
                sheetUPKK.Cells["W1"].Value = "Pemeriksa\r\n(Nurse / Doctor)";
                sheetUPKK.Cells["X1"].Value = "Nama Pemeriksa\r\n(Nurse / Doctor)";
                sheetUPKK.Cells["Y1"].Value = "Hasil Akhir";

                // Styling Header 
                using (var range = sheetUPKK.Cells["A1:Y2"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#D9EAD3"));
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }
                #region data
                var outputupkkAll = MedicalRecordService.GetdataUpkkAll(startDate, endDate, lahirDate, namaKaryawanVendor, Noreg, NamakaryawanTam, Divisi);
                int rowIndexUpkkALL = 3;

                foreach (var data in outputupkkAll)
                {
                    sheetUPKK.Cells[rowIndexUpkkALL, 1].Value = data.LokasiUPKK;
                    sheetUPKK.Cells[rowIndexUpkkALL, 2].Value = data.KategoriKunjungan;
                    sheetUPKK.Cells[rowIndexUpkkALL, 3].Value = data.TanggalKunjungan?.ToString("dd/MM/yyyy");
                    sheetUPKK.Cells[rowIndexUpkkALL, 4].Value = data.Company;
                    sheetUPKK.Cells[rowIndexUpkkALL, 5].Value = data.Divisi;
                    sheetUPKK.Cells[rowIndexUpkkALL, 6].Value = data.Noreg;
                    sheetUPKK.Cells[rowIndexUpkkALL, 7].Value = data.NamaEmployeeVendor;
                    sheetUPKK.Cells[rowIndexUpkkALL, 8].Value = data.Usia;
                    sheetUPKK.Cells[rowIndexUpkkALL, 9].Value = data.JenisKelaminEmployeeVendor;
                    sheetUPKK.Cells[rowIndexUpkkALL, 11].Value = data.LokasiKerja;
                    sheetUPKK.Cells[rowIndexUpkkALL, 12].Value = data.Keluhan;
                    sheetUPKK.Cells[rowIndexUpkkALL, 13].Value = data.TDSistole;
                    sheetUPKK.Cells[rowIndexUpkkALL, 14].Value = data.TDDiastole;
                    sheetUPKK.Cells[rowIndexUpkkALL, 15].Value = data.Nadi;
                    sheetUPKK.Cells[rowIndexUpkkALL, 16].Value = data.Respirasi;
                    sheetUPKK.Cells[rowIndexUpkkALL, 17].Value = data.Suhu;
                    sheetUPKK.Cells[rowIndexUpkkALL, 18].Value = data.Diagnosa;
                    sheetUPKK.Cells[rowIndexUpkkALL, 19].Value = data.KategoriPenyakit;
                    sheetUPKK.Cells[rowIndexUpkkALL, 20].Value = data.SpesifikPenyakit;
                    sheetUPKK.Cells[rowIndexUpkkALL, 21].Value = data.JenisKasus;
                    sheetUPKK.Cells[rowIndexUpkkALL, 22].Value = data.Treatment;
                    sheetUPKK.Cells[rowIndexUpkkALL, 23].Value = data.Pemeriksa;
                    sheetUPKK.Cells[rowIndexUpkkALL, 24].Value = data.NamaPemeriksa;
                    sheetUPKK.Cells[rowIndexUpkkALL, 25].Value = data.HasilAkhir;

                    for (var i = 1; i <= 25; i++)
                    {
                        sheetUPKK.Cells[rowIndexUpkkALL, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }

                    rowIndexUpkkALL++;


                }
                #endregion

                #endregion

                #region table 1
                // Merge Header tabel 1
                sheetUPKK.Cells[rowIndexUpkkALL + 4, 3, rowIndexUpkkALL + 4, 24].Merge = true; // KLINIK/UPKK
                sheetUPKK.Cells[rowIndexUpkkALL + 4, 1, rowIndexUpkkALL + 4 + 2, 1].Merge = true; // UPKK VISIT
                sheetUPKK.Cells[rowIndexUpkkALL + 4, 2, rowIndexUpkkALL + 4 + 2, 2].Merge = true; // TOTAL
                sheetUPKK.Cells[rowIndexUpkkALL + 5, 3, rowIndexUpkkALL + 5, 4].Merge = true; // UPKK Sunter 2 - TSD
                sheetUPKK.Cells[rowIndexUpkkALL + 5, 5, rowIndexUpkkALL + 5, 6].Merge = true; // UPKK SUNTER 2 - PPDD
                sheetUPKK.Cells[rowIndexUpkkALL + 5, 7, rowIndexUpkkALL + 5, 8].Merge = true; // UPKK SUNTER 2 HRGA
                sheetUPKK.Cells[rowIndexUpkkALL + 5, 9, rowIndexUpkkALL + 5, 10].Merge = true; // UPKK LEXUS - LNTC
                sheetUPKK.Cells[rowIndexUpkkALL + 5, 11, rowIndexUpkkALL + 5, 12].Merge = true; // UPKK LEXUS - MENTENG
                sheetUPKK.Cells[rowIndexUpkkALL + 5, 13, rowIndexUpkkALL + 5, 14].Merge = true; // UPKK SPLD
                sheetUPKK.Cells[rowIndexUpkkALL + 5, 15, rowIndexUpkkALL + 5, 16].Merge = true; // UPKK TTC
                sheetUPKK.Cells[rowIndexUpkkALL + 5, 17, rowIndexUpkkALL + 5, 18].Merge = true; // UPKK Sunter 3
                sheetUPKK.Cells[rowIndexUpkkALL + 5, 19, rowIndexUpkkALL + 5, 20].Merge = true; // UPKK CCY
                sheetUPKK.Cells[rowIndexUpkkALL + 5, 21, rowIndexUpkkALL + 5, 22].Merge = true; // UPKK Karawang
                sheetUPKK.Cells[rowIndexUpkkALL + 5, 23, rowIndexUpkkALL + 5, 24].Merge = true; // UPKK Ngoro


                int startColumn = 3; // Kolom C di Excel dimulai dari indeks 3
                int rowTAM = rowIndexUpkkALL + 6; // Sesuaikan baris sesuai struktur tabel

                for (int i = 0; i < 22; i += 2) // 22 kolom TAM & Outsource
                {
                    sheetUPKK.Cells[rowTAM, startColumn + i].Value = "TAM";
                    sheetUPKK.Cells[rowTAM, startColumn + i + 1].Value = "Outsource";
                }



                // Set Header Text tabel 1
                sheetUPKK.Cells[rowIndexUpkkALL + 4, 3].Value = "KLINIK / UPKK";
                sheetUPKK.Cells[rowIndexUpkkALL + 4, 1].Value = "UPKK VISIT";
                sheetUPKK.Cells[rowIndexUpkkALL + 4, 2].Value = "TOTAL";
                sheetUPKK.Cells[rowIndexUpkkALL + 5, 3].Value = "UPKK Sunter 2 - TSD";
                sheetUPKK.Cells[rowIndexUpkkALL + 5, 5].Value = "UPKK SUNTER 2 - PPDD";
                sheetUPKK.Cells[rowIndexUpkkALL + 5, 7].Value = "UPKK SUNTER 2 HRGA";
                sheetUPKK.Cells[rowIndexUpkkALL + 5, 9].Value = "UPKK LEXUS - LNTC";
                sheetUPKK.Cells[rowIndexUpkkALL + 5, 11].Value = "UPKK LEXUS - MENTENG";
                sheetUPKK.Cells[rowIndexUpkkALL + 5, 13].Value = "UPKK SPLD";
                sheetUPKK.Cells[rowIndexUpkkALL + 5, 15].Value = "UPKK TTC";
                sheetUPKK.Cells[rowIndexUpkkALL + 5, 17].Value = "UPKK Sunter 3";
                sheetUPKK.Cells[rowIndexUpkkALL + 5, 19].Value = "UPKK CCY";
                sheetUPKK.Cells[rowIndexUpkkALL + 5, 21].Value = "UPKK Karawang";
                sheetUPKK.Cells[rowIndexUpkkALL + 5, 23].Value = "UPKK Ngoro";

                // Data Sub-header tabel 1
                sheetUPKK.Cells[rowIndexUpkkALL + 7, 1].Value = "Visit Number";
                sheetUPKK.Cells[rowIndexUpkkALL + 8, 1].Value = "Total Visitor";

                // Styling Header tabel 1
                using (var range = sheetUPKK.Cells[rowIndexUpkkALL + 4, 1, rowIndexUpkkALL + 6, 24])
                {
                    range.Style.Font.Bold = true;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                    //range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    //range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    //range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    //range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                //// Border untuk tabel 1
                using (var range = sheetUPKK.Cells[rowIndexUpkkALL + 4, 1, rowIndexUpkkALL + 8, 24])
                {
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }
                #region REMARK data CARA LINQ
                //#region data tabel 1
                ////DATA 1
                //var outputUpkk1 = MedicalRecordService.GetdataUpkk1(startDate, endDate, Noreg);
                //int rowIndexUPKK = rowIndexUpkkALL + 7;
                //foreach (var data in outputUpkk1)
                //{
                //    sheetUPKK.Cells[rowIndexUPKK, 3].Value = data.TAM;
                //    sheetUPKK.Cells[rowIndexUPKK, 4].Value = data.Outsource;
                //    rowIndexUPKK++;
                //}
                ////DATA 2
                //var outputUpkk2 = MedicalRecordService.GetdataUpkk2(startDate, endDate, Noreg);
                //rowIndexUPKK = rowIndexUpkkALL + 7;
                //foreach (var data in outputUpkk2)
                //{
                //    sheetUPKK.Cells[rowIndexUPKK, 5].Value = data.TAM;
                //    sheetUPKK.Cells[rowIndexUPKK, 6].Value = data.Outsource;
                //    rowIndexUPKK++;
                //}
                ////DATA 3
                //var outputUpkk3 = MedicalRecordService.GetdataUpkk3(startDate, endDate, Noreg);
                //rowIndexUPKK = rowIndexUpkkALL + 7;
                //foreach (var data in outputUpkk3)
                //{
                //    sheetUPKK.Cells[rowIndexUPKK, 7].Value = data.TAM;
                //    sheetUPKK.Cells[rowIndexUPKK, 8].Value = data.Outsource;
                //    rowIndexUPKK++;
                //}
                ////DATA 4
                //var outputUpkk4 = MedicalRecordService.GetdataUpkk4(startDate, endDate, Noreg);
                //rowIndexUPKK = rowIndexUpkkALL + 7;
                //foreach (var data in outputUpkk4)
                //{
                //    sheetUPKK.Cells[rowIndexUPKK, 9].Value = data.TAM;
                //    sheetUPKK.Cells[rowIndexUPKK, 10].Value = data.Outsource;
                //    rowIndexUPKK++;
                //}
                ////DATA 5
                //var outputUpkk5 = MedicalRecordService.GetdataUpkk5(startDate, endDate, Noreg);
                //rowIndexUPKK = rowIndexUpkkALL + 7;
                //foreach (var data in outputUpkk5)
                //{
                //    sheetUPKK.Cells[rowIndexUPKK, 11].Value = data.TAM;
                //    sheetUPKK.Cells[rowIndexUPKK, 12].Value = data.Outsource;
                //    rowIndexUPKK++;
                //}
                ////DATA 6
                //var outputUpkk6 = MedicalRecordService.GetdataUpkk6(startDate, endDate, Noreg);
                //rowIndexUPKK = rowIndexUpkkALL + 7;
                //foreach (var data in outputUpkk6)
                //{
                //    sheetUPKK.Cells[rowIndexUPKK, 13].Value = data.TAM;
                //    sheetUPKK.Cells[rowIndexUPKK, 14].Value = data.Outsource;
                //    rowIndexUPKK++;
                //}
                ////DATA 7
                //var outputUpkk7 = MedicalRecordService.GetdataUpkk7(startDate, endDate, Noreg);
                //rowIndexUPKK = rowIndexUpkkALL + 7;
                //foreach (var data in outputUpkk7)
                //{
                //    sheetUPKK.Cells[rowIndexUPKK, 15].Value = data.TAM;
                //    sheetUPKK.Cells[rowIndexUPKK, 16].Value = data.Outsource;
                //    rowIndexUPKK++;
                //}
                ////DATA 8
                //var outputUpkk8 = MedicalRecordService.GetdataUpkk8(startDate, endDate, Noreg);
                //rowIndexUPKK = rowIndexUpkkALL + 7;
                //foreach (var data in outputUpkk8)
                //{
                //    sheetUPKK.Cells[rowIndexUPKK, 17].Value = data.TAM;
                //    sheetUPKK.Cells[rowIndexUPKK, 18].Value = data.Outsource;
                //    rowIndexUPKK++;
                //}
                ////DATA 9
                //var outputUpkk9 = MedicalRecordService.GetdataUpkk9(startDate, endDate, Noreg);
                //rowIndexUPKK = rowIndexUpkkALL + 7;
                //foreach (var data in outputUpkk9)
                //{
                //    sheetUPKK.Cells[rowIndexUPKK, 19].Value = data.TAM;
                //    sheetUPKK.Cells[rowIndexUPKK, 20].Value = data.Outsource;
                //    rowIndexUPKK++;
                //}
                ////DATA 10
                //var outputUpkk10 = MedicalRecordService.GetdataUpkk10(startDate, endDate, Noreg);
                //rowIndexUPKK = rowIndexUpkkALL + 7;
                //foreach (var data in outputUpkk10)
                //{
                //    sheetUPKK.Cells[rowIndexUPKK, 21].Value = data.TAM;
                //    sheetUPKK.Cells[rowIndexUPKK, 22].Value = data.Outsource;
                //    rowIndexUPKK++;
                //}
                ////DATA 11
                //var outputUpkk11 = MedicalRecordService.GetdataUpkk11(startDate, endDate, Noreg);
                //rowIndexUPKK = rowIndexUpkkALL + 7;
                //foreach (var data in outputUpkk11)
                //{
                //    sheetUPKK.Cells[rowIndexUPKK, 23].Value = data.TAM;
                //    sheetUPKK.Cells[rowIndexUPKK, 24].Value = data.Outsource;
                //    rowIndexUPKK++;
                //}


                //int ColumnAwal = 3;
                //int endColumn = 24;   // Kolom terakhir yang ingin dijumlahkan
                //int row = rowIndexUpkkALL + 7;         // Baris "Visit Number"

                //int totalVisitNumber = 0;

                //for (int col = ColumnAwal; col <= endColumn; col++)
                //{
                //    if (sheetUPKK.Cells[row, col].Value != null)
                //    {
                //        totalVisitNumber += Convert.ToInt32(sheetUPKK.Cells[row, col].Value);
                //    }
                //}

                //int totalVisitor = 0;
                //row = rowIndexUpkkALL + 8;
                //for (int col = ColumnAwal; col <= endColumn; col++)
                //{
                //    if (sheetUPKK.Cells[row, col].Value != null)
                //    {
                //        totalVisitor += Convert.ToInt32(sheetUPKK.Cells[row, col].Value);
                //    }
                //}
                //sheetUPKK.Cells[rowIndexUpkkALL + 7, 2].Value = totalVisitNumber;
                //sheetUPKK.Cells[rowIndexUpkkALL + 8, 2].Value = totalVisitor;
                //#endregion
                #endregion

                #region data tabel 1
                //DATA 1
                var outputUpkk1 = MedicalRecordService.GetdataUpkk1(startDate, endDate, lahirDate, namaKaryawanVendor, Noreg, NamakaryawanTam, Divisi);
                int rowIndexUPKK = rowIndexUpkkALL + 7;
                foreach (var data in outputUpkk1)
                {
                    sheetUPKK.Cells[rowIndexUPKK, 2].Value = data.Total;
                    sheetUPKK.Cells[rowIndexUPKK, 3].Value = data.TAM_TSD;
                    sheetUPKK.Cells[rowIndexUPKK, 4].Value = data.Outsource_TSD;
                    sheetUPKK.Cells[rowIndexUPKK, 5].Value = data.TAM_PPDD;
                    sheetUPKK.Cells[rowIndexUPKK, 6].Value = data.Outsource_PPDD;
                    sheetUPKK.Cells[rowIndexUPKK, 7].Value = data.TAM_HRGA;
                    sheetUPKK.Cells[rowIndexUPKK, 8].Value = data.Outsource_HRGA;
                    sheetUPKK.Cells[rowIndexUPKK, 9].Value = data.TAM_LNTC;
                    sheetUPKK.Cells[rowIndexUPKK, 10].Value = data.Outsource_LNTC;
                    sheetUPKK.Cells[rowIndexUPKK, 11].Value = data.TAM_MENTENG;
                    sheetUPKK.Cells[rowIndexUPKK, 12].Value = data.Outsource_MENTENG;
                    sheetUPKK.Cells[rowIndexUPKK, 13].Value = data.TAM_SPLD;
                    sheetUPKK.Cells[rowIndexUPKK, 14].Value = data.Outsource_SPLD;
                    sheetUPKK.Cells[rowIndexUPKK, 15].Value = data.TAM_TTC;
                    sheetUPKK.Cells[rowIndexUPKK, 16].Value = data.Outsource_TTC;
                    sheetUPKK.Cells[rowIndexUPKK, 17].Value = data.TAM_SUNTER3;
                    sheetUPKK.Cells[rowIndexUPKK, 18].Value = data.Outsource_SUNTER3;
                    sheetUPKK.Cells[rowIndexUPKK, 19].Value = data.TAM_CCY;
                    sheetUPKK.Cells[rowIndexUPKK, 20].Value = data.Outsource_CCY;
                    sheetUPKK.Cells[rowIndexUPKK, 21].Value = data.TAM_KARAWANG;
                    sheetUPKK.Cells[rowIndexUPKK, 22].Value = data.Outsource_KARAWANG;
                    sheetUPKK.Cells[rowIndexUPKK, 23].Value = data.TAM_NGORO;
                    sheetUPKK.Cells[rowIndexUPKK, 24].Value = data.Outsource_NGORO;
                    rowIndexUPKK++;
                }



                //int ColumnAwal = 3;
                //int endColumn = 24;   // Kolom terakhir yang ingin dijumlahkan
                //int row = rowIndexUpkkALL + 7;         // Baris "Visit Number"

                //int totalVisitNumber = 0;

                //for (int col = ColumnAwal; col <= endColumn; col++)
                //{
                //    if (sheetUPKK.Cells[row, col].Value != null)
                //    {
                //        totalVisitNumber += Convert.ToInt32(sheetUPKK.Cells[row, col].Value);
                //    }
                //}

                //int totalVisitor = 0;
                //row = rowIndexUpkkALL + 8;
                //for (int col = ColumnAwal; col <= endColumn; col++)
                //{
                //    if (sheetUPKK.Cells[row, col].Value != null)
                //    {
                //        totalVisitor += Convert.ToInt32(sheetUPKK.Cells[row, col].Value);
                //    }
                //}
                //sheetUPKK.Cells[rowIndexUpkkALL + 7, 2].Value = totalVisitNumber;
                //sheetUPKK.Cells[rowIndexUpkkALL + 8, 2].Value = totalVisitor;
                #endregion
                #endregion

                #region table 2
                //HEADER tabel 2
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 1].Value = "Kategori Penyakit";
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 2].Value = "TAM";
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 3].Value = "OutSource";
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 4].Value = "TOTAL";

                using (var range = sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 1, rowIndexUpkkALL + 8 + 5, 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                #region data tabel 2
                var outputUpkk12 = MedicalRecordService.GetdataUpkkTab2(startDate, endDate, lahirDate, namaKaryawanVendor, Noreg,NamakaryawanTam, Divisi);
                rowIndexUPKK = rowIndexUpkkALL + 8 + 6;
                foreach (var data in outputUpkk12)
                {
                    sheetUPKK.Cells[rowIndexUPKK, 1].Value = data.KategoriPenyakit;
                    sheetUPKK.Cells[rowIndexUPKK, 2].Value = data.TAM;
                    sheetUPKK.Cells[rowIndexUPKK, 3].Value = data.Outsource;
                    sheetUPKK.Cells[rowIndexUPKK, 4].Value = data.Total;

                    for (var i = 1; i <= 4; i++)
                    {
                        sheetUPKK.Cells[rowIndexUPKK, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }
                    rowIndexUPKK++;
                }
                #endregion
                #endregion

                #region table 3
                //HEADER tabel 3
                // Merge Header tabel 3
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 7, rowIndexUpkkALL + 8 + 6, 7].Merge = true; // KATEGORI PENYAKIT
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 8, rowIndexUpkkALL + 8 + 5, 9].Merge = true; // UPKK Sunter 2 - TSD
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 10, rowIndexUpkkALL + 8 + 5, 11].Merge = true; // UPKK SUNTER 2 - PPDD
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 12, rowIndexUpkkALL + 8 + 5, 13].Merge = true; // UPKK SUNTER 2 HRGA
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 14, rowIndexUpkkALL + 8 + 5, 15].Merge = true; // UPKK LEXUS - LNTC
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 16, rowIndexUpkkALL + 8 + 5, 17].Merge = true; // UPKK LEXUS - MENTENG
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 18, rowIndexUpkkALL + 8 + 5, 19].Merge = true; // UPKK SPLD
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 20, rowIndexUpkkALL + 8 + 5, 21].Merge = true; // UPKK TTC
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 22, rowIndexUpkkALL + 8 + 5, 23].Merge = true; // UPKK Sunter 3
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 24, rowIndexUpkkALL + 8 + 5, 25].Merge = true; // UPKK CCY
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 26, rowIndexUpkkALL + 8 + 5, 27].Merge = true; // UPKK Karawang
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 28, rowIndexUpkkALL + 8 + 5, 29].Merge = true; // UPKK Ngoro

                // Menulis Judul Header (Kategori Penyakit & UPKK)
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 7].Value = "Kategori Penyakit";
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 8].Value = "UPKK Sunter 2 - TSD";
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 10].Value = "UPKK SUNTER 2 - PPDD";
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 12].Value = "UPKK SUNTER 2 HRGA";
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 14].Value = "UPKK LEXUS - LNTC";
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 16].Value = "UPKK LEXUS - MENTENG";
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 18].Value = "UPKK SPLD";
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 20].Value = "UPKK TTC";
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 22].Value = "UPKK Sunter 3";
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 24].Value = "UPKK CCY";
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 26].Value = "UPKK Karawang";
                sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 28].Value = "UPKK Ngoro";

                int startColumnIndex = 8; // 'H' adalah kolom ke-8 dalam Excel (1-based index)

                for (int i = 0; i < 22; i += 2) // 22 kolom TAM & Outsource
                {
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 6, startColumnIndex + i].Value = "TAM";
                    sheetUPKK.Cells[rowIndexUpkkALL + 8 + 6, startColumnIndex + i + 1].Value = "Outsource";
                }


                using (var range = sheetUPKK.Cells[rowIndexUpkkALL + 8 + 5, 7, rowIndexUpkkALL + 8 + 6, 29])
                {
                    range.Style.Font.Bold = true;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }


                #region data tab3
                var outputUpkk13 = MedicalRecordService.GetdataUpkkTab3(startDate, endDate, lahirDate, namaKaryawanVendor, Noreg, NamakaryawanTam,Divisi);
                rowIndexUPKK = rowIndexUpkkALL + 8 + 7;
                foreach (var data in outputUpkk13)
                {
                    sheetUPKK.Cells[rowIndexUPKK, 7].Value = data.KategoriPenyakit;
                    sheetUPKK.Cells[rowIndexUPKK, 8].Value = data.TAM_TSD;
                    sheetUPKK.Cells[rowIndexUPKK, 9].Value = data.Outsource_TSD;
                    sheetUPKK.Cells[rowIndexUPKK, 10].Value = data.TAM_PPDD;
                    sheetUPKK.Cells[rowIndexUPKK, 11].Value = data.Outsource_PPDD;
                    sheetUPKK.Cells[rowIndexUPKK, 12].Value = data.TAM_HRGA;
                    sheetUPKK.Cells[rowIndexUPKK, 13].Value = data.Outsource_HRGA;
                    sheetUPKK.Cells[rowIndexUPKK, 14].Value = data.TAM_LNTC;
                    sheetUPKK.Cells[rowIndexUPKK, 15].Value = data.Outsource_LNTC;
                    sheetUPKK.Cells[rowIndexUPKK, 16].Value = data.TAM_MENTENG;
                    sheetUPKK.Cells[rowIndexUPKK, 17].Value = data.Outsource_MENTENG;
                    sheetUPKK.Cells[rowIndexUPKK, 18].Value = data.TAM_SPLD;
                    sheetUPKK.Cells[rowIndexUPKK, 19].Value = data.Outsource_SPLD;
                    sheetUPKK.Cells[rowIndexUPKK, 20].Value = data.TAM_TTC;
                    sheetUPKK.Cells[rowIndexUPKK, 21].Value = data.Outsource_TTC;
                    sheetUPKK.Cells[rowIndexUPKK, 22].Value = data.TAM_SUNTER3;
                    sheetUPKK.Cells[rowIndexUPKK, 23].Value = data.Outsource_SUNTER3;
                    sheetUPKK.Cells[rowIndexUPKK, 24].Value = data.TAM_CCY;
                    sheetUPKK.Cells[rowIndexUPKK, 25].Value = data.Outsource_CCY;
                    sheetUPKK.Cells[rowIndexUPKK, 26].Value = data.TAM_KARAWANG;
                    sheetUPKK.Cells[rowIndexUPKK, 27].Value = data.Outsource_KARAWANG;
                    sheetUPKK.Cells[rowIndexUPKK, 28].Value = data.TAM_NGORO;
                    sheetUPKK.Cells[rowIndexUPKK, 29].Value = data.Outsource_NGORO;


                    for (var i = 7; i <= 29; i++)
                    {
                        sheetUPKK.Cells[rowIndexUPKK, i].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    }
                    rowIndexUPKK++;
                }
                #endregion

                #endregion


                // AutoFit Columns
                //sheetUPKK.Cells["A1:X5"].AutoFitColumns();
                //sheetUPKK.Cells["A10:D10"].AutoFitColumns();
                //sheetUPKK.Cells["G10:G11"].AutoFitColumns();
                sheetUPKK.Cells[sheetUPKK.Dimension.Address].AutoFitColumns();

                #endregion


                // Simpan ke MemoryStream
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MedicalRecord.xlsx");
            }
            #endregion
        }

        #endregion
    }


    #endregion

    #region MVC Controller
    [Area(ApplicationModule.OHS)]
    [Permission(PermissionKey.MedicalrecordView)]
    //[Permission(PermissionKey.OHSUPKKView)]
    public class MedicalRecordController : MvcControllerBase
    {
        #region Domain Services
        protected MdmService MdmService => ServiceProxy.GetService<MdmService>();
        protected McuService McuService => ServiceProxy.GetService<McuService>();
        protected UpkkService UpkkService => ServiceProxy.GetService<UpkkService>();
        protected PatientService PatientService => ServiceProxy.GetService<PatientService>();

        #endregion

        public IActionResult Index()
        {
            ViewBag.divisionData = GetDivisionTree();
            return View();
        }
        public IActionResult GetOHSData()
        {
            var data = new List<object>
            {
                new { YearMonth = "Januari", MCU = 10, UPKK = 10, SickLeave = 9, InOutPatient = 4, TanyaOHS = 3 },
                new { YearMonth = "Februari", MCU = 1, UPKK = 5, SickLeave = 6, InOutPatient = 7, TanyaOHS = 2 },
                new { YearMonth = "Maret", MCU = 6, UPKK = 10, SickLeave = 9, InOutPatient = 9, TanyaOHS = 6 },
                new { YearMonth = "April", MCU = 1, UPKK = 8, SickLeave = 6, InOutPatient = 2, TanyaOHS = 1 },
                new { YearMonth = "Mei", MCU = 3, UPKK = 2, SickLeave = 9, InOutPatient = 5, TanyaOHS = 2 },
                new { YearMonth = "Juni", MCU = 1, UPKK = 3, SickLeave = 5, InOutPatient = 4, TanyaOHS = 3 },
                new { YearMonth = "Juli", MCU = 2, UPKK = 5, SickLeave = 6, InOutPatient = 10, TanyaOHS = 8 },
                new { YearMonth = "Agustus", MCU = 6, UPKK = 3, SickLeave = 4, InOutPatient = 5, TanyaOHS = 7 },
                new { YearMonth = "September", MCU = 4, UPKK = 7, SickLeave = 9, InOutPatient = 4, TanyaOHS = 6 },
                new { YearMonth = "Oktober", MCU = 7, UPKK = 3, SickLeave = 7, InOutPatient = 10, TanyaOHS = 9 },
                new { YearMonth = "November", MCU = 3, UPKK = 3, SickLeave = 5, InOutPatient = 7, TanyaOHS = 4 },
                new { YearMonth = "Desember", MCU = 2, UPKK = 2, SickLeave = 8, InOutPatient = 6, TanyaOHS = 4 }
            };

            return Json(data);
        }


        public IActionResult MedicalRecordDetail(string noreg, string startDate, string endDate, string name, string Gender, string Division, int ClassNp)
        {
            var tgllahir = ServiceProxy.GetService<MedicalRecordService>().Gettgllahir(noreg);
            var formatgllahir = tgllahir.Value.ToString("dd/MM/yyyy");
            var now = DateTime.Now;
            if (startDate == null || startDate == "null")
            {
                startDate = "";
                //startDate = new DateTime(now.Year, 1, 1);
            }

            if (endDate == null || endDate == "null")
            {
                endDate = "";
                // endDate = new DateTime(now.Year, 12, 31);
            }

            ViewBag.noreg = noreg;
            ViewBag.startDate = startDate;
            ViewBag.endDate = endDate;
            ViewBag.name = name;
            ViewBag.Gender = Gender;
            ViewBag.Division = Division;
            ViewBag.ClassNp = ClassNp;
            ViewBag.formatgllahir = formatgllahir;




            return View("MedicalRecordDetail");
        }

        [HttpPost]
        public virtual IActionResult LoadMCU(Guid id)//modal pop up
        {
            var commonData = McuService.GetPopUpMcu(id);

            return PartialView("_McuForm", commonData);
            //return PartialView("_McuModal1", commonData);
        }
        [HttpPost]
        public virtual IActionResult LoadUpkk(Guid id)//modal pop up
        {
            var commonData = UpkkService.GetPopUpUpkk(id);

            return PartialView("_UpkkForm", commonData);
            //return PartialView("_McuModal1", commonData);
        }
        [HttpPost]
        public virtual IActionResult LoadPatient(Guid id)//modal pop up
        {
            var commonData = PatientService.GetPopUpPatient(id);

            return PartialView("_PatientForm", commonData);

        }
        public List<DropDownTreeItemModel> GetDivisionTree()
        {
            // Get list of one level hierarchy by noreg, position code, and key date.
            var divisions = ServiceProxy.GetService<PayrollReportService>().GetDivisions();

            // Create new TreeViewItemModel list object.
            var listDropDownTreeItem = new List<DropDownTreeItemModel>();

            // Enumerate through subordinates data.
            foreach (var division in divisions)
            {
                // Create new TreeViewItemModel for each subordinate organization data and add it into the sub-list.
                listDropDownTreeItem.Add(new DropDownTreeItemModel
                {
                    Value = string.Format("{0}#{1}", division.OrgCode, division.ObjectText),
                    Text = division.ObjectText,
                    // Set expanded by default.
                    Expanded = false
                });
            }

            listDropDownTreeItem = listDropDownTreeItem.OrderBy(x => x.Text).ToList();

            // Return partial view with given view model.
            return listDropDownTreeItem;
        }

    }
    #endregion
}

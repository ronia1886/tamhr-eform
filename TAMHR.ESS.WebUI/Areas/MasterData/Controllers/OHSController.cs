using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using TAMHR.ESS.Domain;
using Kendo.Mvc.UI;
using System.Threading.Tasks;
using Kendo.Mvc.Extensions;
using System;
using System.Collections.Generic;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    /// <summary>
    /// Daily Work Schedule API Manager
    /// </summary>
    [Route("api/master-data/ohs")]
    //[Permission(PermissionKey.ManageDailyWorkSchedule)]
    public class OHSApiController : ApiControllerBase
    {
        #region Kategori Penyakit
        //protected override string[] ComparerKeys => new[] { "Id" };
        public KategoriPenyakitService KategoriPenyakitService => ServiceProxy.GetService<KategoriPenyakitService>();

        [HttpGet]
        public IEnumerable<Kategori_PenyakitVIEW> GetsDownloadKp() => KategoriPenyakitService.Gets();

        [HttpPost("gets")]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await KategoriPenyakitService.GetQuery().ToDataSourceResultAsync(request);
        }
        //[HttpPost("gets-kategori-penyakit")]//Grid
        //public async Task<DataSourceResult> GetKategoriPenyakit([DataSourceRequest] DataSourceRequest request)
        //{
        //    var result = await KategoriPenyakitService.getkategoriPenyakit().ToDataSourceResultAsync(request);

        //    return result;
        //}

        [HttpPost("gets-Tingkat-Sakit")]//combobox
        public async Task<DataSourceResult> GetByPlan()
        {
            var CodeP = new List<int> { 11, 12, 13 };

            var nullableIds = CodeP.Cast<int?>().ToList();
            return await KategoriPenyakitService.GetAbsenceByCodePresensi(nullableIds).ToDataSourceResultAsync(new DataSourceRequest());

        }
        [HttpPost("insertKp")]
        public IActionResult CreateKp([FromBody] Kategori_Penyakit kategoripenyakit)
        {
            KategoriPenyakitService.Upsert(kategoripenyakit);
            return CreatedAtAction("Get", new { id = kategoripenyakit.Id });
        }
        [HttpPut("updateKp")]
        public IActionResult UpdateKp([FromBody] Kategori_Penyakit paramKP)
        {
            KategoriPenyakitService.Upsert(paramKP);

            return NoContent();
        }
        [HttpDelete("deleteKp")]
        public IActionResult Delete([FromForm] Guid id)
        {
            KategoriPenyakitService.Delete(id);

            return NoContent();
        }

        [HttpGet("downloadKp")]
        public IActionResult Download()
        {
            var data = GetsDownloadKp();
            var controllerName = RouteData.Values["controller"].ToString();
            var cleanControllerName = controllerName.Substring(0, controllerName.Length - 3);

            return ExportToXlsx(data, $"{cleanControllerName + " Kategori Penyakit"}");
        }
        #endregion
        #region Area
        //protected override string[] ComparerKeys => new[] { "Id" };
        public AreaService AreaService => ServiceProxy.GetService<AreaService>();


        [HttpPost("gets-Grid-Area")]
        public async Task<DataSourceResult> GetGridArea([DataSourceRequest] DataSourceRequest request)
        {
            return await AreaService.GetQuery().ToDataSourceResultAsync(request);
        }
        //[HttpPost("gets-kategori-penyakit")]//Grid
        //public async Task<DataSourceResult> GetKategoriPenyakit([DataSourceRequest] DataSourceRequest request)
        //{
        //    var result = await KategoriPenyakitService.getkategoriPenyakit().ToDataSourceResultAsync(request);

        //    return result;
        //}

        [HttpPost("gets-Divisi")]//combobox
        public async Task<DataSourceResult> GetByDivisi()
        {
            return await AreaService.GetDivisions().ToDataSourceResultAsync(new DataSourceRequest());
        }
        [HttpPost("insertArea")]
        public IActionResult CreateArea([FromBody] AreaTB Param)
        {
            AreaService.Upsert(Param);

            return CreatedAtAction("Get", new { id = Param.Id });
        }
        [HttpPut("updateArea")]
        public IActionResult UpdateArea([FromBody] AreaTB param)
        {
            AreaService.Upsert(param);

            return NoContent();
        }
        [HttpDelete("deleteArea")]
        public IActionResult DeleteArea([FromForm] Guid id)
        {
            AreaService.Delete(id);

            return NoContent();
        }
        [HttpGet("downloadArea")]
        public IActionResult DownloadArea()
        {
            var data = AreaService.GetQuery();
            var controllerName = RouteData.Values["controller"].ToString();
            var cleanControllerName = controllerName.Substring(0, controllerName.Length - 3);

            return ExportToXlsx(data, $"{cleanControllerName + " Area"}");
        }
        #endregion
        #region Equipment

        public EquipmentService EquipmentService => ServiceProxy.GetService<EquipmentService>();


        [HttpPost("gets-Grid-Equipment")]
        public async Task<DataSourceResult> GetGridEquipment([DataSourceRequest] DataSourceRequest request)
        {
            return await EquipmentService.GetQuery().ToDataSourceResultAsync(request);
        }
        //[HttpPost("gets-kategori-penyakit")]//Grid
        //public async Task<DataSourceResult> GetKategoriPenyakit([DataSourceRequest] DataSourceRequest request)
        //{
        //    var result = await KategoriPenyakitService.getkategoriPenyakit().ToDataSourceResultAsync(request);

        //    return result;
        //}

        //[HttpPost("gets-Divisi")]//combobox
        //public async Task<DataSourceResult> GetByDivisi()
        //{
        //    return await AreaService.GetDivisions().ToDataSourceResultAsync(new DataSourceRequest());
        //}
        [HttpPost("gets-AreaDropDown")]//combobox
        public async Task<DataSourceResult> GetByArea()
        {
            return await AreaService.GetAreaDropDown().ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("insertEquipment")]
        public IActionResult CreateEquipment([FromBody] Equipment Param)
        {
            EquipmentService.Upsert(Param);

            return CreatedAtAction("Get", new { id = Param.Id });
        }
        [HttpPut("updateEquipment")]
        public IActionResult UpdateEquipment([FromBody] Equipment param)
        {
            EquipmentService.Upsert(param);

            return NoContent();
        }
        [HttpDelete("DeleteEquipment")]
        public IActionResult DeleteEquipment([FromForm] Guid id)
        {
            EquipmentService.Delete(id);

            return NoContent();
        }
        [HttpGet("downloadEquipment")]
        public IActionResult DownloadEquipment()
        {
            var data = EquipmentService.GetQuery();
            var controllerName = RouteData.Values["controller"].ToString();
            var cleanControllerName = controllerName.Substring(0, controllerName.Length - 3);

            return ExportToXlsx(data, $"{cleanControllerName + " Equipment"}");
        }

        #endregion
        #region UPKK/KLINIK

        public KlinikService KlinikService => ServiceProxy.GetService<KlinikService>();
        protected EmployeeProfileService employeeProfileService => ServiceProxy.GetService<EmployeeProfileService>();


        [HttpPost("gets-Grid-Klinik")]
        public async Task<DataSourceResult> GetGridKlinik([DataSourceRequest] DataSourceRequest request)
        {
            return await KlinikService.GetQuery().ToDataSourceResultAsync(request);
        }
        //[HttpPost("gets-kategori-penyakit")]//Grid
        //public async Task<DataSourceResult> GetKategoriPenyakit([DataSourceRequest] DataSourceRequest request)
        //{
        //    var result = await KategoriPenyakitService.getkategoriPenyakit().ToDataSourceResultAsync(request);

        //    return result;
        //}

        //[HttpPost("gets-Divisi")]//combobox
        //public async Task<DataSourceResult> GetByDivisi()
        //{
        //    return await AreaService.GetDivisions().ToDataSourceResultAsync(new DataSourceRequest());
        //}
        //[HttpPost("gets-AreaDropDown")]//combobox
        //public async Task<DataSourceResult> GetByAreaFromUPKK()
        //{
        //    return await AreaService.GetAreaDropDown().ToDataSourceResultAsync(new DataSourceRequest());
        //}
        [HttpPost("get-unique-column-values-name")]
        public async Task<DataSourceResult> GetUniqueColumnValuesName()
        {
            return await KlinikService.GetUserNameKilink().ToDataSourceResultAsync(new DataSourceRequest());

        }

        [HttpPost("get-category-name")]
        public async Task<DataSourceResult> GetcategoryName()
        {
            return await KlinikService.GetcategoryName().ToDataSourceResultAsync(new DataSourceRequest());

        }

        [HttpPost("insertKlinik")]
        public IActionResult CreateKlinik([FromBody] KlinikTB Param)
        {
            KlinikService.Upsert(Param);

            return CreatedAtAction("Get", new { id = Param.Id });
        }
        [HttpPut("updateKlinik")]
        public IActionResult UpdateKlinik([FromBody] KlinikTB param)
        {
            KlinikService.Upsert(param);

            return NoContent();
        }
        [HttpDelete("DeleteKlinik")]
        public IActionResult DeleteKlinik([FromForm] Guid id)
        {
            KlinikService.Delete(id);

            return NoContent();
        }
        [HttpGet("downloadKlinik")]
        public IActionResult DownloadKlinik()
        {
            var data = KlinikService.GetQuery();
            var controllerName = RouteData.Values["controller"].ToString();
            var cleanControllerName = controllerName.Substring(0, controllerName.Length - 3);

            return ExportToXlsx(data, $"{cleanControllerName + " UPKK"}");
        }

        #endregion

    }

    #endregion

    #region MVC Controller
    /// <summary>
    /// OHS Master Data page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.MasterDataOHSView)]
    public class OHSController : MvcControllerBase
    {

        //protected KategoriPenyakitService KategoriPenyakitService { get { return ServiceProxy.GetService<KategoriPenyakitService>(); } }
        protected KategoriPenyakitService KategoriPenyakitService => ServiceProxy.GetService<KategoriPenyakitService>();
        protected AreaService AreaService { get { return ServiceProxy.GetService<AreaService>(); } }
        protected EquipmentService EquipmentService => ServiceProxy.GetService<EquipmentService>();
        protected KlinikService KlinikService => ServiceProxy.GetService<KlinikService>();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public virtual IActionResult LoadKategoriPenyakit(Guid id)//modal pop up
        {
            var commonData = KategoriPenyakitService.GetPopUpKategoriPenyakit(id);

            return PartialView("_KategoriPenyakitForm", commonData);
        }
        [HttpPost]
        public virtual IActionResult LoadArea(Guid id)//modal pop up
        {
            var commonData = AreaService.GetPopUpArea(id);

            return PartialView("_AreaForm", commonData);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpPost]
        public IActionResult LoadEquipment(Guid id)//modal pop up
        {
            var commonData = EquipmentService.GetPopUpEquipment(id);

            return PartialView("_EquipmentForm", commonData);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [HttpPost]
        public IActionResult LoadKlinik(Guid id)//modal pop up
        {
            var commonData = KlinikService.GetPopUpKlinik(id);

            return PartialView("_KlinikForm", commonData);
        }
    }
    #endregion
}
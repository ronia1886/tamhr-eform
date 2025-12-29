using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    #region API Controller
    /// <summary>
    /// Vehicle API Manager
    /// </summary>
    [Route("api/vehicle")]
    public class VehicleApiController : GenericApiControllerBase<VehicleService, Vehicle>
    {
        protected override string[] ComparerKeys => new[] { "ModelCode", "Suffix" };

        #region Domain Services
        /// <summary>
        /// MDM service object
        /// </summary>
        public MdmService MdmService => ServiceProxy.GetService<MdmService>();
        #endregion

        /// <summary>
        /// Get list of vehicles
        /// </summary>
        /// <remarks>
        /// Get list of vehicles
        /// </remarks>
        /// <returns>List of Vehicles</returns>
        [HttpPost("typecpp")]
        public async Task<DataSourceResult> GetTypeCpp([DataSourceRequest] DataSourceRequest request)
        {
            return await CoreService.GetTypeCPP().ToDataSourceResultAsync(request);
        }

        [HttpPost("typecop")]
        public async Task<DataSourceResult> GetFromPosts()
        {
            return await CoreService.GetTypeCOP().ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getmodelcop")]
        public async Task<DataSourceResult> GetModelCop()
        {
            string kelas = this.Request.Form["kelas"].ToString();
             var checkEmployeeSubgroup = true;

            if (Request.Form.ContainsKey("checkEmployeeSubgroup"))
            {
             checkEmployeeSubgroup = bool.Parse(Request.Form["checkEmployeeSubgroup"]);
             }

             var obj = checkEmployeeSubgroup
            ? CoreService.GetTypeNameVehicleCop(kelas)
            : CoreService.GetTypeNameAllVehicleCop();

            return await obj.ToDataSourceResultAsync(new DataSourceRequest());
            //return await CoreService.GetTypeNameAllVehicleCop().ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getmodelcpp")]
        public async Task<DataSourceResult> GetModelCpp()
        {
            string noreg = this.Request.Form["noreg"].ToString();

            return await CoreService.GetVehicleCpp(noreg).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getmodelscp")]
        public async Task<DataSourceResult> GetModelScp()
        {
            string noreg = this.Request.Form["noreg"].ToString();

            return await CoreService.GetVehicleScp(noreg).ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("gettypebymodel")]
        public async Task<DataSourceResult> GettypebyModel()
        {
            var noreg = this.Request.Form["noreg"].ToString();
            var model = this.Request.Form["model"].ToString();
            var type = this.Request.Form["type"].ToString();
            var checkEmployeeSubgroup = true;

            if (Request.Form.ContainsKey("checkEmployeeSubgroup"))
            {
                checkEmployeeSubgroup = bool.Parse(Request.Form["checkEmployeeSubgroup"]);
            }

            var actualOrganizationStructure = MdmService.GetActualOrganizationStructure(noreg);
            var employeeSubgroup = actualOrganizationStructure.EmployeeSubgroup;

            var obj = checkEmployeeSubgroup
                ? CoreService.GetTypeVehicleByModel(employeeSubgroup, model, type)
                : CoreService.GetTypeVehicles(model, type);

            return await obj.ToDataSourceResultAsync(new DataSourceRequest());
        }

        [HttpPost("getdefaultbyclass")]
        public object GetModelByClass()
        {
            string kelas = this.Request.Form["class"].ToString();
            return CoreService.GetTypeVehicleByKelas(kelas);
        }

        [HttpPost("getbyid")]
        public object GetById()
        {
            System.Guid id = System.Guid.Parse(this.Request.Form["id"].ToString());
            return CoreService.GetById(id);
        }

        /// <summary>
        /// Get list of colors by vehicle id
        /// </summary>
        /// <returns>List of Colors</returns>
        [HttpPost("getcolor")]
        public async Task<DataSourceResult> GetColorById()
        {
            var id = Guid.Parse(Request.Form["id"].ToString());
            var colorCode = Request.Form["colorCode"].ToString();
            var dataColor = CoreService.GetVehicleById(id)?.Colors;

            var listColor = dataColor == null ? new List<string>() : dataColor.Split(',').ToList();

            if (!dataColor.Contains(colorCode))
            {
                listColor.Add(colorCode);
            }

            var configColor = ConfigService.GetGeneralCategories("carColor");

            return await configColor.Where(x => listColor.Contains(x.Code)).ToDataSourceResultAsync(new DataSourceRequest());
        }

        /// <summary>
        /// Get vehicle matrix by id
        /// </summary>
        /// <returns>Vehicle Matrix Object</returns>
        [HttpPost("getmatrixbyid")]
        public object GetMatrixById()
        {
            var id = Guid.Parse(Request.Form["id"].ToString());
            var kelas = Request.Form["class"].ToString();

            return CoreService.GetTypeVehicleMatrixById(id, kelas);
        }

        /// <summary>
        /// Get list of vehicles by matrix
        /// </summary>
        /// <param name="request">Data Source Request Object</param>
        /// <returns>List of Vehicles by Matrix</returns>
        [HttpPost("getbymatrix")]
        public async Task<DataSourceResult> GetByMatrix([DataSourceRequest] DataSourceRequest request)
        {
            var id = Guid.Parse(Request.Form["id"]);

            return await CommonService.GetMatrix(id).ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Create vehicle and matrix
        /// </summary>
        /// <param name="input">Vehicle Object</param>
        public override IActionResult Create([FromBody] Vehicle input)
        {
            var output = base.Create(input);

            CommonService.SaveMatrix(input);

            return output;
        }

        /// <summary>
        /// Update vehicle and matrix
        /// </summary>
        /// <param name="input">Vehicle Object</param>
        public override IActionResult Update([FromBody] Vehicle input)
        {
            var output = base.Update(input);

            CommonService.SaveMatrix(input);

            return output;
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Vehicle page controller
    /// </summary>
    [Area("MasterData")]
    [Permission(PermissionKey.ViewVehicle)]
    public class VehicleController : GenericMvcControllerBase<VehicleService, Vehicle>
    {
        [HttpPost]
        public override IActionResult Load(Guid id)
        {
            object commonData;

            if (id == Guid.Empty)
            {
                commonData = new Vehicle();
            }
            else
            {
                commonData = CommonService.GetById(id);
            }

            return GetViewData(commonData);
        }
    }
    #endregion
}
using Microsoft.AspNetCore.Mvc;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using TAMHR.ESS.Domain;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;

namespace TAMHR.ESS.WebUI.Areas.MasterData.Controllers
{
    // ===================== API Controller =====================
    [Route("api/employee-annual-leave")]
    [Permission(PermissionKey.ManageEmployeeAnnualLeave)]
    public class EmployeeAnnualLeaveApiController
        : GenericApiControllerBase<EmployeeAnnualLeaveService, EmployeeAnnualLeave>
    {
        protected override string[] ComparerKeys => new[] { "NoReg", "Period" };
        public EmployeeAnnualLeaveService EmployeeAnnualLeaveService
            => ServiceProxy.GetService<EmployeeAnnualLeaveService>();

        [HttpPost("getbynoreg")]
        public virtual async Task<DataSourceResult> GetBynoreg(
            [FromForm] string noreg,
            [DataSourceRequest] DataSourceRequest request)
        {
            return await EmployeeAnnualLeaveService
                .GetByNoreg(noreg)
                .ToDataSourceResultAsync(request);
        }

        [HttpPost("getsannualleave")]
        public EmployeeAnnualLeave GetAnnualLeavePeriod(
            [FromForm] string noreg,
            [FromForm] int period,
            [DataSourceRequest] DataSourceRequest request)
        {
            return EmployeeAnnualLeaveService.GetByNoreg(noreg, period);
        }

        [HttpPost("gets")]
        public override async Task<DataSourceResult> GetFromPosts(
            [DataSourceRequest] DataSourceRequest request)
        {
            // Materialize agar GroupBy/First dieksekusi di memory (bukan SQL)
            var data = await EmployeeAnnualLeaveService.GetQuery()
                .OrderByDescending(x => x.Period)
                .ToListAsync();

            var grouped = data
                .GroupBy(x => new { x.Name, x.NoReg })
                .Select(g => g.OrderByDescending(e => e.Period).First())
                .OrderBy(e => e.Name);

            return grouped.ToDataSourceResult(request);
        }

        [HttpDelete("delete")]
        public IActionResult Delete([FromForm] Guid id)
        {
            EmployeeAnnualLeaveService.Delete(id);
            return NoContent();
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody] EmployeeAnnualLeave input)
        {
            // Guid bukan nullable -> gunakan Guid.Empty
            if (input.Id == Guid.Empty)
                input.CreatedBy = ServiceProxy.UserClaim.NoReg;
            else
                input.ModifiedBy = ServiceProxy.UserClaim.NoReg;

            EmployeeAnnualLeaveService.Upsert(input);
            return NoContent();
        }
    }

    // ===================== MVC Controller (halaman) =====================
    [Area("MasterData")]
    [Permission(PermissionKey.ViewEmployeeAnnualLeave)]
    public class EmployeeAnnualLeaveController
        : GenericMvcControllerBase<EmployeeAnnualLeaveService, EmployeeAnnualLeave>
    {
    }
}

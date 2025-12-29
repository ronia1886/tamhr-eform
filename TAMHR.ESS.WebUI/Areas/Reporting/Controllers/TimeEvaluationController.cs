using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.SignalR;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using TAMHR.ESS.Infrastructure.Web.Configurations;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Agit.Common;
using Kendo.Mvc.UI;

namespace TAMHR.ESS.WebUI.Areas.Reporting.Controllers
{
    #region API Controller
    /// <summary>
    /// Api controller for time evaluation
    /// </summary>
    [Route("api/time-evaluation")]
    [Permission(PermissionKey.RunningTimeEvaluation)]
    public class TimeEvaluationApiController : ApiControllerBase
    {
        private static object _completed = true;
        protected TimeEvaluationService TimeEvaluationService => ServiceProxy.GetService<TimeEvaluationService>();

        private IHubContext<ChatHub> _hubContext;
        private IOptions<ApplicationConfiguration> _applicationConfiguration;

        public TimeEvaluationApiController(IHubContext<ChatHub> hubContext, IOptions<ApplicationConfiguration> applicationConfiguration)
            : base()
        {
            _hubContext = hubContext;
            _applicationConfiguration = applicationConfiguration;
        }

        /// <summary>
        /// Get list of tasks by username
        /// </summary>
        /// <remarks>
        /// Get list of tasks by username
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        public DataSourceResult GetFromPosts([FromForm] int month, [FromForm] int year, [DataSourceRequest] DataSourceRequest request)
        {
            return ServiceProxy.GetTableValuedDataSourceResult<TimeEvaluationStoredEntity>(request, new { month, year });
        }

        [HttpGet("download/{jobId}")]
        public IActionResult Download(string jobId)
        {
            byte[] data = null;
            var tempPath = ConfigService.GetConfigValue<string>("Upload.Path");
            var safeJobId = Path.GetFileName(jobId);

            using (var ms = new MemoryStream())
            {
                using (var fs = new FileStream(Path.Combine(tempPath, safeJobId + ".zip"), FileMode.Open))
                {
                    fs.CopyTo(ms);
                    data = ms.ToArray();
                }
            }

            var fileName = string.Format("TIME_EVALUATION_{0:ddMMyyyy}.zip", DateTime.Now);
            var attachmentHeader = string.Format(@"attachment; filename=""{0}""", fileName);

            Response.Headers.Clear();
            Response.ContentType = "application/zip";
            Response.Headers.Add("content-disposition", attachmentHeader);
            Response.Headers.Add("fileName", fileName);

            return new FileContentResult(data, "application/zip");
        }

        [HttpGet("overtime-evaluations/{month}/{year}")]
        public IActionResult Download(int month, int year)
        {
            var data = TimeEvaluationService.GenerateOvertimeEvaluations(month, year);

            return ExportToXlsx(data, $"OVERTIME_EVALUATION_{DateTime.Now.ToString("ddMMyyyy")}.xlsx");
        }

        [HttpPost("gen-proxy/{jobId}/{from}/{to}")]
        public async Task<IActionResult> GenerateProxy(string jobId, DateTime from, DateTime to)
        {
            return await Task.Run(async () =>
            {
                var start = from;
                var total = (to - start).TotalDays;
                var progress = 0;

                while (start <= to)
                {
                    TimeEvaluationService.GenerateProxy(start, start);
                    progress++;
                    await _hubContext.Clients.Group(jobId).SendAsync("progress", progress / (decimal)total * 100, "Ready to go");

                    start = start.AddDays(1);
                }

                return NoContent();
            });
        }

        [HttpPost("long-running/{jobId}/{keyDate}")]
        public async Task<IActionResult> SubmitLongRunning(string jobId, DateTime keyDate)
        {
            return await Task.Run(() =>
            {
                var year = keyDate.Year;
                var month = keyDate.Month;

                TimeEvaluationService.GenerateTimeEvaluation(Guid.NewGuid(), year, month);

                var absencesEntry = TimeEvaluationService.GenerateAbsences(keyDate);
                var timeEventsEntry = TimeEvaluationService.GenerateTimeEvents(keyDate);
                var attendencesEntry = TimeEvaluationService.GenerateAttendances(keyDate);
                var overtimeEntry = TimeEvaluationService.GenerateOvertime(keyDate);
                var overtimeCrossEntry = TimeEvaluationService.GenerateOvertimeCrossDay(keyDate);
                var shiftEntry = TimeEvaluationService.GenerateShift(keyDate);
                //var bdjkEntry = TimeEvaluationService.GenerateBdjk(year, month);
                //var bdjkEntry = TimeEvaluationService.GenerateBdjk(keyDate);
                var safeJobId = Path.GetFileName(jobId);
                var tempPath = ConfigService.GetConfigValue<string>("Upload.Path");
                var filePath = Path.Combine(tempPath, safeJobId + ".zip");
                var zipManager = new ZipManager(new[] { absencesEntry, timeEventsEntry, attendencesEntry, overtimeEntry, overtimeCrossEntry, shiftEntry});

                try
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                catch { }

                using (var ms = new MemoryStream(zipManager.ToZip()))
                {
                    using (var fileStream = new FileStream(filePath, FileMode.OpenOrCreate))
                    {
                        ms.WriteTo(fileStream);
                    }
                }

                return NoContent();
            });
        }
    }
    #endregion

    #region MVC Controller
    [Area("Reporting")]
    [Permission(PermissionKey.ViewTimeEvaluation)]
    public class TimeEvaluationController : MvcControllerBase
    {
        public IActionResult Index()
        {
            return View();
        }
    }
    #endregion
}
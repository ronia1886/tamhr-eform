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

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    /// <summary>
    /// News API Manager
    /// </summary>
    [Route("api/eventscalendar")]
    [Permission(PermissionKey.ManageEventsCalendar)]
    public class EventsCalendarApiController : ApiControllerBase
    {
        /// <summary>
        /// Get list of events calendars
        /// </summary>
        /// <remarks>
        /// Get list of events calendars
        /// </remarks>
        /// <returns>List of Events Calendars</returns>
        [HttpGet]
        public IEnumerable<EventsCalendar> Gets() => CoreService.GetEventsCalendars();

        /// <summary>
        /// Get events calendar by id
        /// </summary>
        /// <remarks>
        /// Get events calendar by id
        /// </remarks>
        /// <param name="id">Events Calendar Id</param>
        /// <returns>Events Calendar Object</returns>
        [HttpGet("{id}")]
        public EventsCalendar Get(Guid id) => CoreService.GetEventsCalendar(id);

        /// <summary>
        /// Get list of events calendars
        /// </summary>
        /// <remarks>
        /// Get list of events calendars
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        public async Task<DataSourceResult> GetFromPosts([FromForm] int year, [DataSourceRequest] DataSourceRequest request)
        {
            return await CoreService.GetEventsCalendars(year).ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Create new events calendar
        /// </summary>
        /// <remarks>
        /// Create new events calendar
        /// </remarks>
        /// <param name="eventsCalendar">Events Calendar Object</param>
        /// <returns>Created Events Calendar Object</returns>
        [HttpPost]
        public IActionResult Create([FromBody] EventsCalendar eventsCalendar)
        {
            CoreService.UpsertEventsCalendar(eventsCalendar);

            return CreatedAtAction("Get", new { id = eventsCalendar.Id });
        }

        /// <summary>
        /// Update events calendar
        /// </summary>
        /// <remarks>
        /// Update events calendar
        /// </remarks>
        /// <param name="eventsCalendar">Events Calendar Object</param>
        /// <returns>No Content</returns>
        [HttpPut]
        public IActionResult Update([FromBody]EventsCalendar eventsCalendar)
        {
            CoreService.UpsertEventsCalendar(eventsCalendar);

            return NoContent();
        }

        /// <summary>
        /// Delete events calendar by id
        /// </summary>
        /// <remarks>
        /// Delete events calendar by id
        /// </remarks>
        /// <param name="id">Events Calendar Id</param>
        /// <returns>No Content</returns>
        [HttpDelete]
        public IActionResult Delete([FromForm]Guid id)
        {
            CoreService.DeleteEventsCalendar(id);

            return NoContent();
        }

        /// <summary>
        /// Download data
        /// </summary>
        /// <remarks>
        /// Download data
        /// </remarks>
        /// <returns>Data in Excel Format</returns>
        [HttpGet("download")]
        public IActionResult Download()
        {
            var data = Gets();

            return ExportToXlsx(data, $"EventsCalendar_{DateTime.Now.ToString("dd_MM_yyyy")}.xlsx");
        }

        /// <summary>
        /// Download data template
        /// </summary>
        /// <remarks>
        /// Download data template
        /// </remarks>
        /// <returns>Data Template in Excel Format</returns>
        [HttpGet("download-template")]
        public virtual IActionResult DownloadTemplate()
        {
            var controllerName = RouteData.Values["controller"].ToString();
            var cleanControllerName = controllerName.Substring(0, controllerName.Length - 3);

            return GenerateTemplate<EventsCalendar>($"{cleanControllerName}");
        }

        /// <summary>
        /// Upload and merge data
        /// </summary>
        /// <remarks>
        /// Upload and merge data
        /// </remarks>
        /// <returns>No Content</returns>
        [HttpPost("merge")]
        public async Task<IActionResult> Merge()
        {
            await UploadAndMergeAsync<EventsCalendar>(Request.Form.Files.FirstOrDefault(), new[] { "Title", "StartDate" });

            return NoContent();
        }
    }
    #endregion
}
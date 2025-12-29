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
    /// Email Template API Manager
    /// </summary>
    [Route("api/email-template")]
    [Permission(PermissionKey.ManageEmailTemplate)]
    public class EmailTemplateApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Email service object
        /// </summary>
        public EmailService EmailService => ServiceProxy.GetService<EmailService>();
        #endregion

        /// <summary>
        /// Get list of email templates
        /// </summary>
        /// <remarks>
        /// Get list of email templates
        /// </remarks>
        /// <returns>List of Email Templates</returns>
        [HttpGet]
        public IEnumerable<EmailTemplate> Gets() => CoreService.GetEmailTemplates();

        /// <summary>
        /// Get email template by id
        /// </summary>
        /// <remarks>
        /// Get email template by id
        /// </remarks>
        /// <param name="id">Email Template Id</param>
        /// <returns>Email Template Object</returns>
        [HttpGet("{id}")]
        public EmailTemplate Get(Guid id) => CoreService.GetEmailTemplate(id);

        /// <summary>
        /// Get list of email templates
        /// </summary>
        /// <remarks>
        /// Get list of email templates
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            return await CoreService.GetEmailTemplates().ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Create new email template
        /// </summary>
        /// <remarks>
        /// Create new email template
        /// </remarks>
        /// <param name="emailTemplate">Email Template Object</param>
        /// <returns>Created Email Template Object</returns>
        [HttpPost]
        public IActionResult Create([FromBody] EmailTemplate emailTemplate)
        {
            CoreService.UpsertEmailTemplate(emailTemplate);

            return CreatedAtAction("Get", new { id = emailTemplate.Id });
        }

        /// <summary>
        /// Send BPKB reminder
        /// </summary>
        /// <remarks>
        /// Send BPKB reminder
        /// </remarks>
        /// <returns>No Content</returns>
        [HttpPost("send-bpkb-reminder")]
        public async Task<IActionResult> SendBpkbReminder()
        {
            await EmailService.SendGetBpkbReminderAsync();

            return NoContent();
        }

        /// <summary>
        /// Send STNK reminder
        /// </summary>
        /// <remarks>
        /// Send STNK reminder
        /// </remarks>
        /// <returns>No Content</returns>
        [HttpPost("send-stnk-reminder")]
        public async Task<IActionResult> SendStnkReminder()
        {
            await EmailService.SendBpkbDateReminderAsync();

            return NoContent();
        }

        /// <summary>
        /// Update email template
        /// </summary>
        /// <remarks>
        /// Update email template
        /// </remarks>
        /// <param name="emailTemplate">Email Template Object</param>
        /// <returns>No Content</returns>
        [HttpPut]
        public IActionResult Update([FromBody]EmailTemplate emailTemplate)
        {
            CoreService.UpsertEmailTemplate(emailTemplate);

            return NoContent();
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

            return GenerateTemplate<EmailTemplate>($"{cleanControllerName}");
        }

        /// <summary>
        /// Delete email template by id
        /// </summary>
        /// <remarks>
        /// Delete email template by id
        /// </remarks>
        /// <param name="id">Email Template Id</param>
        /// <returns>No Content</returns>
        [HttpDelete]
        public IActionResult Delete([FromForm]Guid id)
        {
            CoreService.DeleteEmailTemplate(id);

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

            return ExportToXlsx(data, $"EmailTemplate_{DateTime.Now.ToString("dd_MM_yyyy")}.xlsx");
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
            await UploadAndMergeAsync<EmailTemplate>(Request.Form.Files.FirstOrDefault(), new[] { "MailKey" });

            return NoContent();
        }
    }
    #endregion
}
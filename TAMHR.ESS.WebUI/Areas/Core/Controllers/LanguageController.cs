using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Localization;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Authorization;
using static TAMHR.ESS.Infrastructure.ApplicationConstants;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;
using System.Data.Entity;
using Microsoft.Extensions.Options;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;


namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    /// <summary>
    /// Language API Manager
    /// </summary>
    [Route("api/language")]
    [Permission(PermissionKey.ManageLanguage)]
    public class LanguageApiController : ApiControllerBase
    {
        /// <summary>
        /// Get list of languages
        /// </summary>
        /// <remarks>
        /// Get list of languages
        /// </remarks>
        /// <returns>List of Languages</returns>
        [HttpGet]
        public IEnumerable<Language> Gets() => CoreService.GetLanguagesQuery();

        /// <summary>
        /// Get language by id
        /// </summary>
        /// <remarks>
        /// Get language by id
        /// </remarks>
        /// <param name="id">Language Id</param>
        /// <returns>Language Object</returns>
        [HttpGet("{id}")]
        public Language Get(Guid id) => CoreService.GetLanguage(id);

        /// <summary>
        /// Override trigger after callback action (auto sync with localizer cache).
        /// </summary>
        /// <param name="actionType">This action type enum.</param>
        /// <param name="input">This <see cref="Language"/> object.</param>
        private void TriggerAfter(ActionType actionType, Language input)
        {
            // Get and set localizer factory from DI container.
            var localizerFactory = ServiceProxy.GetLocalizerFactory();

            // Get and set localizer key.
            var key = input.CultureCode + "|" + input.TranslateKey;

            // If action is not delete then update the localizer value by given key.
            if (actionType != ActionType.Delete)
            {
                // Update localizer value by given key.
                localizerFactory.Update(key, input.TranslateValue);
            }
            // Else remove localizer by key.
            else
            {
                // Remove localizer by key.
                localizerFactory.Remove(key);
            }
        }

        /// <summary>
        /// Get list of languages
        /// </summary>
        /// <remarks>
        /// Get list of languages
        /// </remarks>
        /// <param name="request">DataSourceRequest Object</param>
        /// <returns>DataSourceResult Object</returns>
        [HttpPost("gets")]
        public async Task<DataSourceResult> GetFromPosts([DataSourceRequest] DataSourceRequest request)
        {
            //return await CoreService.GetLanguagesQuery()
            //    .ToDataSourceResultAsync(request);

            var data = CoreService.GetLanguagesQuery()
                            .AsNoTracking()
                            .ToList();

            return await data.ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// Create new language
        /// </summary>
        /// <remarks>
        /// Create new language
        /// </remarks>
        /// <param name="language">Language Object</param>
        /// <returns>Created Language Object</returns>
        [HttpPost]
        public IActionResult Create([FromBody] Language language)
        {
            CoreService.UpsertLanguage(language);

            TriggerAfter(ActionType.Create, language);

            return CreatedAtAction("Get", new { id = language.Id });
        }

        /// <summary>
        /// Update language
        /// </summary>
        /// <remarks>
        /// Update language
        /// </remarks>
        /// <param name="language">Language Object</param>
        /// <returns>No Content</returns>
        [HttpPut]
        public IActionResult Update([FromBody]Language language)
        {
            CoreService.UpsertLanguage(language);

            TriggerAfter(ActionType.Update, language);

            return NoContent();
        }

        /// <summary>
        /// Delete language by id
        /// </summary>
        /// <remarks>
        /// Delete language by id
        /// </remarks>
        /// <param name="id">Language Id</param>
        /// <returns>No Content</returns>
        [HttpDelete]
        public IActionResult Delete([FromForm]Guid id)
        {
            var language = CoreService.GetLanguage(id);

            CoreService.DeleteLanguage(id);

            TriggerAfter(ActionType.Delete, language);

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

            return ExportToXlsx(data, $"Language_{DateTime.Now.ToString("dd_MM_yyyy")}.xlsx");
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

            return GenerateTemplate<Language>($"{cleanControllerName}");
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
            await UploadAndMergeAsync<Language>(Request.Form.Files.FirstOrDefault(), new[] { "CultureCode", "TranslateKey" });

            return NoContent();
        }
    }
    #endregion

    #region MVC Controller
    /// <summary>
    /// Language controller
    /// </summary>
    [Area("Core")]
    public class LanguageController : MvcControllerBase
    {
        /// <summary>
        /// Set language by specific ui culture
        /// </summary>
        /// <param name="culture">UI Culture</param>
        /// <param name="returnUrl">Return Url</param>
        /// <returns>Redirect Result Object</returns>
        [Permission(PermissionKey.SetLanguage)]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            var allowedCultures = new[] { "en-US", "id-ID" };
            var safeCulture = "";
            if (culture == "id-ID")
            {
                safeCulture = "id-ID";
            } else
            {
                safeCulture = "en-US";
            }
            var raw = returnUrl ?? "";
            var decoded = WebUtility.UrlDecode(raw);

            string cleanReturnUrl;

            if (string.IsNullOrWhiteSpace(decoded))
            {
                cleanReturnUrl = "~/";
            }
            else if (Url.IsLocalUrl(decoded))
            {
                cleanReturnUrl = decoded;
            }
            else
            {
                if (Uri.TryCreate(decoded, UriKind.Absolute, out var uri))
                {
                    var currentHost = $"{Request.Scheme}://{Request.Host}";

                    if (uri.Scheme == Request.Scheme && uri.Host == Request.Host.Host)
                    {
                        cleanReturnUrl = uri.PathAndQuery;
                    }
                    else
                    {
                        cleanReturnUrl = "~/";
                    }
                }
                else
                {
                    cleanReturnUrl = "~/";
                }
            }

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(
                    new RequestCulture(safeCulture, safeCulture)
                ),
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                }
            );

            return Redirect(cleanReturnUrl);
        }

    }
    #endregion
}
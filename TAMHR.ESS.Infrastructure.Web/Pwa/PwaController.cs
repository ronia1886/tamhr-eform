using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using TAMHR.ESS.Infrastructure.Web.Pwa.Manifest;

namespace TAMHR.ESS.Infrastructure.Web.Pwa
{
    /// <summary>
    /// A controller for manifest.webmanifest, serviceworker.js and offline.html
    /// </summary>
    public class PwaController : Controller
    {
        private readonly PwaOptions _options;
        private readonly IUrlHelper _urlHelper;

        /// <summary>
        /// Creates an instance of the controller.
        /// </summary>
        public PwaController(PwaOptions options, IUrlHelper urlHelper)
        {
            _options = options;
            _urlHelper = urlHelper;
        }

        /// <summary>
        /// Serves a service worker based on the provided settings.
        /// </summary>
        [Route(Constants.ServiceworkerRoute)]
        public async Task<IActionResult> ServiceWorkerAsync()
        {
            Response.ContentType = "application/javascript; charset=utf-8";
            Response.Headers[HeaderNames.CacheControl] = $"max-age={_options.ServiceWorkerCacheControlMaxAge}";

            string fileName = _options.Strategy + ".js";
            Assembly assembly = typeof(PwaController).Assembly;
            Stream resourceStream = assembly.GetManifestResourceStream($"TAMHR.ESS.Infrastructure.Web.Pwa.ServiceWorker.Files.{fileName}");

            using (var reader = new StreamReader(resourceStream))
            {
                string js = await reader.ReadToEndAsync();
                string modified = js
                    .Replace("{version}", _options.CacheId + "::" + _options.Strategy)
                    .Replace("{routes}", string.Join(",", _options.RoutesToPreCache.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(r => "'" + _urlHelper.Content(r.StartsWith("~/") ? r.Trim() : $"~{r.Trim()}") + "'")))
                    .Replace("{offlineRoute}", !string.IsNullOrEmpty(_options.OfflineRoute) ? _urlHelper.Content(_options.OfflineRoute.StartsWith("~/") ? _options.OfflineRoute : $"~{_options.OfflineRoute}") : string.Empty);

                return View(modified);
            }
        }

        /// <summary>
        /// Serves the offline.html file
        /// </summary>
        [Route(Constants.Offlineroute)]
        public async Task<IActionResult> OfflineAsync()
        {
            Response.ContentType = "text/html";

            Assembly assembly = typeof(PwaController).Assembly;
            Stream resourceStream = assembly.GetManifestResourceStream("TAMHR.ESS.Infrastructure.Web.Pwa.ServiceWorker.Files.offline.html");

            using (var reader = new StreamReader(resourceStream))
            {
                return View(await reader.ReadToEndAsync());
            }
        }

        /// <summary>
        /// Serves the offline.html file
        /// </summary>
        [Route(Constants.WebManifestRoute)]
        public IActionResult WebManifest([FromServices] WebManifest wm)
        {
            if (wm == null)
            {
                return NotFound();
            }

            Response.ContentType = "application/manifest+json; charset=utf-8";


            Response.Headers[HeaderNames.CacheControl] = $"max-age={_options.WebManifestCacheControlMaxAge}";

            return new JsonResult(wm.RawJson)
            {
                ContentType = "application/manifest+json; charset=utf-8"
            };
            //return View(wm.RawJson);
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rotativa.AspNetCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Helpers;
using TAMHR.ESS.Infrastructure.Web;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    [Area("Core")]
    public class DefaultController : MvcControllerBase
    {
        public ApprovalService ApprovalService => ServiceProxy.GetService<ApprovalService>();

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Get content type by filename
        /// </summary>
        /// <param name="path">Filename Path</param>
        /// <returns>Content Type</returns>
        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();

            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }

        /// <summary>
        /// Get list of MIME type
        /// </summary>
        /// <returns>List of MIME Type</returns>
        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
        }

        public async Task<IActionResult> Avatar(string p)
        {
            var secretKeyConfig = ConfigService.GetConfig("Avatar.SecretKey");
            if (!Regex.IsMatch(p, @"^[a-zA-Z0-9._-]+$"))
                return BadRequest("Invalid avatar parameter");
            var safeP = Path.GetFileName(p);

            if (secretKeyConfig == null)
            {
                var providerPath = ConfigService.GetConfigValue<string>("Avatar.ProviderPath");
                var formattedPath = string.Format(providerPath, safeP);
                var fileName = Path.GetFileName(formattedPath);
                var contentType = GetContentType(formattedPath);

                if (System.IO.File.Exists(formattedPath))
                {
                    using (var memory = new MemoryStream())
                    {
                        using (var stream = new FileStream(formattedPath, FileMode.Open))
                        {
                            await stream.CopyToAsync(memory);
                        }

                        memory.Position = 0;

                        return File(memory.ToArray(), contentType, fileName);
                    }
                }

                return NotFound("Avatar not found");
            }
            else
            {
                var safez = p.SanitizeFileName(string.Empty);
                var secretKey = secretKeyConfig.ConfigValue;
                var providerUrl = ConfigService.GetConfigValue<string>("Avatar.ProviderUrl");
                var url = string.Format(providerUrl, safez, secretKey);

                return Redirect(url);
            }
        }

        [AllowAnonymous]
        public IActionResult Header(Guid docId)
        {
            var model =  ApprovalService.GetDocumentApprovalById(docId);
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult CommonHeader()
        {
            return View();
        }
    }
}
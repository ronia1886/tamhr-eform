using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Helpers;
using Agit.Common;
using Agit.Domain;
using Agit.Common.Utility;
using Agit.Domain.UnitOfWork;
using FluentFTP;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle pay slip.
    /// </summary>
    public class BupotService : DomainServiceBase
    {
        #region Variables & Properties
        /// <summary>
        /// Config service object.
        /// </summary>
        private readonly ConfigService _configService;

        /// <summary>
        /// Personal data service object.
        /// </summary>
        private readonly PersonalDataService _personalDataService;
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="configService">This <see cref="ConfigService"/> object.</param>
        /// <param name="personalDataService">This <see cref="PersonalDataService"/> object.</param>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public BupotService(ConfigService configService, PersonalDataService personalDataService, IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
            // Get config service object from DI container.
            _configService = configService;

            // Get personal data service object from DI container.
            _personalDataService = personalDataService;
        }
        #endregion

        public async Task<byte[]> Download(string noreg, int month, int period, bool isOffCycle = false, bool viewOnly = true)
        {
            var key = noreg;
            var hashSet = UnitOfWork.GetRepository<UserHash>();

            var parameters = new Dictionary<string, object>
            {
                ["noreg"] = noreg,
                ["monthNumber"] = month.ToString("D2"),
                ["month"] = month.ToString("D2"),
                ["period"] = period,
                ["year"] = period,
            };

            var BupotPath = StringHelper.Format(_configService.GetConfigValue<string>(Configurations.BupotPath), parameters);
            var BupotLocalPath = StringHelper.Format(_configService.GetConfigValue<string>(Configurations.BupotLocalPath), parameters);
            var useFtp = _configService.GetConfigValue(Configurations.BupotUseFtp, defaultValue: false);
            var defaultBupotPasswordFile = StringHelper.Format(_configService.GetConfigValue(Configurations.BupotPasswordFile, string.Empty), parameters);
            var defaultOwnerPassword = _configService.GetConfigValue(Configurations.BupotOwnerPassword, "Transformer6hap");

            var userHash = hashSet.Fetch().AsNoTracking().FirstOrDefault(x => x.NoReg == noreg && x.TypeCode == HashType.Payslip);
            var password = userHash != null ? Cryptography.DecryptWithHash(key, userHash.HashValue) : GetDefaultPassword(noreg);

            var fileName = StringHelper.Format(_configService.GetConfigValue<string>(isOffCycle ? Configurations.BupotOffCycleFilename : Configurations.BupotFilename), parameters);

            byte[] bytes;
            try
            {
                // Attempt to download from FTP
                bytes = await DownloadFromFtp(BupotPath, fileName, isOffCycle, defaultBupotPasswordFile, password, defaultOwnerPassword, viewOnly);
            }
            catch
            {
                // Fallback to local file system if FTP download fails
                bytes = DownloadFromFile(BupotLocalPath, fileName, defaultBupotPasswordFile, password, defaultOwnerPassword, viewOnly);
            }

            return bytes;

        }

        private string GetDefaultPassword(string noreg)
        {
            var personalDataCommonAttribute = _personalDataService.GetPersonalDataAttribute(noreg);
            var defaultParameters = new Dictionary<string, object>
            {
                ["noreg"] = noreg,
                ["nik"] = personalDataCommonAttribute != null && !string.IsNullOrEmpty(personalDataCommonAttribute.Nik) ? personalDataCommonAttribute.Nik : noreg
            };

            return StringHelper.Format(_configService.GetConfigValue(Configurations.BupotDefaultPassword, string.Empty), defaultParameters);
        }

        private async Task<byte[]> DownloadFromFtp(string BupotPath, string fileName, bool isOffCycle, string defaultBupotPasswordFile, string password, string defaultOwnerPassword, bool viewOnly)
        {
            var enableSsl = _configService.GetConfigValue(Configurations.BupotFtpEnableSsl, defaultValue: false);
            var ftpUsername = _configService.GetConfigValue<string>(Configurations.BupotFtpUsername);
            var ftpPassword = _configService.GetConfigValue<string>(Configurations.BupotFtpPassword);

            var uri = new Uri(BupotPath);
            var client = new FtpClient(uri.Host, uri.Port, ftpUsername, ftpPassword)
            {
                DataConnectionType = FtpDataConnectionType.PORT
            };
            await client.ConnectAsync();

            var selectedFiles = new[] { uri.LocalPath + "/" + fileName.Trim(new[] { '\\', '/' }) };

            if (isOffCycle)
            {
                var files = client.GetListing(uri.LocalPath);
                var regex = new Regex(fileName, RegexOptions.IgnoreCase);
                selectedFiles = files.Where(x => x.Type == FtpFileSystemObjectType.File && regex.IsMatch(x.Name))
                                     .Select(x => x.FullName)
                                     .ToArray();
            }

            Assert.ThrowIf(selectedFiles.Length == 0, "eBupot for this period was not found.");

            using (var masterDocument = new PdfDocument())
            {
                foreach (var file in selectedFiles)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await client.DownloadAsync(memoryStream, file);
                        using (var doc = PdfReader.Open(memoryStream, defaultBupotPasswordFile, PdfDocumentOpenMode.Import))
                        {
                            for (var i = 0; i < doc.PageCount; i++)
                            {
                                masterDocument.AddPage(doc.Pages[i]);
                            }
                        }
                    }
                }
                await client.DisconnectAsync();

                if (!viewOnly)
                {
                    AttachSecuritySetting(masterDocument, password, defaultOwnerPassword);
                }

                using (var memoryStream = new MemoryStream())
                {
                    masterDocument.Save(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }

        private byte[] DownloadFromFile(string BupotLocalPath, string fileName, string defaultBupotPasswordFile, string password, string defaultOwnerPassword, bool viewOnly)
        {
            var filePath = Path.Combine(BupotLocalPath, fileName);
            Assert.ThrowIf(!File.Exists(filePath), "eBupot for this period was not found. in local path");

            using (var file = File.OpenRead(filePath))
            using (var ms = new MemoryStream())
            using (var doc = PdfReader.Open(file, defaultBupotPasswordFile, PdfDocumentOpenMode.Modify))
            {
                if (!viewOnly)
                {
                    AttachSecuritySetting(doc, password, defaultOwnerPassword);
                }
                doc.Save(ms);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Attach security setting for given pdf document.
        /// </summary>
        /// <param name="pdfDocument">This <see cref="PdfDocument"/> object.</param>
        /// <param name="password">This user password.</param>
        /// <param name="defaultOwnerPassword">This default owner password.</param>
        private void AttachSecuritySetting(PdfDocument pdfDocument, string password, string defaultOwnerPassword)
        {
            // Get and set enable printing indicator from configuration.
            var enablePrinting = _configService.GetConfigValue(Configurations.BupotEnablePrinting, defaultValue: false);

            // Get and set enable document editing indicator from configuration.
            var enableEditing = _configService.GetConfigValue(Configurations.BupotEnableEditing, defaultValue: false);

            // Get and set security settings object from given pdf document.
            var securitySetting = pdfDocument.SecuritySettings;

            // Set owner password from given parameter.
            securitySetting.OwnerPassword = defaultOwnerPassword;

            // Set user password from given parameter.
            securitySetting.UserPassword = password;

            // Disable printing with full quality.
            securitySetting.PermitFullQualityPrint = enablePrinting;

            // Disable printing.
            securitySetting.PermitPrint = enablePrinting;

            // Disable annotation.
            securitySetting.PermitAnnotations = enableEditing;

            // Disable assemble the document.
            securitySetting.PermitAssembleDocument = enableEditing;

            // Disable content extraction (copy paste text or save image).
            securitySetting.PermitExtractContent = enableEditing;

            // Disable content extraction for accessibility.
            //securitySetting.PermitAccessibilityExtractContent = enableEditing;

            // Disable forms fill.
            securitySetting.PermitFormsFill = enableEditing;

            // Disable modify the document.
            securitySetting.PermitModifyDocument = enableEditing;
        }
    }
}

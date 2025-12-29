using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.Web;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.Web.Helpers;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.Web.Extensions;
using TAMHR.ESS.Infrastructure.Web.Runtime.Providers;
using OBS.Model;
using OBS;

namespace TAMHR.ESS.WebUI.Areas.Core.Controllers
{
    #region API Controller
    [Route("api/files")]
    [ApiController]
    public class FilesApiController : ApiControllerBase
    {
        #region Domain Services
        /// <summary>
        /// Approval service
        /// </summary>
        protected ApprovalService ApprovalService => ServiceProxy.GetService<ApprovalService>();
        #endregion

        /// <summary>
        /// Upload file to wwwroot/uploads.
        /// </summary>
        [HttpPost]
        [Route("upload-public")]
        public async Task<IActionResult> UploadPublicFile()
        {
            var pathProvider = ServiceProxy.GetPathProvider();
            var file = Request.Form.Files.FirstOrDefault();
            var tempPath = pathProvider.ContentPath("temps");

            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(tempPath, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return Ok(new { status = "success", url = "~/temps/" + Path.GetFileName(filePath), oldFileName = file.FileName });
        }

        /// <summary>
        /// Get list of files by document approval id and with field category
        /// </summary>
        /// <param name="docId">Document Approval Id</param>
        /// <param name="fileName">Field Category</param>
        /// <returns>List of Files</returns>
        [HttpGet]
        public IActionResult Index(Guid docId, string fileName)
        {
            if (docId == null)
            {
                return CreatedAtAction("Get", new { });
            }

            if (docId.ToString().Equals("00000000-0000-0000-0000-000000000000"))
            {
                return CreatedAtAction("Get", new { });
            }

            var data = ApprovalService.GetDocumentApprovalFileDocId(docId).ToList();
            var files = from file in data
                        where file.FieldCategory.Contains(fileName)
                        select new
                        {
                            id = file.CommonFile?.Id,
                            name = file.CommonFile?.FileName,
                            size = file.CommonFile?.FileSize,
                            url = file.CommonFile?.FileUrl
                        };

            return CreatedAtAction("Get", new { files = files });
        }

        /// <summary>
        /// Crop image file
        /// </summary>
        /// <param name="imageCropViewModel">Image Crop View Model</param>
        [HttpPost]
        [Route("crop")]
        public IActionResult CropImageFile([FromForm] ImageCropViewModel imageCropViewModel)
        {
            try
            {
                var pathProvider = ServiceProxy.GetPathProvider();
                var fileName = Path.GetFileName(imageCropViewModel.imgUrl);
                var newFileName = "Cropped_" + fileName;
                var tempPath = pathProvider.ContentPath("temps");
                var sourceFilePath = Path.Combine(tempPath, fileName);
                var newFilePath = Path.Combine(tempPath, newFileName);

                using (var sourceBitmap = new Bitmap(sourceFilePath))
                {
                    using (var cropBitmap = sourceBitmap.Clone(new Rectangle(Convert.ToInt32(imageCropViewModel.imgX1), Convert.ToInt32(imageCropViewModel.imgX2), Convert.ToInt32(imageCropViewModel.cropW), Convert.ToInt32(imageCropViewModel.cropH)), sourceBitmap.PixelFormat))
                    {
                        cropBitmap.Save(newFilePath);
                    }
                }

                return Ok(new { status = "success", url = Url.Content("~/temps/" + newFileName), imageUrl = "~/temps/" + newFileName, fileName = newFileName });
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
           
        }

        /// <summary>
        /// Upload file to wwwroot/uploads
        /// </summary>
        [HttpPost]
        [Route("upload-common")]
        public async Task<IActionResult> UploadCommonFile()
        {
            var offsetWidth = int.Parse(ConfigService.GetConfig("Slider.OffsetWidth")?.ConfigValue ?? "0");
            var offsetHeight = int.Parse(ConfigService.GetConfig("Slider.OffsetHeight")?.ConfigValue ?? "0");
            var pathProvider = ServiceProxy.GetPathProvider();
            var file = Request.Form.Files.FirstOrDefault();
            var tempPath = pathProvider.ContentPath("temps");

            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var fileSize = (int)(file.Length / 1024);
            var contentType = file.ContentType;
            var filePath = Path.Combine(tempPath, fileName);
            var newFilePath = Path.Combine(tempPath, "Resize_" + fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            using (var img = Image.FromFile(filePath))
            {
                var thumbImage = ImageHelper.Resize(img, 0, offsetHeight, offsetWidth, offsetHeight);

                thumbImage.Save(newFilePath, ImageFormat.Jpeg);
            }

            return Ok(new { status = "success", url = Url.Content("~/temps/" + Path.GetFileName(newFilePath)) });
        }

        /// <summary>
        /// Upload multiple files asynchronously
        /// </summary>
        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> UploadFilesAsyncActionResult()
        {
            #region versi local server
            //try
            //{
            //    var files = Request.Form.Files;
            //
            //    PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));
            //    var configs = ConfigService.GetConfigs(true);
            //    var configPath = configs.FirstOrDefault(x => x.ConfigKey == "Upload.Path")?.ConfigValue;
            //
            //    var filesPath = !string.IsNullOrEmpty(configPath) ? configPath : pathProvider.ContentPath("uploads");
            //    if (!Directory.Exists(filesPath))
            //    {
            //        Directory.CreateDirectory(filesPath);
            //    }
            //
            //    List<CommonFile> uploadedFiles = new List<CommonFile>();
            //
            //    foreach (var file in files)
            //    {
            //        var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName;
            //        var fileSize = (int)(file.Length / 1024);
            //
            //        fileName = fileName.Contains("\\")
            //            ? fileName.Trim('"').Substring(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1)
            //            : fileName.Trim('"');
            //
            //        var commonFile = new CommonFile()
            //        {
            //            FileUrl = "",
            //            FileName = fileName,
            //            FileSize = fileSize,
            //            FileType = file.ContentType,
            //        };
            //
            //        ApprovalService.SaveCommonFile(commonFile);
            //
            //        fileName = commonFile.Id.ToString() + "-" + fileName;
            //
            //        var fullFilePath = Path.Combine(filesPath, fileName);
            //
            //        if (file.Length <= 0)
            //        {
            //            continue;
            //        }
            //
            //        using (var stream = new FileStream(fullFilePath, FileMode.Create))
            //        {
            //            await file.CopyToAsync(stream);
            //            commonFile.FileUrl = Url.ToAbsoluteContent(CreateUrl(fullFilePath));
            //            ApprovalService.SaveCommonFile(commonFile);
            //            uploadedFiles.Add(commonFile);
            //        }
            //    }
            //
            //    return CreatedAtAction("Get", new { files = uploadedFiles });
            //}
            //catch (Exception ex)
            //{
            //    return this.BadRequest(ex.Message);
            //}
            #endregion
            #region versi obs
            try
            {
                var files = Request.Form.Files;

                var configs = ConfigService.GetConfigs(true);
                var bucketName = configs.FirstOrDefault(x => x.ConfigKey == "OBS.BucketName")?.ConfigValue;
                var accessKey = configs.FirstOrDefault(x => x.ConfigKey == "OBS.AccessKey")?.ConfigValue;
                var secretKey = configs.FirstOrDefault(x => x.ConfigKey == "OBS.SecretKey")?.ConfigValue;
                var obsEndpoint = configs.FirstOrDefault(x => x.ConfigKey == "OBS.EndPoint")?.ConfigValue;

                var obsConfig = new ObsConfig
                {
                    Endpoint = obsEndpoint
                };

                var obsClient = new ObsClient(accessKey, secretKey, obsConfig);

                List<CommonFile> uploadedFiles = new List<CommonFile>();

                foreach (var file in files)
                {
                    if (file.Length <= 0) continue;

                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    if (fileName.Contains("\\"))
                        fileName = fileName.Substring(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1);

                    var fileSize = (int)(file.Length / 1024);

                    var commonFile = new CommonFile
                    {
                        FileUrl = "",
                        FileName = fileName,
                        FileSize = fileSize,
                        FileType = file.ContentType
                    };

                    // Simpan metadata awal
                    ApprovalService.SaveCommonFile(commonFile);

                    var obsFileName = $"{commonFile.Id}-{fileName}";
                    string objectKey = configs.FirstOrDefault(x => x.ConfigKey == "OBS.Upload.Path")?.ConfigValue;
                    objectKey = objectKey + "/" + obsFileName;

                    // Upload ke OBS
                    using (var stream = file.OpenReadStream())
                    {
                        var putRequest = new PutObjectRequest
                        {
                            BucketName = bucketName,
                            ObjectKey = objectKey,
                            InputStream = stream,
                            ContentType = file.ContentType
                        };

                        await Task.Run(() => obsClient.PutObject(putRequest));
                    }

                    // URL file di OBS
                    commonFile.FileUrl = objectKey;

                    // Update metadata
                    ApprovalService.SaveCommonFile(commonFile);

                    uploadedFiles.Add(commonFile);
                }

                return Ok(new { files = uploadedFiles });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            #endregion
        }

        /// <summary>
        /// Download file asynchronously by id
        /// </summary>
        /// <param name="id">Common File Id</param>
        /// <returns>File to be Download</returns>
        [HttpGet]
        [Route("download")]
        public async Task<IActionResult> DownloadFilesAsyncActionResult(Guid id)
        {
            #region download path lokal
            //var file = ApprovalService.GetCommonFileById(id);
            //
            //if (file != null)
            //{
            //    string fullPath = CheckExistingPath(file);
            //    if (System.IO.File.Exists(fullPath))
            //    {
            //        using (var memory = new MemoryStream())
            //        {
            //            using (var stream = new FileStream(fullPath, FileMode.Open))
            //            {
            //                await stream.CopyToAsync(memory);
            //            }
            //
            //            memory.Position = 0;
            //
            //            return File(memory.ToArray(), GetContentType(fullPath), Path.GetFileName(fullPath));
            //        }
            //    }
            //
            //}
            //return NotFound("File Not Found");
            #endregion
            #region
            var file = ApprovalService.GetCommonFileById(id);

            if (file != null)
            {
                var configs = ConfigService.GetConfigs(true);
                var bucketName = configs.FirstOrDefault(x => x.ConfigKey == "OBS.BucketName")?.ConfigValue;
                var accessKey = configs.FirstOrDefault(x => x.ConfigKey == "OBS.AccessKey")?.ConfigValue;
                var secretKey = configs.FirstOrDefault(x => x.ConfigKey == "OBS.SecretKey")?.ConfigValue;
                var obsEndpoint = configs.FirstOrDefault(x => x.ConfigKey == "OBS.EndPoint")?.ConfigValue;

                var obsConfig = new ObsConfig
                {
                    Endpoint = obsEndpoint
                };
                var obsClient = new ObsClient(accessKey, secretKey, obsConfig);

                // Cari objectKey mirip CheckExistingPath (sourceFile / destFile)
                string objectKey = CheckExistingPathObs(file, obsClient, bucketName);

                // Cek beneran ada di OBS
                if (ObjectExists(obsClient, bucketName, objectKey))
                {
                    // Download file
                    var getReq = new GetObjectRequest()
                    {
                        BucketName = bucketName,
                        ObjectKey = objectKey
                    };

                    using (var getResp = obsClient.GetObject(getReq))
                    using (var memory = new MemoryStream())
                    {
                        await getResp.OutputStream.CopyToAsync(memory);
                        memory.Position = 0;

                        string contentType = GetContentType(objectKey);
                        //return File(memory.ToArray(), file.FileType, file.FileName);
                        return File(memory.ToArray(), contentType, file.FileName);
                    }
                }
            }

            return NotFound("File Not Found");
            #endregion
        }

        /// <summary>
        /// Delete file asynchronously by common file object
        /// </summary>
        /// <param name="file">Common File Object</param>
        [HttpDelete]
        [Route("delete")]
        public IActionResult DeleteFilesAsyncActionResult(CommonFile file)
        {
            #region versi local
            //string fullPath = CheckExistingPath(file);
            //
            //if (System.IO.File.Exists(fullPath))
            //{
            //    System.IO.File.Delete(fullPath);
            //}
            //
            //ApprovalService.DeleteDocumentFile(file.Id);
            //
            //return CreatedAtAction("Get", new { });
            #endregion
            #region versi obs
            try
            {
                var configs = ConfigService.GetConfigs(true);
                var bucketName = configs.FirstOrDefault(x => x.ConfigKey == "OBS.BucketName")?.ConfigValue;
                var accessKey = configs.FirstOrDefault(x => x.ConfigKey == "OBS.AccessKey")?.ConfigValue;
                var secretKey = configs.FirstOrDefault(x => x.ConfigKey == "OBS.SecretKey")?.ConfigValue;
                var obsEndpoint = configs.FirstOrDefault(x => x.ConfigKey == "OBS.EndPoint")?.ConfigValue;

                var obsConfig = new ObsConfig { Endpoint = obsEndpoint };
                var obsClient = new ObsClient(accessKey, secretKey, obsConfig);

                // Cari path mana yg ada (sourceFile / destFile)
                string objectKey = CheckExistingPathObs(file, obsClient, bucketName);

                if (!string.IsNullOrEmpty(objectKey))
                {
                    // Delete dari OBS
                    var deleteRequest = new DeleteObjectRequest()
                    {
                        BucketName = bucketName,
                        ObjectKey = objectKey
                    };
                    obsClient.DeleteObject(deleteRequest);
                }

                // Delete metadata dari DB
                ApprovalService.DeleteDocumentFile(file.Id);

                return Ok(new { Success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
            #endregion
        }

        /// <summary>
        /// Create URL from local path
        /// </summary>
        /// <param name="localPath">Local Path</param>
        /// <returns>URL in string</returns>
        private string CreateUrl(string localPath)
        {
            var reg = new Regex(@".*\\wwwroot\\");
            var result = reg.Replace(localPath, @"~/");
            result = result.Contains("\\") ? result.Replace('\\', '/') : result.Trim('"');

            return result;

        }

        /// <summary>
        /// Check existing path and move file if 
        /// </summary>
        /// <param name="file">Common File</param>
        /// <returns></returns>
        private string CheckExistingPath(CommonFile file)
        {
            var fileName = file.Id + "-" + file.FileName;

            PathProvider pathProvider = (PathProvider)HttpContext.RequestServices.GetService(typeof(PathProvider));
            var configs = ConfigService.GetConfigs(true);

            var configPath = configs.FirstOrDefault(x => x.ConfigKey == "Upload.Path")?.ConfigValue;

            var filesPath = !string.IsNullOrEmpty(configPath) ? configPath : "";

            string sourceFile = System.IO.Path.Combine(filesPath, fileName);

            var documentApproval = ApprovalService.GetDocumentApprovalCommonFileById(file.Id);

            if (documentApproval != null)
            {

                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                var docNumber = rgx.Replace(documentApproval.DocumentNumber, "-");

                string targetPath = Path.Combine(configPath, documentApproval.CreatedOn.Year.ToString(), docNumber);
                string destFile = System.IO.Path.Combine(targetPath, fileName);

                if (System.IO.File.Exists(sourceFile))
                {
                    return sourceFile;
                }

                else if (System.IO.File.Exists(destFile))
                {
                    return destFile;
                }
            }

            return sourceFile;
        }

        private string CheckExistingPathObs(CommonFile file, ObsClient obsClient, string bucketName)
        {
            var fileName = file.Id + "-" + file.FileName;
            var configs = ConfigService.GetConfigs(true);
            var configPath = configs.FirstOrDefault(x => x.ConfigKey == "OBS.Upload.Path")?.ConfigValue ?? "";

            // Source path di OBS
            string sourceFile = $"{configPath}/{fileName}";

            var documentApproval = ApprovalService.GetDocumentApprovalCommonFileById(file.Id);
            if (documentApproval != null)
            {
                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                var docNumber = rgx.Replace(documentApproval.DocumentNumber, "-");

                string destFile = $"{configPath}/{documentApproval.CreatedOn.Year}/{docNumber}/{fileName}";

                // Cek apakah sourceFile ada di OBS
                if (ObjectExists(obsClient, bucketName, sourceFile))
                {
                    return sourceFile;
                }
                // Kalau ga ada, cek destFile
                else if (ObjectExists(obsClient, bucketName, destFile))
                {
                    return destFile;
                }
            }

            return sourceFile; // fallback walau mungkin ga ada
        }

        private bool ObjectExists(ObsClient obsClient, string bucketName, string objectKey)
        {
            try
            {
                var meta = obsClient.GetObjectMetadata(new GetObjectMetadataRequest()
                {
                    BucketName = bucketName,
                    ObjectKey = objectKey
                });
                return meta != null;
            }
            catch (ObsException ex)
            {
                if (ex.StatusCode.ToString() != "")
                    return false;
                throw; // error lain biar dilempar keluar
            }
        }
    }
    #endregion
}
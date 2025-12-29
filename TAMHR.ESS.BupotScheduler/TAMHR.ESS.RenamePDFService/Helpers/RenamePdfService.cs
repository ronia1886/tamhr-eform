
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using FluentFTP;
using UglyToad.PdfPig;
using TAMHR.ESS.RenamePDFService.Models;
using System.Net;

namespace TAMHR.ESS.RenamePDFService.Helpers
{
    public class RenamePdfService : IRenamePdfService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RenamePdfService> _logger;
        private readonly string _sqlConnectionString;

        public RenamePdfService(IConfiguration configuration, ILogger<RenamePdfService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _sqlConnectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public void RenamePDFs()
        {
            string errorMessage = "";
            try
            {
                _logger.LogInformation("Starting RenamePDFs method.");

                string querymonth = "SELECT ConfigValue FROM TB_M_CONFIG WHERE ConfigKey='BUPOT.TotalMonth'";
                string gettotalmonthStr = ExecuteScalarQuery(_sqlConnectionString, querymonth);

                if (!int.TryParse(gettotalmonthStr, out int gettotalmonth))
                {
                    errorMessage = "Invalid or missing value in Config";
                    _logger.LogError(errorMessage);
                    return;
                }

                // Retrieving FTP settings from the database
                FtpSettingsModel ftpSettings = GetFtpSettingsFromDatabase();

                if (ftpSettings == null)
                {
                    errorMessage = "Failed to retrieve FTP settings from the database.";
                    _logger.LogError(errorMessage);
                    return;
                }

                _logger.LogInformation($"Success Connect FTP");

                // Connect to FTP server using retrieved settings
                using (FtpClient ftp = new FtpClient(ftpSettings.Server, ftpSettings.Port, ftpSettings.Username, ftpSettings.Password))
                {
                    ftp.DataConnectionType = FtpDataConnectionType.PORT;

                    // Process files for the current and previous three months
                    for (int i = 0; i <= gettotalmonth; i++)
                    {
                        DateTime targetMonth = DateTime.Now.AddMonths(-i);
                        ProcessMonth(ftp, ftpSettings, targetMonth.Year, targetMonth.Month);
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = "Error in RenamePDFs: " + ex.Message;
                if(ex.InnerException!= null)
                {
                    errorMessage += ", Detail Error: " + ex.InnerException.Message;
                    errorMessage += " ( " + ex.ToString() + " ) ";
                }

                _logger.LogError(errorMessage);

                
                
            }

            //insert to TB_R_LOG
            if (errorMessage != "")
            {
                string hostName = Dns.GetHostName();
                string applicationName = "TAMHR-ESS";
                string query = "INSERT INTO TB_R_Log " +
                    "VALUES(newid(),'"+ applicationName + "',[dbo].[usf_GetNextLogID]('"+ applicationName + "'),'Data Changes','Create','eBupot-Rename PDF Service','"+ hostName + "','Failed','" + errorMessage + "','system',getdate(),null,null,1)";
                ExecuteQuery(_sqlConnectionString, query);
            }
            
        }


        private void ProcessMonth(FtpClient ftp, FtpSettingsModel ftpSettings, int year, int month)
        {
            string yearStr = year.ToString();
            string monthStr = month.ToString("D2");

            // Ensure source directory exists
            string sourceDirectory = GetFormattedFtpSourceDirectory(yearStr, monthStr);
            EnsureFtpDirectoryExists(ftp, sourceDirectory);

            // Ensure target directory exists
            string targetDirectory = GetFtpTargetDirectory(yearStr, monthStr);
            EnsureFtpDirectoryExists(ftp, targetDirectory);

            // Retrieve PDF files from the source directory
            FtpListItem[] pdfFiles = ftp.GetListing(sourceDirectory, FtpListOption.Modify | FtpListOption.Size);

            foreach (FtpListItem file in pdfFiles)
            {
                if (file.Type == FtpFileSystemObjectType.File && file.Name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    ProcessPdfFile(ftp, file, sourceDirectory, targetDirectory, yearStr, monthStr);
                }
            }
        }




        private FtpSettingsModel GetFtpSettingsFromDatabase()
        {
            FtpSettingsModel ftpSettings = new FtpSettingsModel();

            try
            {
                string sqlConnectionString = _configuration.GetConnectionString("DefaultConnection");

                // Query to retrieve full FTP URL
                //string queryServer = "SELECT ConfigValue FROM TB_M_CONFIG WHERE ConfigKey='BUPOT.Path'";
                string queryServer = "SELECT ConfigValue FROM TB_M_CONFIG WHERE ConfigKey='BUPOT.SchedulerFtps'";
                string fullFtpUrl = ExecuteScalarQuery(sqlConnectionString, queryServer);

                // Extract FTP server hostname
                string ftpServer = GetFtpServerFromUrl(fullFtpUrl);

                // Query to retrieve FTP username
                string queryUsername = "SELECT ConfigValue FROM TB_M_CONFIG WHERE ConfigKey='BUPOT.FtpUsername'";
                string ftpUsername = ExecuteScalarQuery(sqlConnectionString, queryUsername);

                // Query to retrieve FTP password
                string queryPassword = "SELECT ConfigValue FROM TB_M_CONFIG WHERE ConfigKey='BUPOT.FtpPassword'";
                string ftpPassword = ExecuteScalarQuery(sqlConnectionString, queryPassword);

                // Query to retrieve FTP port
                string queryPort = "SELECT ConfigValue FROM TB_M_CONFIG WHERE ConfigKey='BUPOT.SchedulerFtpPort'";
                string ftpPort = ExecuteScalarQuery(sqlConnectionString, queryPort);

                // Hardcoding port to 21
                //int ftpPort = 21;

                // Assign retrieved values to ftpSettings
                ftpSettings.Server = ftpServer;
                ftpSettings.Username = ftpUsername;
                ftpSettings.Password = ftpPassword;
                ftpSettings.Port = Convert.ToInt32(ftpPort);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving FTP settings from database: {ex.Message}");
            }

            return ftpSettings;
        }

        private string ExecuteScalarQuery(string connectionString, string query)
        {
            string result = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        result = command.ExecuteScalar()?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error executing SQL query: {ex.Message}");
            }

            return result;
        }

        private string ExecuteQuery(string connectionString, string query)
        {
            string result = "";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                result = "Error executing SQL query: " + ex.Message;
                _logger.LogError(result);
            }

            return result;
        }

        private string GetFtpServerFromUrl(string ftpUrl)
        {
            try
            {
                // Check if the URL starts with any FTP protocol (ftp://, sftp://, ftps://)
                string[] ftpPrefixes = { "ftp://", "sftp://", "ftps://" };

                foreach (string prefix in ftpPrefixes)
                {
                    if (ftpUrl.StartsWith(prefix))
                    {
                        // Remove the prefix including the hostname
                        int prefixLength = prefix.Length;
                        int hostEndIndex = ftpUrl.IndexOf('/', prefixLength); // Find where the hostname ends
                        if (hostEndIndex != -1)
                        {
                            return ftpUrl.Substring(0, hostEndIndex); // Keep only the hostname part
                        }
                        else
                        {
                            return ftpUrl; // If no path part after hostname, return the entire URL
                        }
                    }
                }

                // If no known prefix found, return the whole URL (fallback case)
                return ftpUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error extracting FTP server from URL: {ex.Message}");
                throw;
            }
        }


        private void EnsureFtpDirectoryExists(FtpClient ftp, string directoryPath)
        {
            _logger.LogInformation($"Check directory {directoryPath} is exists, ftp config: {ftp.Host}:{ftp.Port}");
            if (!ftp.DirectoryExists(directoryPath))
            {
                _logger.LogInformation($"Directory {directoryPath} does not exist. Creating directory.");
                ftp.CreateDirectory(directoryPath);
            }
        }


        // Updated ProcessPdfFile Method
        private void ProcessPdfFile(FtpClient ftp, FtpListItem file, string sourceDirectory, string targetDirectory, string yearStr, string monthStr)
        {
            using (var stream = new System.IO.MemoryStream())
            {
                try
                {
                    ftp.Download(stream, sourceDirectory + file.Name);
                    stream.Position = 0;

                    using (PdfDocument document = PdfDocument.Open(stream))
                    {
                        _logger.LogInformation($"Processing PDF File: {sourceDirectory}{file.Name}");

                        string extractedText117 = ExtractTextFromCoordinates(document, 117, 658.7);
                        string extractedText400 = ExtractTextFromCoordinates(document, 389, 654);
                        string extractedText199 = ExtractTextFromCoordinates(document, 208, 660.041);

                        _logger.LogInformation($"Extracted Text (117, 658.7): {extractedText117}");
                        _logger.LogInformation($"Extracted Text (389, 654): {extractedText400}");
                        _logger.LogInformation($"Extracted Text (199.58203125, 668.50975): {extractedText199}");

                        // Get the NoReg values from the database
                        string noreg117 = GetNoregFromDatabase(_configuration.GetConnectionString("DefaultConnection"), extractedText117);
                        string noreg199 = GetNoregFromDatabase(_configuration.GetConnectionString("DefaultConnection"), extractedText199);
                        string noreg400 = GetNoregFromDatabase(_configuration.GetConnectionString("DefaultConnection"), extractedText400);

                        // Process renaming based on extracted noreg values
                        if (!string.IsNullOrEmpty(noreg117) && Regex.IsMatch(noreg117, @"^\d{13,}$"))
                        {
                            ProcessRenaming(ftp, file, sourceDirectory, noreg117, yearStr, monthStr, targetDirectory);
                        }
                        else if (!string.IsNullOrEmpty(noreg400) && Regex.IsMatch(noreg400, @"^\d{15,}$"))
                        {
                            ProcessRenaming(ftp, file, sourceDirectory, noreg400, yearStr, monthStr, targetDirectory);
                        }
                        else if (!string.IsNullOrEmpty(noreg199) && Regex.IsMatch(noreg199, @"^\d{6,}$"))
                        {
                            ProcessRenaming(ftp, file, sourceDirectory, noreg199, yearStr, monthStr, targetDirectory);
                        }
                        else
                        {
                            _logger.LogWarning($"No meaningful text found at specified coordinates for file {file.Name}. Skipping renaming.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing PDF file {sourceDirectory}{file.Name}: {ex.Message}");
                }
            }
        }


        private void ProcessRenaming(FtpClient ftp, FtpListItem file, string sourceDirectory, string noreg, string yearStr, string monthStr, string targetDirectory)
        {
            try
            {
                bool renamed = RenameFileOnFtp(ftp, file, sourceDirectory, noreg, yearStr, monthStr);
                if (renamed)
                {
                    MoveFileToDirectory(ftp, sourceDirectory, $"{noreg}-{monthStr}-{yearStr}.pdf", targetDirectory);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error renaming or moving file {file.Name}: {ex.Message}");
            }
        }

        private void MoveFileToDirectory(FtpClient ftp, string sourceDirectory, string newFileName, string targetDirectory)
        {
            string sourceFilePath = sourceDirectory + newFileName;
            string targetFilePath = targetDirectory + newFileName;

            if (!ftp.FileExists(sourceFilePath))
            {
                _logger.LogWarning($"Source file {sourceFilePath} does not exist.");
                return;
            }

            if (!ftp.DirectoryExists(targetDirectory))
            {
                ftp.CreateDirectory(targetDirectory);
            }

            ftp.MoveFile(sourceFilePath, targetFilePath);
            _logger.LogInformation($"Moved file from {sourceFilePath} to {targetFilePath}");
        }


        // Updated RenameFileOnFtp Method
        private bool RenameFileOnFtp(FtpClient ftp, FtpListItem file, string ftpDirectory, string noreg, string year, string month)
        {
            string newFileName = $"{noreg}-{month}-{year}.pdf";
            string newFilePath = ftpDirectory + newFileName;

            if (ftp.FileExists(newFilePath))
            {
                _logger.LogInformation($"File {newFileName} already exists in the source directory. Skipping renaming.");
                return false;
            }

            int counter = 1;
            while (ftp.FileExists(newFilePath))
            {
                newFileName = $"{noreg}-{month}-{year}_{counter}.pdf";
                newFilePath = ftpDirectory + newFileName;
                counter++;
            }

            ftp.Rename(ftpDirectory + file.Name, newFilePath);
            _logger.LogInformation($"Renamed to: {newFilePath}");
            return true;
        }



        private string ExtractTextFromCoordinates(PdfDocument document, double targetX, double targetY)
        {
            double searchRadius = 10.0;
            string extractedText = string.Empty;

            foreach (var page in document.GetPages())
            {
                foreach (var word in page.GetWords())
                {
                    if (IsWithinSearchRadius(word.BoundingBox.Left, word.BoundingBox.Bottom, targetX, targetY, searchRadius))
                    {
                        extractedText += word.Text + " ";
                    }
                }
            }

            return extractedText.Trim();
        }

        private static bool IsWithinSearchRadius(double wordX, double wordY, double targetX, double targetY, double radius)
        {
            return Math.Abs(wordX - targetX) <= radius && Math.Abs(wordY - targetY) <= radius;
        }

        private string GetNoregFromDatabase(string connectionString, string extractedText)
        {
            string noreg = string.Empty;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT NoReg FROM TB_M_PERSONAL_DATA_TAX_STATUS WHERE npwp = @ExtractedText";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ExtractedText", extractedText);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            noreg = reader["NoReg"].ToString();
                        }
                    }
                }

                if (string.IsNullOrEmpty(noreg))
                {
                    query = "SELECT NoReg FROM TB_M_PERSONAL_DATA pd join TB_M_PERSONAL_DATA_COMMON_ATTRIBUTE pdc on pd.CommonAttributeId=pdc.Id WHERE nik = @ExtractedText";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ExtractedText", extractedText);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                noreg = reader["NoReg"].ToString();
                            }
                        }
                    }
                }
            }

            return noreg;
        }
        private string GetFormattedFtpSourceDirectory(string year, string month)
        {
            string query = "SELECT ConfigValue FROM TB_M_CONFIG WHERE ConfigKey='BUPOT.SchedulerSourcePath'";
            string sourceDirectoryTemplate = ExecuteScalarQuery(_sqlConnectionString, query);

            if (string.IsNullOrEmpty(sourceDirectoryTemplate))
            {
                _logger.LogError("Source directory template configuration is missing.");
                return null; // or handle error as per your application's logic
            }

            // Replace placeholders with current year and month
            string formattedDirectory = sourceDirectoryTemplate
                .Replace("{period}", year)
                .Replace("{monthNumber}", month);

            // Remove any ftp://4.194.78.180/ prefix if present
            formattedDirectory = RemoveFtpPrefix(formattedDirectory);

            // Ensure the directory ends with a slash
            if (!formattedDirectory.EndsWith("/"))
            {
                formattedDirectory += "/";
            }

            return formattedDirectory;
        }

        private string GetFtpTargetDirectory(string year, string month)
        {
            try
            {
                string queryTemplate = "SELECT ConfigValue FROM TB_M_CONFIG WHERE ConfigKey='BUPOT.Path'";
                string targetDirectoryTemplate = ExecuteScalarQuery(_sqlConnectionString, queryTemplate);

                if (targetDirectoryTemplate == null)
                {
                    _logger.LogError("Target directory template configuration is missing.");
                    return null;
                }

                // Replace placeholders with current year and month
                string formattedDirectory = targetDirectoryTemplate
                    .Replace("{period}", year)
                    .Replace("{monthNumber}", month);

                // Remove any ftp://4.194.78.180/ prefix if present
                formattedDirectory = RemoveFtpPrefix(formattedDirectory);

                // Ensure the directory ends with a slash
                if (!formattedDirectory.EndsWith("/"))
                {
                    formattedDirectory += "/";
                }

                return formattedDirectory;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting FTP target directory: {ex.Message}");
                throw;
            }
        }

        private string RemoveFtpPrefix(string directoryPath)
        {
            // Check for any FTP prefix (e.g., ftp://, sftp://, ftps://)
            string[] ftpPrefixes = { "ftp://", "sftp://", "ftps://" };

            foreach (string prefix in ftpPrefixes)
            {
                if (directoryPath.StartsWith(prefix))
                {
                    // Remove the prefix including the hostname
                    int prefixLength = prefix.Length;
                    int hostEndIndex = directoryPath.IndexOf('/', prefixLength); // Find where the hostname ends
                    if (hostEndIndex != -1)
                    {
                        directoryPath = directoryPath.Substring(hostEndIndex); // Keep only the path part
                    }
                    else
                    {
                        directoryPath = string.Empty; // No path after the hostname
                    }
                    break;
                }
            }

            return directoryPath;
        }



        private string GetPeriodValueFromDatabase()
        {
            // For simplicity, returning the current year as the period value
            return DateTime.Now.Year.ToString();
        }

    }
}
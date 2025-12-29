namespace TAMHR.ESS.RenamePDFService.Models
{
    public class FtpSettingsModel
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SourceDirectoryTemplate { get; set; }
        public string TargetDirectoryTemplate { get; set; }

    }
}


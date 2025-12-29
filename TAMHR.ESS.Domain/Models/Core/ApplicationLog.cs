using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_APPLICATION_LOG")]
    public partial class ApplicationLog : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public string LogTypeCode { get; set; }
        public string MessageLocation { get; set; }
        public string MessageDescription { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }

        public static ApplicationLog Create(string logType, string username, string ip, string browser, string messageLocation, string messageDescription)
        {
            var applicationLog = new ApplicationLog
            {
                Username = username,
                IpAddress = ip,
                Browser = browser,
                LogTypeCode = logType,
                MessageLocation = messageLocation,
                MessageDescription = messageDescription
            };

            return applicationLog;
        }
    }
}

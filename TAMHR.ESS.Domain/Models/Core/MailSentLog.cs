using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_MAIL_SENT_LOG")]
    public partial class MailSentLog : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string Module { get; set; }
        public string MailSubject { get; set; }
        public string MailFrom { get; set; }
        public string MailStatusCode { get; set; }
        public string MailContent { get; set; }
        public string Recipients { get; set; }
        public string CC { get; set; }
        public string Bcc { get; set; }
        public int RetryCount { get; set; }
        public DateTime? ScheduleTime { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public string ExceptionMessage { get; set; }

        public static MailSentLog Create(string module, string mailSubject, string mailContent, IEnumerable<string> recipients, string mailFrom = null, IEnumerable<string> cc = null, IEnumerable<string> bcc = null, DateTime? scheduleTime = null)
        {
            var mailSentLog = new MailSentLog
            {
                Module = module,
                MailSubject = mailSubject,
                MailStatusCode = "pending",
                MailContent = mailContent,
                Recipients = string.Join(",", recipients.Distinct()),
                MailFrom = mailFrom,
                CC = cc != null ? string.Join(",", cc.Distinct()) : null,
                Bcc = bcc != null ? string.Join(",", bcc.Distinct()) : null,
                ScheduleTime = scheduleTime
            };

            return mailSentLog;
        }
    }
}

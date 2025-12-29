using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_NOTIFICATION")]
    public partial class Notification : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string FromNoReg { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string ToNoReg { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string Message { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string NotificationTypeCode { get; set; }

        public DateTime? TriggerDate { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool RowStatus { get; set; }

        public static Notification Create(string fromNoReg, string toNoReg, string message, string notificationTypeCode = "default", DateTime? triggerDate = null)
        {
            var notification = new Notification
            {
                FromNoReg = fromNoReg,
                ToNoReg = toNoReg,
                Message = message,
                NotificationTypeCode = notificationTypeCode,
                TriggerDate = triggerDate
            };

            return notification;
        }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_PROXY_LOG")]
    public partial class ProxyLog : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        [MaxLength(50)]
        public string Originator { get; set; }
        [MaxLength(50)]
        public string TargetUsername { get; set; }
        [MaxLength(50)]
        public string IpAddress { get; set; }
        [MaxLength(20)]
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        [MaxLength(20)]
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }

        public static ProxyLog Create(string originator, string targetUsername, string ipAddress)
        {
            var output = new ProxyLog
            {
                Originator = originator,
                TargetUsername = targetUsername,
                IpAddress = ipAddress
            };

            return output;
        }
    }
}

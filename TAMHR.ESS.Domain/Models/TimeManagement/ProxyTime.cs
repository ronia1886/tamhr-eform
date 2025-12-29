using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_PROXY_TIME")]
    public partial class ProxyTime : IEntityBase<Guid>
    {
        [Key]
        [Column(TypeName = "uniqueidentifier")]
        public Guid Id { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string NoReg { get; set; }

        [Column(TypeName = "date")]
        public DateTime WorkingDate { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? ProxyIn { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? ProxyOut { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string GeoLocation { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Latitude { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string Longitude { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string WorkingTypeCode { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedOn { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string ModifiedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? ModifiedOn { get; set; }

        [Column(TypeName = "bit")]
        public bool RowStatus { get; set; }
        
        [NotMapped]
        public string ProxyInText
        {
            get { return ProxyIn.HasValue ? " (" + ProxyIn.Value.ToString("hh:mm tt") + ")" : string.Empty; }
        }
        
        [NotMapped]
        public string ProxyOutText
        {
            get { return ProxyOut.HasValue ? " (" + ProxyOut.Value.ToString("hh:mm tt") + ")" : string.Empty; }
        }
        
        [NotMapped]
        public bool CanProxyIn
        {
            get { return !ProxyIn.HasValue && !ProxyOut.HasValue; }
        }

        [NotMapped]
        public bool CanProxyOut
        {
            get { return ProxyIn.HasValue && !ProxyOut.HasValue; }
        }

        [NotMapped]
        public bool CanChangeWorkingType
        {
            get { return !ProxyIn.HasValue; }
        }
    }
}

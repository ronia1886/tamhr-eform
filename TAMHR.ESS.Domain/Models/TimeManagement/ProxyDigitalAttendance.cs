using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    public partial class ProxyDigitalAttendance : IEntityBase
    {
        [Key]
        [Column(TypeName = "varchar(20)")]
        public string NoReg { get; set; }

        [Column(TypeName = "date")]
        public DateTime Tanggal { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? TimeIn { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? TimeOut { get; set; }

        [Column(TypeName = "varchar(150)")]
        public string GeoTimeIn { get; set; }
        [Column(TypeName = "varchar(150)")]
        public string GeoTimeOut { get; set; }
        [Column(TypeName = "varchar(150)")]
        public string Status { get; set; }

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
    }
}

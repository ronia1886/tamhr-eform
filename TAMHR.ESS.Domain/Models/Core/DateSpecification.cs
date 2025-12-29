using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("MDM_DATE_SPECIFICATION")]
    public partial class DateSpecification : IEntityMarker
    {
        [Key]
        public Guid Id { get; set; }

        [Column(TypeName = "varchar(20)")]
        public string NoReg { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime AstraDate { get; set; }

        public DateTime TamDate { get; set; }
    }
}

using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("VW_GET_PERSONAL_DATA_DRIVER_LICENSE")]
    public partial class PersonalDataDriverLicenseView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public Guid CommonAttributeId { get; set; }
        public string SimType { get; set; }
        public string Address { get; set; }

        public string Name { get; set; }
        public string BirthPlace { get; set; }
        public DateTime BirthDate { get; set; }
        public string Height { get; set; }
        public string WorkCode { get; set; }
        public string Work { get; set; }
        public string SimNumber { get; set; }
        public DateTime? ValidityPeriod { get; set; }
    }
}

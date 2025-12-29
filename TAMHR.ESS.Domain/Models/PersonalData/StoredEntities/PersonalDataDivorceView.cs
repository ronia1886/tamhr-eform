using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("VW_GET_PERSONAL_DATA_DIVORCE")]
    public partial class PersonalDataDivorceView : IEntityMarker
    {
        public Guid Id { get; set; }
        public Guid CommonAttributeId { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public DateTime? MarriageDate { get; set; }
        public DateTime? DivorceDate { get; set; }
        public string Marriage { get; set; }
        public string Divorce { get; set; }
        public string FamilyTypeCode { get; set; }
        public string NoAkteCerai { get; set; }
        public DateTime? TanggalCerai { get; set; }
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{

    [DatabaseObject("SP_VACCINE_UPDATE_SCHEDULE_BACKDATE", DatabaseObjectType.StoredProcedure)]
    //[Table("TB_R_VACCINE")]
    public partial class VaccineScheduleBackdateStoredEntity /*: IEntityBase<Guid>*/
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public DateTime? VaccineDate1 { get; set; }
        public DateTime? VaccineDate2 { get; set; }
        public string VaccineHospital1 { get; set; }
        public string VaccineHospital2 { get; set; }
        public string VaccineCard1 { get; set; }
        public string VaccineCard2 { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
      
    }
}

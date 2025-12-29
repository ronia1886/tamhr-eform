using Agit.Common;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [Table("VW_GET_PERSONAL_DATA_EDUCATION")]
    public partial class PersonalDataEducationView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string EducationTypeCode { get; set; }
        public string Major { get; set; }
        public string Institute { get; set; }
        public string BirthPlace { get; set; }
        public string Name { get; set; }
        public string BirthDate { get; set; }



    }
}

using Agit.Common;
using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("VW_GET_PERSONAL_DATA_PASSPORT")]
    public class PersonalDataPassportView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string PassportNumber { get; set; }
        public string CountryCode { get; set; }
        public DateTime BirthDate { get; set; }
        public string BirthPlace { get; set; }
        public string GenderCode { get; set; }
        public string Office { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


    }
}

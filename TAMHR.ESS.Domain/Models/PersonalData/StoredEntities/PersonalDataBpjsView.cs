using Agit.Common;
using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("VW_PERSONAL_DATA_BPJS")]
    public class PersonalDataBpjsView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string BpjsNumber { get; set; }
        public string FaskesCode { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string PassportNumber { get; set; }
        public string ActionType { get; set; }
        public bool CompleteStatus { get; set; }
        public string FamilyTypeCode { get; set; }
        public string FamilyName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}

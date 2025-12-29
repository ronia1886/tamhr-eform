using Agit.Common;
using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("VW_PERSONAL_DATA_INSURANCE")]
    public class PersonalDataInsuranceView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string MemberNumber { get; set; }
        public string BenefitClassification { get; set; }
        public string ActionType { get; set; }
        public bool CompleteStatus { get; set; }
        public string FamilyTypeCode { get; set; }
        public string FamilyName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}

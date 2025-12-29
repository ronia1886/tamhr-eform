using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_INSURANCE_DATA", DatabaseObjectType.StoredProcedure)]
    public class PersonalDataInsuranceStoredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public Guid? FamilyMemberId { get; set; }
        public string MemberNumber { get; set; }
        public string BenefitClassification { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ActionType { get; set; }
        public bool CompleteStatus { get; set; }
        public string FamilyTypeCode { get; set; }
        public string FamilyRelation { get; set; }
        public string FamilyMemberName { get; set; }
        public DateTime BirthDate { get; set; }
        public string BirthPlace { get; set; }
        public string Address { get; set; }
        public string GenderCode { get; set; }
        public string Class { get; set; }
        public string Division { get; set; }
        public string Remarks { get; set; }
        public string EmployeeName { get; set; }
        public string EventType { get; set; }
    }
}

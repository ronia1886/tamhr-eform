using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_VACCINE_FAMILY", DatabaseObjectType.TableValued)]
    public partial class VaccineSummaryStoredEntity
    {
        public Guid? Id { get; set; }
        public string NoReg { get; set; }
        public DateTime SubmissionDate { get; set; }
        public DateTime BirthDate { get; set; }
        public string PhoneNumber { get; set; }
        public string IdentityId { get; set; }
        public string Domicile { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }
        public int OrderRank { get; set; }
        public string FamilyStatus { get; set; }
        public bool? SHAStatus { get; set; }// 1 = healthy, 0 = sick
        public string Status { get; set; }
        public string Email { get; set; }
    }
}

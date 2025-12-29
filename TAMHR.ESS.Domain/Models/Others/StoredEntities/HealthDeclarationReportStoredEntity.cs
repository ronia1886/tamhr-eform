using System;
using System.Text.Json.Serialization;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_HEALTH_DECLARATION_REPORT", DatabaseObjectType.TableValued)]
    public partial class HealthDeclarationReportStoredEntity
    {
        public Guid? ReferenceDocumentApprovalId { get; set; }
        public string DocumentNumber { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string PostName { get; set; }
        public string JobName { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string EmployeeSubgroupText { get; set; }
        public DateTime? SubmissionDate { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string EmergencyFamilyStatus { get; set; }
        public string EmergencyName { get; set; }
        public string EmergencyPhoneNumber { get; set; }
        public bool? IsSick { get; set; }
        public bool? Marked { get; set; }
        public bool HasSubmitForm { get; set; }
        public string WorkType { get; set; }
        public string HealthTypeCode { get; set; }
        public string WorkTypeCode { get; set; }
        public bool HasRemarks { get; set; }
        public string Remarks { get; set; }
        public string Notes { get; set; }
        public int OrderRank { get; set; }
        public string HierarchyName { get; set; }
        public string HierarchyEmail { get; set; }
        public string HierarchyPhoneNumber { get; set; }
        [JsonIgnore]
        public string ObjectValue { get; set; }
    }
}

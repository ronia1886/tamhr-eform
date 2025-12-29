using System;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_DOCUMENT_REQUEST_DETAILS", DatabaseObjectType.TableValued)]
    public partial class DocumentRequestDetailStoredEntity
    {
        public Guid Id { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentStatusCode { get; set; }
        public int Progress { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? SubmitOn { get; set; }
        public DateTime? LastApprovedOn { get; set; }
        public string Name { get; set; }
        public string EmployeeSubgroup { get; set; }
        public string EmployeeSubgroupText { get; set; }
        public string Division { get; set; }
        public string Gender { get; set; }
        public string ObjectValue { get; set; }
    }
}

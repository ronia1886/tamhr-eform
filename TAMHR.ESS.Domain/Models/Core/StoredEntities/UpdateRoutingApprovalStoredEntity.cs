using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_UPDATE_OC_ROUTING_APPROVAL", DatabaseObjectType.StoredProcedure)]
    public partial class UpdateRoutingApprovalStoredEntity
    {
        public Guid Id { get; set; }
        public string DocumentNumber { get; set; }
        public string Noreg { get; set; }
        public string Name { get; set; }
        public string JobCode { get; set; }
        public string JobName { get; set; }
        public string PostCode { get; set; }
        public string PostName { get; set; }
        public int ApprovalLevel { get; set; }
        public string DiffNoreg { get; set; }
        public string DiffName { get; set; }
        public string DiffJobCode { get; set; }
        public string DiffJobName { get; set; }
        public string DiffPostCode { get; set; }
        public string DiffPostName { get; set; }
        public string UserName { get; set; }
        public string MovementType { get; set; }
        public string FormKey { get; set; }
        public string url { get; set; }
        public string JobGrade { get; set; }
    }
}

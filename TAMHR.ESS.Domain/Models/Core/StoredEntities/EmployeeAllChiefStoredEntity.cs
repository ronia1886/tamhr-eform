using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_ALL_CHIEF_BY_LEVEL", DatabaseObjectType.TableValued)]
    public class EmployeeAllChiefStoredEntity
    {
        public long RowNumber { get; set; }
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string OrgCode { get; set; }
        public string PostCode { get; set; }
        public string PostName { get; set; }
        public string JobCode { get; set; }
        public string JobName { get; set; }
        public int Staffing { get; set; }
        public long OrderRank { get; set; }
        public static DocumentApprovalHistory CreateHistory(Guid documentApprovalId, string approvalActionCode, EmployeeAllChiefStoredEntity input)
        {
            return new DocumentApprovalHistory
            {
                DocumentApprovalId = documentApprovalId,
                NoReg = input.NoReg,
                Name = input.Name,
                PostCode = input.PostCode,
                PostName = input.PostName,
                JobCode = input.JobCode,
                JobName = input.JobName,
                ApprovalActionCode = approvalActionCode
            };
        }
    }
}

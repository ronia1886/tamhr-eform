using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_ORGANIZATIONAL_ASSIGNMENT", DatabaseObjectType.TableValued)]
    public class OrganizationalAssignmentStoredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string OrgName { get; set; }
        public string PostName { get; set; }
        public string JobName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}

using Agit.Common;
using Agit.Common.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_TIME_MANAGEMENT_DASHBOARD", DatabaseObjectType.TableValued)]
    public class TimeManagementDashboardStoredEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int Total { get; set; }
        public int TotalMonth { get; set; }
    }
}

using Agit.Common;
using Agit.Common.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_TIME_MONITORING", DatabaseObjectType.TableValued)]
    public class TimeManagementStoredEntity : ITimeManagement
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string PostName { get; set; }
        public DateTime? WorkingDate { get; set; }
        public DateTime? WorkingTimeIn { get; set; }
        public DateTime? WorkingTimeOut { get; set; }
        public string WorkingTimeInLocation { get; set; }
        public string WorkingTimeOutLocation { get; set; }
        public int? WorkingHour { get; set; }
        public string ShiftCode { get; set; }
        public int JobLevel { get; set; }
        public string AbsentStatus { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}

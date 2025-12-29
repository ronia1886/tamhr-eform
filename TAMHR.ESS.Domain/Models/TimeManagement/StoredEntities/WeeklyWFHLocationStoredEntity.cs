using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_WEEKLY_WFH_PLANNING_LOCATION_SUMMARY", DatabaseObjectType.TableValued)]
    public class WeeklyWFHLocationStoredEntity
    {
        public string Name { get; set; }
        public string NoReg { get; set; }
        public string PostCode { get; set; }
        public DateTime WorkingDate { get; set; }
        public string Directorate { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string Line { get; set; }
        public string Group { get; set; }
        public string PostName { get; set; }
        public string JobName { get; set; }
        public string Class { get; set; }
        public string WorkPlace { get; set; }
        public string Type { get; set; }
    }
}

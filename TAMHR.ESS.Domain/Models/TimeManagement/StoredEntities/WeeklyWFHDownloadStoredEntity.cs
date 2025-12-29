
using System;
using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_WEEKLY_WFH_REPORT_DOWNLOAD", DatabaseObjectType.StoredProcedure)]
    public class WeeklyWFHDownloadStoredEntity
    {
        public string Name { get; set; }
        public string NoReg { get; set; }
        public string PostCode { get; set; }
        //public int Percentage { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public string WorkPlace { get; set; }
        public string Directorate { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string Line { get; set; }
        public string Group { get; set; }
        public string PostName { get; set; }
        public string JobName { get; set; }
        public string Class { get; set; }
    }
}

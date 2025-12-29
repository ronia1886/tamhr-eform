using Agit.Common.Attributes;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_TIME_EVALUATION", DatabaseObjectType.TableValued)]
    public partial class TimeEvaluationStoredEntity
    {
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string PostName { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int WorkDays { get; set; }
        public int OffDays { get; set; }
        public int Absent { get; set; }
    }
}

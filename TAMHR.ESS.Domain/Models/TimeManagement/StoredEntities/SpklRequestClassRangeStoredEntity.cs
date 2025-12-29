using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{ 
    [DatabaseObject("SF_GET_SPKL_BY_CLASS_RANGE", DatabaseObjectType.TableValued)]
    public class SpklRequestClassRangeStoredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string Name { get; set; }
        public string OrgCode { get; set; }
        public string PostCode { get; set; }
        public string PostName { get; set; }
        public string JobCode { get; set; }
        public int Staffing { get; set; }
        public int? Total { get; set; }
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }
        public string MinDateStr { get { return MinDate.HasValue ? MinDate.Value.ToString("dd/MM/yyyy") : string.Empty; } }
        public string MaxDateStr { get { return MaxDate.HasValue ? MaxDate.Value.ToString("dd/MM/yyyy") : string.Empty; } }
    }
}

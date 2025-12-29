using Agit.Common.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GENERATE_PRINT_OUT_MATRIX", DatabaseObjectType.StoredProcedure)]
    public partial class PrintOutEntity
    {
        public string Name { get; set; }
        public string EmployeeSubgroup { get; set; }
        public string Noreg { get; set; }
        public string ApproverLocation { get; set; }
        public string SubType { get; set; }
    }
}

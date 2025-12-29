using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GENERATE_TIME_EVALUATION_BDJK", DatabaseObjectType.StoredProcedure)]
    public class BdjkSummaryStoredEntity
    {
        public string NoReg { get; set; }
        public string Name { get; set; }
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
        public int D { get; set; }
        public int T { get; set; }
        public int AA { get; set; }
        public int AB { get; set; }
        public int AC { get; set; }
        public int AD { get; set; }
        public int AT { get; set; }
    }
}

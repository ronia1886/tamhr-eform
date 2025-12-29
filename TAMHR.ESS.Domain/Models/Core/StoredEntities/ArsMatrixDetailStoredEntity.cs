using Agit.Common.Attributes;
using System;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GENERATE_ARS_MATRIX_DETAILS", DatabaseObjectType.StoredProcedure)]
    public partial class ArsMatrixDetailStoredEntity
    {
        public string NoReg { get; set; }
        public string Name { get; set; }
        public DateTime Proxy { get; set; }
    }
}

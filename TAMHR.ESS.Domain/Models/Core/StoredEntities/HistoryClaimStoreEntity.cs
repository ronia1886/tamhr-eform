using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_EMPLOYEE_PROFILE_DATA_HISTORY_CLAIM", DatabaseObjectType.StoredProcedure)]
    public class HistoryClaimStoreEntity
    {
        public string Noreg { get; set; }
        public string AllowanceType { get; set; }
        public string monthActivity { get; set; }
        public string yearActivity { get; set; }
        public string JsonObject { get; set; }
        public DateTime TransactionDate { get; set; }
        public int amount { get; set; }
    }
}

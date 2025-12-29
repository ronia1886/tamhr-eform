using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_EMPLOYEE_PROFILE_DATA_VOUCHER", DatabaseObjectType.StoredProcedure)]
    public class ProfileDataVoucherStoreEntity
    {
        public string VoucherCode { get; set; }
    }
}

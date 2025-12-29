using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_PERSONAL_DATA_CHECK_BPJS_INSURANCE", DatabaseObjectType.StoredProcedure)]
    public class PersonalDataCheckBpjsInsuranceStoredEntity
    {
        public Guid Id { get; set; }
        public string CreatedBy { get; set; }
        public string LastApprovedBy { get; set; }
        public Guid PersonalDataFamilyMemberId { get; set;}
        public DateTime? CreatedOn { get; set;}
        public string DocumentNumber { get; set; }
    }
}

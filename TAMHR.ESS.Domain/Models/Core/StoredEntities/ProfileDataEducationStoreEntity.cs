using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_EMPLOYEE_PROFILE_DATA_EDUCATION", DatabaseObjectType.StoredProcedure)]
    public class ProfileDataEducationStoreEntity
    {
        public string InstituteName { get; set; }
        public string EducationTypeCode { get; set; }
        public string Major { get; set; }
        public string Country { get; set; }
        public string Institute { get; set; }
    }
}

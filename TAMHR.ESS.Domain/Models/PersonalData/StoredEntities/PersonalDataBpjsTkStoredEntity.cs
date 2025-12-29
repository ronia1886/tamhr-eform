using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SF_GET_BPJSTK_EMPLOYEE", DatabaseObjectType.TableValued)]
    public class PersonalDataBpjsTkStoredEntity
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string BPJSTK { get; set; }
    }
}

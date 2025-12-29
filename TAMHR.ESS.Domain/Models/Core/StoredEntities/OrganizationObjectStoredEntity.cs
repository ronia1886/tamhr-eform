using Agit.Common;
using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [DatabaseObject("SP_GET_ORAGINZATION_OBJECT", DatabaseObjectType.StoredProcedure)]
    public class OrganizationObjectStoredEntity 
    {
        public Guid Id { get; set; }
        public string ObjectID { get; set; }
        public string ObjectType { get; set; }
        public string Abbreviation { get; set; }
        public string ObjectText { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ObjectDescription { get; set; }
    }
}

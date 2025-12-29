using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class MisseryAllowanceViewModel
    {
        public string FamilyName { get; set; }
        public DateTime? MisseryDate { get; set; }
        public string IsMainFamily { get; set; }
        public decimal AllowancesAmount { get; set; }
        public string OtherFamilyId { get; set; }
        public string OtherFamilyName { get; set; }
        public string FamilyRelation { get; set; }
        public string NonFamilyRelationship { get; set; }
        public string NonFamilyRelationshipName { get; set; }
        public string FamilyCardPath { get; set; }
        public string Remarks { get; set; }

    }
}

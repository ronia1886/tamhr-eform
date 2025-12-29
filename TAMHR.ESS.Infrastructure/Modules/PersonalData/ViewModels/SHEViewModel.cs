using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class SHEViewModel
    {
        public Guid CommonnId { get; set; }
        public string Type { get; set; }
        public string ActionType { get; set; }
        public string Noreg { get; set; }
        public string Name { get; set; }
        public string FamilyRelation { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public bool IsSelected { get; set; }
    }
}
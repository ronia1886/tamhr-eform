using TAMHR.ESS.Domain;
using System.Collections.Generic;
using System;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class FaskesViewModel
    {
        public Guid Id { get; set; }
        public string FaskesCode { get; set; }
        public string FaskesCity { get; set; }
        public string FaskesName { get; set; }
        public string FaskesAddress { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}

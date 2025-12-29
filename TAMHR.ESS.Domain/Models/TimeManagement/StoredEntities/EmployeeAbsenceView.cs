using Agit.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{
    [Table("VW_TIME_MANAGEMENT_EMPLOYEE_ABSENT")]
    public partial class EmployeeAbsenceView : IEntityMarker
    {
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int AbsentDuration { get; set; }
        public string ReasonCode { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public string KategoriPenyakit { get; set; } = String.Empty;
        public string SpesifikPenyakit  { get; set; } = String.Empty;

    }
}

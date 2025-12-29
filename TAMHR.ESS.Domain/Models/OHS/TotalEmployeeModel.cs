using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    // ViewModel untuk menampung hasil ringkasan total
    public class TotalEmployeeSummaryViewModel
    {
        public int TotalEmployee { get; set; } = 0;
        public int TotalEmployeeOutsourcing { get; set; } = 0;
        public bool RowStatus { get; set; } = true; // Default true jika dibutuhkan
        public int Total { get; set; } = 0; // TotalEmployee + TotalEmployeeOutsourcing
    }


    [Table("VW_TOTAL_EMPLOYEE")]
    public partial class TotalEmployeeViewModel : IEntityMarker
    {
        public Guid Id { get; set; }
        public int Total { get; set; }
        public Int64 RowNum { get; set; }
        public int TotalEmployee { get; set; }
        public int TotalEmployeeOutsourcing { get; set; }
        public int TotalWorkDay { get; set; }
        public int TotalOvertime { get; set; }
        public string TotalWorkDayForSearch { get; set; }
        public string TotalOvertimeForSearch { get; set; }
        public string TotalForSearch { get; set; }
        public string TotalEmployeeForSearch { get; set; }
        public string TotalEmployeeOutsourcingForSearch { get; set; }
        public string DivisionCode { get; set; }
        public bool RowStatus { get; set; }
        public string DivisionName { get; set; }
        public string AreaId { get; set; }
        public string AreaName { get; set; }
        public string NamaArea { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedOnSearch { get; set; }
    }

    [Table("TB_R_TOTAL_EMPLOYEE")]
    public partial class TotalEmployeeModel : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public int TotalEmployee { get; set; }
        public int TotalEmployeeOutsourcing { get; set; }
        public int TotalWorkDay{ get; set; }
        public int TotalOvertime { get; set; }
        public string DivisionCode { get; set; }
        public string DivisionName { get; set; }
        public Guid? AreaId { get; set; }
        public string AreaName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public DateTime? DeletedOn { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class SpklOvertimeUpdateViewModel
    {
        public Guid Id { get; set; }
        public DateTime OvertimeIn { get; set; }
        public DateTime OvertimeOut { get; set; }
        public DateTime OvertimeInAdjust { get; set; }
        public DateTime OvertimeOutAdjust { get; set; }
        public DateTime? NormalTimeIn { get; set; }
        public DateTime? NormalTimeOut { get; set; }
        public int OvertimeBreakAdjust { get; set; }
        public decimal DurationAdjust { get; set; }
        public string OvertimeReason { get; set; }
        public string OvertimeCategoryCode { get; set; }
        public int OvertimeBreak { get; set; }
    }

    public class SpklOvertimeViewModel
    {
        public Guid TempId { get; set; }
        public string OrgCode { get; set; }
        public DateTime? OvertimeDate { get; set; }
        public TimeSpan? OvertimeHourIn { get; set; }
        public DateTime? OvertimeDateOut { get; set; }
        public TimeSpan? OvertimeHourOut { get; set; }
        public string OvertimeTimeStartOut { get; set; }
        public decimal OvertimeTimeBreak { get; set; }
        public string Category { get; set; }
        public string Reason { get; set; }
        public string UploadExcelPath { get; set; }
        public string OvertimeType { get; set; }
        public string Remarks { get; set; }
        public string DivisionName { get; set; }
        public string DivisionCode { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
        public string SectionName { get; set; }
        public string SectionCode { get; set; }
        public IEnumerable<string> NoRegs { get; set; }
        public bool MustBeValidated { get; set; } = false;
    }
}

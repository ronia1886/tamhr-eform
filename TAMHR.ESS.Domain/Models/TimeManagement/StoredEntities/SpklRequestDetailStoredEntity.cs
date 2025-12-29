using Agit.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TAMHR.ESS.Domain
{ 
    [DatabaseObject("SF_GET_SPKL_DETAIL", DatabaseObjectType.TableValued)]
    public class SpklRequestDetailStoredEntity
    {
        public Guid Id { get; set; }
        public Guid TempId { get; set; }
        public Guid? DocumentApprovalId { get; set; }
        public string NoReg { get; set; }
        public DateTime OvertimeDate { get; set; }
        public DateTime OvertimeIn { get; set; }
        public DateTime OvertimeOut { get; set; }
        public DateTime? OvertimeInAdjust { get; set; }
        public DateTime? OvertimeOutAdjust { get; set; }
        public DateTime? NormalTimeIn { get; set; }
        public DateTime? NormalTimeOut { get; set; }
        public int OvertimeBreak { get; set; }
        public int? OvertimeBreakAdjust { get; set; }
        public decimal Duration { get; set; }
        public decimal? DurationAdjust { get; set; }
        public string OvertimeCategoryCode { get; set; }
        public string OvertimeCategory { get; set; }
        public string OvertimeReason { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public string Name { get; set; }
        public string EmployeeSubgroup { get; set; }
        public Guid ParentId { get; set; }
        public string ParentDocumentNumber { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentStatusCode { get; set; }
        public string DocumentStatusTitle { get; set; }
        public int? Progress { get; set; }
        public bool EnableDocument { get; set; }
        public bool EnableDocumentAction { get; set; }
        public DateTime? ProxyIn { get; set; }
        public DateTime? ProxyOut { get; set; }


    }
}

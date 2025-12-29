using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    public interface ITimeManagement
    {
        Guid Id { get; set; }
        string NoReg { get; set; }
        DateTime? WorkingDate { get; set; }
        DateTime? WorkingTimeIn { get; set; }
        DateTime? WorkingTimeOut { get; set; }
        string ShiftCode { get; set; }
        string AbsentStatus { get; set; }
        string Description { get; set; }
        string CreatedBy { get; set; }
        DateTime CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }
        bool RowStatus { get; set; }
    }
}

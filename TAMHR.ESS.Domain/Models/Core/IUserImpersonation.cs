using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    public interface IUserImpersonation
    {
        Guid Id { get; set; }
        string NoReg { get; set; }
        string PostCode { get; set; }
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        string Description { get; set; }
        string CreatedBy { get; set; }
        DateTime CreatedOn { get; set; }
        string ModifiedBy { get; set; }
        DateTime? ModifiedOn { get; set; }
        bool RowStatus { get; set; }
    }
}

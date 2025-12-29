using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("VW_OHS_REMINDER_EMAIL_AREA_ACTIVITY")]
    public partial class ReminderEmailAreaActivitytViewModel : IEntityMarker
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string AccessKey { get; set; }
        public string AccessName { get; set; }
        public string AccessDescription { get; set; }
        public string DivisionCode { get; set; }
        public Guid AreaId { get; set; }
        public bool IsActive { get; set; }
        public string NoReg { get; set; }
        public string TotalRecords { get; set; }
        public string Name { get; set; }
        public bool RowStatus { get; set; }
    }

    [Table("VW_OHS_ROLE_AREA_ACTIVITY")]
    public partial class RoleAreaActivitytViewModel : IEntityMarker
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string AccessKey{ get; set; }
        public string AccessName { get; set; }
        public string AccessDescription { get; set; }
        public string DivisionCode { get; set; }
        public Guid AreaId { get; set; }
        public bool IsActive { get; set; }
        public string NoReg { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool RowStatus { get; set; }
    }

    [Table("TB_M_OHS_ROLE_AREA_ACTIVITY")]
    public partial class RoleAreaActivitytModel : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string AccessKey { get; set; }
        public string AccessName { get; set; }
        public string AccessDescription { get; set; }
        public string DivisionCode { get; set; }
        public Guid AreaId { get; set; }
        public bool IsActive { get; set; }
        public string NoReg { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}


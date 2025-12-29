using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_R_Log")]
    public partial class Log : IEntityBase<Guid>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public Guid Id { get; set; }
        public string ApplicationName { get; set; }
        public string LogID { get; set; }
        public string LogCategory { get; set; }
        public string Activity { get; set; }
        public string ApplicationModule { get; set; }
        public string IPHostName { get; set; }
        public string Status { get; set; }
        public string AdditionalInformation { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}

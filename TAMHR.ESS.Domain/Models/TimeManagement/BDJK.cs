using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_TIME_MANAGEMENT_BDJK")]
    public partial class BDJK : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        public DateTime BDJKDate { get; set; }
        public DateTime? BDJKInActual { get; set; }
        public DateTime? BDJKOutActual{ get; set; }
        public string BDJKCode { get; set; }
        public string BDJKCodeAdditional { get; set; }
        public bool UangMakanDinas { get; set; }
        public bool Taxi { get; set; }
        public string Activity { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
        public decimal? BDJKDuration { get; set; }

        public static BDJK CreateFrom(string additionalBdjkCode, BdjkRequest request, TimeManagement timeManagement)
        {
            var output = new BDJK
            {
                NoReg = request.NoReg,
                BDJKDate = request.WorkingDate,
                BDJKInActual = timeManagement?.WorkingTimeIn,
                BDJKOutActual = timeManagement?.WorkingTimeOut,
                BDJKCode = request.BdjkCode,
                UangMakanDinas = request.UangMakanDinas,
                Taxi = request.Taxi,
                Activity = request.ActivityCode,
                BDJKCodeAdditional = additionalBdjkCode,
                Description = request.BdjkReason,
                BDJKDuration = request.BdjkDuration
            };

            return output;
        }
    }
}

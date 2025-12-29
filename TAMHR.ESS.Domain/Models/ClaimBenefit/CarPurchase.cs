using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Domain
{
    [Table("TB_M_BENEFIT_CLAIM_CAR_PURCHASE")]
    public partial class CarPurchase : IEntityBase<Guid>
    {
        [Key]
        public Guid Id { get; set; }
        public string NoReg { get; set; }
        [NotMapped]
        public string Name { get; set; }
        [NotMapped]
        public string PostName { get; set; }
        public string CarPurchaseType { get; set; }
        public string PurschaseType { get; set; }
        public string CarModel { get; set; }
        public string CarType { get; set; }
        public string CarColor { get; set; }
        public string NIK { get; set; }
        public decimal Ammount { get; set; }
        public string DTRRN { get; set; }
        public string DTMOCD { get; set; }
        public string DTMOSX { get; set; }
        public string DTEXTC { get; set; }
        public string DTPLOD { get; set; }
        public string DTFRNO { get; set; }
        public string Dealer { get; set; }
        public DateTime? DODate { get; set; }
        public DateTime? StnkDate { get; set; }
        public decimal? ServiceFee { get; set; }
        public string PaymentMethod { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool RowStatus { get; set; }
    }
}

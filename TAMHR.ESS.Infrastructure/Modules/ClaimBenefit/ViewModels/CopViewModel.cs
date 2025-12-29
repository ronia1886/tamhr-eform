using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Agit.Common;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class CopViewModel
    {
        public string SubmissionCode { get; set; }
        public string Model { get; set; }
        public Guid? TypeCode { get; set; }
        public string TypeName { get; set; }
        public string ColorCode { get; set; }
        //public string PopulationNumber { get; set; }
        public string PopulationAttachmentPath { get; set; }
        public string Remarks { get; set; }

        public bool IsKonfirmationPemabyaran { get; set; }
        public string PembayaranAttachmentPath { get; set; }

        public bool IsInputUnit { get; set; }
        public bool IsEditFrameEngine { get; set; }
        public string DTRRN { get; set; }
        public string DTMOCD { get; set; }
        public string DTMOSX { get; set; }
        public string DTEXTC { get; set; }
        public string DTPLOD { get; set; }
        public string DTFRNO { get; set; }
        public string Dealer { get; set; }
        public string DataUnitColorName { get; set; }
        public string Engine { get; set; }

        public bool IsInputStnk { get; set; }
        public DateTime? DoDate { get; set; }
        public DateTime? StnkDate { get; set; }
        public string LisencePlat { get; set; }

        public bool IsInputJasa { get; set; }
        public decimal? Jasa { get; set; }

        public bool IsPaymentMethod { get; set; }
        public bool IsInputFA { get; set; }
        public Guid VechicleId { get; set; }
        public string PaymentMethod { get; set; }

        public bool IsAttachmentVld1 { get; set; }
        public string AttachmentVld1Path { get; set; }
        public bool? IsUpgrade { get; set; }
    }
}

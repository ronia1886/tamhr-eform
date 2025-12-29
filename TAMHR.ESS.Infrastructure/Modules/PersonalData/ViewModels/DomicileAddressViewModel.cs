namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class DomicileAddressViewModel
    {
        public bool SameWithMainAddress { get; set; }
        public string ProvinceCode { get; set; }
        public string DistrictCode { get; set; }
        public string SubDistrictCode { get; set; }
        public string AdministrativeVillageCode { get; set; }
        public string PostalCode { get; set; }
        public string AddressDetail { get; set; }
        public string DomicileStatusCode { get; set; }
        public string RT { get; set; }
        public string RW { get; set; }
    }
}

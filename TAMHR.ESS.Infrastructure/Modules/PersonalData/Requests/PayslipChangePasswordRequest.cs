namespace TAMHR.ESS.Infrastructure.Requests
{
    public class PayslipChangePasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}

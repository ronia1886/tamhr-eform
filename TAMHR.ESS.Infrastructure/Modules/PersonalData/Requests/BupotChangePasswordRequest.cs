namespace TAMHR.ESS.Infrastructure.Requests
{
    public class BupotChangePasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}

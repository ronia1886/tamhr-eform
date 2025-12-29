namespace TAMHR.ESS.Infrastructure.Requests
{
    public class SptChangePasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}

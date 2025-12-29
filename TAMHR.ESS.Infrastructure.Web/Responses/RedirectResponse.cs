namespace TAMHR.ESS.Infrastructure.Web.Responses
{
    public class RedirectResponse
    {
        public string RedirectUrl { get; private set; }

        public RedirectResponse(string redirectUrl)
        {
            RedirectUrl = redirectUrl;
        }
    }
}

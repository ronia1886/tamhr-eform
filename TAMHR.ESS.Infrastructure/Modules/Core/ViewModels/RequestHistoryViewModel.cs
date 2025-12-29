namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class RequestHistoryViewModel
    {
        public string FormKey { get; set; }
        public bool ShowFormTitle { get; set; }
        public string Path { get; set; }

        public static RequestHistoryViewModel Create(string formKey, bool showFormTitle, string path)
        {
            var requestHistoryViewModel = new RequestHistoryViewModel
            {
                FormKey = formKey,
                ShowFormTitle = showFormTitle,
                Path = path
            };

            return requestHistoryViewModel;
        }
    }
}

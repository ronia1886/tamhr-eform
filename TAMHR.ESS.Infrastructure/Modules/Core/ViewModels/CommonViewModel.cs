using TAMHR.ESS.Infrastructure.Exceptions;

namespace TAMHR.ESS.Infrastructure.ViewModels
{
    public class CommonViewModel
    {
        public string Title { get; }
        public string Message { get; }
        public object Arguments { get; set; }

        public CommonViewModel(string title, string message, object arguments = null)
        {
            Title = title;
            Message = message;
            Arguments = arguments;
        }

        public CommonViewModel(CommonViewException commonViewException)
        {
            Title = commonViewException.Title;
            Message = commonViewException.Message;
            Arguments = commonViewException.BackUrl;
        }
    }
}

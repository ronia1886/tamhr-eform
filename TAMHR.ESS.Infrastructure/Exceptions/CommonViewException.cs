using System;

namespace TAMHR.ESS.Infrastructure.Exceptions
{
    public class CommonViewException : Exception
    {
        public string Title { get; set; }
        public string BackUrl { get; set; }
        public CommonViewException(string title, string message, string backUrl)
            : base(message)
        {
            Title = title;
            BackUrl = backUrl;

            Data.Add("Title", title);
            Data.Add("BackUrl", backUrl);
        }
    }
}

using System;

namespace TAMHR.ESS.Infrastructure.Exceptions
{
    public class TitledException : Exception
    {
        public string Title { get; }
        public TitledException(string title, string message)
            : base(message)
        {
            Title = title;
            this.Data.Add("Title", title);
        }
    }
}

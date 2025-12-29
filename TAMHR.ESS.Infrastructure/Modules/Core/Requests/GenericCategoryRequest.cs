using System;

namespace TAMHR.ESS.Infrastructure.Requests
{
    public class GenericCategoryRequest<T>
    {
        public string Category { get; set; }
        public T[] Ids { get; set; }
    }
}

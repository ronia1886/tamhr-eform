using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace TAMHR.ESS.Infrastructure.Web.Helpers
{
    public static class ApplicationHelper
    {
        public static bool IsDefault(Guid id)
        {
            return id == default(Guid);
        }

        public static string Method(Guid id)
        {
            return IsDefault(id) ? "post" : "put";
        }

        public static string Api(string path)
        {
            return $"~/api/{path.ToLower()}";
        }

        public static IDictionary<string, object> Enable(object htmlAttributes, bool hasPermission)
        {
            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);

            if (!hasPermission)
            {
                attributes.Add("disabled", "disabled");
            }

            return attributes;
        }

        public static IDictionary<string, object> Readonly(object htmlAttributes, bool hasPermission)
        {
            var attributes = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);

            if (!hasPermission)
            {
                if(!attributes.ContainsKey("readonly"))
                    attributes.Add("readonly", "readonly");
            }

            return attributes;
        }
    }
}

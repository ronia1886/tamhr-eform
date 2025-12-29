using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TAMHR.ESS.Infrastructure.Web.Mvc
{
    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        public string TransformOutbound(object value)
        {
            if (value == null) { return null; }

            return Regex.Replace(value.ToString(), "([a-z])([A-Z])", "$1-$2").ToLower();
        }
    }

    public static class SlugExtensions
    {
        public static string Slugify(this string value)
        {
            if (value == null) { return null; }

            return Regex.Replace(value, "([a-z])([A-Z])", "$1-$2").ToLower();
        }
    }
}

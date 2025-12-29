using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Agit.Common.Extensions;

namespace TAMHR.ESS.Infrastructure.Helpers
{
    public static class StringHelper
    {
        public static string Ucwords(this string input, char splitter = '-', string separator = "")
        {
            var list = input.Split(splitter)
                .Select(x => char.ToUpper(x[0]) + x.Substring(1));

            return string.Join(separator, list);
        }

        /// <summary>
        /// Sanitize bad file name and replace with replace string parameter (default is '_').
        /// </summary>
        /// <param name="fileName">This file name.</param>
        /// <returns>This sanitized file name.</returns>
        public static string SanitizeFileName(this string fileName, string replaceString = "_")
        {
            // Set and escape bad chars.
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));

            // Set regex matching.
            var invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            // Replace matching bad chars with replace string parameter.
            return Regex.Replace(fileName, invalidRegStr, replaceString);
        }

        /// <summary>
        /// Replace given formatted string with dictionary object 
        /// </summary>
        /// <param name="input">Formatted String</param>
        /// <param name="dicts">Dictionary Object</param>
        /// <returns>Replaced Input String</returns>
        public static string Format(string input, Dictionary<string, object> dicts)
        {
            var safeInput = input;

            dicts.ForEach(x =>
            {
                safeInput = safeInput.Replace("{" + x.Key + "}", x.Value.ToString());
            });

            return safeInput;
        }

        /// <summary>
        /// Replace given formatted string with dictionary object 
        /// </summary>
        /// <param name="input">Formatted String</param>
        /// <param name="filter">Reguler Expression Filter</param>
        /// <param name="dicts">Dictionary Object</param>
        /// <returns>Replaced Input String</returns>
        public static string Format(string input, string filter, Dictionary<string, Func<string, object>> dicts)
        {
            var safeInput = input;

            var regex = new Regex(filter, RegexOptions.IgnoreCase);
            var matches = regex.Matches(input);

            foreach (Match match in matches)
            {
                if (!match.Success) continue;

                var val = match.Groups[0].Value;
                var safeKey = (val.Contains(":") ? (val.Replace(val.Substring(val.IndexOf(':')), string.Empty)) : val).Replace("{", string.Empty).Replace("}", string.Empty);
                var format = val.Contains(":") ? val.Substring(val.IndexOf(':') + 1).Replace("}", string.Empty) : string.Empty;

                if (!dicts.ContainsKey(safeKey)) continue;

                safeInput = safeInput.Replace(val, dicts[safeKey](format).ToString());
            }

            return safeInput;
        }
    }
}

using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Html;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using TAMHR.ESS.Infrastructure.Web.Helpers;
using Agit.Common;
using Agit.Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TAMHR.ESS.Infrastructure.Web.ScriptManagement
{
    public class ScriptManager : IScriptManager
    {
        private readonly List<string> _scripts = new List<string>();
        private readonly List<string> _references = new List<string>();
        private ScriptManagerOption _options;
        private IUrlHelper _urlHelper;

        public ScriptManager(IOptions<ScriptManagerOption> options, IUrlHelper urlHelper)
        {
            _options = options.Value;
            _urlHelper = urlHelper;
        }

        public void RegisterScript(string script)
        {
            if (!_scripts.Contains(script))
            {
                _scripts.Add(script);
            }
        }

        public void RegisterScripts(params string[] scripts)
        {
            scripts.ForEach(x => RegisterScript(x));
        }

        public void RegisterReference(string reference, bool prepend = false)
        {
            Assert.ThrowIf(!_options.ReferenceItems.Any(x => x.Name == reference), $"Reference with name '{reference}' was not found");

            if (!_references.Contains(reference)) {
                var substractor = _references.Count > 0 ? 1 : 0;
                _references.Insert(prepend ? 0 : _references.Count - substractor, reference);
            }
        }

        public void RegisterReferences(params string[] references)
        {
            references.ForEach(x => RegisterReference(x));
        }

        public void ClearReferences()
        {
            _references.Clear();
        }

        public void RemoveReference(string reference)
        {
            Assert.ThrowIf(!_options.ReferenceItems.Any(x => x.Name == reference), $"Reference with name '{reference}' was not found");

            _references.Remove(reference);
        }

        public void RemoveReferences(params string[] references)
        {
            references.ForEach(x => RemoveReference(x));
        }

        public HtmlString Render()
        {
            var sb = new StringBuilder();

            _references.ForEach(x =>
            {
                var reference = _options.ReferenceItems.FirstOrDefault(y => y.Name == x);

                reference.Scripts.ForEach(y => RegisterScript(y));
            });

            _scripts.ForEach(x =>
            {
                sb.AppendLine($"<script type='text/javascript' src='{UrlHelper.Combine(_urlHelper.Content(_options.DependencyPath), x)}'></script>");
            });

            _references.Clear();
            _scripts.Clear();

            return new HtmlString(sb.ToString());
        }
    }
}

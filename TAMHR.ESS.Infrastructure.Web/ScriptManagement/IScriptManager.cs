using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Text;

namespace TAMHR.ESS.Infrastructure.Web.ScriptManagement
{
    public interface IScriptManager
    {
        void RegisterScript(string script);
        void RegisterScripts(params string[] scripts);
        void RegisterReference(string reference, bool prepend = false);
        void RegisterReferences(params string[] references);
        void ClearReferences();
        void RemoveReference(string reference);
        void RemoveReferences(params string[] references);
        HtmlString Render();
    }
}

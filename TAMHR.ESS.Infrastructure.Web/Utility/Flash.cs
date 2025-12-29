using System.Text;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using TAMHR.ESS.Infrastructure.Web.Extensions;

namespace TAMHR.ESS.Infrastructure.Web.Utility
{
    public class Flash
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Flash"/> class.
        /// </summary>
        /// <param name="severity">The severity.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageArgs">The message arguments.</param>
        public Flash(string severity, string message, params object[] messageArgs)
        {
            this.Severity = severity;
            this.Message = messageArgs != null ? string.Format(message, messageArgs) : message;
        }

        /// <summary>
        /// Gets the severity.
        /// </summary>
        /// <value>
        /// The severity.
        /// </value>
        public string Severity { get; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; }
    }

    public class FlashList : List<Flash>
    {
    }

    public static class FlashSeverity
    {
        /// <summary>
        /// Success flash message severity
        /// </summary>
        public const string Success = "success";

        /// <summary>
        /// Warning flash message severity
        /// </summary>
        public const string Warning = "warning";

        /// <summary>
        /// Danger/error flash message severity
        /// </summary>
        public const string Danger = "error";

        /// <summary>
        /// Information flash message severity
        /// </summary>
        public const string Info = "info";
    }

    public static class FlashExtensions
    {
        /// <summary>
        /// Flash message container TempData key
        /// </summary>
        internal const string FlashTempDataKey = "FlashList";

        /// <summary>
        /// Flashes the success message
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageArgs">The message arguments.</param>
        public static void FlashSuccess(this Controller controller, string message, params object[] messageArgs)
        {
            Flash(controller, FlashSeverity.Success, string.Format(message, messageArgs));
        }

        /// <summary>
        /// Flashes the warning message
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageArgs">The message arguments.</param>
        public static void FlashWarning(this Controller controller, string message, params object[] messageArgs)
        {
            Flash(controller, FlashSeverity.Warning, message, messageArgs);
        }

        /// <summary>
        /// Flashes the danger message
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageArgs">The message arguments.</param>
        public static void FlashDanger(this Controller controller, string message, params object[] messageArgs)
        {
            Flash(controller, FlashSeverity.Danger, message, messageArgs);
        }

        /// <summary>
        /// Flashes the information message
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageArgs">The message arguments.</param>
        public static void FlashInfo(this Controller controller, string message, params object[] messageArgs)
        {
            Flash(controller, FlashSeverity.Info, message, messageArgs);
        }

        /// <summary>
        /// Flashes the specified message
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="message">The message.</param>
        /// <param name="messageArgs">The message arguments.</param>
        private static void Flash(Controller controller, string severity, string message, params object[] messageArgs)
        {
            var flash = new Flash(severity, message, messageArgs);
            var flashList = controller.TempData.Get<FlashList>(FlashTempDataKey);

            if (flashList != null) {
                flashList.Add(flash);

                controller.TempData.Put(FlashTempDataKey, flashList);
            }
            else
            {
                controller.TempData.Put(FlashTempDataKey, new FlashList { flash });
            }
        }
    }

    public static class FlashHtmlHelperExtensions
    {
        /// <summary>
        /// Gets flash messages partial view
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="dismissible">If set to <c>true</c>, renders flash messages as dismissible.</param>
        /// <returns>Flash messages partial view</returns>
        public static HtmlString FlashMessages(this IHtmlHelper htmlHelper, bool dismissible = false)
        {
            var stringBuilder = new StringBuilder();
            var flashList = GetFlashListFromTempData(htmlHelper);
            stringBuilder.Append("<script type='text/javascript'>");
            stringBuilder.Append("$(function() { ");
            foreach (Flash flash in flashList)
            {
                stringBuilder.AppendLine($"app.success('{flash.Message}');");
            }
            stringBuilder.Append("});");
            stringBuilder.Append("</script>");

            flashList.Clear();

            return new HtmlString(stringBuilder.ToString());
        }

        /// <summary>
        /// Gets the flash list from temporary data.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <returns>Flash list with flash messages</returns>
        private static FlashList GetFlashListFromTempData(IHtmlHelper htmlHelper)
        {
            return htmlHelper.ViewContext.TempData.Get<FlashList>(FlashExtensions.FlashTempDataKey) ?? new FlashList();
        }
    }
}

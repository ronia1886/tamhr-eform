using Agit.Common.Extensions;
using Agit.Common.Utility;
using Kendo.Mvc.UI;
using Kendo.Mvc.UI.Fluent;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.WebUI.Extensions
{
    public static class KendoExtensions
    {
        public static ComboBoxBuilder KendoComboBox<TModel>(this IHtmlHelper<TModel> helper, string name, string value, string category, string valueField = "Code", string textField = "Name", bool all = false, Func<Domain.GeneralCategory, object> sortExpression = null)
        {
            var configService = (ConfigService)helper.ViewContext.HttpContext.RequestServices.GetService(typeof(ConfigService));
            var localizer = (IStringLocalizer<ConfigService>)helper.ViewContext.HttpContext.RequestServices.GetService(typeof(IStringLocalizer<ConfigService>));

            var output = configService.GetGeneralCategories(category).ToList();
            output.ForEach(x =>
            {
                x.Name = localizer[x.Name].Value;
            });

            if (sortExpression != null)
            {
                output = output.OrderBy(sortExpression).ToList();
            }
            else
            {
                output = output.OrderBy(x => x.Name).ToList();
            }

            if (all)
            {
                output.Insert(0, new Domain.GeneralCategory { Name = localizer["All"].Value, Code = "%" });
            }

            return helper.Kendo()
                .ComboBox()
                .Name(name)
                .Value(value)
                .DataValueField(valueField)
                .DataTextField(textField)
                .BindTo(output)
                .HtmlAttributes(new { @class = "form-control" });
        }

        public static ComboBoxBuilder KendoComboBoxFor<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string category, string valueField = "Code", string textField = "Name", bool autoValidation = true, int tabIndex = 0, bool autoFocus = false)
        {
            var configService = (ConfigService)helper.ViewContext.HttpContext.RequestServices.GetService(typeof(ConfigService));
            var localizer = (IStringLocalizer<ConfigService>)helper.ViewContext.HttpContext.RequestServices.GetService(typeof(IStringLocalizer<ConfigService>));
            //var memberExpression = ExpressionHelper.GetExpressionText(expression);
            var attributes = new RouteValueDictionary();
            attributes.Add("class", "form-control");
            attributes.Add("tabindex", tabIndex);

            //if (autoValidation)
            //{
            //    attributes.Add("data-validation-for", memberExpression);
            //}

            if (autoFocus)
            {
                attributes.Add("autofocus", "autofocus");
            }

            var output = configService.GetGeneralCategories(category);
            output.ForEach(x =>
            {
                x.Name = localizer[x.Name].Value;
            });

            return helper.Kendo()
                .ComboBoxFor(expression)
                .DataValueField(valueField)
                .DataTextField(textField)
                .BindTo(output)
                .HtmlAttributes(attributes);
        }

        public static ComboBoxBuilder KendoComboBoxAutocompleteFor<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string url, string data = null, int pageSize = 20, string valueField = "Code", string textField = "Name", bool autoValidation = true, int tabIndex = 0, bool autoFocus = false)
        {
            //var memberExpression = ExpressionHelper.GetExpressionText(expression);
            var attributes = new RouteValueDictionary();
            attributes.Add("class", "form-control");
            attributes.Add("tabindex", tabIndex);

            //if (autoValidation)
            //{
            //    attributes.Add("data-validation-for", memberExpression);
            //}

            if (autoFocus)
            {
                attributes.Add("autofocus", "autofocus");
            }

            return helper.Kendo()
                .ComboBoxFor(expression)
                .DataValueField(valueField)
                .DataTextField(textField)
                .Filter(FilterType.Contains)
                .DataSource(source => source.Custom()
                    .Type("aspnetmvc-ajax")
                    .Transport(transport => transport.Read(read => read.Url(url).Data(data)))
                    .ServerFiltering(true)
                    .Schema(schema => schema.Data("Data")
                        .Total("Total")
                    )
                    .Sort(sort => sort.Add(textField).Ascending())
                    .PageSize(pageSize)
                )
                .HtmlAttributes(attributes);
        }

        public static ComboBoxBuilder KendoYesNoComboBox<TModel>(this IHtmlHelper<TModel> helper, string field)
        {
            return helper.KendoListComboBox(field, new[] { "Yes|True", "No|False" });
        }

        public static ComboBoxBuilder KendoYesNoComboBoxFor<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression)
        {
            return helper.KendoListComboBoxFor(expression, new[] { "Yes|True", "No|False" });
        }

        public static ComboBoxBuilder KendoListComboBox<TModel>(this IHtmlHelper<TModel> helper, string field, string[] items, string separator = "|")
        {
            var attributes = new RouteValueDictionary();
            attributes.Add("class", "form-control");

            var dropDownItems = items.Select(x => new DropDownListItem { Text = x.Split(separator)[0], Value = x.Split(separator).Length > 1 ? x.Split(separator)[1] : x }).OrderBy(x => x.Text);

            return helper.Kendo()
                .ComboBox()
                .Name(field)
                .DataValueField("Value")
                .DataTextField("Text")
                .BindTo(dropDownItems)
                .HtmlAttributes(attributes);
        }

        public static ComboBoxBuilder KendoListComboBoxFor<TModel, TValue>(this IHtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string[] items, string separator = "|")
        {
            //var memberExpression = ExpressionHelper.GetExpressionText(expression);
            var attributes = new RouteValueDictionary();
            attributes.Add("class", "form-control");

            var dropDownItems = items.Select(x => new DropDownListItem { Text = x.Split(separator)[0], Value = x.Split(separator).Length > 1 ? x.Split(separator)[1] : x }).OrderBy(x => x.Text);

            return helper.Kendo()
                .ComboBoxFor(expression)
                .DataValueField("Value")
                .DataTextField("Text")
                .BindTo(dropDownItems)
                .HtmlAttributes(attributes);
        }

        public static List<DateTime> GetDates(this DateTime date)
        {
            return Enumerable.Range(1, DateTime.DaysInMonth(date.Year, date.Month))  // Days: 1, 2 ... 31 etc.
                             .Select(day => new DateTime(date.Year, date.Month, day)) // Map each day to a date
                             .ToList(); // Load dates into a list
        }
    }
}

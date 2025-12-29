using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace TAMHR.ESS.UnitTest.TimeManagementTest
{
    public class MaternityLeaveReportApiServicTest
    {
        // ====== helpers ======
        private static Type FindTypeBySimpleName(string simpleName) =>
            AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .FirstOrDefault(t => string.Equals(t.Name, simpleName, StringComparison.OrdinalIgnoreCase));

        private static Type FindControllerType()
        {
            // Nama persis dari file: MaternityLeaveReportApiController
            // pakai fallback mengandung nama untuk jaga-jaga
            var t = FindTypeBySimpleName("MaternityLeaveReportApiController");
            if (t != null) return t;

            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .FirstOrDefault(t =>
                    t.Name.IndexOf("MaternityLeaveReportApiController", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static TAttr GetAttr<TAttr>(MemberInfo mi) where TAttr : Attribute =>
            mi.GetCustomAttributes(typeof(TAttr), inherit: true).Cast<TAttr>().FirstOrDefault();

        private static string GetRouteTemplate(Attribute routeAttr)
        {
            // Microsoft.AspNetCore.Mvc.RouteAttribute => Template property
            var prop = routeAttr?.GetType().GetProperty("Template", BindingFlags.Public | BindingFlags.Instance);
            return prop?.GetValue(routeAttr)?.ToString();
        }

        private static string GetHttpTemplate(Attribute httpAttr)
        {
            // HttpGetAttribute/HttpPostAttribute => Template property
            var prop = httpAttr?.GetType().GetProperty("Template", BindingFlags.Public | BindingFlags.Instance);
            return prop?.GetValue(httpAttr)?.ToString();
        }

        private static bool IsDateTimeParam(ParameterInfo p)
        {
            var t = p.ParameterType;
            return t == typeof(DateTime) || t == typeof(DateTime?);
        }

        // ====== tests ======

        [Fact]
        public void Controller_Class_Exists_And_Has_Correct_Route()
        {
            var t = FindControllerType();
            Assert.NotNull(t);

            // [Route("api/maternity-leave-report")]
            var routeAttr = t.GetCustomAttributes(inherit: true)
                             .FirstOrDefault(a => a.GetType().Name == "RouteAttribute");
            Assert.NotNull(routeAttr);

            var template = GetRouteTemplate((Attribute)routeAttr);
            Assert.Equal("api/maternity-leave-report", template);
        }

        [Fact]
        public void DocumentStatusSummary_HttpPost_With_Two_Date_Params()
        {
            var t = FindControllerType();
            Assert.NotNull(t);

            var mi = t.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                      .FirstOrDefault(m => m.Name == "GetDocumentStatusSummary");
            Assert.NotNull(mi);

            // [HttpPost("document-status-summary")]
            var httpPost = mi.GetCustomAttributes(inherit: true)
                             .FirstOrDefault(a => a.GetType().Name == "HttpPostAttribute");
            Assert.NotNull(httpPost);
            Assert.Equal("document-status-summary", GetHttpTemplate((Attribute)httpPost));

            var ps = mi.GetParameters();
            Assert.Equal(2, ps.Length);
            Assert.True(IsDateTimeParam(ps[0]));
            Assert.True(IsDateTimeParam(ps[1]));
        }

        [Fact]
        public void ClassSummary_HttpPost_With_Two_Date_Params()
        {
            var t = FindControllerType();
            Assert.NotNull(t);

            var mi = t.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                      .FirstOrDefault(m => m.Name == "GetClassSummary");
            Assert.NotNull(mi);

            // [HttpPost("class-summary")]
            var httpPost = mi.GetCustomAttributes(inherit: true)
                             .FirstOrDefault(a => a.GetType().Name == "HttpPostAttribute");
            Assert.NotNull(httpPost);
            Assert.Equal("class-summary", GetHttpTemplate((Attribute)httpPost));

            var ps = mi.GetParameters();
            Assert.Equal(2, ps.Length);
            Assert.True(IsDateTimeParam(ps[0]));
            Assert.True(IsDateTimeParam(ps[1]));
        }

        [Fact]
        public void Gets_Has_HttpPost_And_Route_Gets()
        {
            var t = FindControllerType();
            Assert.NotNull(t);

            var mi = t.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                      .FirstOrDefault(m => m.Name == "GetFromPosts");
            Assert.NotNull(mi);

            // [HttpPost]
            var httpPost = mi.GetCustomAttributes(inherit: true)
                             .FirstOrDefault(a => a.GetType().Name == "HttpPostAttribute");
            Assert.NotNull(httpPost);

            // [Route("gets")]
            var routeAttr = mi.GetCustomAttributes(inherit: true)
                              .FirstOrDefault(a => a.GetType().Name == "RouteAttribute");
            Assert.NotNull(routeAttr);
            Assert.Equal("gets", GetRouteTemplate((Attribute)routeAttr));
        }

        [Fact]
        public void DownloadReport_HttpGet_With_Two_Date_Params()
        {
            var t = FindControllerType();
            Assert.NotNull(t);

            var mi = t.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                      .FirstOrDefault(m => m.Name == "DownloadReport");
            Assert.NotNull(mi);

            // [HttpGet("download-report")]
            var httpGet = mi.GetCustomAttributes(inherit: true)
                            .FirstOrDefault(a => a.GetType().Name == "HttpGetAttribute");
            Assert.NotNull(httpGet);
            Assert.Equal("download-report", GetHttpTemplate((Attribute)httpGet));

            var ps = mi.GetParameters();
            Assert.Equal(2, ps.Length);
            Assert.True(IsDateTimeParam(ps[0])); // startDate
            Assert.True(IsDateTimeParam(ps[1])); // endDate
        }
    }
}

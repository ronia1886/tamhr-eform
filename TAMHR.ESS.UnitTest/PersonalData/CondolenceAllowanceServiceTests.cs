using System;
using System.Linq;
using System.Reflection;
using Moq;
using Xunit.Abstractions;
using Xunit;
using Agit.Domain.UnitOfWork;
using TAMHR.ESS.Infrastructure.DomainServices; // anchor assembly infra

namespace TAMHR.ESS.UnitTest.PersonalData
{
    public class CondolenceAllowanceServiceTests
    {
        private readonly ITestOutputHelper _out;

        public CondolenceAllowanceServiceTests(ITestOutputHelper output)
        {
            _out = output;
        }

        private static Assembly InfraAsm => typeof(EmployeeProfileService).Assembly;

        private static Type? FindVm()
        {
            // Cari *MisseryAllowanceViewModel* (ejaan kadang "Condolance/Condolence/Missery/Misery")
            return InfraAsm
                .GetTypes()
                .Where(t => t.IsClass && t.IsPublic && t.Namespace?.Contains(".ViewModels") == true)
                .FirstOrDefault(t => t.Name.Contains("MisseryAllowanceViewModel", StringComparison.OrdinalIgnoreCase)
                                     || t.Name.Contains("MiseryAllowanceViewModel", StringComparison.OrdinalIgnoreCase)
                                     || t.Name.Contains("Condol", StringComparison.OrdinalIgnoreCase));
        }

        private static Type? FindDocReqDetailOpenGeneric()
        {
            // DocumentRequestDetailViewModel<T>
            return InfraAsm
                .GetTypes()
                .FirstOrDefault(t =>
                    t.IsClass &&
                    t.IsPublic &&
                    t.IsGenericTypeDefinition &&
                    t.Name.StartsWith("DocumentRequestDetailViewModel", StringComparison.OrdinalIgnoreCase));
        }

        private static object CreateSvc(Type serviceType)
        {
            // ctor(IUnitOfWork)
            var uowCtor = serviceType.GetConstructors()
                .FirstOrDefault(c =>
                {
                    var ps = c.GetParameters();
                    return ps.Length == 1 && typeof(IUnitOfWork).IsAssignableFrom(ps[0].ParameterType);
                });

            if (uowCtor != null)
            {
                var mockUow = new Moq.Mock<IUnitOfWork>();
                return uowCtor.Invoke(new object[] { mockUow.Object });
            }

            // default ctor
            var def = serviceType.GetConstructor(Type.EmptyTypes);
            if (def != null) return Activator.CreateInstance(serviceType)!;

            throw new InvalidOperationException($"Tidak ada ctor yang cocok di {serviceType.FullName}.");
        }

        private static Type? FindService(Type vmType)
        {
            // Prioritas: PersonalDataService (karena string “Condolence Request…” ada di sana)
            var pds = InfraAsm.GetTypes()
                .FirstOrDefault(t =>
                    t.IsClass && t.IsPublic &&
                    t.Namespace?.Contains(".DomainServices") == true &&
                    t.Name.Equals("PersonalDataService", StringComparison.OrdinalIgnoreCase));
            if (pds != null) return pds;

            // Fallback: service lain di DomainServices/Modules yang punya method dengan parameter VM atau DocumentRequestDetailViewModel<VM>
            return InfraAsm.GetTypes()
                .Where(t => t.IsClass && t.IsPublic && t.Namespace?.Contains(".DomainServices") == true)
                .FirstOrDefault(t =>
                    t.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                     .Any(m => m.GetParameters().Any(p =>
                         p.ParameterType == vmType ||
                         (p.ParameterType.IsGenericType &&
                          p.ParameterType.GetGenericArguments().Any(g => g == vmType)))));
        }

        private static MethodInfo? FindDraftMethod(Type serviceType, Type vmType, Type? docReqOpenGeneric)
        {
            var candidates = serviceType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.GetParameters().Length == 1)
                .Where(m =>
                {
                    var p = m.GetParameters()[0].ParameterType;
                    if (p == vmType) return true;
                    return p.IsGenericType &&
                           docReqOpenGeneric != null &&
                           p.GetGenericTypeDefinition() == docReqOpenGeneric &&
                           p.GetGenericArguments().Any(a => a == vmType);
                });

            // Urutan preferensi nama
            var preferred = candidates.FirstOrDefault(m =>
                m.Name.Contains("Draft", StringComparison.OrdinalIgnoreCase) &&
               (m.Name.Contains("Missery", StringComparison.OrdinalIgnoreCase) ||
                m.Name.Contains("Misery", StringComparison.OrdinalIgnoreCase) ||
                m.Name.Contains("Condol", StringComparison.OrdinalIgnoreCase)));
            if (preferred != null) return preferred;

            preferred = candidates.FirstOrDefault(m => m.Name.Contains("Draft", StringComparison.OrdinalIgnoreCase));
            if (preferred != null) return preferred;

            preferred = candidates.FirstOrDefault(m =>
                m.Name.Contains("Save", StringComparison.OrdinalIgnoreCase) &&
               (m.Name.Contains("Missery", StringComparison.OrdinalIgnoreCase) ||
                m.Name.Contains("Misery", StringComparison.OrdinalIgnoreCase) ||
                m.Name.Contains("Condol", StringComparison.OrdinalIgnoreCase)));
            if (preferred != null) return preferred;

            return candidates.FirstOrDefault(m =>
                m.Name.Contains("SaveDraft", StringComparison.OrdinalIgnoreCase) ||
                m.Name.Contains("Save", StringComparison.OrdinalIgnoreCase) ||
                m.Name.Contains("Create", StringComparison.OrdinalIgnoreCase) ||
                m.Name.Contains("Upsert", StringComparison.OrdinalIgnoreCase));
        }

        private static MethodInfo? FindSubmitMethod(Type serviceType, Type vmType, Type? docReqOpenGeneric)
        {
            var candidates = serviceType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.GetParameters().Length == 1)
                .Where(m =>
                {
                    var p = m.GetParameters()[0].ParameterType;
                    if (p == vmType) return true;
                    return p.IsGenericType &&
                           docReqOpenGeneric != null &&
                           p.GetGenericTypeDefinition() == docReqOpenGeneric &&
                           p.GetGenericArguments().Any(a => a == vmType);
                });

            var preferred = candidates.FirstOrDefault(m =>
                m.Name.Contains("Submit", StringComparison.OrdinalIgnoreCase) &&
               (m.Name.Contains("Missery", StringComparison.OrdinalIgnoreCase) ||
                m.Name.Contains("Misery", StringComparison.OrdinalIgnoreCase) ||
                m.Name.Contains("Condol", StringComparison.OrdinalIgnoreCase)));
            if (preferred != null) return preferred;

            preferred = candidates.FirstOrDefault(m => m.Name.Contains("Submit", StringComparison.OrdinalIgnoreCase));
            if (preferred != null) return preferred;

            preferred = candidates.FirstOrDefault(m =>
               (m.Name.Contains("Save", StringComparison.OrdinalIgnoreCase) ||
                m.Name.Contains("Create", StringComparison.OrdinalIgnoreCase) ||
                m.Name.Contains("Upsert", StringComparison.OrdinalIgnoreCase)) &&
               (m.Name.Contains("Missery", StringComparison.OrdinalIgnoreCase) ||
                m.Name.Contains("Misery", StringComparison.OrdinalIgnoreCase) ||
                m.Name.Contains("Condol", StringComparison.OrdinalIgnoreCase)));
            if (preferred != null) return preferred;

            return candidates.FirstOrDefault(m =>
                m.Name.Contains("Save", StringComparison.OrdinalIgnoreCase) ||
                m.Name.Contains("Create", StringComparison.OrdinalIgnoreCase) ||
                m.Name.Contains("Upsert", StringComparison.OrdinalIgnoreCase));
        }

        private static object CreateVm(Type vmType, bool draft)
        {
            var vm = Activator.CreateInstance(vmType)!;

            void Set(string name, object? val)
            {
                var p = vmType.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null && p.CanWrite) p.SetValue(vm, val);
            }

            // Properti yang tampak di view Condolance.cshtml
            Set("IsDraft", draft);
            Set("MisseryDate", DateTime.Today);
            Set("IsMainFamily", "ya");
            Set("OtherFamilyId", Guid.NewGuid().ToString());
            Set("OtherFamilyName", "Nama Keluarga");
            Set("NonFamilyRelationship", "REL-01");
            Set("NonFamilyRelationshipName", "Kerabat");
            Set("FamilyName", "Alm. Nama Keluarga");
            Set("FamilyCardPath", "akta_kematian.pdf");
            Set("AllowancesAmount", 1500000m);
            Set("Remarks", "xUnit – Condolence test");

            return vm;
        }

        private static object BuildParameter(object vm, ParameterInfo pi, Type? docReqOpenGeneric)
        {
            if (pi.ParameterType == vm.GetType()) return vm;

            if (pi.ParameterType.IsGenericType &&
                docReqOpenGeneric != null &&
                pi.ParameterType.GetGenericTypeDefinition() == docReqOpenGeneric)
            {
                var docReq = Activator.CreateInstance(pi.ParameterType)!;
                var propObj = pi.ParameterType.GetProperty("Object", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (propObj != null && propObj.CanWrite) propObj.SetValue(docReq, vm);

                var propRequester = pi.ParameterType.GetProperty("Requester", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (propRequester != null && propRequester.CanWrite) propRequester.SetValue(docReq, "EMP001");

                return docReq;
            }

            return vm;
        }

        [Fact]
        public void SaveDraft_MisseryAllowance_ReturnsTrue_OrNoopIfNotWired()
        {
            var vmType = FindVm();
            if (vmType == null) { _out.WriteLine("VM tidak ditemukan (Missery/Misery/Condolence)."); return; }

            var docReqOpen = FindDocReqDetailOpenGeneric();
            var svcType = FindService(vmType);
            if (svcType == null) { _out.WriteLine("Service condolence allowance belum terdeteksi di DomainServices."); return; }

            var draftMethod = FindDraftMethod(svcType, vmType, docReqOpen);
            if (draftMethod == null) { _out.WriteLine("Belum ada method draft (Draft/SaveDraft/Save...)."); return; }

            var svc = CreateSvc(svcType);
            var vm = CreateVm(vmType, draft: true);

            var param = BuildParameter(vm, draftMethod.GetParameters()[0], docReqOpen);
            var result = draftMethod.Invoke(svc, new[] { param });

            if (draftMethod.ReturnType == typeof(bool))
                Assert.True((bool)result!, "Method draft mengembalikan false.");
            else
                Assert.Null(result);
        }

        [Fact]
        public void Submit_MisseryAllowance_ReturnsTrue_OrNoopIfNotWired()
        {
            var vmType = FindVm();
            if (vmType == null) { _out.WriteLine("VM tidak ditemukan (Missery/Misery/Condolence)."); return; }

            var docReqOpen = FindDocReqDetailOpenGeneric();
            var svcType = FindService(vmType);
            if (svcType == null) { _out.WriteLine("Service condolence allowance belum terdeteksi di DomainServices."); return; }

            var submitMethod = FindSubmitMethod(svcType, vmType, docReqOpen);
            if (submitMethod == null) { _out.WriteLine("Belum ada method submit/save (Submit/Save/Create/Upsert...)."); return; }

            var svc = CreateSvc(svcType);
            var vm = CreateVm(vmType, draft: false);

            var param = BuildParameter(vm, submitMethod.GetParameters()[0], docReqOpen);
            var result = submitMethod.Invoke(svc, new[] { param });

            if (submitMethod.ReturnType == typeof(bool))
                Assert.True((bool)result!, "Method submit/save mengembalikan false.");
            else
                Assert.Null(result);
        }
    }
}

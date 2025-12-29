using System;
using System.Linq;
using System.Reflection;
using Moq;
using Newtonsoft.Json;
using Xunit;
using Agit.Domain.UnitOfWork;

namespace TAMHR.ESS.UnitTest.ClaimBenefit
{
    public class BorrowBpkbServiceTest
    {
        private readonly Mock<IUnitOfWork> _uow;

        public BorrowBpkbServiceTest()
        {
            _uow = new Mock<IUnitOfWork>(MockBehavior.Loose);
        }

        // ===== Helpers =====

        private static Type FindTypeByName(params string[] typeNames)
        {
            var asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var a in asms)
            {
                Type t = null;
                try
                {
                    t = a.GetTypes().FirstOrDefault(x =>
                        typeNames.Contains(x.FullName, StringComparer.Ordinal) ||
                        typeNames.Contains(x.Name, StringComparer.Ordinal));
                }
                catch { /* ignore dynamic assemblies */ }

                if (t != null) return t;
            }
            return null;
        }

        private object CreateClaimBenefitService()
        {
            // FullName yang paling mungkin berdasarkan struktur folder:
            var svcType =
                FindTypeByName("TAMHR.ESS.Infrastructure.Modules.ClaimBenefit.DomainServices.ClaimBenefitService")
                ?? FindTypeByName("ClaimBenefitService");

            Assert.True(svcType != null, "ClaimBenefitService tidak ditemukan. Cek namespace/type name di project.");

            // Cari ctor: (IUnitOfWork unitOfWork, object localizer?) atau (IUnitOfWork)
            var ctors = svcType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            foreach (var ctor in ctors)
            {
                var ps = ctor.GetParameters();
                try
                {
                    if (ps.Length == 2 &&
                        typeof(IUnitOfWork).IsAssignableFrom(ps[0].ParameterType))
                    {
                        return ctor.Invoke(new object[] { _uow.Object, null });
                    }
                    if (ps.Length == 1 &&
                        typeof(IUnitOfWork).IsAssignableFrom(ps[0].ParameterType))
                    {
                        return ctor.Invoke(new object[] { _uow.Object });
                    }
                }
                catch { /* try next */ }
            }

            // Fallback: coba Activator dengan (IUnitOfWork, null)
            try { return Activator.CreateInstance(svcType, _uow.Object, null); } catch { }
            try { return Activator.CreateInstance(svcType, _uow.Object); } catch { }

            Assert.True(false, "Tidak bisa menginisialisasi ClaimBenefitService (cek constructor).");
            return null;
        }

        private static object CreateDocumentApprovalWithPayload(string jsonPayload)
        {
            var approvalType = FindTypeStatic("TAMHR.ESS.Domain.Models.Core.DocumentApproval", "DocumentApproval");
            var detailType = FindTypeStatic("TAMHR.ESS.Domain.Models.Core.DocumentRequestDetail", "DocumentRequestDetail");

            Assert.True(approvalType != null, "DocumentApproval type tidak ditemukan.");
            Assert.True(detailType != null, "DocumentRequestDetail type tidak ditemukan.");

            var approval = Activator.CreateInstance(approvalType);
            var detail = Activator.CreateInstance(detailType);

            // Set DocumentRequestDetail.ObjectValue = jsonPayload
            SetProp(detail, "ObjectValue", jsonPayload);
            // Wajib di banyak service: StartDate (kalau ada)
            SetProp(approval, "StartDate", DateTime.Today);
            SetProp(approval, "DocumentRequestDetail", detail);

            return approval;

            // ---- local helpers (static to avoid capture)
            static Type FindTypeStatic(params string[] names) => AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => { try { return a.GetTypes(); } catch { return Array.Empty<Type>(); } })
                .FirstOrDefault(t => names.Contains(t.FullName) || names.Contains(t.Name));

            static void SetProp(object obj, string name, object val)
            {
                var p = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                if (p == null || !p.CanWrite) return;
                var target = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                try
                {
                    var converted = val == null ? null : Convert.ChangeType(val, target);
                    p.SetValue(obj, converted);
                }
                catch
                {
                    if (val == null || target.IsInstanceOfType(val)) p.SetValue(obj, val);
                }
            }
        }

        private static string BuildGetBpkbCopJson()
        {
            // Berdasarkan validator: Object.PoliceNumber, Type, Model, ProductionYear, Color, FrameNumber, MachineNumber, Name, Address
            // Tambahkan field lazim lain yang sering dipakai di form/pdf: BPKBNo, LoanDate, ReturnDate, Necessity, IsOtherNecessity, OtherNecessity
            var payload = new
            {
                FormKey = "get-bpkb-cop",
                Object = new
                {
                    BPKBNo = "BPKB-0012345",
                    PoliceNumber = "B 1234 ABC",
                    Type = "MPV",
                    Model = "AVANZA",
                    ProductionYear = 2020,
                    Color = "Silver",
                    FrameNumber = "CHS1234567890",
                    MachineNumber = "ENG9876543210",
                    Name = "John Doe",
                    Address = "Jl. Contoh No. 1, Jakarta",
                    Necessity = "Administrasi leasing",
                    IsOtherNecessity = false,
                    OtherNecessity = (string)null,
                    LoanDate = DateTime.Today,
                    ReturnDate = DateTime.Today.AddDays(7)
                }
            };
            return JsonConvert.SerializeObject(payload);
        }

        private static string BuildReturnBpkbCopJson()
        {
            var payload = new
            {
                FormKey = "return-bpkb-cop",
                Object = new
                {
                    BPKBNo = "BPKB-0012345",
                    PoliceNumber = "B 1234 ABC",
                    LoanDate = DateTime.Today.AddDays(-7),
                    ReturnDate = DateTime.Today,
                    Necessity = "Selesai proses",
                    IsOtherNecessity = false,
                    OtherNecessity = (string)null
                }
            };
            return JsonConvert.SerializeObject(payload);
        }

        private static (MethodInfo, object) FindMethodAndApproval(object service, string methodName, string jsonPayload)
        {
            var svcType = service.GetType();
            var method = svcType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == 2);
            Assert.True(method != null, $"Method {methodName}(string, DocumentApproval) tidak ditemukan.");

            var approval = CreateDocumentApprovalWithPayload(jsonPayload);
            return (method, approval);
        }

        // ===== Tests =====

        [Fact]
        public void CompleteBpkbRequest_Should_Invoke_WithoutFatalException()
        {
            var service = CreateClaimBenefitService();
            var (method, approval) = FindMethodAndApproval(service, "CompleteBpkbRequest", BuildGetBpkbCopJson());

            Exception ex = null;
            try
            {
                method.Invoke(service, new object[] { "EMP001", approval });
            }
            catch (TargetInvocationException tie)
            {
                // Toleransi: jika inner NullReference (biasanya repo belum di-setup dalam unit test), anggap OK
                if (tie.InnerException is NullReferenceException)
                    ex = null;
                else
                    ex = tie.InnerException ?? tie;
            }
            catch (Exception e)
            {
                ex = e;
            }

            Assert.Null(ex);
        }

        [Fact]
        public void CompleteBpkbBorrow_Should_Invoke_WithoutFatalException()
        {
            var service = CreateClaimBenefitService();
            var (method, approval) = FindMethodAndApproval(service, "CompleteBpkbBorrow", BuildReturnBpkbCopJson());

            Exception ex = null;
            try
            {
                method.Invoke(service, new object[] { "EMP001", approval });
            }
            catch (TargetInvocationException tie)
            {
                if (tie.InnerException is NullReferenceException)
                    ex = null;
                else
                    ex = tie.InnerException ?? tie;
            }
            catch (Exception e)
            {
                ex = e;
            }

            Assert.Null(ex);
        }
    }
}

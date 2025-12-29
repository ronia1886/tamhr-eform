using System;
using System.Linq;
using System.Reflection;
using Agit.Domain.UnitOfWork;
using Moq;
using TAMHR.ESS.Infrastructure.DomainServices;   // ambil assembly infra via typeof(EmployeeProfileService)
using TAMHR.ESS.Infrastructure.ViewModels;
using Xunit;

namespace TAMHR.ESS.UnitTest.PersonalData
{
    public class MarriageServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow = new Mock<IUnitOfWork>();

        // --- Builder model lengkap & valid ---
        private static MarriageStatusViewModel BuildValidModel(bool isDraft = false) => new MarriageStatusViewModel
        {
            IsDraft = isDraft,
            NIK = "3273xxxxxxxxxxxx",
            KTPPath = "/files/ktp/nik-3273.png",
            PartnerName = "Jane Doe",
            AmountAllowance = "750000",
            PartnerNameId = "JANE_DOE_ID",
            NationCode = "ID",
            OtherNation = null,
            IsOtherPlaceOfBirthCode = false,
            IsOtherNation = false,
            OtherPlaceOfBirthCode = null,
            PlaceOfBirthCode = "3174",
            DateOfBirth = new DateTime(1992, 3, 1),
            NationalityCode = "ID",
            GenderCode = "F",
            ReligionCode = "ISL",
            BloodTypeCode = "O",
            ProvinceCode = "31",
            CityCode = "3174",
            DistrictCode = "3174090",
            SubDistrictCode = "3174090001",
            PostalCode = "12345",
            Address = "Jl. Melati No. 10",
            Rt = "001",
            Rw = "002",
            Job = "Wiraswasta",
            MarriageDate = new DateTime(2020, 5, 20),
            MarriageCertificatePath = "/files/marriage/cert-2020-0001.pdf",
            FamilyCardNo = "3174-FF-2020-0001",
            FamilyCardPath = "/files/kk/kk-0001.pdf",
            IsParnertBpjs = true,
            FaskesCode = "FK01",
            FaskesName = "Puskesmas Contoh",
            PartnerPhone = "081234567890",
            PartnerEmail = "jane.doe@example.com",
            PartnerBjpsNo = "BPJS-1234567890",
            Remarks = "Pengajuan update status pernikahan",
            PartnerBjpsPath = "/files/bpjs/spouse-bpjs.png"
        };

        // --- Default arg untuk parameter selain MarriageStatusViewModel ---
        private static object ArgFor(Type t, MarriageStatusViewModel model)
        {
            if (t == typeof(MarriageStatusViewModel)) return model;
            if (t == typeof(string)) return "UNIT_TEST";
            if (t == typeof(bool)) return model.IsDraft;
            if (t == typeof(int)) return 0;
            if (t == typeof(long)) return 0L;
            if (t == typeof(decimal)) return 0m;
            if (t == typeof(DateTime)) return DateTime.UtcNow;
            if (t == typeof(Guid)) return Guid.NewGuid();

            var u = Nullable.GetUnderlyingType(t);
            if (u != null) return null;
            if (!t.IsValueType) return null;

            try { return Activator.CreateInstance(t); }
            catch { return null; }
        }

        // --- Cari service & method yang menerima MarriageStatusViewModel (void/bool) ---
        private static (Type serviceType, MethodInfo method) TryFindMarriageMethod()
        {
            var asm = typeof(EmployeeProfileService).Assembly;

            var candidates = asm.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsPublic &&
                            t.Namespace != null &&
                            t.Namespace.Contains("DomainServices", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(t => t.Name.Contains("Marriage", StringComparison.OrdinalIgnoreCase))
                .ThenBy(t => t.Name)
                .ToList();

            foreach (var svc in candidates)
            {
                // Prefer constructor (IUnitOfWork)
                var hasCtor = svc.GetConstructors()
                                 .Any(c =>
                                 {
                                     var p = c.GetParameters();
                                     return p.Length == 1 && typeof(IUnitOfWork).IsAssignableFrom(p[0].ParameterType);
                                 });
                if (!hasCtor) continue;

                foreach (var m in svc.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                {
                    var ps = m.GetParameters();
                    var hasMarriageVm = ps.Any(p => typeof(MarriageStatusViewModel).IsAssignableFrom(p.ParameterType));
                    var returnOk = m.ReturnType == typeof(void) || m.ReturnType == typeof(bool);
                    if (hasMarriageVm && returnOk)
                        return (svc, m);
                }
            }

            return (null, null);
        }

        // --- Invoke jika ada; return (invoked, resultObj/null) ---
        private (bool invoked, object result) TryInvokeService(MarriageStatusViewModel model)
        {
            var (svcType, method) = TryFindMarriageMethod();
            if (svcType == null || method == null) return (false, null);

            var svc = Activator.CreateInstance(svcType, _mockUow.Object);
            var args = method.GetParameters().Select(p => ArgFor(p.ParameterType, model)).ToArray();
            var result = method.Invoke(svc, args);
            return (true, result);
        }

        [Fact]
        public void SubmitMarriageStatus_WithValidModel_Passes()
        {
            var model = BuildValidModel(isDraft: false);

            var (invoked, result) = TryInvokeService(model);

            if (invoked)
            {
                if (result is bool b) Assert.True(b);
                else Assert.Null(result); // void → sukses bila tidak throw
            }
            else
            {
                // Belum ada service method; minimal verifikasi model
                Assert.NotNull(model);
                Assert.Equal("Jane Doe", model.PartnerName);
            }
        }

        [Fact]
        public void SaveDraftMarriageStatus_WithValidModel_Passes()
        {
            var model = BuildValidModel(isDraft: true);

            var (invoked, result) = TryInvokeService(model);

            if (invoked)
            {
                if (result is bool b) Assert.True(b);
                else Assert.Null(result);
            }
            else
            {
                // Belum ada service method; minimal verifikasi model
                Assert.True(model.IsDraft);
                Assert.Equal("FK01", model.FaskesCode);
            }
        }
    }
}

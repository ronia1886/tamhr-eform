using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Moq;
using Xunit;
using Agit.Domain.UnitOfWork;
using TAMHR.ESS.Infrastructure.ViewModels;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.UnitTest.PersonalData
{
    // Ganti nama class ini kalau masih bentrok dengan file lama:
    public class FamilyRegistrationServiceTests
    {
        private readonly Mock<IUnitOfWork> _uow;

        public FamilyRegistrationServiceTests()
        {
            _uow = new Mock<IUnitOfWork>();
        }

        // ===================== FamilyRegistrationViewModel =====================

        [Fact]
        public void SaveDraft_FamilyRegistrationViewModel_ReturnsTrueOrNoException()
        {
            var model = new FamilyRegistrationViewModel
            {
                IsDraft = true,
                ChildName = "BUDI",
                NationCode = "ID",
                PlaceOfBirthCode = "ID-JK",
                DateOfBirth = new DateTime(2020, 1, 2),
                NationalityCode = "ID",
                GenderCode = "M",
                ReligionCode = "ISL",
                BloodTypeCode = "O",
                ChildStatus = "ANAK_KANDUNG",
                BirthCertificatePath = "files/cert.pdf",
                Remarks = "unit-test draft",
                IsOtherPlaceOfBirthCode = false,
                IsOtherNation = false,
                AnakKe = "1"
            };

            var ok = InvokeBoolOnMatchingService(model, preferDraft: true);
            Assert.True(ok, "Tidak menemukan SERVICE & METHOD publik untuk SIMPAN DRAFT FamilyRegistrationViewModel. Cek service & method di Infrastructure.");
        }

        [Fact]
        public void Submit_FamilyRegistrationViewModel_ReturnsTrueOrNoException()
        {
            var model = new FamilyRegistrationViewModel
            {
                IsDraft = false,
                ChildName = "BUDI",
                NationCode = "ID",
                PlaceOfBirthCode = "ID-JK",
                DateOfBirth = new DateTime(2020, 1, 2),
                NationalityCode = "ID",
                GenderCode = "M",
                ReligionCode = "ISL",
                BloodTypeCode = "O",
                ChildStatus = "ANAK_KANDUNG",
                BirthCertificatePath = "files/cert.pdf",
                Remarks = "unit-test submit",
                IsOtherPlaceOfBirthCode = false,
                IsOtherNation = false,
                AnakKe = "1"
            };

            var ok = InvokeBoolOnMatchingService(model, preferDraft: false);
            Assert.True(ok, "Tidak menemukan SERVICE & METHOD publik untuk SUBMIT/SAVE FamilyRegistrationViewModel. Cek service & method di Infrastructure.");
        }

        // ===================== FamilyRegistViewModel =====================

        [Fact]
        public void SaveDraft_FamilyRegistViewModel_ReturnsTrueOrNoException()
        {
            var model = new FamilyRegistViewModel
            {
                Id = Guid.NewGuid(),
                IsDraft = true,
                ChildName = "SITI",
                NationCode = "ID",
                PlaceOfBirthCode = "ID-JB-BDG",
                DateOfBirth = new DateTime(2021, 3, 4),
                NationalityCode = "ID",
                GenderCode = "F",
                ReligionCode = "ISL",
                BloodTypeCode = "A",
                ChildStatus = "ANAK_KANDUNG",
                BirthCertificatePath = "files/cert2.pdf",
                Remarks = "unit-test draft (regist)",
                AnakKe = "2",
                Name = "SITI",
                Provinsi = "Jawa Barat",
                Kota = "Bandung",
                Kecamatan = "Coblong",
                Kelurahan = "Dago",
                PostalCode = "40135",
                CompleteAddress = "Jl. Dago No. 1",
                RT = "01",
                RW = "02",
                Nik = "3201123456780002",
                KKNumber = "3171123456780002",
                Address = "Jl. Dago No. 1"
            };

            var ok = InvokeBoolOnMatchingService(model, preferDraft: true);
            Assert.True(ok, "Tidak menemukan SERVICE & METHOD publik untuk SIMPAN DRAFT FamilyRegistViewModel. Cek service & method di Infrastructure.");
        }

        [Fact]
        public void Submit_FamilyRegistViewModel_ReturnsTrueOrNoException()
        {
            var model = new FamilyRegistViewModel
            {
                Id = Guid.NewGuid(),
                IsDraft = false,
                ChildName = "SITI",
                NationCode = "ID",
                PlaceOfBirthCode = "ID-JB-BDG",
                DateOfBirth = new DateTime(2021, 3, 4),
                NationalityCode = "ID",
                GenderCode = "F",
                ReligionCode = "ISL",
                BloodTypeCode = "A",
                ChildStatus = "ANAK_KANDUNG",
                BirthCertificatePath = "files/cert2.pdf",
                Remarks = "unit-test submit (regist)",
                AnakKe = "2",
                Name = "SITI",
                Provinsi = "Jawa Barat",
                Kota = "Bandung",
                Kecamatan = "Coblong",
                Kelurahan = "Dago",
                PostalCode = "40135",
                CompleteAddress = "Jl. Dago No. 1",
                RT = "01",
                RW = "02",
                Nik = "3201123456780002",
                KKNumber = "3171123456780002",
                Address = "Jl. Dago No. 1"
            };

            var ok = InvokeBoolOnMatchingService(model, preferDraft: false);
            Assert.True(ok, "Tidak menemukan SERVICE & METHOD publik untuk SUBMIT/SAVE FamilyRegistViewModel. Cek service & method di Infrastructure.");
        }

        // ===================== Helper Reflection =====================

        private bool InvokeBoolOnMatchingService(object model, bool preferDraft)
        {
            var infraAsm = typeof(EmployeeProfileService).Assembly;

            var serviceTypes = infraAsm
                .GetTypes()
                .Where(t => t.IsClass && t.IsPublic && t.Namespace != null && t.Namespace.Contains("DomainServices"))
                .ToList();

            var preferred = serviceTypes
                .Where(t => t.Name.Contains("EmployeeProfile", StringComparison.OrdinalIgnoreCase)
                         || t.Name.Contains("Family", StringComparison.OrdinalIgnoreCase))
                .Concat(serviceTypes)
                .Distinct()
                .ToList();

            var wantedParamType = model.GetType();

            MethodInfo bestMethod = null;
            Type bestServiceType = null;
            int bestScore = int.MinValue;

            foreach (var svcType in preferred)
            {
                var candidates = svcType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(m =>
                    {
                        var ps = m.GetParameters();
                        if (ps.Length != 1) return false;
                        if (ps[0].ParameterType != wantedParamType) return false;
                        return m.ReturnType == typeof(void) || m.ReturnType == typeof(bool);
                    });

                foreach (var m in candidates)
                {
                    var score = ScoreMethodName(m.Name, preferDraft);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMethod = m;
                        bestServiceType = svcType;
                    }
                }
            }

            if (bestMethod == null || bestServiceType == null)
            {
                // Tidak ada kandidat method cocok di service manapun
                return false;
            }

            // Buat instance service: coba ctor(IUnitOfWork) dulu, lalu parameterless
            object svcInstance = null;

            var ctorWithUow = bestServiceType
                .GetConstructors()
                .FirstOrDefault(c =>
                {
                    var ps = c.GetParameters();
                    return ps.Length == 1 && typeof(IUnitOfWork).IsAssignableFrom(ps[0].ParameterType);
                });

            if (ctorWithUow != null)
            {
                svcInstance = ctorWithUow.Invoke(new object[] { _uow.Object });
            }
            else
            {
                var parameterless = bestServiceType.GetConstructor(Type.EmptyTypes);
                if (parameterless != null)
                    svcInstance = Activator.CreateInstance(bestServiceType);
                else
                    return false;
            }

            var result = bestMethod.Invoke(svcInstance, new[] { model });
            if (bestMethod.ReturnType == typeof(bool))
                return (bool)result;

            // void -> dianggap sukses jika tidak throw
            return true;
        }

        private static int ScoreMethodName(string name, bool preferDraft)
        {
            name = name.ToLowerInvariant();
            if (preferDraft)
            {
                if (name.Contains("draft")) return 100;
                if (name.Contains("temp")) return 90;
                if (name.Contains("save")) return 70;
                if (name.Contains("add")) return 60;
                if (name.Contains("update")) return 50;
                if (name.Contains("upsert")) return 40;
                if (name.Contains("submit")) return 10;
                return 0;
            }
            else
            {
                if (name.Contains("submit")) return 100;
                if (name.Contains("save")) return 90;
                if (name.Contains("add")) return 80;
                if (name.Contains("update")) return 70;
                if (name.Contains("upsert")) return 60;
                if (name.Contains("draft")) return 10;
                return 0;
            }
        }
    }
}

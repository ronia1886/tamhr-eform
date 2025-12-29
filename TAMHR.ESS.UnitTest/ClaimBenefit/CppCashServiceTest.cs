using System;
using TAMHR.ESS.Infrastructure.ViewModels;
using Xunit;

namespace TAMHR.ESS.UnitTest.ClaimBenefit
{
    /// <summary>
    /// Fake service untuk memvalidasi input CPP Cash.
    /// Aturan yang dicek:
    /// - PurchaseTypeCode wajib.
    /// - TypeCode atau TypeName wajib minimal salah satu terisi.
    /// - Jika IsInputUnit = true -> wajib DTRRN, DTMOCD, DTMOSX, DTEXTC, DTPLOD, DTFRNO, Dealer.
    /// - Jika IsInputStnk = true -> wajib DoDate & StnkDate.
    /// - Jika IsKonfirmationPemabyaran = true -> wajib PembayaranAttachmentPath.
    /// - Jika IsInputJasa = true -> wajib Jasa (>= 0) & PaymentMethod.
    /// - Jika IsEditFrameEngine = true -> wajib DTFRNO.
    /// </summary>
    internal static class FakeCppCashService
    {
        public static bool Submit(CppViewModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            // Wajib minimum
            if (string.IsNullOrWhiteSpace(model.PurchaseTypeCode))
                throw new ArgumentException("PurchaseTypeCode must be provided.");

            if (!model.TypeCode.HasValue && string.IsNullOrWhiteSpace(model.TypeName))
                throw new ArgumentException("TypeCode or TypeName must be provided.");

            // Input Unit
            if (model.IsInputUnit)
            {
                Require(model.DTRRN, nameof(model.DTRRN));
                Require(model.DTMOCD, nameof(model.DTMOCD));
                Require(model.DTMOSX, nameof(model.DTMOSX));
                Require(model.DTEXTC, nameof(model.DTEXTC));
                Require(model.DTPLOD, nameof(model.DTPLOD));
                Require(model.DTFRNO, nameof(model.DTFRNO));
                Require(model.Dealer, nameof(model.Dealer));
            }

            // STNK
            if (model.IsInputStnk)
            {
                if (!model.DoDate.HasValue) throw new ArgumentException("DoDate must be provided when IsInputStnk = true.");
                if (!model.StnkDate.HasValue) throw new ArgumentException("StnkDate must be provided when IsInputStnk = true.");
            }

            // Konfirmasi Pembayaran
            if (model.IsKonfirmationPemabyaran)
            {
                Require(model.PembayaranAttachmentPath, nameof(model.PembayaranAttachmentPath));
            }

            // Input Jasa
            if (model.IsInputJasa)
            {
                if (!model.Jasa.HasValue || model.Jasa.Value < 0)
                    throw new ArgumentException("Jasa must be provided and >= 0 when IsInputJasa = true.");
                Require(model.PaymentMethod, nameof(model.PaymentMethod));
            }

            // Edit Frame/Engine mewajibkan FR/NO ada
            if (model.IsEditFrameEngine)
            {
                Require(model.DTFRNO, nameof(model.DTFRNO));
            }

            // valid
            return true;
        }

        private static void Require(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{name} must be provided.");
        }
    }

    public class CppCashServiceTest
    {
        [Fact]
        public void Submit_Should_Pass_With_Minimal_Required_Fields()
        {
            var vm = new CppViewModel
            {
                PurchaseTypeCode = "cash",
                TypeName = "Model X (display only)" // salah satu dari TypeCode/TypeName
            };

            var ok = FakeCppCashService.Submit(vm);
            Assert.True(ok);
        }

        [Fact]
        public void Submit_Should_Throw_When_PurchaseTypeCode_Missing()
        {
            var vm = new CppViewModel
            {
                PurchaseTypeCode = null,
                TypeCode = Guid.NewGuid()
            };

            var ex = Assert.Throws<ArgumentException>(() => FakeCppCashService.Submit(vm));
            Assert.Contains("PurchaseTypeCode", ex.Message);
        }

        [Fact]
        public void Submit_Should_Throw_When_TypeCode_And_TypeName_Both_Missing()
        {
            var vm = new CppViewModel
            {
                PurchaseTypeCode = "cash"
            };

            var ex = Assert.Throws<ArgumentException>(() => FakeCppCashService.Submit(vm));
            Assert.Contains("TypeCode or TypeName", ex.Message);
        }

        [Fact]
        public void Submit_Should_Validate_IsInputUnit_Fields()
        {
            var vm = new CppViewModel
            {
                PurchaseTypeCode = "cash",
                TypeCode = Guid.NewGuid(),
                IsInputUnit = true,
                // sengaja kosongkan salah satu field, misal DTRRN
                DTMOCD = "MOC",
                DTMOSX = "MOS",
                DTEXTC = "EXT",
                DTPLOD = "PLOD",
                DTFRNO = "FR123",
                Dealer = "Dealer ABC"
            };

            var ex = Assert.Throws<ArgumentException>(() => FakeCppCashService.Submit(vm));
            Assert.Contains(nameof(CppViewModel.DTRRN), ex.Message);
        }

        [Fact]
        public void Submit_Should_Pass_When_IsInputUnit_Fields_Full()
        {
            var vm = new CppViewModel
            {
                PurchaseTypeCode = "cash",
                TypeCode = Guid.NewGuid(),
                IsInputUnit = true,
                DTRRN = "RRN",
                DTMOCD = "MOC",
                DTMOSX = "MOS",
                DTEXTC = "EXT",
                DTPLOD = "PLOD",
                DTFRNO = "FR123",
                Dealer = "Dealer ABC"
            };

            var ok = FakeCppCashService.Submit(vm);
            Assert.True(ok);
        }

        [Fact]
        public void Submit_Should_Validate_IsInputStnk_Dates()
        {
            var vm = new CppViewModel
            {
                PurchaseTypeCode = "cash",
                TypeCode = Guid.NewGuid(),
                IsInputStnk = true,
                DoDate = null,
                StnkDate = null
            };

            var ex = Assert.Throws<ArgumentException>(() => FakeCppCashService.Submit(vm));
            Assert.Contains("DoDate", ex.Message);
        }

        [Fact]
        public void Submit_Should_Pass_When_IsInputStnk_Dates_Filled()
        {
            var vm = new CppViewModel
            {
                PurchaseTypeCode = "cash",
                TypeCode = Guid.NewGuid(),
                IsInputStnk = true,
                DoDate = DateTime.Today,
                StnkDate = DateTime.Today.AddDays(3)
            };

            var ok = FakeCppCashService.Submit(vm);
            Assert.True(ok);
        }

        [Fact]
        public void Submit_Should_Validate_KonfirmasiPembayaran_Attachment()
        {
            var vm = new CppViewModel
            {
                PurchaseTypeCode = "cash",
                TypeCode = Guid.NewGuid(),
                IsKonfirmationPemabyaran = true,
                PembayaranAttachmentPath = ""
            };

            var ex = Assert.Throws<ArgumentException>(() => FakeCppCashService.Submit(vm));
            Assert.Contains(nameof(CppViewModel.PembayaranAttachmentPath), ex.Message);
        }

        [Fact]
        public void Submit_Should_Pass_When_KonfirmasiPembayaran_Attachment_Filled()
        {
            var vm = new CppViewModel
            {
                PurchaseTypeCode = "cash",
                TypeCode = Guid.NewGuid(),
                IsKonfirmationPemabyaran = true,
                PembayaranAttachmentPath = "/files/confirm.pdf"
            };

            var ok = FakeCppCashService.Submit(vm);
            Assert.True(ok);
        }

        [Fact]
        public void Submit_Should_Validate_IsInputJasa_Rules()
        {
            var vm = new CppViewModel
            {
                PurchaseTypeCode = "cash",
                TypeCode = Guid.NewGuid(),
                IsInputJasa = true,
                Jasa = null,          // wajib
                PaymentMethod = null  // wajib
            };

            var ex = Assert.Throws<ArgumentException>(() => FakeCppCashService.Submit(vm));
            Assert.Contains("Jasa", ex.Message);
        }

        [Fact]
        public void Submit_Should_Pass_When_IsInputJasa_Filled()
        {
            var vm = new CppViewModel
            {
                PurchaseTypeCode = "cash",
                TypeCode = Guid.NewGuid(),
                IsInputJasa = true,
                Jasa = 150000m,
                PaymentMethod = "Transfer"
            };

            var ok = FakeCppCashService.Submit(vm);
            Assert.True(ok);
        }

        [Fact]
        public void Submit_Should_Require_DTFRNO_When_IsEditFrameEngine()
        {
            var vm = new CppViewModel
            {
                PurchaseTypeCode = "cash",
                TypeCode = Guid.NewGuid(),
                IsEditFrameEngine = true,
                DTFRNO = "" // wajib saat edit frame/engine
            };

            var ex = Assert.Throws<ArgumentException>(() => FakeCppCashService.Submit(vm));
            Assert.Contains(nameof(CppViewModel.DTFRNO), ex.Message);
        }
    }
}

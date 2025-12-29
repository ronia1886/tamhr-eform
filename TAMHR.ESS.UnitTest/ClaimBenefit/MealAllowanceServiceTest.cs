using System;
using System.Collections.Generic;
using System.Linq;
using TAMHR.ESS.Infrastructure.ViewModels;
using Xunit;

namespace TAMHR.ESS.UnitTest.ClaimBenefit
{
    /// <summary>
    /// Fake service untuk memvalidasi dan "submit" MealAllowanceViewModel.
    /// Aturan yang dicek:
    /// - Period wajib.
    /// - WageTypeCode wajib.
    /// - data wajib & minimal 1.
    /// - Setiap item: Date, StartHour, EndHour wajib; EndHour > StartHour; Amount > 0.
    /// - AmountTotal harus = jumlah Amount di data.
    /// </summary>
    internal static class FakeMealAllowanceService
    {
        public static bool Submit(MealAllowanceViewModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            if (!model.Period.HasValue)
                throw new ArgumentException("Period must be provided.");

            if (string.IsNullOrWhiteSpace(model.WageTypeCode))
                throw new ArgumentException("WageTypeCode must be provided.");

            if (model.data == null || model.data.Count == 0)
                throw new ArgumentException("data must contain at least one entry.");

            foreach (var (row, idx) in model.data.Select((r, i) => (r, i)))
            {
                if (!row.Date.HasValue)
                    throw new ArgumentException($"data[{idx}].Date must be provided.");

                if (!row.StartHour.HasValue)
                    throw new ArgumentException($"data[{idx}].StartHour must be provided.");

                if (!row.EndHour.HasValue)
                    throw new ArgumentException($"data[{idx}].EndHour must be provided.");

                if (row.EndHour.Value <= row.StartHour.Value)
                    throw new ArgumentException($"data[{idx}].EndHour must be greater than StartHour.");

                if (row.Amount <= 0)
                    throw new ArgumentException($"data[{idx}].Amount must be greater than 0.");
            }

            var sum = model.data.Sum(x => x.Amount);
            if (model.AmountTotal != sum)
                throw new ArgumentException($"AmountTotal ({model.AmountTotal}) must equal sum of detail Amounts ({sum}).");

            return true;
        }
    }

    public class MealAllowanceServiceTest
    {
        private MealAllowanceViewModel MakeValidModel(decimal amount1 = 50000m)
        {
            return new MealAllowanceViewModel
            {
                Period = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1),
                WageTypeCode = "ML01",
                Remarks = "Test meal allowance",
                data = new List<DataMealAllowanceViewModel>
                {
                    new DataMealAllowanceViewModel
                    {
                        Date = DateTime.Today,
                        StartHour = new TimeSpan(12, 0, 0),
                        EndHour = new TimeSpan(13, 0, 0),
                        Amount = amount1,
                        IdSupportingAttachmentPath = null,
                        SupportingAttachmentPath = "/files/meal1.pdf"
                    }
                },
                AmountTotal = amount1
            };
        }

        [Fact]
        public void Submit_Should_Pass_With_Minimal_Valid_Input()
        {
            var vm = MakeValidModel();
            var ok = FakeMealAllowanceService.Submit(vm);
            Assert.True(ok);
        }

        [Fact]
        public void Submit_Should_Throw_When_Period_Missing()
        {
            var vm = MakeValidModel();
            vm.Period = null;

            var ex = Assert.Throws<ArgumentException>(() => FakeMealAllowanceService.Submit(vm));
            Assert.Contains("Period", ex.Message);
        }

        [Fact]
        public void Submit_Should_Throw_When_WageTypeCode_Missing()
        {
            var vm = MakeValidModel();
            vm.WageTypeCode = null;

            var ex = Assert.Throws<ArgumentException>(() => FakeMealAllowanceService.Submit(vm));
            Assert.Contains("WageTypeCode", ex.Message);
        }

        [Fact]
        public void Submit_Should_Throw_When_Data_Null()
        {
            var vm = MakeValidModel();
            vm.data = null;

            var ex = Assert.Throws<ArgumentException>(() => FakeMealAllowanceService.Submit(vm));
            Assert.Contains("data must contain at least one entry", ex.Message);
        }

        [Fact]
        public void Submit_Should_Throw_When_Data_Empty()
        {
            var vm = MakeValidModel();
            vm.data = new List<DataMealAllowanceViewModel>();

            var ex = Assert.Throws<ArgumentException>(() => FakeMealAllowanceService.Submit(vm));
            Assert.Contains("data must contain at least one entry", ex.Message);
        }

        [Fact]
        public void Submit_Should_Throw_When_Entry_Missing_StartHour()
        {
            var vm = MakeValidModel();
            vm.data[0].StartHour = null;

            var ex = Assert.Throws<ArgumentException>(() => FakeMealAllowanceService.Submit(vm));
            Assert.Contains("StartHour", ex.Message);
        }

        [Fact]
        public void Submit_Should_Throw_When_Entry_Missing_EndHour()
        {
            var vm = MakeValidModel();
            vm.data[0].EndHour = null;

            var ex = Assert.Throws<ArgumentException>(() => FakeMealAllowanceService.Submit(vm));
            Assert.Contains("EndHour", ex.Message);
        }

        [Fact]
        public void Submit_Should_Throw_When_EndHour_Not_Greater_Than_StartHour()
        {
            var vm = MakeValidModel();
            vm.data[0].EndHour = vm.data[0].StartHour; // sama, tidak > 

            var ex = Assert.Throws<ArgumentException>(() => FakeMealAllowanceService.Submit(vm));
            Assert.Contains("EndHour must be greater than StartHour", ex.Message);
        }

        [Fact]
        public void Submit_Should_Throw_When_Amount_Not_Positive()
        {
            var vm = MakeValidModel();
            vm.data[0].Amount = 0m;

            var ex = Assert.Throws<ArgumentException>(() => FakeMealAllowanceService.Submit(vm));
            Assert.Contains("Amount must be greater than 0", ex.Message);
        }

        [Fact]
        public void Submit_Should_Throw_When_AmountTotal_Mismatch()
        {
            var vm = MakeValidModel();
            vm.AmountTotal = vm.data.Sum(x => x.Amount) + 1000m;

            var ex = Assert.Throws<ArgumentException>(() => FakeMealAllowanceService.Submit(vm));
            Assert.Contains("AmountTotal", ex.Message);
        }

        [Fact]
        public void Submit_Should_Pass_With_Multiple_Rows_And_Correct_Sum()
        {
            var vm = MakeValidModel(30000m);
            vm.data.Add(new DataMealAllowanceViewModel
            {
                Date = DateTime.Today,
                StartHour = new TimeSpan(18, 0, 0),
                EndHour = new TimeSpan(19, 0, 0),
                Amount = 20000m,
                SupportingAttachmentPath = "/files/meal2.pdf"
            });

            vm.AmountTotal = vm.data.Sum(x => x.Amount); // pastikan sesuai

            var ok = FakeMealAllowanceService.Submit(vm);
            Assert.True(ok);
        }
    }
}

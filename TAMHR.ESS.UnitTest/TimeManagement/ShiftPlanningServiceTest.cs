using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Xunit;
using TAMHR.ESS.Infrastructure.ViewModels;

namespace TAMHR.ESS.UnitTest.TimeManagementTests
{
    public class ShiftPlanningServiceTest
    {
        private static ShiftPlanningViewModel MakeSample()
        {
            return new ShiftPlanningViewModel
            {
                Year = "2025",
                Month = "03",                 // gunakan "MM"
                TypeShift = "REGULAR",
                UploadExcelPath = "/uploads/shift_planning_2025_03.xlsx",
                Remarks = "March planning",
                Request = new List<ShiftPlanningRequestViewModel>
                {
                    new ShiftPlanningRequestViewModel {
                        date = new DateTime(2025, 3, 1),
                        noreg = "10001",
                        name = "Alice",
                        shiftCode = "S1",
                        shift = "Morning"
                    },
                    new ShiftPlanningRequestViewModel {
                        date = new DateTime(2025, 3, 2),
                        noreg = "10001",
                        name = "Alice",
                        shiftCode = "S2",
                        shift = "Evening"
                    },
                    new ShiftPlanningRequestViewModel {
                        date = new DateTime(2025, 3, 1),
                        noreg = "10002",
                        name = "Bob",
                        shiftCode = "S3",
                        shift = "Night"
                    }
                }
            };
        }

        [Fact]
        public void Construct_Should_Hold_All_Values()
        {
            var vm = MakeSample();

            Assert.Equal("2025", vm.Year);
            Assert.Equal("03", vm.Month);
            Assert.Equal("REGULAR", vm.TypeShift);
            Assert.EndsWith(".xlsx", vm.UploadExcelPath, StringComparison.OrdinalIgnoreCase);
            Assert.Equal("March planning", vm.Remarks);

            Assert.NotNull(vm.Request);
            Assert.Equal(3, vm.Request.Count);
            Assert.All(vm.Request, r => Assert.False(string.IsNullOrWhiteSpace(r.noreg)));
            Assert.Contains(vm.Request, r => r.shiftCode == "S1");
        }

        [Fact]
        public void Requests_Must_Match_Year_And_Month()
        {
            var vm = MakeSample();

            bool MonthMatches(ShiftPlanningViewModel m)
            {
                if (!int.TryParse(m.Year, out var y)) return false;
                if (!int.TryParse(m.Month, out var mm)) return false;

                return m.Request.All(r => r.date.Year == y && r.date.Month == mm);
            }

            Assert.True(MonthMatches(vm));
        }

        [Fact]
        public void Requests_Should_Not_Duplicate_By_NoReg_And_Date()
        {
            var vm = MakeSample();

            var keys = vm.Request
                .Select(r => (r.noreg, r.date.Date))
                .ToList();

            // tidak boleh ada duplikat NoReg + Tanggal
            var distinct = keys.Distinct().Count();
            Assert.Equal(keys.Count, distinct);
        }

        [Fact]
        public void Json_RoundTrip_Should_Preserve_Data()
        {
            var vm = MakeSample();

            var json = JsonSerializer.Serialize(vm);
            Assert.False(string.IsNullOrWhiteSpace(json));

            var back = JsonSerializer.Deserialize<ShiftPlanningViewModel>(json);
            Assert.NotNull(back);

            Assert.Equal(vm.Year, back.Year);
            Assert.Equal(vm.Month, back.Month);
            Assert.Equal(vm.TypeShift, back.TypeShift);
            Assert.Equal(vm.Remarks, back.Remarks);

            Assert.NotNull(back.Request);
            Assert.Equal(vm.Request.Count, back.Request.Count);

            // cek salah satu item
            var originalRequest = vm.Request.First();
            var deserializedRequest = back.Request.First();
            Assert.Equal(originalRequest.noreg, deserializedRequest.noreg);
            Assert.Equal(originalRequest.date, deserializedRequest.date);
            Assert.Equal(originalRequest.shiftCode, deserializedRequest.shiftCode);
            Assert.Equal(originalRequest.shift, deserializedRequest.shift);

        }
    }
}

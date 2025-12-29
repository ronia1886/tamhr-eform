using System;
using System.Collections.Generic;
using System.Linq;
using TAMHR.ESS.Infrastructure.ViewModels;
using Xunit;

namespace TAMHR.ESS.UnitTest.ClaimBenefit
{
    public class ShiftMealAllowanceServiceTest
    {
        /// <summary>
        /// Agregasi Qty & Total pada setiap summary:
        /// - Jika StartPeriod/EndPeriod diisi, hanya tanggal dalam rentang (inklusif) yang dihitung.
        /// - Jika ShiftType diisi, hanya karyawan dengan ShiftCode yang cocok yang dihitung.
        /// - Total = jumlah Amount semua karyawan yang terhitung.
        /// Aman untuk koleksi null.
        /// </summary>
        private static void AggregateSummaries(ShiftMealAllowanceViewModel vm)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));

            DateTime? start = TryParseDate(vm.StartPeriod);
            DateTime? end = TryParseDate(vm.EndPeriod);

            vm.Summaries ??= Enumerable.Empty<ShiftMealAllowanceSummaryViewModel>();

            foreach (var sum in vm.Summaries)
            {
                var employees = (sum.Employees ?? Enumerable.Empty<ShiftMealAllowanceEmployeeViewModel>());

                // filter periode per summary date
                bool inPeriod =
                    (!start.HasValue || sum.Date.Date >= start.Value.Date) &&
                    (!end.HasValue || sum.Date.Date <= end.Value.Date);

                if (!inPeriod)
                {
                    sum.Qty = 0;
                    sum.Total = 0m;
                    continue;
                }

                // filter ShiftType jika diisi
                if (!string.IsNullOrWhiteSpace(vm.ShiftType))
                {
                    employees = employees.Where(e =>
                        string.Equals(e.ShiftCode, vm.ShiftType, StringComparison.OrdinalIgnoreCase));
                }

                sum.Qty = employees.Count();
                sum.Total = employees.Sum(e => e.Amount);
            }
        }

        private static DateTime? TryParseDate(string s)
            => DateTime.TryParse(s, out var d) ? d : (DateTime?)null;

        [Fact]
        public void AggregateSummaries_Should_Calc_Qty_And_Total_For_Each_Date()
        {
            // Arrange
            var vm = new ShiftMealAllowanceViewModel
            {
                Division = "DIV-1",
                Department = "DEP-1",
                StartPeriod = "2025-01-01",
                EndPeriod = "2025-01-31",
                ShiftType = null, // tidak filter shift
                Remarks = "January Window",
                Summaries = new List<ShiftMealAllowanceSummaryViewModel>
                {
                    new ShiftMealAllowanceSummaryViewModel
                    {
                        Date = new DateTime(2025, 1, 10),
                        Employees = new List<ShiftMealAllowanceEmployeeViewModel>
                        {
                            new ShiftMealAllowanceEmployeeViewModel { NoReg="1001", Name="Ali",  Classification=2, Amount=25000m, ShiftCode="S1" },
                            new ShiftMealAllowanceEmployeeViewModel { NoReg="1002", Name="Budi", Classification=3, Amount=25000m, ShiftCode="S2" },
                        }
                    },
                    new ShiftMealAllowanceSummaryViewModel
                    {
                        Date = new DateTime(2025, 1, 20),
                        Employees = new List<ShiftMealAllowanceEmployeeViewModel>
                        {
                            new ShiftMealAllowanceEmployeeViewModel { NoReg="1003", Name="Citra", Classification=4, Amount=30000m, ShiftCode="S1" },
                        }
                    }
                }
            };

            // Act
            AggregateSummaries(vm);

            // Assert
            var s1 = vm.Summaries.First(s => s.Date == new DateTime(2025, 1, 10));
            Assert.Equal(2, s1.Qty);
            Assert.Equal(50000m, s1.Total);

            var s2 = vm.Summaries.First(s => s.Date == new DateTime(2025, 1, 20));
            Assert.Equal(1, s2.Qty);
            Assert.Equal(30000m, s2.Total);
        }

        [Fact]
        public void AggregateSummaries_Should_Filter_By_ShiftType_When_Set()
        {
            // Arrange
            var vm = new ShiftMealAllowanceViewModel
            {
                StartPeriod = "2025-02-01",
                EndPeriod = "2025-02-28",
                ShiftType = "S1", // hanya S1 dihitung
                Summaries = new List<ShiftMealAllowanceSummaryViewModel>
                {
                    new ShiftMealAllowanceSummaryViewModel
                    {
                        Date = new DateTime(2025, 2, 5),
                        Employees = new List<ShiftMealAllowanceEmployeeViewModel>
                        {
                            new ShiftMealAllowanceEmployeeViewModel { NoReg="2001", Name="Dina",  Amount=20000m, ShiftCode="S1" },
                            new ShiftMealAllowanceEmployeeViewModel { NoReg="2002", Name="Eko",   Amount=22000m, ShiftCode="S2" },
                            new ShiftMealAllowanceEmployeeViewModel { NoReg="2003", Name="Fajar", Amount=20000m, ShiftCode="S1" },
                        }
                    }
                }
            };

            // Act
            AggregateSummaries(vm);

            // Assert: hanya S1 yang dihitung => Qty=2, Total=40000
            var s = vm.Summaries.First();
            Assert.Equal(2, s.Qty);
            Assert.Equal(40000m, s.Total);
        }

        [Fact]
        public void AggregateSummaries_Should_Zero_Out_Summary_When_Date_Outside_Period()
        {
            // Arrange
            var vm = new ShiftMealAllowanceViewModel
            {
                StartPeriod = "2025-03-01",
                EndPeriod = "2025-03-31",
                Summaries = new List<ShiftMealAllowanceSummaryViewModel>
                {
                    new ShiftMealAllowanceSummaryViewModel
                    {
                        Date = new DateTime(2025, 2, 28), // di luar periode
                        Employees = new List<ShiftMealAllowanceEmployeeViewModel>
                        {
                            new ShiftMealAllowanceEmployeeViewModel { NoReg="3001", Name="Gita", Amount=18000m, ShiftCode="S1" },
                        }
                    }
                }
            };

            // Act
            AggregateSummaries(vm);

            // Assert
            var s = vm.Summaries.First();
            Assert.Equal(0, s.Qty);
            Assert.Equal(0m, s.Total);
        }

        [Fact]
        public void AggregateSummaries_Should_Handle_Null_Collections_Safely()
        {
            // Arrange
            var vm = new ShiftMealAllowanceViewModel
            {
                StartPeriod = "2025-01-01",
                EndPeriod = "2025-12-31",
                Summaries = new List<ShiftMealAllowanceSummaryViewModel>
                {
                    new ShiftMealAllowanceSummaryViewModel
                    {
                        Date = DateTime.Today,
                        Employees = null // null -> dianggap kosong
                    }
                }
            };

            // Act
            AggregateSummaries(vm);

            // Assert
            var s = vm.Summaries.First();
            Assert.Equal(0, s.Qty);
            Assert.Equal(0m, s.Total);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using TAMHR.ESS.Infrastructure.ViewModels;
using Xunit;

namespace TAMHR.ESS.UnitTest.ClaimBenefit
{
    public class PtaAllowanceViewModelTests
    {
        /// <summary>
        /// Mengisi Qty & Total pada vm.Summaries berdasarkan daftar Employees dari vm.Departments.
        /// Aturan:
        /// - Karyawan dihitung ke summary jika ClassFrom <= Classification <= ClassTo.
        /// - Total = Qty * Amount (Amount bertipe double pada SummaryPta).
        /// </summary>
        private static void ApplySummaryAggregation(PtaAllowanceViewModel vm, DateTime eventDate)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));
            if (vm.Departments == null) vm.Departments = Enumerable.Empty<DepartmentPta>();
            if (vm.Summaries == null) vm.Summaries = Enumerable.Empty<SummaryPta>();

            // Kumpulkan seluruh karyawan
            var allEmployees = vm.Departments
                .SelectMany(d => d.Employees ?? Enumerable.Empty<EmployeePta>())
                .ToList();

            foreach (var s in vm.Summaries)
            {
                // parsing ClassFrom / ClassTo yang bertipe string
                int from = 0, to = int.MaxValue;
                _ = int.TryParse(s.ClassFrom, out from);
                _ = int.TryParse(s.ClassTo, out to);

                var hit = allEmployees
                    .Where(e => e != null && e.Classification >= from && e.Classification <= to)
                    .ToList();

                s.Qty = hit.Count;
                s.Total = s.Qty * s.Amount;
                s.EventDate = eventDate;
                s.Employees = hit;
            }
        }

        [Fact]
        public void ApplySummaryAggregation_Should_Calculate_Qty_And_Total_Correctly()
        {
            // Arrange: bikin VM lengkap (departemen + employees) dan definisi summary by range class
            var vm = new PtaAllowanceViewModel
            {
                CateogryPTA = "Innovation",
                date = new DateTime(2025, 1, 15),
                Amouont = 1234567m, // ini field di VM (typo bawaan), kita biarkan sesuai model
                ProposalPath = "/files/proposal.pdf",
                AccountType = "SAV",
                BankCode = "014",
                AccountNumber = "9876543210",
                AccountName = "John Doe",
                User = "EMP001",
                Remarks = "PTA Q1",
                Departments = new List<DepartmentPta>
                {
                    new DepartmentPta
                    {
                        ObjectID = "D1",
                        ObjectText = "Dept A",
                        Employees = new List<EmployeePta>
                        {
                            new EmployeePta { Id = Guid.NewGuid(), NoReg="1001", Name="Ali",   Classification=2, Reward="A", OrgCode="A01" },
                            new EmployeePta { Id = Guid.NewGuid(), NoReg="1002", Name="Budi",  Classification=4, Reward="B", OrgCode="A01" },
                            new EmployeePta { Id = Guid.NewGuid(), NoReg="1003", Name="Citra", Classification=1, Reward="A", OrgCode="A01" },
                        }
                    },
                    new DepartmentPta
                    {
                        ObjectID = "D2",
                        ObjectText = "Dept B",
                        Employees = new List<EmployeePta>
                        {
                            new EmployeePta { Id = Guid.NewGuid(), NoReg="2001", Name="Dina", Classification=5, Reward="B", OrgCode="B01" },
                            new EmployeePta { Id = Guid.NewGuid(), NoReg="2002", Name="Eko",  Classification=3, Reward="A", OrgCode="B01" },
                        }
                    }
                },
                // Definisi summary sesuai range kelas & nominal per orang
                Summaries = new List<SummaryPta>
                {
                    new SummaryPta
                    {
                        Range = "C1-C3",
                        ClassFrom = "1",
                        ClassTo   = "3",
                        Reward = "A",
                        RewardText = "Kategori A",
                        Amount = 100_000d
                    },
                    new SummaryPta
                    {
                        Range = "C4-C6",
                        ClassFrom = "4",
                        ClassTo   = "6",
                        Reward = "B",
                        RewardText = "Kategori B",
                        Amount = 150_000d
                    }
                }
            };

            // Act
            var eventDate = new DateTime(2025, 1, 31);
            ApplySummaryAggregation(vm, eventDate);

            // Assert:
            // Summary 1 (Class 1..3) -> Ali (2), Citra (1), Eko (3) => Qty=3, Total=3*100000
            var s1 = vm.Summaries.First(s => s.Range == "C1-C3");
            Assert.Equal(3, s1.Qty);
            Assert.Equal(300_000d, s1.Total, 5);
            Assert.Equal(eventDate, s1.EventDate);
            Assert.All(s1.Employees, e => Assert.InRange(e.Classification, 1, 3));

            // Summary 2 (Class 4..6) -> Budi (4), Dina (5) => Qty=2, Total=2*150000
            var s2 = vm.Summaries.First(s => s.Range == "C4-C6");
            Assert.Equal(2, s2.Qty);
            Assert.Equal(300_000d, s2.Total, 5);
            Assert.Equal(eventDate, s2.EventDate);
            Assert.All(s2.Employees, e => Assert.InRange(e.Classification, 4, 6));
        }

        [Fact]
        public void BankInfo_Should_Be_Complete_When_All_Fields_Filled()
        {
            // Arrange
            var vm = new PtaAllowanceViewModel
            {
                AccountType = "SAV",
                BankCode = "014",
                AccountNumber = "1234567890",
                AccountName = "Jane Doe"
            };

            // Act
            bool complete =
                !string.IsNullOrWhiteSpace(vm.AccountType) &&
                !string.IsNullOrWhiteSpace(vm.BankCode) &&
                !string.IsNullOrWhiteSpace(vm.AccountNumber) &&
                !string.IsNullOrWhiteSpace(vm.AccountName);

            // Assert
            Assert.True(complete);
        }

        [Fact]
        public void ApplySummaryAggregation_Should_Handle_Null_Collections_Safely()
        {
            // Arrange
            var vm = new PtaAllowanceViewModel
            {
                Departments = null,  // sengaja null
                Summaries = new List<SummaryPta>
                {
                    new SummaryPta { ClassFrom = "1", ClassTo = "9", Amount = 50_000d }
                }
            };

            // Act (tidak boleh throw)
            ApplySummaryAggregation(vm, DateTime.Today);

            // Assert
            var s = vm.Summaries.First();
            Assert.Equal(0, s.Qty);
            Assert.Equal(0d, s.Total, 5);
            Assert.NotEqual(default, s.EventDate);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;

using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;

using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;

namespace TAMHR.ESS.UnitTest.Reporting
{
    public class DailyReconciliationServiceTest
    {
        private readonly Mock<IUnitOfWork> _uow = new Mock<IUnitOfWork>();

        private readonly Mock<IRepository<DailyReconciliationReportView>> _dailyRepo
            = new Mock<IRepository<DailyReconciliationReportView>>();

        private readonly Mock<IRepository<OrganizationStructureView>> _orgRepo
            = new Mock<IRepository<OrganizationStructureView>>();

        private readonly DailyReconciliationService _service;

        public DailyReconciliationServiceTest()
        {
            _uow.Setup(u => u.GetRepository<DailyReconciliationReportView>())
                .Returns(_dailyRepo.Object);

            _uow.Setup(u => u.GetRepository<OrganizationStructureView>())
                .Returns(_orgRepo.Object);

            _service = new DailyReconciliationService(_uow.Object);
        }

        [Fact]
        public void GetDailyReconciliation_Should_Filter_By_Name_And_Date_And_Order()
        {
            var df = new DateTime(2025, 1, 1);
            var dt = new DateTime(2025, 1, 31);

            var data = new List<DailyReconciliationReportView>
            {
                new DailyReconciliationReportView { NoReg = "E001", Name = "John Doe",           WorkingDate = new DateTime(2025,1,05) },
                new DailyReconciliationReportView { NoReg = "E001", Name = "John Doe",           WorkingDate = new DateTime(2025,1,03) },
                new DailyReconciliationReportView { NoReg = "E003", Name = "Johnny Appleseed",   WorkingDate = new DateTime(2025,1,10) },
                new DailyReconciliationReportView { NoReg = "E002", Name = "Joey Tribbiani",     WorkingDate = new DateTime(2025,1,10) },
                new DailyReconciliationReportView { NoReg = "E004", Name = "John Wick",          WorkingDate = new DateTime(2024,12,31) }, // out of range
                new DailyReconciliationReportView { NoReg = "E000", Name = "Alice",              WorkingDate = new DateTime(2025,1,15) }   // name not match
            }.AsQueryable();

            _dailyRepo.Setup(r => r.Fetch()).Returns(data);

            var result = _service.GetDailyReconciliation("Joh", df, dt).ToList();

            Assert.Equal(3, result.Count);
            Assert.Equal(new[] { "E001", "E001", "E003" }, result.Select(x => x.NoReg).ToArray());
            Assert.Equal(new[] { new DateTime(2025, 1, 03), new DateTime(2025, 1, 05), new DateTime(2025, 1, 10) },
                         result.Select(x => x.WorkingDate.Date).ToArray());
        }

        [Fact]
        public void GetDirectorates_Returns_Projected_And_Ordered_By_ObjectText()
        {
            var orgs = new List<OrganizationStructureView>
            {
                new OrganizationStructureView { OrgCode = "D01", ParentOrgCode = "ROOT", ObjectText = "Directorate A", ObjectDescription = "Directorate" },
                new OrganizationStructureView { OrgCode = "D02", ParentOrgCode = "ROOT", ObjectText = "Directorate B", ObjectDescription = "Directorate" },
                // duplicate secara nilai, tapi kelas tidak override Equals => Distinct tidak menghapus
                new OrganizationStructureView { OrgCode = "D01", ParentOrgCode = "ROOT", ObjectText = "Directorate A", ObjectDescription = "Directorate" },
                // non directorate
                new OrganizationStructureView { OrgCode = "X01", ParentOrgCode = "ROOT", ObjectText = "Division X",    ObjectDescription = "Division" }
            }.AsQueryable();

            _orgRepo.Setup(r => r.Fetch()).Returns(orgs);

            var res = _service.GetDirectorates().ToList();

            // Dengan implementasi saat ini, Distinct tidak menghapus duplikat -> hasil 3 item
            Assert.Equal(3, res.Count);
            Assert.Equal(new[] { "Directorate A", "Directorate A", "Directorate B" },
                         res.Select(x => x.ObjectText).ToArray());
        }

        [Fact]
        public void GetDivisions_Returns_Projected_And_Ordered_By_ObjectText()
        {
            var orgs = new List<OrganizationStructureView>
            {
                new OrganizationStructureView { OrgCode = "DIV01", ParentOrgCode = "D01", ObjectText = "Finance", ObjectDescription = "Division" },
                new OrganizationStructureView { OrgCode = "DIV02", ParentOrgCode = "D01", ObjectText = "HR",      ObjectDescription = "Division" },
                // duplicate secara nilai
                new OrganizationStructureView { OrgCode = "DIV02", ParentOrgCode = "D01", ObjectText = "HR",      ObjectDescription = "Division" },
                // other level
                new OrganizationStructureView { OrgCode = "D01",   ParentOrgCode = "ROOT", ObjectText = "Directorate A", ObjectDescription = "Directorate" }
            }.AsQueryable();

            _orgRepo.Setup(r => r.Fetch()).Returns(orgs);

            var res = _service.GetDivisions().ToList();

            // Distinct pada class tanpa equality custom tidak menghapus duplikat
            Assert.Equal(3, res.Count);
            Assert.Equal(new[] { "Finance", "HR", "HR" }, res.Select(x => x.ObjectText).ToArray());
        }

        [Fact]
        public void GetDivisionsFromMultiDirectorate_Joins_By_ParentOrgCode()
        {
            var directorates = new List<string> { "D01", "D99" };

            var orgs = new List<OrganizationStructureView>
            {
                new OrganizationStructureView { OrgCode = "DIV01", ParentOrgCode = "D01", ObjectText = "Finance", ObjectDescription = "Division" },
                new OrganizationStructureView { OrgCode = "DIV02", ParentOrgCode = "D02", ObjectText = "HR",      ObjectDescription = "Division" },
                new OrganizationStructureView { OrgCode = "DIV03", ParentOrgCode = "D99", ObjectText = "IT",      ObjectDescription = "Division" },
                new OrganizationStructureView { OrgCode = "SEC01", ParentOrgCode = "DIV01", ObjectText = "Sec",   ObjectDescription = "Section" },
            }.AsQueryable();

            _orgRepo.Setup(r => r.Fetch()).Returns(orgs);

            var res = _service.GetDivisionsFromMultiDirectorate(directorates).ToList();

            Assert.Equal(2, res.Count);
            Assert.Equal(new[] { "Finance", "IT" }, res.Select(x => x.ObjectText).ToArray());
        }

        [Fact]
        public void GetDepartments_Filters_By_Division()
        {
            var orgs = new List<OrganizationStructureView>
            {
                new OrganizationStructureView { OrgCode = "DEP01", ParentOrgCode = "DIV01", ObjectText = "Dept A", ObjectDescription = "Department" },
                new OrganizationStructureView { OrgCode = "DEP02", ParentOrgCode = "DIV02", ObjectText = "Dept B", ObjectDescription = "Department" },
                new OrganizationStructureView { OrgCode = "DIV01", ParentOrgCode = "D01",   ObjectText = "Div 1",  ObjectDescription = "Division" },
            }.AsQueryable();

            _orgRepo.Setup(r => r.Fetch()).Returns(orgs);

            var res = _service.GetDepartments("DIV02").ToList();

            Assert.Single(res);
            Assert.Equal("DEP02", res[0].OrgCode);
            Assert.Equal("Dept B", res[0].ObjectText);
        }

        [Fact]
        public void GetDepartmentsFromMultiDivision_Joins_When_List_Not_Empty()
        {
            var divList = new List<string> { "DIV01", "DIVXX" };

            var orgs = new List<OrganizationStructureView>
            {
                new OrganizationStructureView { OrgCode = "DEP01", ParentOrgCode = "DIV01", ObjectText = "Dept A", ObjectDescription = "Department" },
                new OrganizationStructureView { OrgCode = "DEP02", ParentOrgCode = "DIV02", ObjectText = "Dept B", ObjectDescription = "Department" },
                new OrganizationStructureView { OrgCode = "DEP03", ParentOrgCode = "DIVXX", ObjectText = "Dept C", ObjectDescription = "Department" },
                new OrganizationStructureView { OrgCode = "SEC01", ParentOrgCode = "DEP01", ObjectText = "Sec A",  ObjectDescription = "Section" },
            }.AsQueryable();

            _orgRepo.Setup(r => r.Fetch()).Returns(orgs);

            var res = _service.GetDepartmentsFromMultiDivision(divList).ToList();

            Assert.Equal(2, res.Count);
            Assert.Equal(new[] { "Dept A", "Dept C" }, res.Select(x => x.ObjectText).ToArray());
        }

        [Fact]
        public void GetSectionsFromMultiDepartmentAndDivision_Unions_Parents_And_Filters()
        {
            var deptList = new List<string> { "DEP01" };
            var divList = new List<string> { "DIV99" };

            var orgs = new List<OrganizationStructureView>
            {
                new OrganizationStructureView { OrgCode = "SEC01", ParentOrgCode = "DEP01", ObjectText = "Sec A", ObjectDescription = "Section" },
                new OrganizationStructureView { OrgCode = "SEC02", ParentOrgCode = "DIV99", ObjectText = "Sec B", ObjectDescription = "Section" },
                new OrganizationStructureView { OrgCode = "SEC03", ParentOrgCode = "DEP02", ObjectText = "Sec C", ObjectDescription = "Section" },
                new OrganizationStructureView { OrgCode = "GRP01", ParentOrgCode = "SEC01", ObjectText = "Grp A", ObjectDescription = "Group"  },
            }.AsQueryable();

            _orgRepo.Setup(r => r.Fetch()).Returns(orgs);

            var res = _service.GetSectionsFromMultiDepartmentAndDivision(deptList, divList).ToList();

            Assert.Equal(2, res.Count);
            Assert.Equal(new[] { "Sec A", "Sec B" }, res.Select(x => x.ObjectText).ToArray());
        }

        [Fact]
        public void GetLinesFromMultiSection_Filters_By_Parent()
        {
            var secList = new List<string> { "SEC01", "SECXX" };

            var orgs = new List<OrganizationStructureView>
            {
                new OrganizationStructureView { OrgCode = "LIN01", ParentOrgCode = "SEC01", ObjectText = "Line 1", ObjectDescription = "Line" },
                new OrganizationStructureView { OrgCode = "LIN02", ParentOrgCode = "SEC02", ObjectText = "Line 2", ObjectDescription = "Line" },
                new OrganizationStructureView { OrgCode = "LIN03", ParentOrgCode = "SECXX", ObjectText = "Line 3", ObjectDescription = "Line" },
                new OrganizationStructureView { OrgCode = "GRP01", ParentOrgCode = "LIN01", ObjectText = "Group 1",ObjectDescription = "Group" },
            }.AsQueryable();

            _orgRepo.Setup(r => r.Fetch()).Returns(orgs);

            var res = _service.GetLinesFromMultiSection(secList).ToList();

            Assert.Equal(2, res.Count);
            Assert.Equal(new[] { "LIN01", "LIN03" }, res.Select(x => x.OrgCode).ToArray());
        }

        [Fact]
        public void GetGroupsFromMultiLine_Filters_By_Parent()
        {
            var lineList = new List<string> { "LIN01" };

            var orgs = new List<OrganizationStructureView>
            {
                new OrganizationStructureView { OrgCode = "GRP01", ParentOrgCode = "LIN01", ObjectText = "Group 1", ObjectDescription = "Group" },
                new OrganizationStructureView { OrgCode = "GRP02", ParentOrgCode = "LIN02", ObjectText = "Group 2", ObjectDescription = "Group" },
                new OrganizationStructureView { OrgCode = "LIN01", ParentOrgCode = "SEC01", ObjectText = "Line 1",  ObjectDescription = "Line"  },
            }.AsQueryable();

            _orgRepo.Setup(r => r.Fetch()).Returns(orgs);

            var res = _service.GetGroupsFromMultiLine(lineList).ToList();

            Assert.Single(res);
            Assert.Equal("GRP01", res[0].OrgCode);
            Assert.Equal("Group 1", res[0].ObjectText);
        }
    }
}

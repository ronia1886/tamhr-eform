using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.UnitTest.PersonalData
{
    /// <summary>
    /// Kontrak ringan untuk di-mock (tidak menyentuh DB).
    /// Kalau nanti ada service benerannya (mis. MemberProfileService),
    /// cukup ganti IMemberProfileService ini ke interface/kelas aslinya.
    /// </summary>
    public interface IMemberProfileService
    {
        IEnumerable<SubordinateStroredEntity> GetSubordinates(string orgCode = "*", int orgLevel = 0);
        IEnumerable<SubordinateFamilyStroredEntity> GetSubordinateFamilies(string orgCode = "*", int orgLevel = 0);
    }

    public class MemberProfileServiceTests
    {
        private readonly Mock<IMemberProfileService> _mockSvc;

        public MemberProfileServiceTests()
        {
            _mockSvc = new Mock<IMemberProfileService>();
        }

        [Fact]
        public void GetSubordinates_Returns_Data_From_Service()
        {
            // Arrange
            var today = new DateTime(2024, 12, 31);

            var model = new List<SubordinateStroredEntity>
            {
                new SubordinateStroredEntity
                {
                    Id = Guid.NewGuid(),
                    NoReg = "0001",
                    Name = "A. Dadang",
                    PostName = "Out City Operator",
                    JobName = "Operator",
                    OrgName = "Warehouse Department",
                    EmployeeSubgroupText = "Staff",
                    Division = "Service Parts Logistic Division",
                    Department = "Warehouse Department",
                    Section = "Parts Issuing Section",
                    Extension = "1234",
                    EntryDate = today
                },
                new SubordinateStroredEntity
                {
                    Id = Guid.NewGuid(),
                    NoReg = "0002",
                    Name = "Aas Astri Aisyah",
                    PostName = "Import Parts Staff",
                    JobName = "Staff",
                    OrgName = "Inventory Management Department",
                    EmployeeSubgroupText = "Staff",
                    Division = "Service Parts Logistic Division",
                    Department = "Inventory Management Department",
                    Section = "Import Parts Section",
                    Extension = "9876",
                    EntryDate = today.AddDays(-7)
                }
            };

            _mockSvc
                .Setup(s => s.GetSubordinates("*", 0))
                .Returns(model);

            // Act
            var result = _mockSvc.Object.GetSubordinates("*", 0).ToList();

            // Assert
            Assert.Equal(2, result.Count);

            var first = result.First(r => r.NoReg == "0001");
            Assert.Equal("A. Dadang", first.Name);
            Assert.Equal("Out City Operator", first.PostName);
            Assert.Equal("Operator", first.JobName);
            Assert.Equal("Warehouse Department", first.OrgName);
            Assert.Equal("Staff", first.EmployeeSubgroupText);
            Assert.Equal("Service Parts Logistic Division", first.Division);
            Assert.Equal("Warehouse Department", first.Department);
            Assert.Equal("Parts Issuing Section", first.Section);
            Assert.Equal("1234", first.Extension);
            Assert.Equal(today, first.EntryDate);

            var second = result.First(r => r.NoReg == "0002");
            Assert.Equal("Aas Astri Aisyah", second.Name);
            Assert.Equal("Import Parts Staff", second.PostName);
            Assert.Equal("Staff", second.JobName);
            Assert.Equal("Inventory Management Department", second.OrgName);
            Assert.Equal("Staff", second.EmployeeSubgroupText);
            Assert.Equal("Service Parts Logistic Division", second.Division);
            Assert.Equal("Inventory Management Department", second.Department);
            Assert.Equal("Import Parts Section", second.Section);
            Assert.Equal("9876", second.Extension);
            Assert.Equal(today.AddDays(-7), second.EntryDate);

            _mockSvc.Verify(s => s.GetSubordinates("*", 0), Times.Once);
        }

        [Fact]
        public void GetSubordinates_Verify_Called_With_Params()
        {
            // Arrange
            var model = Enumerable.Empty<SubordinateStroredEntity>();
            _mockSvc
                .Setup(s => s.GetSubordinates(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(model);

            // Act
            _ = _mockSvc.Object.GetSubordinates("DIV-ISTD", 2).ToList();

            // Assert
            _mockSvc.Verify(s => s.GetSubordinates("DIV-ISTD", 2), Times.Once);
        }

        [Fact]
        public void GetSubordinateFamilies_Returns_Data_From_Service()
        {
            // Arrange
            var start = new DateTime(2024, 1, 1);
            var end = new DateTime(9999, 12, 31);

            var model = new List<SubordinateFamilyStroredEntity>
            {
                new SubordinateFamilyStroredEntity
                {
                    Id = Guid.NewGuid(),
                    NoReg = "0001",
                    Name = "A. Dadang",
                    PostName = "Out City Operator",
                    JobName = "Operator",
                    OrgName = "Warehouse Department",
                    EmployeeSubgroupText = "Staff",
                    Division = "Service Parts Logistic Division",
                    Department = "Warehouse Department",
                    Section = "Parts Issuing Section",
                    Extension = "1234",
                    EntryDate = new DateTime(2020, 6, 1),
                    TaxStatus = "K0",
                    FamilyName = "Istri",
                    FamilyTypeCode = "suamiistri",
                    BirthDate = new DateTime(1990, 5, 2),
                    BirthPlace = "Bandung",
                    StartDate = start,
                    EndDate = end
                }
            };

            _mockSvc
                .Setup(s => s.GetSubordinateFamilies("*", 0))
                .Returns(model);

            // Act
            var result = _mockSvc.Object.GetSubordinateFamilies("*", 0).ToList();

            // Assert
            Assert.Single(result);
            var r = result[0];

            Assert.Equal("0001", r.NoReg);
            Assert.Equal("A. Dadang", r.Name);
            Assert.Equal("Out City Operator", r.PostName);
            Assert.Equal("Operator", r.JobName);
            Assert.Equal("Warehouse Department", r.OrgName);
            Assert.Equal("Staff", r.EmployeeSubgroupText);
            Assert.Equal("Service Parts Logistic Division", r.Division);
            Assert.Equal("Warehouse Department", r.Department);
            Assert.Equal("Parts Issuing Section", r.Section);
            Assert.Equal("1234", r.Extension);
            Assert.Equal(new DateTime(2020, 6, 1), r.EntryDate);
            Assert.Equal("K0", r.TaxStatus);
            Assert.Equal("Istri", r.FamilyName);
            Assert.Equal("suamiistri", r.FamilyTypeCode);
            Assert.Equal(new DateTime(1990, 5, 2), r.BirthDate);
            Assert.Equal("Bandung", r.BirthPlace);
            Assert.Equal(start, r.StartDate);
            Assert.Equal(end, r.EndDate);

            _mockSvc.Verify(s => s.GetSubordinateFamilies("*", 0), Times.Once);
        }

        [Fact]
        public void GetSubordinateFamilies_Verify_Called_With_Params()
        {
            // Arrange
            var model = Enumerable.Empty<SubordinateFamilyStroredEntity>();
            _mockSvc
                .Setup(s => s.GetSubordinateFamilies(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(model);

            // Act
            _ = _mockSvc.Object.GetSubordinateFamilies("DIV-ISTD", 2).ToList();

            // Assert
            _mockSvc.Verify(s => s.GetSubordinateFamilies("DIV-ISTD", 2), Times.Once);
        }
    }
}

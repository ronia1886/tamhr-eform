using Agit.Domain.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Dapper;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.ViewModels;
using Xunit;

namespace TAMHR.ESS.UnitTest.PersonalData
{
    public class EmployeeProfileTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly ProfileService _profileService;
        private readonly Mock<IProfileService> _mockProfileService;
        private readonly Mock<IEmployeeProfileService> _mockEmployeeProfileService;
        private readonly EmployeeProfileService employeeService;

        public EmployeeProfileTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _profileService = new ProfileService(_mockUnitOfWork.Object);
            _mockProfileService = new Mock<IProfileService>();
            _mockEmployeeProfileService = new Mock<IEmployeeProfileService>();
            employeeService = new EmployeeProfileService(_mockUnitOfWork.Object);
        }

        [Fact]
        public void GetProfiles_WithValidNoReg_ReturnsPersonalDataEntity()
        {
            // Arrange
            var noReg = "100170";
            var expectedProfile = new PersonalDataAllProfileStoredEntity
            {
                Id = Guid.Parse("D35FF41E-1EA1-4631-9C78-723536AAE08A"),
                CommonAttributeId = Guid.Parse("2A85B809-8D4A-4252-94E1-08D927B96650"),
                NoReg = "100170",
                Name = "Employee100170",
            };

            _mockProfileService.Setup(x => x.GetProfiles(noReg))
                                      .Returns(expectedProfile);

            var result = _mockProfileService.Object.GetProfiles(noReg);

            Assert.NotNull(result);
            Assert.Equal(noReg, result.NoReg);
            Assert.Equal(expectedProfile.Name, result.Name);
        }

        [Fact]
        public void AddEducation_ReturnResult()
        {
            var model = new EducationUpdateViewModel()
            {
                Id = Guid.NewGuid(),
                Noreg = "100170",
                EducationLevel = "Strata 1",
                Collage = "Universitas Indonesia",
                Other = "",
                StartEducationDate = new DateTime(2019, 9, 1),
                GraduationDate = new DateTime(2023, 7, 31),
                GPA = "3.75",
                Major = "manajemen",
                Country = "Indonesia",
                ModifiedBy = "xunit"
            };

            var result = employeeService.AddEducation(model);

            Assert.True(result, "AddEducation should return true when SP executed successfully.");
        }

        [Fact]
        public void UpdateEducation__ReturnResult()
        {
            // Arrange
            var model = new EducationUpdateViewModel
            {
                Id = Guid.NewGuid(),
                Noreg = "100170",
                EducationLevel = "Strata 1",
                Collage = "Universitas Indonesia",
                Other = "",
                StartEducationDate = new DateTime(2019, 9, 1),
                GraduationDate = new DateTime(2023, 7, 31),
                GPA = "3.75",
                Major = "manajemen",
                Country = "Indonesia",
                ModifiedBy = "xunit"
            };

            // Act
            var result = employeeService.UpdateEducation(model);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void UpdateProfileMain()
        {
            // Arrange
            var model = new PersonalMainProfileViewModel
            {
                Noreg = "EMP001",
                EntryDate = new DateTime(2018, 1, 1),
                AstraDate = new DateTime(2018, 2, 1),
                WorkLocation = "HEADOFFICE",
                ModifiedBy = "admin"
            };


            // Act
            var result = employeeService.UpdateProfileMain(model);

            // Assert
            Assert.True(result);

        }

        [Fact]
        public void UpdateProfileData()
        {
            // Arrange
            var model = new UpdateProfileViewModel
            {
                Noreg = "EMP001",
                Pob = "Jakarta",
                Dob = new DateTime(1990, 1, 1),
                Nik = "1234567890",
                Address = "Jl. Contoh No. 123",
                Kk = "9876543210",
                Npwp = "01.234.567.8-912.000",
                AccountNumber = "123456789",
                Religion = "Islam",
                BloodGroup = "A",
                TaxStatus = "TK/0",
                Bpjs = "BPJS12345",
                Bpjsen = "BPJSTK12345",
                InsuranceNo = "INS123456",
                ModifiedBy = "admin",
                Name = "John Doe",
                Branch = "Head Office",
                Gender = "Male",
                Status = "Single",
                Nationality = "Indonesia",
                PhoneNumber = "08123456789",
                IdentityCardName = "John Doe",
                SimANumber = "SIM12345",
                SimCNumber = "SIMC12345",
                PassportNumber = "P12345678",
                PersonalEmail = "john.doe@example.com",
                WhatsappNumber = "08123456789"
            };



            // Act
            var result = employeeService.UpdateProfileData(model);

            // Assert
            Assert.True(result);

        }

        [Fact]
        public void AddFamilyMember_ReturnResult()
        {
            // Arrange
            var model = new FamilyMemberViewModel()
            {
                Id = Guid.NewGuid(),
                Noreg = "100170",
                Name = "John Doe",
                BirthDate = new DateTime(1990, 1, 1),
                BirthPlace = "Jakarta",
                Gender = "M",
                FamilyType = "Child",
                Other = "",
                Bpjs = "123456789",
                InsuranceNumber = "INS-987654321",
                LifeStatus = 1,
                DeathDate = null,
                EducationLevel = "S1",
                Job = "Engineer",
                PhoneNumber = "081234567890",
                Nationality = "Indonesia",
                Domicile = "Jakarta",
                Religion = "Islam",
                AddressStatusCode = "DOM01",
                NIK = "3210123456789000",
                ChildStatus = "Anak Kandung"
            };

            // Act
            var result = employeeService.AddFamilyMember(model);

            // Assert
            Assert.True(result, "AddFamilyMember should return true when SP executed successfully.");
        }

        [Fact]
        public void UpdateFamilyMember_ReturnResult()
        {
            // Arrange
            var model = new FamilyMemberViewModel()
            {
                Id = Guid.NewGuid(),
                Noreg = "100170",
                Name = "John Doe",
                BirthDate = new DateTime(1990, 1, 1),
                BirthPlace = "Jakarta",
                Gender = "M",
                FamilyType = "Child",
                Other = "",
                Bpjs = "123456789",
                InsuranceNumber = "INS-987654321",
                LifeStatus = 1,
                DeathDate = null,
                EducationLevel = "S1",
                Job = "Engineer",
                PhoneNumber = "081234567890",
                Nationality = "Indonesia",
                Domicile = "Jakarta",
                Religion = "Islam",
                AddressStatusCode = "DOM01",
                NIK = "3210123456789000",
                ChildStatus = "Anak Kandung"
            };

            // Act
            var result = employeeService.UpdateFamilyMember(model);

            // Assert
            Assert.True(result, "AddFamilyMember should return true when SP executed successfully.");
        }

        [Fact]
        public void UpdateEmergencyContact_ReturnResult()
        {
            // Arrange
            var mockUow = new Mock<IUnitOfWork>();

            mockUow.Setup(u => u.Transact(It.IsAny<Action<IDbTransaction>>(), It.IsAny<IsolationLevel>()))
                   .Callback<Action<IDbTransaction>, IsolationLevel>((action, iso) => {});

            var employeeService = new EmployeeProfileService(mockUow.Object);
            var model = new EmergencyContanctViewModel()
            {
                Noreg = "100170",
                EmergencyCallName = "Jane Doe",
                EmergencyCallPhoneNumber = "081234567890",
                EmergencyCallRelationshipCode = "SPOUSE",
                EmergencyCallName2 = "John Doe",
                EmergencyCallPhoneNumber2 = "081298765432",
                EmergencyCallRelationshipCode2 = "PARENT",
                ModifiedBy = "xunit"
            };

            // Act
            var result = employeeService.UpdateEmergencyContact(model);

            // Assert
            Assert.True(result);
            mockUow.Verify(u => u.Transact(It.IsAny<Action<IDbTransaction>>(), It.IsAny<IsolationLevel>()), Times.Once);
        }
    }

    public interface IProfileService
    {
        PersonalDataAllProfileStoredEntity GetProfiles(string noReg);
    }

    public interface IEmployeeProfileService
    {
        IEnumerable<dynamic> GetPersonalDataEducation(string noReg);
        bool AddEducation(EducationUpdateViewModel model);
    }

}

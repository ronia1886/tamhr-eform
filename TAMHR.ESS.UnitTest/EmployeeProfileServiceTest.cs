using Agit.Domain.Extensions;
using Agit.Domain.UnitOfWork;
using Moq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using TAMHR.ESS.Infrastructure.DomainServices;
using TAMHR.ESS.Infrastructure.ViewModels;
using Xunit;

namespace TAMHR.ESS.UnitTest
{
    public class EmployeeProfileServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly EmployeeProfileService _employeeProfileService;

        public EmployeeProfileServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _employeeProfileService = new EmployeeProfileService(_mockUnitOfWork.Object);
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
            var result = _employeeProfileService.UpdateProfileMain(model);

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
            var result = _employeeProfileService.UpdateProfileData(model);

            // Assert
            Assert.True(result);
            
        }
        [Fact]
        public void AddEducation()
        {
            // Arrange
            var model = new EducationUpdateViewModel
            {
                Noreg = "EMP001",
                EducationLevel = "Bachelor",
                Collage = "State University",
                Other = "Additional info",
                GraduationDate = new DateTime(2020, 6, 15),
                GPA = "3.75",
                Major = "Computer Science"
            };

            // Act
            var result = _employeeProfileService.AddEducation(model);

            // Assert
            Assert.True(result);
           
        }

        [Fact]
        public void UpdateEducation()
        {
            // Arrange
            var model = new EducationUpdateViewModel
            {
                Id = Guid.NewGuid(),
                Noreg = "EMP001",
                EducationLevel = "Bachelor",
                Collage = "State University",
                Other = "Additional info",
                GraduationDate = new DateTime(2020, 6, 15),
                GPA = "3.75",
                Major = "Computer Science",
                ModifiedBy = "admin"
            };

            

            // Act
            var result = _employeeProfileService.UpdateEducation(model);

            // Assert
            Assert.True(result);

        }
    }
}

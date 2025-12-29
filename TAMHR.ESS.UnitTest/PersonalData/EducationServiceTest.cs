using System;
using Moq;
using Xunit;
using Agit.Domain.UnitOfWork;
using TAMHR.ESS.Infrastructure.DomainServices;   // EmployeeProfileService
using TAMHR.ESS.Infrastructure.ViewModels;      // EducationUpdateViewModel

namespace TAMHR.ESS.UnitTest.PersonalData
{
    public class EducationServiceTest
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly EmployeeProfileService _employeeProfileService;

        public EducationServiceTest()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _employeeProfileService = new EmployeeProfileService(_mockUnitOfWork.Object);
        }

        [Fact]
        public void AddEducation_ShouldSucceed()
        {
            // Mapping dari EducationViewModel ke EducationUpdateViewModel:
            // LevelOfEducationCode -> EducationLevel
            // CollegeName / OtherCollegeName -> Collage (+ Other)
            // DepartmentCode -> Major
            // Period (DateTime?) -> GraduationDate
            // GPA (decimal?) -> GPA (string)
            var model = new EducationUpdateViewModel
            {
                Noreg = "EMP001",
                EducationLevel = "Bachelor",           // dari LevelOfEducationCode
                Collage = "State University",          // dari CollegeName
                Other = null,                          // dari OtherCollegeName jika IsOtherCollegeName == true
                Major = "Computer Science",            // dari DepartmentCode (jika berupa nama/kode, sesuaikan)
                GraduationDate = new DateTime(2020, 6, 15), // dari Period
                GPA = "3.75"                           // dari GPA decimal? ToString("0.00")
            };

            var result = _employeeProfileService.AddEducation(model);

            Assert.True(result);
        }

        [Fact]
        public void UpdateEducation_ShouldSucceed()
        {
            var model = new EducationUpdateViewModel
            {
                Id = Guid.NewGuid(),
                Noreg = "EMP001",
                EducationLevel = "Master",
                Collage = "Tech Institute",
                Other = "Scholarship Program",
                Major = "Information Systems",
                GraduationDate = new DateTime(2023, 7, 1),
                GPA = "3.90",
                ModifiedBy = "admin"
            };

            var result = _employeeProfileService.UpdateEducation(model);

            Assert.True(result);
        }
    }
}

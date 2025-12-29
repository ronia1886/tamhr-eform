using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using Xunit;

namespace TAMHR.ESS.UnitTest.PersonalData
{
    public class EmployeeHealthServiceTest
    {
        private readonly Mock<IRepository<PersonalDataBpjs>> _mockBpjsRepo;
        private readonly Mock<IRepository<PersonalDataInsurance>> _mockInsuranceRepo;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly PersonalDataService _service;

        public EmployeeHealthServiceTest()
        {
            _mockBpjsRepo = new Mock<IRepository<PersonalDataBpjs>>();
            _mockInsuranceRepo = new Mock<IRepository<PersonalDataInsurance>>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _mockUnitOfWork.Setup(u => u.GetRepository<PersonalDataBpjs>())
                .Returns(_mockBpjsRepo.Object);
            _mockUnitOfWork.Setup(u => u.GetRepository<PersonalDataInsurance>())
                .Returns(_mockInsuranceRepo.Object);

            _service = new PersonalDataService(_mockUnitOfWork.Object, null);
        }

        [Fact]
        public void UpsertMultiplePersonalDataBpjs_ShouldCallUpsertAndSaveChanges()
        {
            // Arrange
            var data = new List<PersonalDataBpjs>
            {
                new PersonalDataBpjs { Id = Guid.NewGuid(), NoReg = "1001", CompleteStatus = false }
            };

            // Act
            _service.UpsertMultiplePersonalDataBpjs(data);

            // Assert
            _mockBpjsRepo.Verify(r => r.Upsert<Guid>(
                It.IsAny<PersonalDataBpjs>(),
                It.Is<string[]>(arr => arr.Length == 0)),
                Times.Once);

            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void UpsertMultiplePersonalDataInsurance_ShouldCallUpsertAndSaveChanges()
        {
            // Arrange
            var data = new List<PersonalDataInsurance>
            {
                new PersonalDataInsurance { Id = Guid.NewGuid(), NoReg = "2001", CompleteStatus = true }
            };

            // Act
            _service.UpsertMultiplePersonalDataInsurance(data);

            // Assert
            _mockInsuranceRepo.Verify(r => r.Upsert<Guid>(
                It.IsAny<PersonalDataInsurance>(),
                It.Is<string[]>(arr => arr == null || arr.Length >= 0)),
                Times.Once);
            _mockUnitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }
    }
}

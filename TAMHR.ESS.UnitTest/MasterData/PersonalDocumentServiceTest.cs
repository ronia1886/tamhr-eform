using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Infrastructure.DomainServices;
using Xunit;

namespace TAMHR.ESS.UnitTest.MasterData
{
    public class PersonalDocumentServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<PersonalDataDocument>> _mockPersonalDataDocRepo;
        private readonly Mock<IRepository<User>> _mockUserReadonlyRepo;
        private readonly PersonalDataDocumentService _service;
        public PersonalDocumentServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockPersonalDataDocRepo = new Mock<IRepository<PersonalDataDocument>>();

            _mockUserReadonlyRepo = new Mock<IRepository<User>>();

            _unitOfWork.Setup(u => u.GetRepository<User>()).Returns(_mockUserReadonlyRepo.Object);
            _unitOfWork.Setup(u => u.GetRepository<PersonalDataDocument>()).Returns(_mockPersonalDataDocRepo.Object);

            _service = new PersonalDataDocumentService(_unitOfWork.Object);
        }

        [Fact]
        public void GetDocuments_ShouldReturnAllDocuments()
        {
            // Arrange
            var fakeDocs = new List<PersonalDataDocument>
            {
                new PersonalDataDocument { Id = Guid.NewGuid(), Name = "KTP" },
                new PersonalDataDocument { Id = Guid.NewGuid(), Name = "SIM" }
            }.AsQueryable();

            _mockPersonalDataDocRepo.Setup(r => r.Fetch()).Returns(fakeDocs);

            // Act
            var result = _service.GetDocuments();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, d => d.Name == "KTP");
            Assert.Contains(result, d => d.Name == "SIM");

            // Verifikasi repo dipanggil
            _mockPersonalDataDocRepo.Verify(r => r.Fetch(), Times.Once);
        }

        [Fact]
        public void GetActiveUsers_ShouldReturnOnlyActiveUsers()
        {
            // Arrange
            var fakeUsers = new List<User>
            {
                new User { Id = Guid.NewGuid(), Name = "User1", RowStatus = true },
                new User { Id = Guid.NewGuid(), Name = "User2", RowStatus = false },
                new User { Id = Guid.NewGuid(), Name = "User3", RowStatus = true }
            }.AsQueryable();

            _mockUserReadonlyRepo.Setup(r => r.Fetch()).Returns(fakeUsers);

            // Act
            var result = _service.GetActiveUsers();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, u => Assert.True(u.RowStatus));
            Assert.Contains(result, u => u.Name == "User1");
            Assert.Contains(result, u => u.Name == "User3");

            _mockUserReadonlyRepo.Verify(r => r.Fetch(), Times.Once);
        }

        [Fact]
        public void Upsert_ShouldInsert_WhenNewDocument()
        {
            // Arrange
            var newDoc = new PersonalDataDocument
            {
                Id = Guid.Empty,
                NoReg = "EMP001",
                DocumentTypeCode = "KTP"
            };

            _mockPersonalDataDocRepo.Setup(r => r.Find(It.IsAny<Expression<Func<PersonalDataDocument, bool>>>())).Returns(new List<PersonalDataDocument>().AsQueryable());

            // Act
            _service.Upsert(newDoc);

            // Assert
            _mockPersonalDataDocRepo.Verify(r => r.Attach(It.IsAny<PersonalDataDocument>(), It.IsAny<string[]>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }
    }
}

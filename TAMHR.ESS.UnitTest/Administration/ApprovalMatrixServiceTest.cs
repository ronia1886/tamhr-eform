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

namespace TAMHR.ESS.UnitTest.Administration
{
    public class ApprovalMatrixServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<ApprovalMatrix>> _mockApprovalMatrix;
        private readonly ApprovalMatrixService _service;

        public ApprovalMatrixServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockApprovalMatrix = new Mock<IRepository<ApprovalMatrix>>();
            _service = new ApprovalMatrixService(_unitOfWork.Object);

            _unitOfWork.Setup(u => u.GetRepository<ApprovalMatrix>())
                           .Returns(_mockApprovalMatrix.Object);

            _unitOfWork.Setup(u => u.Transact(It.IsAny<Action>(), It.IsAny<System.Data.IsolationLevel>()))
                        .Callback<Action, System.Data.IsolationLevel>((action, _) => action());

            _unitOfWork.Setup(u => u.SaveChanges());
        }

        [Fact]
        public void Create_ApprovalMatrix()
        {
            // Arrange
            var model = new ApprovalMatrix { Id = Guid.NewGuid() };

            // Act
            _service.Upsert(model);

            // Assert
            _mockApprovalMatrix.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void Update_ApprovalMatrix()
        {
            // Arrange
            var model = new ApprovalMatrix { Id = Guid.NewGuid() };

            // Act
            _service.Upsert(model);

            // Assert
            _mockApprovalMatrix.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void DeleteById_ApprovalMatrix()
        {
            // Arrange
            var id = Guid.NewGuid();
            var approvalMatrix = new ApprovalMatrix { Id = id };

            _mockApprovalMatrix.Setup(r => r.FindById(id)).Returns(approvalMatrix);

            _unitOfWork.Setup(u => u.SaveChanges()).Returns(1);

            // Act
            var result = _service.Delete(id);

            // Assert
            _mockApprovalMatrix.Verify(r => r.DeleteById(id), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
            Assert.True(result);
        }
    }
}

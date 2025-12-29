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
    public class FormServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<Form>> _mockForm;
        private readonly FormService _service;

        public FormServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockForm = new Mock<IRepository<Form>>();
            _service = new FormService(_unitOfWork.Object);

            _unitOfWork.Setup(u => u.GetRepository<Form>())
                           .Returns(_mockForm.Object);

            _unitOfWork.Setup(u => u.Transact(It.IsAny<Action>(), It.IsAny<System.Data.IsolationLevel>()))
                        .Callback<Action, System.Data.IsolationLevel>((action, _) => action());

            _unitOfWork.Setup(u => u.SaveChanges());
        }

        [Fact]
        public void CreateForm()
        {
            // Arrange
            var model = new Form { Id = Guid.NewGuid() };
            var actor = "EMP001";

            // Act
            _service.Upsert(actor,model);

            // Assert
            _mockForm.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void UpdateForm()
        {
            // Arrange
            var model = new Form { Id = Guid.NewGuid() };
            var actor = "EMP001";

            // Act
            _service.Upsert(actor, model);

            // Assert
            _mockForm.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void DeleteById_Language()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userImpersonation = new Form { Id = id };

            _mockForm.Setup(r => r.FindById(id)).Returns(userImpersonation);

            _unitOfWork.Setup(u => u.SaveChanges()).Returns(1);

            // Act
            _service.Delete(id);

            // Assert
            _mockForm.Verify(r => r.DeleteById(id), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }
    }
}

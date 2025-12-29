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
    public class EmailTemplateServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<EmailTemplate>> _mockEmailTemplate;
        private readonly Mock<IRepository<Config>> _mockConfig;
        private readonly CoreService _service;
        private readonly EmailService _emailService;

        public EmailTemplateServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockEmailTemplate = new Mock<IRepository<EmailTemplate>>();
            _mockConfig = new Mock<IRepository<Config>>();
            _service = new CoreService(_unitOfWork.Object);
            _emailService = new EmailService(_unitOfWork.Object);

            _unitOfWork.Setup(u => u.GetRepository<EmailTemplate>())
                           .Returns(_mockEmailTemplate.Object);
            _unitOfWork.Setup(u => u.GetRepository<Config>())
                           .Returns(_mockConfig.Object);

            _unitOfWork.Setup(u => u.Transact(It.IsAny<Action>(), It.IsAny<System.Data.IsolationLevel>()))
                        .Callback<Action, System.Data.IsolationLevel>((action, _) => action());

            _unitOfWork.Setup(u => u.SaveChanges());
        }

        //[Fact]
        //public async Task SendBpbk_Should_Call_SendGetBpkbReminderAsync()
        //{
        //    // Arrange
        //    var mockEmailService = new Mock<ITestEmailService>();

        //    // Act
        //    await _emailService.SendGetBpkbReminderAsync();

        //    // Assert
        //    mockEmailService.Verify(s => s.SendGetBpkbReminderAsync(), Times.Once);
        //}

        [Fact]
        public void Create_EmailTemplate()
        {
            // Arrange
            var model = new EmailTemplate { Id = Guid.NewGuid() };

            // Act
            _service.UpsertEmailTemplate(model);

            // Assert
            _mockEmailTemplate.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void Update_EmailTemplate()
        {
            // Arrange
            var model = new EmailTemplate { Id = Guid.NewGuid() };

            // Act
            _service.UpsertEmailTemplate(model);

            // Assert
            _mockEmailTemplate.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void DeleteById_EmailTemplate()
        {
            // Arrange
            var id = Guid.NewGuid();
            var approvalMatrix = new EmailTemplate { Id = id };

            _mockEmailTemplate.Setup(r => r.FindById(id)).Returns(approvalMatrix);

            _unitOfWork.Setup(u => u.SaveChanges()).Returns(1);

            // Act
            _service.DeleteEmailTemplate(id);

            // Assert
            _mockEmailTemplate.Verify(r => r.DeleteById(id), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }
    }

    public interface ITestEmailService
    {
        Task SendGetBpkbReminderAsync();
    }
}

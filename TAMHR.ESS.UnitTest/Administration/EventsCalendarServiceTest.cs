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
    public class EventsCalendarServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<EventsCalendar>> _mockEventsCalendar;
        private readonly CoreService _service;

        public EventsCalendarServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockEventsCalendar = new Mock<IRepository<EventsCalendar>>();
            _service = new CoreService(_unitOfWork.Object);

            _unitOfWork.Setup(u => u.GetRepository<EventsCalendar>())
                           .Returns(_mockEventsCalendar.Object);

            _unitOfWork.Setup(u => u.Transact(It.IsAny<Action>(), It.IsAny<System.Data.IsolationLevel>()))
                        .Callback<Action, System.Data.IsolationLevel>((action, _) => action());

            _unitOfWork.Setup(u => u.SaveChanges());
        }

        [Fact]
        public void CreateEventsCalendar()
        {
            // Arrange
            var model = new EventsCalendar { Id = Guid.NewGuid() };

            // Act
            _service.UpsertEventsCalendar(model);

            // Assert
            _mockEventsCalendar.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void UpdateEventsCalendar()
        {
            // Arrange
            var model = new EventsCalendar { Id = Guid.NewGuid() };

            // Act
            _service.UpsertEventsCalendar(model);

            // Assert
            _mockEventsCalendar.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void DeleteById_EventsCalendar()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userImpersonation = new EventsCalendar { Id = id };

            _mockEventsCalendar.Setup(r => r.FindById(id)).Returns(userImpersonation);

            _unitOfWork.Setup(u => u.SaveChanges()).Returns(1);

            // Act
            _service.DeleteEventsCalendar(id);

            // Assert
            _mockEventsCalendar.Verify(r => r.DeleteById(id), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }
    }
}

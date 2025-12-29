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
    public class NewsServiceTest
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IRepository<News>> _mockNews;
        private readonly NewsService _service;

        public NewsServiceTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _mockNews = new Mock<IRepository<News>>();
            _service = new NewsService(_unitOfWork.Object);

            _unitOfWork.Setup(u => u.GetRepository<News>())
                           .Returns(_mockNews.Object);

            _unitOfWork.Setup(u => u.Transact(It.IsAny<Action>(), It.IsAny<System.Data.IsolationLevel>()))
                        .Callback<Action, System.Data.IsolationLevel>((action, _) => action());

            _unitOfWork.Setup(u => u.SaveChanges());
        }

        [Fact]
        public void UpdateViewCount_ShouldIncrement_WhenNewsExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var news = new News { Id = id, ViewCount = 10 };

            _mockNews.Setup(r => r.FindById(id)).Returns(news);

            // Act
            _service.UpdateViewCount(id);

            // Assert
            Assert.Equal(11, news.ViewCount);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void UpdateReadCount()
        {
            // Arrange
            var id = Guid.NewGuid();
            var news = new News { Id = id, ReadCount = 5 };

            _mockNews.Setup(r => r.FindById(id)).Returns(news);

            // Act
            _service.UpdateReadCount(id);

            // Assert
            Assert.Equal(6, news.ReadCount);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void CreateNews()
        {
            // Arrange
            var model = new News { Id = Guid.NewGuid() };

            // Act
            _service.UpsertNews(model);

            // Assert
            _mockNews.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void UpdateNews()
        {
            // Arrange
            var model = new News { Id = Guid.NewGuid() };

            // Act
            _service.UpsertNews(model);

            // Assert
            _mockNews.Verify(r => r.Upsert<Guid>(model, It.IsAny<string[]>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }

        [Fact]
        public void DeleteById_News()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userImpersonation = new News { Id = id };

            _mockNews.Setup(r => r.FindById(id)).Returns(userImpersonation);

            _unitOfWork.Setup(u => u.SaveChanges()).Returns(1);

            // Act
            _service.DeleteNews(id);

            // Assert
            _mockNews.Verify(r => r.DeleteById(id), Times.Once);
            _unitOfWork.Verify(u => u.SaveChanges(), Times.Once);
        }
    }
}

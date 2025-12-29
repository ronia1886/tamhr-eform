using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Z.EntityFramework.Plus;
using Agit.Common.Extensions;
using Scriban;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle news
    /// </summary>
    public class NewsService : DomainServiceBase
    {
        #region Repositories
        /// <summary>
        /// News repository
        /// </summary>
        protected IRepository<News> NewsRepository => UnitOfWork.GetRepository<News>();
        #endregion

        #region Private Properties
        /// <summary>
        /// Field that hold properties that can be updated for news entity
        /// </summary>
        private readonly string[] _newsProperties = new[] { "Title", "SlugUrl", "ImageUrl", "BodyHtml", "ShortDescription", "OrderIndex", "Sticky" };
        #endregion

        #region Constructor
        /// <summary>
        /// Constructore
        /// </summary>
        /// <param name="unitOfWork">Concrete UnitOfWork</param>
        public NewsService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get list of news
        /// </summary>
        /// <param name="cache">Cache flag (if true get from cache).</param>
        /// <returns>List of News</returns>
        public IEnumerable<News> GetListNews(bool cache = false)
        {
            var news = NewsRepository.Fetch()
                .AsNoTracking();

            return cache ? news.FromCache("news") : news;
        }

        /// <summary>
        /// Get list of news without body
        /// </summary>
        /// <param name="cache">Cache flag (if true get from cache).</param>
        /// <returns>List of News</returns>
        public IEnumerable<News> GetListNewsWithoutBody(bool cache = false)
        {
            var news = NewsRepository.Fetch()
                .AsNoTracking()
                .Select(x => new News {
                    Id = x.Id,
                    Title = x.Title,
                    ViewCount = x.ViewCount,
                    ReadCount = x.ReadCount,
                    ImageUrl = x.ImageUrl,
                    Sticky = x.Sticky,
                    SlugUrl = x.SlugUrl,
                    ShortDescription = x.ShortDescription,
                    OrderIndex = x.OrderIndex,
                    CreatedBy = x.CreatedBy,
                    CreatedOn = x.CreatedOn,
                    ModifiedBy = x.ModifiedBy,
                    ModifiedOn = x.ModifiedOn,
                    RowStatus = x.RowStatus
                });

            return cache ? news.FromCache("news") : news;
        }

        /// <summary>
        /// Get news by id
        /// </summary>
        /// <param name="id">News Id</param>
        /// <returns>News Object</returns>
        public News GetNews(Guid id)
        {
            return NewsRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        /// <summary>
        /// Get news by slug url
        /// </summary>
        /// <param name="slug">Slug Url</param>
        /// <returns>News Object</returns>
        public News GetBySlug(string slug)
        {
            return NewsRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.SlugUrl.ToLower() == slug.ToLower())
                .FirstOrDefaultIfEmpty();
        }

        /// <summary>
        /// Update news view count by id
        /// </summary>
        /// <param name="id">News Id</param>
        public void UpdateViewCount(Guid id)
        {
            var news = NewsRepository.FindById(id);

            if (news == null) return;

            if (news.ViewCount < int.MaxValue)
            {
                news.ViewCount++;
            }

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Update news read count by id
        /// </summary>
        /// <param name="id">News Id</param>
        public void UpdateReadCount(Guid id)
        {
            var news = NewsRepository.FindById(id);

            if (news == null) return;

            if (news.ReadCount < int.MaxValue)
            {
                news.ReadCount++;
            }

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Update or insert news
        /// </summary>
        /// <param name="news">news Object</param>
        public async Task UpsertNews(News news)
        {
            var isNew = news.Id == Guid.Empty;
            var now = DateTime.Now;
            var emailService = new EmailService(UnitOfWork);
            var coreService = new CoreService(UnitOfWork);
            var configService = new ConfigService(UnitOfWork);

            NewsRepository.Upsert<Guid>(news, _newsProperties);

            UnitOfWork.SaveChanges();

            if (isNew)
            {
                var mailManager = emailService.CreateEmailManager();

                var recipient = configService.GetConfigValue<string>("Group.Email");
                var localUrl = configService.GetConfigValue<string>("Application.LocalUrl");
                var applicationUrl = configService.GetConfigValue<string>("Application.Url");
                var emailTemplate = coreService.GetEmailTemplate("news-template");
                var instanceKey = $"app-news-" + news.Id.ToString("N");
                var mailSubject = emailTemplate.Subject;
                var mailFrom = emailTemplate.MailFrom;
                var template = Template.Parse(emailTemplate.MailContent);
                var imageUrl = (news.ImageUrl ?? string.Empty).Replace("~", localUrl);
                var url = $"{applicationUrl}/news/{news.SlugUrl}";

                var mailContent = template.Render(new
                {
                    now.Year,
                    imageUrl,
                    news.ShortDescription,
                    Url = url
                });

                await mailManager.SendAsync(mailFrom, mailSubject, mailContent, recipient).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Soft delete news by id and its dependencies if any
        /// </summary>
        /// <param name="id">News Id</param>
        public void SoftDeleteNews(Guid id)
        {
            var news = NewsRepository.FindById(id);

            news.RowStatus = false;

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Delete news by id and its dependencies if any
        /// </summary>
        /// <param name="id">News Id</param>
        public void DeleteNews(Guid id)
        {
            NewsRepository.DeleteById(id);

            UnitOfWork.SaveChanges();
        }

        /// <summary>
        /// Get sticky news without HTML body asynchronously
        /// </summary>
        /// <returns>List of News</returns>
        public async Task<IEnumerable<News>> GetStickyNewsWithoutBody()
        {
            var stickyNews = await NewsRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Sticky)
                .Select(x => new News {
                    Id = x.Id,
                    Title = x.Title,
                    SlugUrl = x.SlugUrl,
                    ImageUrl = x.ImageUrl,
                    Sticky = x.Sticky,
                    OrderIndex = x.OrderIndex,
                    RowStatus = x.RowStatus
                })
                .OrderBy(x => x.OrderIndex)
                .Take(10)
                .ToListAsync()
                .ConfigureAwait(false);

            return stickyNews;
        } 
        #endregion
    }
}

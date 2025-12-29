using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Common;
using Agit.Domain;
using Agit.Common.Extensions;
using Agit.Domain.UnitOfWork;
using Agit.Domain.Repository;
using TAMHR.ESS.Infrastructure.Exceptions;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle proxy time form.
    /// </summary>
    public class ProxyTimeFormService : DomainServiceBase
    {
        #region Domain Repositories
        /// <summary>
        /// Proxy time repository object.
        /// </summary>
        protected IRepository<ProxyTime> ProxyTimeRepository => UnitOfWork.GetRepository<ProxyTime>();
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public ProxyTimeFormService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        public void ValidateProxy(string noreg, DateTime now)
        {
            var currentDate = now.Date;

            var formSet = UnitOfWork.GetRepository<Form>();

            var form = formSet.Fetch()
                .FirstOrDefault(x => x.FormKey == ApplicationForm.ProxyTimeForm);

            var startDate = form.StartDate ?? new DateTime(now.Year, 1, 1);
            var endDate = form.EndDate ?? new DateTime(9999, 12, 31);

            var isOpen = form == null || (now >= startDate && now <= endDate);

            Assert.ThrowIf(!isOpen, new CommonViewException("Information", "Currently you cannot view this form.", "~/"));

            var canProxy = ProxyTimeRepository.Fetch()
                .Any(x => x.NoReg == noreg && x.WorkingDate == currentDate);

            Assert.ThrowIf(!canProxy, new CommonViewException("Information", "Currently you cannot view this form.", "~/"));
        }

        public ProxyTime GetProxyTime(string noreg, DateTime date)
        {
            var proxyTime = ProxyTimeRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.NoReg == noreg && x.WorkingDate == date)
                .FirstOrDefaultIfEmpty();

            proxyTime.WorkingDate = date;
            proxyTime.NoReg = noreg;

            return proxyTime;
        }

        public void ProxyIn(string noreg, DateTime now, string latitude, string longitude)
        {
            var currentDate = now.Date;

            ValidateProxy(noreg, now);

            var proxyTime = ProxyTimeRepository.Fetch()
                .FirstOrDefault(x => x.NoReg == noreg && x.WorkingDate == currentDate);

            Assert.ThrowIf(proxyTime == null, "Date doesnt exist");
            Assert.ThrowIf(proxyTime.ProxyIn.HasValue, "Already proxy in, you can only proxy in once.");

            proxyTime.ProxyIn = now;
            proxyTime.Latitude = latitude;
            proxyTime.Longitude = longitude;
            proxyTime.GeoLocation = latitude + "," + longitude;

            UnitOfWork.SaveChanges();
        }

        public void ProxyOut(string noreg, DateTime now)
        {
            var currentDate = now.Date;

            ValidateProxy(noreg, now);

            var proxyTime = ProxyTimeRepository.Fetch()
                .FirstOrDefault(x => x.NoReg == noreg && x.WorkingDate == currentDate);

            Assert.ThrowIf(proxyTime == null, "Date doesnt exist");
            Assert.ThrowIf(proxyTime.ProxyOut.HasValue, "Failed to do this action. You've already proxy out");

            proxyTime.ProxyOut = now;

            UnitOfWork.SaveChanges();
        }
        #endregion
    }
}

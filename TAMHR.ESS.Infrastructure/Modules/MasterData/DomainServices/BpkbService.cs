using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Common.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle bpkb master data.
    /// </summary>
    public class BpkbService : GenericDomainServiceBase<Bpkb>
    {
        #region Domain Repositories
        /// <summary>
        /// BPKB repository object.
        /// </summary>
        protected IRepository<Bpkb> BpkbRepository => UnitOfWork.GetRepository<Bpkb>();

        /// <summary>
        /// BPKB readonly repository object.
        /// </summary>
        protected IReadonlyRepository<BpkbView> BpkbReadonlyRepository => UnitOfWork.GetRepository<BpkbView>();

        /// <summary>
        /// User repository object.
        /// </summary>
        protected IRepository<User> UserRepository => UnitOfWork.GetRepository<User>();

        /// <summary>
        /// BPKB request repository object.
        /// </summary>
        protected IRepository<BpkbRequest> BpkbRequestRepository => UnitOfWork.GetRepository<BpkbRequest>();
        #endregion

        #region Variables & Properties
        /// <summary>
        /// Field that hold properties that can be updated for BPKB entity.
        /// </summary>
        protected override string[] Properties => new[] {
            "NoBPKB",
            "LicensePlat",
            "Type",
            "Model",
            "CreatedYear",
            "Color",
            "VINNo",
            "EngineNo",
            "VehicleOwner",
            "Address"
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public BpkbService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        public IQueryable<BpkbView> GetViews()
        {
            return BpkbReadonlyRepository.Fetch().AsNoTracking();
        }

        public BpkbView GetView(Guid id)
        {
            return GetViews().Where(x => x.Id == id).FirstOrDefaultIfEmpty();
        }

        public IQueryable<Bpkb> GetByNoreg(string Noreg)
        {
            return BpkbRepository.Fetch().Where(x => x.NoReg == Noreg);
        }

        public IQueryable<object> GetDetails(Guid parentId)
        {
            var bpkb = BpkbRepository.Fetch().AsNoTracking().FirstOrDefault(x => x.Id == parentId);

            var query = from req in BpkbRequestRepository.Fetch().AsNoTracking()
                        join usr in UserRepository.Fetch().AsNoTracking() on req.NoReg equals usr.NoReg into pgroup
                        from usr in pgroup.DefaultIfEmpty()
                        where req.RowStatus && req.BPKBId == parentId && req.NoReg == bpkb.NoReg
                        select new
                        {
                            req.Id,
                            req.BPKBId,
                            req.NoReg,
                            usr.Name,
                            req.RequestType,
                            req.BorrowDate,
                            req.ReturnDate,
                            req.BorrowPurpose
                        };

            return query;
        }
        #endregion
    }
}

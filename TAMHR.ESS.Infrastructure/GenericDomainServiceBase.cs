using Agit.Common;
using Agit.Common.Extensions;
using Agit.Domain;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TAMHR.ESS.Infrastructure
{
    /// <summary>
    /// Generic domain service class for CRUD operation.
    /// </summary>
    /// <typeparam name="T">This domain entity class.</typeparam>
    public abstract class GenericDomainServiceBase<T> : GenericDomainServiceBase<T, Guid> where T : class, IEntityBase<Guid>
    {
        public GenericDomainServiceBase(IUnitOfWork unitOfWork)
          : base(unitOfWork)
        {
        }
    }
    public abstract class GenericDomainServiceBase<T, D> : DomainServiceBase where T : class, IEntityBase<D>
    {
        protected IRepository<T> Repository => this.UnitOfWork.GetRepository<T>();

        protected abstract string[] Properties { get; }

        protected IRepository<T> CommonRepository => UnitOfWork.GetRepository<T>();

        public GenericDomainServiceBase(IUnitOfWork unitOfWork)
          : base(unitOfWork)
        {
        }

        public virtual IQueryable<T> GetQuery()
        {
            return EntityFrameworkQueryableExtensions.AsNoTracking<T>(this.Repository.Fetch());
        }

        public virtual T GetById(D id)
        {
            return this.GetQuery().FirstOrDefault<T>((Expression<Func<T, bool>>)(x => x.Id.Equals((object)id)));
        }

        public virtual void Upsert(T data)
        {
            // Marked given domain entity as updated with specified properties to update.
            CommonRepository.Upsert<Guid>(data, Properties);

            // Push pending changes into database.
            UnitOfWork.SaveChanges();
        }

        public virtual bool Delete(T data)
        {
            this.Repository.Delete(data);
            return this.UnitOfWork.SaveChanges() > 0;
        }

        public virtual void DeleteById(D id)
        {
            // Delete domain entity by id from repository.
            CommonRepository.DeleteById(id);

            // Push pending changes into database.
            UnitOfWork.SaveChanges();
        }

        public virtual bool SoftDeleteById(D id)
        {
            this.Repository.FindById((object)id).RowStatus = false;
            return this.UnitOfWork.SaveChanges() > 0;
        }

        public IEnumerable<T> Gets()
        {
            return GetQuery().ToList();
        }
    }
    //public abstract class GenericDomainServiceBase<T> : DomainServiceBase
    //    where T : class, IEntityBase<Guid>
    //{
    //    #region Fields & Properties
    //    /// <summary>
    //    /// Property that hold <see cref="T"/> domain entity repository object.
    //    /// </summary>
    //    protected IRepository<T> CommonRepository => UnitOfWork.GetRepository<T>();

    //    /// <summary>
    //    /// Field that hold list of properties to update.
    //    /// </summary>
    //    protected abstract string[] Properties { get; }
    //    #endregion

    //    #region Constructor
    //    /// <summary>
    //    /// Public constructor.
    //    /// </summary>
    //    /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
    //    public GenericDomainServiceBase(IUnitOfWork unitOfWork)
    //        : base(unitOfWork)
    //    {
    //    }
    //    #endregion

    //    #region Public Methods
    //    /// <summary>
    //    /// Get <see cref="IQueryable"/> objects from <see cref="T"/> domain entity.
    //    /// </summary>
    //    /// <returns>This list of <see cref="T"/> query objects.</returns>
    //    public virtual IQueryable<T> GetQuery()
    //    {
    //        return CommonRepository.Fetch()
    //            .AsNoTracking();
    //    }

    //    /// <summary>
    //    /// Get all domain entities.
    //    /// </summary>
    //    /// <returns>This list of <see cref="T"/> domain entity objects</returns>
    //    public IEnumerable<T> Gets()
    //    {
    //        return GetQuery().ToList();
    //    }

    //    /// <summary>
    //    /// Get domain entity by id.
    //    /// </summary>
    //    /// <param name="id">This domain entity id.</param>
    //    /// <returns>This <see cref="T"/> domain entity object.</returns>
    //    public T Get(Guid id)
    //    {
    //        // Get domain entity by id and return default object if not exist.
    //        return GetQuery()
    //            .Where(x => x.Id == id)
    //            .FirstOrDefaultIfEmpty();
    //    }

    //    /// <summary>
    //    /// Update or insert domain entity data.
    //    /// </summary>
    //    /// <param name="data">This <see cref="T"/> domain entity.</param>
    //    public virtual void Upsert(T data)
    //    {
    //        // Marked given domain entity as updated with specified properties to update.
    //        CommonRepository.Upsert<Guid>(data, Properties);

    //        // Push pending changes into database.
    //        UnitOfWork.SaveChanges();
    //    }

    //    /// <summary>
    //    /// Soft delete domain entity by id.
    //    /// </summary>
    //    /// <param name="id">This domain entity id.</param>
    //    public virtual void SoftDelete(Guid id)
    //    {
    //        // Get domain entity by id from repository.
    //        var item = CommonRepository.FindById(id);

    //        // Update row status to false.
    //        item.RowStatus = false;

    //        // Push pending changes into database.
    //        UnitOfWork.SaveChanges();
    //    }

    //    /// <summary>
    //    /// Delete domain entity by id.
    //    /// </summary>
    //    /// <param name="id">This domain entity id.</param>
    //    public virtual void Delete(Guid id)
    //    {
    //        // Delete domain entity by id from repository.
    //        CommonRepository.DeleteById(id);

    //        // Push pending changes into database.
    //        UnitOfWork.SaveChanges();
    //    }
    //    #endregion
    //}
}

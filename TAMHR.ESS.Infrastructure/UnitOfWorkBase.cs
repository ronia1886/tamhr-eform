using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Agit.Common;
using Agit.Common.Cache;
using Agit.Domain.Repository;
using Agit.Domain.Repository.Adapters;
using Agit.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace TAMHR.ESS.Infrastructure
{
    public abstract class UnitOfWorkBase : DbContext, IUnitOfWork, IDisposable
    {
        private readonly GenericCache _genericCache = new GenericCache();

        public UnitOfWorkBase(DbContextOptions options)
            : base(options)
        {
        }

        public void Transact(Action action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (Database.CurrentTransaction != null)
            {
                action();
                return;
            }

            var dbContextTransaction = Database.BeginTransaction(isolationLevel);
            try
            {
                action();
                dbContextTransaction.Commit();
            }
            catch (Exception ex)
            {
                ClearChangeTrackers();
                dbContextTransaction.Rollback();
                throw new Exception(ex.ToString());
            }
        }

        public void Transact(Action<IDbTransaction> action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (Database.CurrentTransaction != null)
            {
                action(Database.CurrentTransaction.GetDbTransaction());
                return;
            }

            var dbContextTransaction = Database.BeginTransaction(isolationLevel);
            try
            {
                DbTransaction dbTransaction = dbContextTransaction.GetDbTransaction();
                action(dbTransaction);
                dbContextTransaction.Commit();
            }
            catch (Exception ex)
            {
                ClearChangeTrackers();
                dbContextTransaction.Rollback();
                throw new Exception(ex.ToString());
            }
        }

        public IRepository<T> GetRepository<T>() where T : class
        {
            return _genericCache.Get(() => new EFRepositoryAdapter<T>(this));
        }

        public DbConnection GetConnection()
        {
            return Database.GetDbConnection();
        }

        protected abstract string GetActor();

        protected abstract Type GetDomainType();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (Type item in from x in GetDomainType().Assembly.GetTypes()
                                  where typeof(IEntityMarker).IsAssignableFrom(x) && x.IsClass
                                  select x)
            {
                modelBuilder.Entity(item);
            }

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            IEnumerable<EntityEntry> enumerable = ChangeTracker.Entries();
            DateTime now = DateTime.Now;
            foreach (EntityEntry item in enumerable)
            {
                if (item.State == EntityState.Deleted || item.State == EntityState.Detached || item.State == EntityState.Unchanged || !(item.Entity is IEntityBase))
                {
                    continue;
                }

                IEntityBase entityBase = item.Entity as IEntityBase;
                if (item.State == EntityState.Modified)
                {
                    entityBase.ModifiedBy = GetActor();
                    entityBase.ModifiedOn = now;
                    continue;
                }

                if (string.IsNullOrEmpty(entityBase.CreatedBy))
                {
                    entityBase.CreatedBy = GetActor();
                }

                entityBase.CreatedOn = now;
                entityBase.RowStatus = true;
            }

            return base.SaveChanges();
        }

        private void ClearChangeTrackers()
        {
            (from e in ChangeTracker.Entries()
             where e.Entity != null
             select e).ToList().ForEach(delegate (EntityEntry e)
             {
                 e.State = EntityState.Detached;
             });
        }
    }
}

using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Domain;
using Agit.Common.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle access role.
    /// </summary>
    public class AccessRoleService : DomainServiceBase
    {
        #region Domain Repositories
        /// <summary>
        /// Access role repository object.
        /// </summary>
        protected IRepository<AccessRole> AccessRoleRepository => UnitOfWork.GetRepository<AccessRole>();

        /// <summary>
        /// Access role readonly repository object.
        /// </summary>
        protected IReadonlyRepository<AccessRoleView> AccessRoleReadonlyRepository => UnitOfWork.GetRepository<AccessRoleView>();

        /// <summary>
        /// Access group readonly repository object.
        /// </summary>
        protected IReadonlyRepository<AccessGroupView> AccessGroupReadonlyRepository => UnitOfWork.GetRepository<AccessGroupView>();
        #endregion

        #region Variables & Properties
        /// <summary>
        /// List of entity fields that can be updated.
        /// </summary>
        private readonly string[] _properties = new[] { "AccessCode", "RoleId", "AccessTypeCode" }; 
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public AccessRoleService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get all list of access role objects.
        /// </summary>
        /// <returns>This list of <see cref="AccessRoleView"/> objects.</returns>
        public IEnumerable<AccessRoleView> Gets()
        {
            // Get and set list of access role objects without object tracking.
            var output = AccessRoleReadonlyRepository.Fetch()
                .AsNoTracking();

            // Return the list.
            return output;
        }

        /// <summary>
        /// Get access role by id.
        /// </summary>
        /// <param name="id">This access role id.</param>
        /// <returns>This <see cref="AccessRoleView"/> object.</returns>
        public AccessRoleView Get(Guid id)
        {
            // Get access role by id and return default object if empty.
            return Gets()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        /// <summary>
        /// Get list of access groups by type.
        /// </summary>
        /// <param name="type">This access role type.</param>
        /// <returns>This list of <see cref="AccessGroupView"/> objects.</returns>
        public IEnumerable<AccessGroupView> GetAccessGroups(string type)
        {
            // Get list of access groups by type without object tracking.
            return AccessGroupReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Type == type).ToList();
        }

        /// <summary>
        /// Update or insert access role.
        /// </summary>
        /// <param name="accessRole">This <see cref="AccessRole"/> object.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool Upsert(AccessRole accessRole)
        {
            // Update or insert access role with specified list of properties to update.
            AccessRoleRepository.Upsert<Guid>(accessRole, _properties);

            // Push pending changes into database and return the boolean result.
            return UnitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Soft delete access role by id and its dependencies if any.
        /// </summary>
        /// <param name="id">This access role id.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool SoftDelete(Guid id)
        {
            // Get access role by id.
            var accessRole = AccessRoleRepository.FindById(id);

            // Update the row status value to false.
            accessRole.RowStatus = false;

            // Push pending changes into database and return the boolean result.
            return UnitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Delete access role by id and its dependencies if any.
        /// </summary>
        /// <param name="id">This access role id.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool Delete(Guid id)
        {
            // Mark access role object as deleted.
            AccessRoleRepository.DeleteById(id);

            // Push pending changes into database and return the boolean result.
            return UnitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Get mapping query.
        /// </summary>
        /// <param name="code">This access role code.</param>
        /// <returns>This list of <see cref="AccessGroupView"/> objects.</returns>
        public IQueryable<AccessGroupView> GetMappingQuery(string code)
        {
            // Get list of access groups by access role code without object tracking order by name.
            return AccessGroupReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => string.IsNullOrEmpty(code) || x.Code == code)
                .OrderBy(x => x.Name);
        }
        #endregion
    }
}

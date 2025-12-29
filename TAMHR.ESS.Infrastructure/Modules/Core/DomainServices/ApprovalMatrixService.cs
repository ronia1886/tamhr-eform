using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TAMHR.ESS.Domain;
using Agit.Domain;
using Agit.Common.Extensions;
using Agit.Domain.Repository;
using Agit.Domain.UnitOfWork;

namespace TAMHR.ESS.Infrastructure.DomainServices
{
    /// <summary>
    /// Service class that handle approval matrix.
    /// </summary>
    public class ApprovalMatrixService : DomainServiceBase
    {
        #region Domain Repositories
        /// <summary>
        /// Approval matrix repository object.
        /// </summary>
        protected IRepository<ApprovalMatrix> ApprovalMatrixRepository => UnitOfWork.GetRepository<ApprovalMatrix>();

        /// <summary>
        /// Approval matrix readonly repository object.
        /// </summary>
        protected IReadonlyRepository<ApprovalMatrixView> ApprovalMatrixReadonlyRepository => UnitOfWork.GetRepository<ApprovalMatrixView>();

        /// <summary>
        /// Approval group readonly repository object.
        /// </summary>
        protected IReadonlyRepository<ApprovalGroupView> ApprovalGroupReadonlyRepository => UnitOfWork.GetRepository<ApprovalGroupView>();
        protected IReadonlyRepository<InitiatorPatternView> InitiatorPatternViewRepository => UnitOfWork.GetRepository<InitiatorPatternView>();
        protected IReadonlyRepository<InitiatorTypeView> InitiatorTypeViewRepository => UnitOfWork.GetRepository<InitiatorTypeView>();
        #endregion

        #region Variables & Properties
        /// <summary>
        /// List of entity fields that can be updated.
        /// </summary>
        private readonly string[] _properties = new[] {
            "FormId",
            "InitiatorPattern",
            "InitiatorType",
            "Approver",
            "ApproverType",
            "ApproverLevel",
            "ApproveAll",
            "StartDate",
            "EndDate",
            "Permissions",
            "MandatoryInput",
            "Excludes"
        };
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="unitOfWork">This <see cref="IUnitOfWork"/> concrete object.</param>
        public ApprovalMatrixService(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get all approval matrices.
        /// </summary>
        /// <returns>This list of <see cref="ApprovalMatrixView"/> objects.</returns>
        public IQueryable<ApprovalMatrixView> Gets()
        {
            // Get list of approval matrices without object tracking.
            return ApprovalMatrixReadonlyRepository.Fetch()
                .AsNoTracking();
        }

        /// <summary>
        /// Get approval matrix by id.
        /// </summary>
        /// <param name="id">This approval matrix id.</param>
        /// <returns>This <see cref="ApprovalMatrix"/> object.</returns>
        public ApprovalMatrix Get(Guid id)
        {
            // Get approval matrix by id without object tracking and return default if empty.
            return ApprovalMatrixRepository.Fetch()
                .AsNoTracking()
                .Where(x => x.Id == id)
                .FirstOrDefaultIfEmpty();
        }

        /// <summary>
        /// Update or insert approval matrix.
        /// </summary>
        /// <param name="approvalMatrix">This <see cref="ApprovalMatrix"/> object.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool Upsert(ApprovalMatrix approvalMatrix)
        {
            // If excludes parameter was present then convert it to (item1),(item2),... style.
            if (!string.IsNullOrEmpty(approvalMatrix.Excludes))
            {
                // Update excludes field.
                approvalMatrix.Excludes = "(" + approvalMatrix.Excludes.Replace(",", "),(") + ")";
            }

            // Update or insert approval matrix with specified list of properties to update.
            ApprovalMatrixRepository.Upsert<Guid>(approvalMatrix, _properties);

            // Push pending changes into database and return the boolean result.
            return UnitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Delete approval matrix by id and its dependencies if any.
        /// </summary>
        /// <param name="id">This approval matrix id.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool Delete(Guid id)
        {
            // Mark approval matrix object as deleted.
            ApprovalMatrixRepository.DeleteById(id);

            // Push pending changes into database and return the boolean result.
            return UnitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Soft delete approval matrix by id and its dependencies if any.
        /// </summary>
        /// <param name="id">This approval matrix id.</param>
        /// <returns>True if success, false otherwise.</returns>
        public bool SoftDelete(Guid id)
        {
            // Get approval matrix by id.
            var approvalMatrix = ApprovalMatrixRepository.FindById(id);

            // Update the row status value to false.
            approvalMatrix.RowStatus = false;

            // Push pending changes into database and return the boolean result.
            return UnitOfWork.SaveChanges() > 0;
        }

        /// <summary>
        /// Get mapping query.
        /// </summary>
        /// <param name="code">This approval group code.</param>
        /// <returns>This list of <see cref="ApprovalGroupView"/> objects.</returns>
        public IQueryable<ApprovalGroupView> GetMappingQuery(string code)
        {
            // Get list of approval groups by code without object tracking ordered by name.
            return ApprovalGroupReadonlyRepository.Fetch()
                .AsNoTracking()
                .Where(x => string.IsNullOrEmpty(code) || x.Code == code)
                .OrderBy(x => x.Name);
        }

        public IQueryable<InitiatorPatternView> GetInitiatorPatterns()
        {
            // Get list of approval matrices without object tracking.
            return InitiatorPatternViewRepository.Fetch()
                .AsNoTracking();
        }

        public IQueryable<InitiatorTypeView> GetInitiatorType()
        {
            // Get list of approval matrices without object tracking.
            return InitiatorTypeViewRepository.Fetch()
                .AsNoTracking();
        }
        #endregion
    }
}

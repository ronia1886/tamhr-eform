using Agit.Common;
using Agit.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using TAMHR.ESS.Domain;
using TAMHR.ESS.Domain.Models.Core.StoredEntities;

namespace TAMHR.ESS.Infrastructure
{
    /// <summary>
    /// Unit of work concrete class.
    /// </summary>
    public class UnitOfWork : UnitOfWorkBase
    {
        #region Variables & Properties
        /// <summary>
        /// Field that hold http context accessor concrete object.
        /// </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="options">This <see cref="DbContextOptions"/> object.</param>
        public UnitOfWork(DbContextOptions<UnitOfWork> options)
            : base(options)
        {
            // Set database timeout to 180 minutes.
            Database.SetCommandTimeout(90);
        }

        public DbSet<Config> Configs { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<UserView> UserViews { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<ActualOrganizationStructure> ActualOrganizationStructures { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Panggil metode pembantu untuk mendaftarkan semua entitas secara otomatis
            //modelBuilder.RegisterAllEntities(typeof(UnitOfWork).Assembly);
            // Panggil metode pembantu untuk mendaftarkan semua entitas dari assembly proyek domain
            //modelBuilder.RegisterAllEntities(typeof(UserView).Assembly);

            foreach (Type item in from x in GetDomainType().Assembly.GetTypes()
                                  where typeof(IEntityMarker).IsAssignableFrom(x) && x.IsClass
                                  select x)
            {
                modelBuilder.Entity(item);
            }

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="options">This <see cref="DbContextOptions"/> object.</param>
        /// <param name="httpContextAccessor">This <see cref="IHttpContextAccessor"/> concrete object.</param>
        public UnitOfWork(DbContextOptions<UnitOfWork> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            // Get and set http context accessor from DI container.
            _httpContextAccessor = httpContextAccessor;

            // Set database timeout to 180 minutes.
            Database.SetCommandTimeout(90);
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Get created by from current user session if present.
        /// </summary>
        /// <returns>This actor.</returns>
        protected override string GetActor()
        {
            // Default actor if current user session was not present.
            var actor = "system";

            // If there is http context accessor object then set the actor from current user session.
            if (_httpContextAccessor != null && _httpContextAccessor.HttpContext != null)
            {
                // Get actor from user claim.
                actor = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "NoReg")?.Value ?? "system";
            }

            // Return the actor.
            return actor;
        }

        /// <summary>
        /// Get domain type where domain entity placed (used for domain entity auto loader in Entity Framework).
        /// With this you dont need to register the domain entity one by one in DbContext.
        /// </summary>
        /// <returns></returns>
        protected override Type GetDomainType()
        {
            // Return the type of domain entity (this can be any classes as far as inside domain entity assembly).
            return typeof(Config);
        }
        #endregion
    }
}

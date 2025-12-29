using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TAMHR.ESS.Domain;

namespace TAMHR.ESS.Infrastructure
{
    public partial class MainDbContext : DbContext
    {
        public MainDbContext()
            : base("name=DefaultConnection")
        {
            Database.SetInitializer<MainDbContext>(null);
#if DEBUG
            Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
#endif
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }

        public DbSet<Language> Languages { get; set; }
        public DbSet<Config> Config { get; set; }
    }
}

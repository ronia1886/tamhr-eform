using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TAMHR.ESS.Infrastructure
{
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Reflection;

    public static class ModelBuilderExtensions
    {
        public static void RegisterAllEntities(this ModelBuilder modelBuilder, params Assembly[] assemblies)
        {
            var types = assemblies.SelectMany(a => a.GetExportedTypes())
                .Where(c => c.IsClass && !c.IsAbstract && c.IsPublic &&
                    c.Namespace != null && c.Namespace.EndsWith(".Domain")) // Sesuaikan dengan namespace proyek domain Anda
                .ToList();

            foreach (var type in types)
            {
                modelBuilder.Entity(type);
            }
        }
    }
}

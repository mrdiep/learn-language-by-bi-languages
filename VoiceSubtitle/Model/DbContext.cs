using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace VoiceSubtitle.Model
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {
            // Turn off the Migrations, (NOT a code first Db)
            Database.SetInitializer<AppDbContext>(null);
        }

        public DbSet<SourcePath> SourcePath { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Database does not pluralize table names
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

        
    }
}
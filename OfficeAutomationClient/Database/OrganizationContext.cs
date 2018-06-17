using OfficeAutomationClient.Model;
using System.Data.Entity;

namespace OfficeAutomationClient.Database
{
    class OrganizationContext : DbContext
    {
        public OrganizationContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {

        }

        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<PersonalDetail> PersonalDetails { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
#if INIT_ORGANIZATION_DB
            var initializer = new DbInitializer(modelBuilder);
            System.Data.Entity.Database.SetInitializer(initializer);
#endif
            modelBuilder.Entity<Organization>().ToTable("Organization");
            modelBuilder.Entity<Person>().ToTable("Person");
            modelBuilder.Entity<PersonalDetail>().ToTable("PersonalDerail");
        }
    }
}

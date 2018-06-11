using OfficeAutomationClient.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace OfficeAutomationClient.Database
{
    class OrganizationContext : DbContext
    {
        public OrganizationContext(string nameOrConnectionString) : base(nameOrConnectionString)
        {

        }

        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Person> People { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Organization>().ToTable("Organization");
            modelBuilder.Entity<Person>().ToTable("Person");
        }
    }
}

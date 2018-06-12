using OfficeAutomationClient.Model;
using OfficeAutomationClient.OA;
using SQLite.CodeFirst;
using System.Data.Entity;
using System.Linq;

namespace OfficeAutomationClient.Database
{
    internal class DbInitializer : SqliteDropCreateDatabaseAlways<OrganizationContext>
    {
        public DbInitializer(DbModelBuilder modelBuilder) : base(modelBuilder)
        {
        }

        protected override void Seed(OrganizationContext context)
        {
#if DEBUG
            var org = Business.Instance.GetOrganizations();
            if (null == org || org.Count == 0) return;

            context.Set<Organization>().AddRange(org);
            context.SaveChanges();


            foreach (var dept in context.Organizations.Where(o => o.Type == Model.OrganizationType.Dept && o.Num > 0))
            {
                var people = Business.Instance.GetPeople(dept);
                if (null == people || people.Count == 0) continue;

                context.Set<Person>().AddRange(people);
            }
            context.SaveChanges();
#endif
        }
    }
}

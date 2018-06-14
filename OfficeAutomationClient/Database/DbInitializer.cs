using OfficeAutomationClient.Model;
using OfficeAutomationClient.OA;
using SQLite.CodeFirst;
using System.Data.Entity;
using System.Linq;
using System.Threading;

namespace OfficeAutomationClient.Database
{
    internal class DbInitializer : SqliteDropCreateDatabaseAlways<OrganizationContext>
    {
        public DbInitializer(DbModelBuilder modelBuilder) : base(modelBuilder)
        {
        }

        protected override void Seed(OrganizationContext context)
        {
#if INIT_ORGANIZATION_DB
            while (true)
            {
                var org = Business.Instance.GetOrganizations();
                if (null == org || org.Count == 0)
                {
                    Thread.Sleep(500);
                    continue;
                }

                context.Set<Organization>().AddRange(org);
                context.SaveChanges();
                break;
            }


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

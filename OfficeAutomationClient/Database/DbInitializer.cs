#if INIT_ORGANIZATION_DB
using System.Data.Entity;
using System.Linq;
using OfficeAutomationClient.Model;
using OfficeAutomationClient.OA;
using SQLite.CodeFirst;

namespace OfficeAutomationClient.Database
{
    internal class DbInitializer : SqliteDropCreateDatabaseAlways<OrganizationContext>
    {
        public DbInitializer(DbModelBuilder modelBuilder) : base(modelBuilder)
        {
        }

        protected override async void Seed(OrganizationContext context)
        {
            var org = await Business.Instance.GetOrganizations();
            context.Set<Organization>().AddRange(org);
            context.SaveChanges();

            foreach (var dept in context.Organizations.Where(o => o.Type == OrganizationType.Dept && o.Num > 0))
            {
                var people = await Business.Instance.GetPeople(dept);
                if (null == people || people.Count == 0) continue;

                context.Set<Person>().AddRange(people);
            }

            context.SaveChanges();

            foreach (var person in context.People)
            {
                var detail = await Business.Instance.GetPersonalDetail(person);
                if (null == detail) continue;

                context.Set<PersonalDetail>().Add(detail);
            }

            context.SaveChanges();
        }
    }
}
#endif
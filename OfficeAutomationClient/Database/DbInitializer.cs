using OfficeAutomationClient.OA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OfficeAutomationClient.Database
{
    static class DbInitializer
    {
        internal static void Initialize(OrganizationContext context)
        {
#if DEBUG
            if (!context.Database.CreateIfNotExists()) return;

            if (!context.Organizations.Any())
            {
                var organizations = Business.Instance.GetOrganizations();

                context.Organizations.AddRange(organizations);
                context.SaveChanges();
            }
            if (!context.People.Any())
            {
                foreach (var dept in context.Organizations.Where(o => o.Type == Model.OrganizationType.Dept))
                {
                    var people = Business.Instance.GetPeople(dept);

                    context.People.AddRange(people);
                }
                context.SaveChanges();
            }
#endif
        }
    }
}

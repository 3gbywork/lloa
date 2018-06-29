using System.Collections.ObjectModel;

namespace OfficeAutomationClient.Model
{
    public class Organization
    {
        public string ID { get; set; }
        public string PID { get; set; }
        public OrganizationType Type { get; set; }
        public string Title { get; set; }
        public int Num { get; set; }
    }

    public class Organizations : Collection<Organization>
    {
    }

    public enum OrganizationType
    {
        Company,
        SubCompany,
        Dept
    }
}
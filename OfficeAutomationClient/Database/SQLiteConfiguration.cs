using System.Data.Entity;
using System.Data.SQLite.EF6;

namespace OfficeAutomationClient.Database
{
    public class SQLiteConfiguration : DbConfiguration
    {
        public SQLiteConfiguration()
        {
            //SetProviderFactory("System.Data.SQLite", SQLiteFactory.Instance);
            SetProviderFactory("System.Data.SQLite.EF6", SQLiteProviderFactory.Instance);
            SetDefaultConnectionFactory(new SQLiteConnectionFactory("System.Data.SQLite.EF6"));
        }
    }
}
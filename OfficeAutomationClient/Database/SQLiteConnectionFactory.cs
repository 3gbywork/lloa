using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.SQLite;

namespace OfficeAutomationClient.Database
{
    public class SQLiteConnectionFactory : IDbConnectionFactory
    {
        private readonly string _providerName;

        private Func<string, DbProviderFactory> _providerFactoryCreator;

        public SQLiteConnectionFactory(string providerName)
        {
            _providerName = providerName;
        }

        internal Func<string, DbProviderFactory> ProviderFactory
        {
            get => _providerFactoryCreator ?? DbConfiguration.DependencyResolver.GetService<DbProviderFactory>;
            set => _providerFactoryCreator = value;
        }

        public DbConnection CreateConnection(string nameOrConnectionString)
        {
            var connectionString = nameOrConnectionString;
            if (!TreatAsConnectionString(nameOrConnectionString))
            {
                connectionString = new SQLiteConnectionStringBuilder
                {
                    DataSource = nameOrConnectionString,
                    Version = 3
                    //Password = Business.Instance.QueryPassword(Business.AssemblyName).CreateString(),
                }.ConnectionString;
            }

            DbConnection connection = null;
            try
            {
                connection = ProviderFactory(_providerName).CreateConnection();

                DbInterception.Dispatch.Connection.SetConnectionString(
                    connection,
                    new DbConnectionPropertyInterceptionContext<string>().WithValue(connectionString));
            }
            catch
            {
                // Fallback to hard-coded type if provider didn't work
                connection = new SQLiteConnection();

                DbInterception.Dispatch.Connection.SetConnectionString(
                    connection,
                    new DbConnectionPropertyInterceptionContext<string>().WithValue(connectionString));
            }

            return connection;
        }

        public static bool TreatAsConnectionString(string nameOrConnectionString)
        {
            return nameOrConnectionString.IndexOf('=') >= 0;
        }
    }
}
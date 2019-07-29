using NHibernate.Cfg.Loquacious;
using NHibernate.Dialect;

namespace DryIoc.Facilities.NHibernate.Tests
{
	public static class AppConfig
	{
		//public const string TestConnectionString = "Data Source=DataStore.db;Version=3";
		public const string TestConnectionString = "Server=localhost;Database=DryIocTransactionsTest;Trusted_Connection=True;";

		public static void SetupConnection(IDbIntegrationConfigurationProperties configuration)
		{
			//configuration.Dialect<SQLiteDialect>();
			//configuration.Driver<SQLite20Driver>();

			configuration.Dialect<MsSql2012Dialect>();
		}
	}
}

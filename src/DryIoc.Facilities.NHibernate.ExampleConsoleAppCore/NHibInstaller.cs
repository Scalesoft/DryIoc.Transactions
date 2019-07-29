using System;
using System.Collections.Generic;
using System.Linq;
using DryIoc.Facilities.NHibernate.ExampleConsoleApp.EntityMapping;
using DryIoc.Transactions;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Connection;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;
using Environment = NHibernate.Cfg.Environment;

namespace DryIoc.Facilities.NHibernate.ExampleConsoleApp
{
	internal class NHibInstaller : INHibernateInstaller
	{
		public bool IsDefault => true;

		public string SessionFactoryKey => "def";

		public Maybe<IInterceptor> Interceptor => Maybe.None<IInterceptor>();

		public Configuration Config
		{
			get
			{
				var configuration = new Configuration()
					.DataBaseIntegration(db =>
					{
						db.LogSqlInConsole = false;
						db.LogFormattedSql = true;

						db.Dialect<SQLiteDialect>();
						db.Driver<SQLite20Driver>();
						db.ConnectionProvider<DriverConnectionProvider>();
						db.ConnectionString = "Data Source=DataStore.db;Version=3";
					})
					.SetProperty(Environment.CurrentSessionContextClass, "thread_static");

				configuration.AddMapping(GetMappings());

				return configuration;
			}
		}

		private HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			var mappings = GetMappingClasses();
			mapper.AddMappings(mappings);
			var mapping = mapper.CompileMappingForAllExplicitlyAddedEntities();

			return mapping;
		}

		private IEnumerable<Type> GetMappingClasses()
		{
			var baseType = typeof(IMapping);
			var assembly = baseType.Assembly;

			return assembly.GetExportedTypes().Where(
				t => baseType.IsAssignableFrom(t)
			);
		}

		public void Registered(ISessionFactory factory)
		{
		}

		public Configuration Deserialize()
		{
			return null;
		}

		public void Serialize(Configuration configuration)
		{
		}

		public void AfterDeserialize(Configuration configuration)
		{
		}
	}
}

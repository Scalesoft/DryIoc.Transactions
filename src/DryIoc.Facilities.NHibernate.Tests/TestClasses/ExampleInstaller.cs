// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using DryIoc.Facilities.NHibernate.Tests.EntityMappings;
using DryIoc.Transactions;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Connection;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Mapping.ByCode;
using Environment = NHibernate.Cfg.Environment;

namespace DryIoc.Facilities.NHibernate.Tests.TestClasses
{
	internal class ExampleInstaller : INHibernateInstaller
	{
		private readonly string _DatabaseFileName;
		private const string ConnectionString = "Data Source=db-{0}.db;Version=3";

		public const string Key = "sf.default";
		private readonly Maybe<IInterceptor> interceptor;

		public ExampleInstaller() : this(nameof(ExampleInstaller))
		{
		}

		public ExampleInstaller(string databaseFileName)
		{
			_DatabaseFileName = databaseFileName;
			interceptor = Maybe.None<IInterceptor>();
		}

		public ExampleInstaller(string databaseFileName, IInterceptor interceptor)
		{
			_DatabaseFileName = databaseFileName;
			this.interceptor = Maybe.Some(interceptor);
		}

		public Maybe<IInterceptor> Interceptor => interceptor;

		public Configuration Config => BuildConfiguration();

		public bool IsDefault => true;

		public string SessionFactoryKey => Key;

		private Configuration BuildConfiguration()
		{
			var connectionString = AppConfig.TestConnectionString;
			Contract.Assume(connectionString != null, "please set the \"test\" connection string in app.config");

			var configuration = new Configuration()
				.DataBaseIntegration(db =>
				{
					db.LogSqlInConsole = false;
					db.LogFormattedSql = true;
					db.Dialect<SQLiteDialect>();
					db.Driver<SQLite20Driver>();
					db.ConnectionProvider<DriverConnectionProvider>();
					db.ConnectionString = string.Format(
						CultureInfo.InvariantCulture,
						ConnectionString,
						_DatabaseFileName
					);
				})
				.SetProperty(Environment.CurrentSessionContextClass, "thread_static");

			configuration.AddMapping(GetMappings());

			return configuration;
		}

		public void Registered(ISessionFactory factory)
		{
		}

		public virtual Configuration Deserialize()
		{
			return null;
		}

		public virtual void Serialize(Configuration configuration)
		{
		}

		public virtual void AfterDeserialize(Configuration configuration)
		{
		}

		public static IEnumerable<Type> GetMappingClasses()
		{
			var baseType = typeof(IMapping);
			var assembly = baseType.Assembly;

			return assembly.GetExportedTypes().Where(
				t => baseType.IsAssignableFrom(t)
			);
		}

		private static HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			var mappings = GetMappingClasses();
			mapper.AddMappings(mappings);
			var mapping = mapper.CompileMappingForAllExplicitlyAddedEntities();

			return mapping;
		}
	}
}

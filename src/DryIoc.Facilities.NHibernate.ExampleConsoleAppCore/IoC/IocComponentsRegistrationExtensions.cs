using System;
using DryIoc.Facilities.NHibernate.ExampleConsoleApp.Repository;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DryIoc.Facilities.NHibernate.ExampleConsoleApp.IoC
{
	public static class IocComponentsRegistrationExtensions
	{
		public static void RegisterAllComponents(
			this IContainer container,
			IConfiguration configuration
		)
		{
			container.RegisterComponents();

			var serviceCollection = new ServiceCollection();
			serviceCollection.RegisterComponents(configuration);

			container.Populate(serviceCollection);
		}

		private static void RegisterComponents(
			this IServiceCollection services,
			IConfiguration configuration
		)
		{
			services.AddSingleton<Logger>();
		}

		private static void RegisterComponents(this IRegistrator container)
		{
			container.Register(
				Made.Of(
					() => Arg.Of<ILoggerFactory>().CreateLogger(Arg.Index<Type>(0)),
					request => request.Parent.ImplementationType
				)
			);

			container.Register<INHibernateInstaller, NHibInstaller>(Reuse.Singleton);
		}
	}
}

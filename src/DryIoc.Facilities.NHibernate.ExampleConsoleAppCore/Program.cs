using System;
using System.Threading.Tasks;
using DryIoc.Facilities.AutoTx.Extensions;
using DryIoc.Facilities.NHibernate.ExampleConsoleApp.IoC;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NLog;
using NLog.Extensions.Hosting;
using Environment = System.Environment;
using Logger = DryIoc.Facilities.NHibernate.ExampleConsoleApp.Repository.Logger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace DryIoc.Facilities.NHibernate.ExampleConsoleApp
{
	internal class Program
	{
		private static async Task Main(string[] args)
		{
			var builder = new HostBuilder()
				.ConfigureAppConfiguration((hostContext, configuration) =>
				{
					var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
				})
				.UseServiceProviderFactory(new DryIocServiceProviderFactory<Container>())
				.ConfigureContainer<Container>((hostContext, container) =>
				{
					container.RegisterAllComponents(hostContext.Configuration);

					container.AddAutoTx();
					container.AddNHibernate();
				})
				.ConfigureLogging(logging =>
				{
					logging.ClearProviders();
					logging.SetMinimumLevel(LogLevel.Trace);
				})
				.UseNLog();

			var app = builder.UseConsoleLifetime().Build();

			Configure(app);

			await app.RunAsync();

			var scope = (Logger) app.Services.GetService(typeof(Logger));
			scope.WriteToLog(string.Format("{0} - Stopped", DateTime.UtcNow));

			foreach (var target in LogManager.Configuration.AllTargets)
				target.Dispose();
		}

		private static void Configure(IHost host)
		{
			var scope = (Logger) host.Services.GetService(typeof(Logger));
			var up = (Configuration) host.Services.GetService(typeof(Configuration));

			new SchemaUpdate(up).Execute(false, true);

			Console.WriteLine("Current log contents:");
			Console.WriteLine("[utc date] - [text]");
			Console.WriteLine("-------------------");
			scope.ReadLog(Console.WriteLine);
			scope.WriteToLog(string.Format("{0} - Started", DateTime.UtcNow));
		}
	}
}

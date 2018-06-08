// Copyright 2004-2012 Castle Project, Henrik Feldt &contributors - https://github.com/castleproject
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

using System.Diagnostics;
using Castle.Transactions;
using Castle.Transactions.Activities;
using DryIoc;
using DryIoc.Facilities.AutoTx.Abstraction;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Castle.Facilities.AutoTx
{
	///<summary>
	///  <para>A facility for automatically handling transactions using the lightweight
	///    transaction manager. This facility does not depend on
	///    any other facilities.</para> <para>Install the facility in your container with
	///                                   <code>c.AddFacility&lt;AutoTxFacility&gt;</code>
	///                                 </para>
	///</summary>
	public class AutoTxFacility : IFacility
	{
		public void Init(IContainer container)
		{
			ILogger _Logger = NullLogger.Instance;

			// check we have a logger factory
			if (container.IsRegistered(typeof (ILoggerFactory)))
			{
				// get logger factory
				var loggerFactory = container.Resolve<ILoggerFactory>();
				// get logger
				_Logger = loggerFactory.CreateLogger(typeof (AutoTxFacility));
			}

			if (_Logger.IsEnabled(LogLevel.Debug))
				_Logger.LogDebug("initializing AutoTxFacility");

			if (!container.IsRegistered(typeof(ILogger)))
			{
				Trace.TraceWarning("Missing ILogger in container; add it or you'll have no logging of errors!");
				container.UseInstance(typeof(ILogger), NullLogger.Instance);
			}

			// add capability to inject info about requested service to the constructor
			container.Register(Made.Of(
				() => new ServiceRequestInfo(Arg.Index<RequestInfo>(0)), 
				request => request));

			// the interceptor needs to be created for every method call
			container.Register<TransactionInterceptor>(Reuse.Transient);
			container.Register<ITransactionMetaInfoStore, TransactionClassMetaInfoStore>(Reuse.Singleton);
			container.RegisterMany(new[] {typeof(ITransactionManager), typeof(TransactionManager)}, typeof(TransactionManager), Reuse.Singleton);

			// the activity manager shouldn't have the same lifestyle as TransactionInterceptor, as it
			// calls a static .Net/Mono framework method, and it's the responsibility of
			// that framework method to keep track of the call context.
			container.Register<IActivityManager, ThreadLocalActivityManager>(Reuse.Singleton);

			//container.Register<IDirectoryAdapter, DirectoryAdapter>(Reuse.PerTransaction);
			//container.Register<IFileAdapter, FileAdapter>(Reuse.PerTransaction);
			//container.Register<IMapPath, MapPathImpl>(Reuse.Transient);
			
			var componentInspector = new TransactionalComponentInspector();

			//container.ComponentModelBuilder.AddContributor(componentInspector);

			_Logger.LogDebug(
				"inspecting previously registered components; this might throw if you have configured your components in the wrong way");

			//((INamingSubSystem) container.GetSubSystem(SubSystemConstants.NamingKey))
			//	.GetAllHandlers()
			//	.Do(x => componentInspector.ProcessModel(container, x.ComponentModel))
			//	.Run();

			foreach (var serviceRegistrationInfo in container.GetServiceRegistrations())
			{
				componentInspector.ProcessModel(container, serviceRegistrationInfo);
			}

			_Logger.LogDebug(
				@"Initialized AutoTxFacility:

If you are experiencing problems, go to https://github.com/castleproject/Castle.Transactions and file a ticket for the Transactions project.
You can enable verbose logging for .Net by adding this to you .config file:

	<system.diagnostics>
		<sources>
			<source name=""System.Transactions"" switchValue=""Information"">
				<listeners>
					<add name=""tx"" type=""Castle.Transactions.Logging.TraceListener, Castle.Transactions""/>
				</listeners>
			</source>
		</sources>
	</system.diagnostics>

If you wish to e.g. roll back a transaction from within a transactional method you can resolve/use the ITransactionManager's
CurrentTransaction property and invoke Rollback on it. Be ready to catch TransactionAbortedException from the caller. You can enable
debugging through log4net.
");
		}
	}
}
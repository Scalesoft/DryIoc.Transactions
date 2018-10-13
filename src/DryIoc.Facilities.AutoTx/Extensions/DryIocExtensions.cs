using System;
using DryIoc.Transactions;
using Microsoft.Extensions.Logging;

namespace DryIoc.Facilities.AutoTx.Extensions
{
    public static class DryIocExtensions
    {
		public static void AddAutoTx(this IContainer container, AmbientTransactionOption ambientTransaction = AmbientTransactionOption.Enabled)
		{
			var autoTxFacility = new AutoTxFacility();
			autoTxFacility.Init(container, ambientTransaction);
	    }

	    public static void AddLoggerResolving(this IContainer container)
	    {
		    container.Register<ILogger>(Made.Of(
			    () => LoggerFactoryExtensions.CreateLogger(Arg.Of<ILoggerFactory>(), Arg.Index<Type>(0)),
			    request => request.Parent.ImplementationType));
		}

		/// <summary>
		/// Method for resetting Activity. Required for example for ASP.NET Core (after using Transaction in Startup class).
		/// </summary>
	    public static void ResetAutoTxActivityContext(this IContainer container)
		{
			container.Resolve<IActivityManager>().ResetActivity();
		}

	    public static void Release(this IContainer container, object instance)
	    {
			// TODO probably not required
	    }
	}
}

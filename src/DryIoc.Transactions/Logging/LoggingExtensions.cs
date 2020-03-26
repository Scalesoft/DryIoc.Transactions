using System;
using DryIoc.Transactions.Helpers;
using Microsoft.Extensions.Logging;

namespace DryIoc.Transactions.Logging
{
    public static class LoggingExtensions
    {
		public static ILogger CreateChildLogger(this ILoggerFactory loggerFactory, string name, Type parentType)
	    {
		    return loggerFactory.CreateLogger($"{TypeNameHelper.GetTypeDisplayName(parentType)}.{name}");
	    }
	}
}

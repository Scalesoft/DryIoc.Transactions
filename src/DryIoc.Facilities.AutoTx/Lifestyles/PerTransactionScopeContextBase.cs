﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using DryIoc.Facilities.AutoTx.Errors;
using DryIoc.Transactions;
using Microsoft.Extensions.Logging;

namespace DryIoc.Facilities.AutoTx.Lifestyles
{
	public abstract class PerTransactionScopeContextBase : IScopeContext
	{
		private readonly object _LockObject = new object();
		private readonly ConcurrentDictionary<string, Scope> _ScopePerTransactionIdStorage;
		private readonly ILogger _Logger;
		protected bool Disposed { get; private set; }

		protected PerTransactionScopeContextBase(ILogger logger)
		{
			_Logger = logger;
			_ScopePerTransactionIdStorage = new ConcurrentDictionary<string, Scope>();
		}

		protected abstract string RootScopeName { get; }

		protected abstract Maybe<ITransaction> GetSemanticTransaction();

		public bool IsCurrentTransaction => GetSemanticTransaction().HasValue;

		public IScope GetCurrentOrDefault()
		{
			return GetCurrentOrDefault(null);
		}

		public IScope GetCurrentOrDefault(Type serviceType)
		{
			if (Disposed)
				throw new ObjectDisposedException("PerTransactionLifestyleManagerBase",
					"You cannot resolve with a disposed lifestyle.");

			if (!IsCurrentTransaction)
			{
				throw new MissingTransactionException(
					$"No transaction in context when trying to instantiate model for resolve type {serviceType}. "
					+ "If you have verified that your call stack contains a method with the [Transaction] attribute, "
					+ "then also make sure that you have registered the AutoTx Facility."
				);
			}

			var transaction = GetSemanticTransaction().Value;

			Contract.Assume(transaction.State != TransactionState.Disposed,
				"because then it would not be active but would have been popped");

			var key = transaction.LocalIdentifier;

			if (_ScopePerTransactionIdStorage.TryGetValue(key, out var scope))
			{
				return scope;
			}

			lock (_LockObject)
			{
				if (_ScopePerTransactionIdStorage.TryGetValue(key, out scope))
				{
					return scope;
				}

				if (_Logger.IsEnabled(LogLevel.Debug))
					_Logger.LogDebug($"Scope for key '{key}' not found in per-tx storage. Creating new Scope instance.");

#pragma warning disable CA2000
				scope = new Scope(name: RootScopeName);
#pragma warning restore CA2000
				_ScopePerTransactionIdStorage.TryAdd(key, scope);

				transaction.Inner.TransactionCompleted += (sender, args) =>
				{
					if (_Logger.IsEnabled(LogLevel.Debug))
						_Logger.LogDebug($"Transaction#{key} completed, disposing whole Scope assigned to this transaction");

					lock (_LockObject)
					{
						var scopeFromStorage = _ScopePerTransactionIdStorage[key];
						if (!Disposed)
						{
							_ScopePerTransactionIdStorage.TryRemove(key, out _);
							scopeFromStorage.Dispose();
						}
					}
				};
			}

			return scope;
		}

		public IScope SetCurrent(SetCurrentScopeHandler setCurrentScope)
		{
			throw new NotSupportedException("Setting the new scope is not supported, because the transaction scope is created automatically.");
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool managed)
		{
			Contract.Ensures(!managed || Disposed);

			if (!managed)
				return;

			if (Disposed)
			{
				if (_Logger.IsEnabled(LogLevel.Information))
				{
					_Logger.LogInformation(
						"repeated call to Dispose. will show stack-trace if logging is in debug mode as the next log line. this method call is idempotent");

					if (_Logger.IsEnabled(LogLevel.Debug))
						_Logger.LogDebug(new StackTrace().ToString());
				}

				return;
			}

			try
			{
				lock (_LockObject)
				{
					if (_ScopePerTransactionIdStorage.Count > 0)
					{
						var items = string.Join(
							$", {Environment.NewLine}",
							_ScopePerTransactionIdStorage
								.Select(x => $"(id: {x.Key}, item: {x.Value.ToString()})")
								.ToArray());

						if (_Logger.IsEnabled(LogLevel.Warning))
							_Logger.LogWarning("Storage contains {0} items! Items: {{ {1} }}",
								_ScopePerTransactionIdStorage.Count,
								items);
					}

					foreach (var scope in _ScopePerTransactionIdStorage.Values)
					{
						scope.Dispose();
					}
					_ScopePerTransactionIdStorage.Clear();
				}
			}
			finally
			{
				Disposed = true;
			}
		}
	}

	public class PerTransactionScopeContext : PerTransactionScopeContextBase
	{
		private readonly ITransactionManager _TransactionManager;
		public const string ScopeContextName = "TransactionScopeContext";

		public PerTransactionScopeContext(ITransactionManager transactionManager, ILogger logger) : base(logger)
		{
			_TransactionManager = transactionManager;
		}

		protected override string RootScopeName => ScopeContextName;

		protected override Maybe<ITransaction> GetSemanticTransaction()
		{
			if (Disposed)
				throw new ObjectDisposedException(nameof(PerTransactionScopeContext),
					"The lifestyle manager is disposed and cannot be used.");

			return _TransactionManager.CurrentTransaction;
		}
	}

	public class PerTopTransactionScopeContext : PerTransactionScopeContextBase
	{
		private readonly ITransactionManager _TransactionManager;
		public const string ScopeContextName = "TopTransactionScopeContext";

		public PerTopTransactionScopeContext(ITransactionManager transactionManager, ILogger logger) : base(logger)
		{
			_TransactionManager = transactionManager;
		}

		protected override string RootScopeName => ScopeContextName;

		protected override Maybe<ITransaction> GetSemanticTransaction()
		{
			if (Disposed)
				throw new ObjectDisposedException(nameof(PerTopTransactionScopeContext),
					"The lifestyle manager is disposed and cannot be used.");

			return _TransactionManager.CurrentTopTransaction;
		}
	}
}

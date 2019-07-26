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
using System.Diagnostics.Contracts;
using System.Transactions;
using DryIoc.Facilities.AutoTx;
using DryIoc.Facilities.NHibernate.Errors;
using DryIoc.Facilities.NHibernate.UnitOfWork;
using DryIoc.Transactions;
using Microsoft.Extensions.Logging;
using NHibernate;

namespace DryIoc.Facilities.NHibernate
{
	using ITransaction = Transactions.ITransaction;

	/// <summary>
	/// 	The session manager is an object wrapper around the "real" manager which is managed
	/// 	by a custom per-transaction lifestyle. If you wish to implement your own manager, you can
	/// 	pass a function to this object at construction time and replace the built-in session manager.
	/// </summary>
	public class SessionManager : ISessionManager
	{
		private readonly Func<ISession> _GetSession;
		private readonly ITransactionManager _TransactionManager;
		private readonly Func<UnitOfWorkStore> _UowStore;
		private readonly AutoTxOptions _AutoTxOptions;
		private readonly ILogger _Logger;

		/// <summary>
		/// 	Constructor.
		/// </summary>
		/// <param name = "getSession"></param>
		/// <param name="transactionManager"></param>
		/// <param name="uowStore"></param>
		/// <param name="autoTxOptions"></param>
		/// <param name="logger"></param>
		public SessionManager(Func<ISession> getSession, ITransactionManager transactionManager, Func<UnitOfWorkStore> uowStore, AutoTxOptions autoTxOptions, ILogger logger)
		{
			Contract.Requires(getSession != null);
			Contract.Ensures(this._GetSession != null);

			_GetSession = getSession;
			_TransactionManager = transactionManager;
			_UowStore = uowStore;
			_AutoTxOptions = autoTxOptions;
			_Logger = logger;
		}

		public ISession OpenSession()
		{
			Maybe<ITransaction> transaction = ObtainCurrentTransaction();

			//This is a new transaction or no transaction is required
			if (!transaction.HasValue)
			{
				var session = _GetSession();

				if (session == null)
					throw new NHibernateFacilityException(
						"The Func<ISession> passed to SessionManager returned a null session. Verify your registration.");

				return session;
			}
			else
			{
				var session = GetStoredSession(transaction.Value);

				//There is an active transaction but no session is created yet
				if (session == null)
				{
					session = _GetSession();

					if (session == null)
						throw new NHibernateFacilityException(
							"The Func<ISession> passed to SessionManager returned a null session. Verify your registration.");

					//Store the session so I can reused
					var uow = StoreSession(transaction.Value, session);

					//Attach to the TransactionEvent for finishing transaction (commit or rollback)
					var unitOfWorkStore = _UowStore.Invoke();
					transaction.Value.Inner.TransactionCompleted += (sender, args) =>
					{
						try
						{
							var removedUow = unitOfWorkStore.GetAndClear(transaction.Value);
							if (removedUow != uow)
							{
								_Logger.LogCritical("Removed different UoW from store than UoW prepared to finish!");
								throw new InvalidOperationException("Removed different UoW from store than UoW prepared to finish!");
							}

							FinishStoredSession(uow, args.Transaction.TransactionInformation.Status);
						}
						catch (HibernateException exception)
						{
							_Logger.LogWarning(exception, "Error in the O-R persistence layer in NHibernate");
							throw;
						}
						catch (Exception exception)
						{
							_Logger.LogWarning(exception, "Unknown error");
							throw;
						}
					};

					return session;
				}
				else
				{
					return session;
				}
			}
		}

		/// <summary>
		/// Gets the current transaction from de AutoTx facility via an ITransactionManager
		/// </summary>
		/// <returns>The current transaction</returns>
		private Maybe<ITransaction> ObtainCurrentTransaction()
		{
			return _TransactionManager.CurrentTopTransaction;
		}

		private IUnitOfWork CreateUnitOfWork(ISession session)
		{
			switch (_AutoTxOptions.AmbientTransaction)
			{
				case AmbientTransactionOption.Enabled:
					return new NHibernateImplicitUnitOfWork(session);
				case AmbientTransactionOption.Disabled:
					return new NHibernateExplicitUnitOfWork(session);
				default:
					throw new ArgumentOutOfRangeException($"Unknown value: {_AutoTxOptions.AmbientTransaction}", (Exception) null);
			}
		}

		/// <summary>
		/// Stores a session
		/// </summary>
		/// <param name="transaction">current transaction</param>
		/// <param name="session">current session</param>
		private IUnitOfWork StoreSession(ITransaction transaction, ISession session)
		{
			var unitOfWork = CreateUnitOfWork(session);
			var uowStore = _UowStore.Invoke();
			uowStore.Add(transaction, unitOfWork);
			return unitOfWork;
		}

		/// <summary>
		/// Retrieves the session
		/// </summary>
		/// <param name="transaction">current transaction</param>
		/// <returns></returns>
		private ISession GetStoredSession(ITransaction transaction)
		{
			var uowStore = _UowStore.Invoke();
			var uow = uowStore.TryGet(transaction);
			return uow?.CurrentSession;
		}

		/// <summary>
		/// Removes the session from store
		/// </summary>
		/// <param name="unitOfWork"></param>
		/// <param name="transactionStatus"></param>
		/// <returns></returns>
#pragma warning disable CA1822
		private void FinishStoredSession(IUnitOfWork unitOfWork, TransactionStatus transactionStatus)
		{
			using (unitOfWork)
			{
				switch (transactionStatus)
				{
					case TransactionStatus.Committed:
						unitOfWork.Commit();
						break;
					case TransactionStatus.Aborted:
					case TransactionStatus.Active:
					case TransactionStatus.InDoubt:
						unitOfWork.Rollback();
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(transactionStatus), transactionStatus, null);
				}
			}
		}
#pragma warning restore CA1822
	}
}

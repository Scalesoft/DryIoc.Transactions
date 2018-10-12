using System;
using System.Collections.Concurrent;
using DryIoc.Facilities.NHibernate.UnitOfWork;
using DryIoc.Transactions;

namespace DryIoc.Facilities.NHibernate
{
	public class UnitOfWorkStore
	{
		private readonly ConcurrentDictionary<string, IUnitOfWork> _Store;

		public UnitOfWorkStore()
		{
			_Store = new ConcurrentDictionary<string, IUnitOfWork>();
		}

		public void Add(ITransaction transaction, IUnitOfWork unitOfWork)
		{
			var success = _Store.TryAdd(transaction.LocalIdentifier, unitOfWork);
			if (!success)
				throw new ArgumentException($"Transaction {transaction.LocalIdentifier} has already assigned UnitOfWork");
		}

		public IUnitOfWork TryGet(ITransaction transaction)
		{
			_Store.TryGetValue(transaction.LocalIdentifier, out var unitOfWork);
			return unitOfWork;
		}

		public IUnitOfWork GetAndClear(ITransaction transaction)
		{
			_Store.TryRemove(transaction.LocalIdentifier, out var unitOfWork);
			return unitOfWork;
		}
	}
}
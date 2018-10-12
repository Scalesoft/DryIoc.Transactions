using System.Collections.Generic;
using DryIoc.Facilities.NHibernate.UnitOfWork;
using DryIoc.Transactions;

namespace DryIoc.Facilities.NHibernate
{
	public class UnitOfWorkStore
	{
		private readonly Dictionary<string, IUnitOfWork> _Store;

		public UnitOfWorkStore()
		{
			_Store = new Dictionary<string, IUnitOfWork>();
		}

		public void Add(ITransaction transaction, IUnitOfWork unitOfWork)
		{
			_Store.Add(transaction.LocalIdentifier, unitOfWork);
		}

		public IUnitOfWork TryGet(ITransaction transaction)
		{
			_Store.TryGetValue(transaction.LocalIdentifier, out var unitOfWork);
			return unitOfWork;
		}

		public IUnitOfWork GetAndClear(ITransaction transaction)
		{
			var unitOfWork = _Store[transaction.LocalIdentifier];
			_Store.Remove(transaction.LocalIdentifier);
			return unitOfWork;
		}
	}
}
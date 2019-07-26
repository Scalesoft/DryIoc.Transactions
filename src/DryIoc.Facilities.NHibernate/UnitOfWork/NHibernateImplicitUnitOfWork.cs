using NHibernate;

namespace DryIoc.Facilities.NHibernate.UnitOfWork
{
	public sealed class NHibernateImplicitUnitOfWork : IUnitOfWork
	{
		public NHibernateImplicitUnitOfWork(ISession session)
		{
			CurrentSession = session;
		}

		public ISession CurrentSession { get; }

		public void Dispose()
		{
			CurrentSession.Dispose();
		}

		public void Commit()
		{
		}

		public void Rollback()
		{
		}
	}
}

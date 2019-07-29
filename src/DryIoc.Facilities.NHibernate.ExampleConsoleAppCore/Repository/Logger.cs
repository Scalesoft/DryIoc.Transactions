using System;
using DryIoc.Transactions;
using NHibernate;

namespace DryIoc.Facilities.NHibernate.ExampleConsoleApp.Repository
{
	public class Logger
	{
		private readonly ISessionFactory _SessionFactory;

		public Logger(
			ISessionFactory sessionFactory
		)
		{
			_SessionFactory = sessionFactory;
		}

		[Transaction]
		public virtual void WriteToLog(string text)
		{
			using (var s = _SessionFactory.OpenSession())
				s.Save(new LogLine(text));
		}

		[Transaction]
		public virtual void ReadLog(Action<string> reader)
		{
			using (var s = _SessionFactory.OpenSession())
				foreach (var line in s.CreateCriteria<LogLine>().List<LogLine>())
					reader(line.Line);
		}
	}
}

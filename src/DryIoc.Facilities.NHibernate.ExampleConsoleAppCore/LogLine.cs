using System;

namespace DryIoc.Facilities.NHibernate.ExampleConsoleApp
{
	[Serializable]
	public class LogLine
	{
		/// <summary>
		/// Gets the log-line.
		/// </summary>
		public virtual string Line { get; protected set; }

		/// <summary>
		/// Gets the ID of the line in the log.
		/// </summary>
		public virtual Guid Id { get; protected set; }

		protected LogLine()
		{
		}

		public LogLine(string line)
		{
			Line = line;
		}
	}
}

using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace DryIoc.Facilities.NHibernate.ExampleConsoleApp.EntityMapping
{
	public class LogLineMap : ClassMapping<LogLine>, IMapping
	{
		public LogLineMap()
		{
			Id(x => x.Id, map => map.Generator(Generators.Guid));
			Property(x => x.Line, opt => { opt.Column("`val`"); });
		}
	}
}

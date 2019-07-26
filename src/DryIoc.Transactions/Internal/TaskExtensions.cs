using System.Threading;
using System.Threading.Tasks;

namespace DryIoc.Transactions.Internal
{
	// reference: http://blogs.msdn.com/b/pfxteam/archive/2009/05/31/9674669.aspx
	public static class TaskExtensions
	{
		public static Task IgnoreExceptions(this Task task)
		{
			task.ContinueWith(
				c =>
				{
					var ignored = c.Exception;
				},
				CancellationToken.None,
				TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
				TaskScheduler.Default
			);
			return task;
		}
	}
}

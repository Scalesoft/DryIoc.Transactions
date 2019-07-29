using System;
using System.Reflection;
using DryIoc.Facilities.AutoTx.Extensions;
using FastExpressionCompiler.LightExpression;

namespace DryIoc.Facilities.AutoTx.Lifestyles
{
	public abstract class PerTransactionReuseBase : IReuse
	{
		private readonly PerTransactionScopeContextBase _PerTransactionScopeContextBase;

		public PerTransactionReuseBase(PerTransactionScopeContextBase perTransactionScopeContextBase)
		{
			_PerTransactionScopeContextBase = perTransactionScopeContextBase;
		}

		public abstract int Lifespan { get; }

		public object Name => null;

		/// <summary>Returns item from transaction scope.</summary>
		/// <param name="scopeContext">Transaction scope context to select from.</param>
		/// <param name="request">Container Request info for resolving service</param>
		/// <param name="itemId">Scoped item ID for lookup.</param>
		/// <param name="createValue">Delegate for creating the item.</param>
		/// <returns>Reused item.</returns>
		public static object GetOrAddItemOrDefault(PerTransactionScopeContextBase scopeContext, Request request, int itemId, CreateScopedValue createValue)
		{
			if (scopeContext == null)
			{
				throw new ArgumentNullException(nameof(scopeContext));
			}
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

#pragma warning disable CA2000
			var scope = scopeContext.GetCurrentOrDefault(request.ServiceType);
#pragma warning restore CA2000
			return scope?.GetOrAdd(itemId, createValue);
		}

		private static readonly MethodInfo _GetOrAddOrDefaultMethod =
			typeof(PerTransactionReuseBase).GetSingleMethodOrNull("GetOrAddItemOrDefault");

		/// <summary>Returns expression call to <see cref="GetOrAddItemOrDefault"/>.</summary>
		public Expression Apply(Request request, Expression serviceFactoryExpr)
		{
			if (request == null)
			{
				throw new ArgumentNullException(nameof(request));
			}

			var itemId = request.TracksTransientDisposable ? -1 : request.FactoryID;

			return Expression.Call(_GetOrAddOrDefaultMethod,
				Expression.Constant(_PerTransactionScopeContextBase),
				Expression.Constant(request),
				Expression.Constant(itemId),
				Expression.Lambda<CreateScopedValue>(serviceFactoryExpr));
		}

		public bool CanApply(Request request)
		{
			return _PerTransactionScopeContextBase.IsCurrentTransaction;
		}

		public abstract Expression ToExpression(Func<object, Expression> fallbackConverter);
	}

	public class PerTransactionReuse : PerTransactionReuseBase
	{
		public PerTransactionReuse(PerTransactionScopeContext perTransactionScopeContext) : base(perTransactionScopeContext)
		{
		}

		public override int Lifespan => 50;

		public static readonly Lazy<Expression> PerTransactionReuseExpr = new Lazy<Expression>(() =>
			Expression.Property(null, typeof(AutoTxReuse).GetPropertyOrNull("PerTransaction")));

		public override Expression ToExpression(Func<object, Expression> fallbackConverter)
		{
			return PerTransactionReuseExpr.Value;
		}
	}

	public class PerTopTransactionReuse : PerTransactionReuseBase
	{
		public PerTopTransactionReuse(PerTopTransactionScopeContext perTopTransactionScopeContext) : base(perTopTransactionScopeContext)
		{
		}

		public override int Lifespan => 55;

		public static readonly Lazy<Expression> PerTopTransactionReuseExpr = new Lazy<Expression>(() =>
			Expression.Property(null, typeof(AutoTxReuse).GetPropertyOrNull("PerTopTransaction")));

		public override Expression ToExpression(Func<object, Expression> fallbackConverter)
		{
			return PerTopTransactionReuseExpr.Value;
		}
	}
}

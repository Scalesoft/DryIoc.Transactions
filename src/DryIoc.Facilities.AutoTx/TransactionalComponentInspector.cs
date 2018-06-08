﻿#region license

// Copyright 2004-2012 Castle Project, Henrik Feldt &contributors - https://github.com/castleproject
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

#endregion

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Castle.Transactions;
using DryIoc;
using DryIoc.Facilities.AutoTx.Errors;
using DryIoc.Facilities.AutoTx.Extensions;

namespace Castle.Facilities.AutoTx
{
	/// <summary>
	/// 	Transaction component inspector that selects the methods
	/// 	available to get intercepted with transactions.
	/// </summary>
	internal class TransactionalComponentInspector// : MethodMetaInspector
	{
		private ITransactionMetaInfoStore _MetaStore;

		public void ProcessModel(IContainer container, ServiceRegistrationInfo model)
		{
			if (_MetaStore == null)
				_MetaStore = container.Resolve<ITransactionMetaInfoStore>();

			Contract.Assume(model.Factory.ImplementationType != null);

			Validate(model);
			AddInterceptor(container, model);
		}

		private void Validate(ServiceRegistrationInfo model)
		{
			Contract.Requires(model.Factory.ImplementationType != null);
			Contract.Ensures(model.Factory.ImplementationType != null);

			Maybe<TransactionalClassMetaInfo> meta;
			List<string> problematicMethods;
			if (model.ServiceType == null
			    || model.ServiceType.IsInterface
			    || !(meta = _MetaStore.GetMetaFromType(model.Factory.ImplementationType)).HasValue
			    || (problematicMethods = (from method in meta.Value.TransactionalMethods
			                              where !method.IsVirtual
			                              select method.Name).ToList()).Count == 0)
				return;

			throw new AutoTxFacilityException(string.Format("The class {0} wants to use transaction interception, " +
			                                          "however the methods must be marked as virtual in order to do so. Please correct " +
			                                          "the following methods: {1}", model.Factory.ImplementationType.FullName,
			                                          string.Join(", ", problematicMethods.ToArray())));
		}

		private void AddInterceptor(IContainer container, ServiceRegistrationInfo model)
		{
			Contract.Requires(model.Factory.ImplementationType != null);
			var meta = _MetaStore.GetMetaFromType(model.Factory.ImplementationType);

			if (!meta.HasValue)
				return;

			//TODO remove this old Interceptor registration:
			//model.Dependencies.Add(new DependencyModel(null, typeof (TransactionInterceptor), false));
			//model.Interceptors.Add(new InterceptorReference(typeof (TransactionInterceptor)));
			
			container.Intercept<TransactionInterceptor>(model.ServiceType, model.Factory.ImplementationType);
		}

		[Pure]
		protected string ObtainNodeName()
		{
			return "transaction-interceptor";
		}
	}
}
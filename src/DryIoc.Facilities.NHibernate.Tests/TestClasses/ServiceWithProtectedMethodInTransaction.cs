﻿// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
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
using DryIoc.Transactions;
using NUnit.Framework;

namespace DryIoc.Facilities.NHibernate.Tests.TestClasses
{
	public class ServiceWithProtectedMethodInTransaction
	{
		private readonly ISessionManager _SessionManager;

		public ServiceWithProtectedMethodInTransaction(ISessionManager sessionManager)
		{
			_SessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
		}

		public void Do()
		{
			var id = SaveIt();
			ReadAgain(id);
		}

		protected void ReadAgain(Guid id)
		{
			using (var s = _SessionManager.OpenSession())
			{
				var t = s.Load<Thing>(id);
				Assert.That(t.Id, Is.EqualTo(id));
			}
		}

		[Transaction]
		protected virtual Guid SaveIt()
		{
			var session = _SessionManager.OpenSession();
			return (Guid)session.Save(new Thing(45.0));
		}
	}
}
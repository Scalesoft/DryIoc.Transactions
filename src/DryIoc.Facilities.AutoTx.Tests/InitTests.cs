#region license

// Copyright 2009-2011 Henrik Feldt - http://logibit.se/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using DryIoc.Facilities.AutoTx.Errors;
using DryIoc.Facilities.AutoTx.Extensions;
using DryIoc.Transactions;
using NUnit.Framework;

namespace DryIoc.Facilities.AutoTx.Tests
{
	public class InitTests
	{
		[Test]
		public void Cannot_Register_Class_Without_Virtual_Method()
		{
			var c = new Container();
			c.Register<FaultyComponent>(Reuse.Singleton);
			
			try
			{
				c.AddAutoTx();
				Assert.Fail("invalid component registration should be noted.");
			}
			catch (AutoTxFacilityException ex)
			{
				Assert.That(ex.Message.Contains("FaultyMethod"));
				Assert.That(ex.Message.Contains("virtual"));
			}
		}

		internal class FaultyComponent
		{
			[Transaction]
			public void FaultyMethod()
			{
			}
		}
	}
}
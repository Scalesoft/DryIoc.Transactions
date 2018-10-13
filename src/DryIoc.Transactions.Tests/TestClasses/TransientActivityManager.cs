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

using DryIoc.Transactions.Activities;
using Microsoft.Extensions.Logging;

namespace DryIoc.Transactions.Tests.TestClasses
{
	public class TransientActivityManager : IActivityManager
	{
		readonly Activity activity;

		public TransientActivityManager(ILogger logger)
		{
			activity = new Activity(logger);
		}

		/// <summary>
		///   Gets the current activity.
		/// </summary>
		/// <value> The current activity. </value>
		Activity IActivityManager.GetCurrentActivity()
		{
			return activity;
		}

		public void CreateNewActivity()
		{
		}

		public void ResetActivity()
		{
		}
	}
}
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

namespace DryIoc.Facilities.EFCore
{
	/// <summary>
	/// 	Specifies the default DbContext management strategy.
	/// </summary>
	public enum DefaultLifeStyleOption : uint // internally, this uint corresponds to the order in which components are registered
	{
		/// <summary>
		/// 	Specifies that DbContext should be opened and closed per transaction. This has the semantics
		/// 	that the DbContext is kept per top transaction, unless the dependent transaction is forked, in
		/// 	which case, a new DbContext is resolved to avoid sharing the DbContext accross threads.
		/// </summary>
		PerTransaction = 0,

		/// <summary>
		/// 	Specifies that DbContext should be opened and closed per web request.
		/// </summary>
		PerWebRequest = 1,

		/// <summary>
		/// 	Specifies that the DbContext should be transiently registered.
		/// </summary>
		Transient = 2
	}
}
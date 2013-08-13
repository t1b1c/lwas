/*
 * Copyright 2006-2013 TIBIC SOLUTIONS
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;

namespace LWAS.Extensible.Interfaces.WebParts
{
	public interface ITemplatingItem
	{
		bool IsNew
		{
			get;
			set;
		}
		bool IsReadOnly
		{
			get;
			set;
		}
		bool IsCurrent
		{
			get;
			set;
		}
		bool IsGrouping
		{
			get;
			set;
		}
		bool IsTotals
		{
			get;
			set;
		}
		bool HasChanges
		{
			get;
			set;
		}
		bool IsValid
		{
			get;
			set;
		}
		object Data
		{
			get;
			set;
		}
		string InvalidMember
		{
			get;
			set;
		}
		IDictionary<string, List<BaseDataBoundControl>> BoundControls
		{
			get;
		}
	}
}

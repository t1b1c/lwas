/*
 * Copyright 2006-2015 TIBIC SOLUTIONS
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
using System.Web.UI.WebControls;

namespace LWAS.Extensible.Interfaces.WebParts
{
	public interface ITemplatable
	{
		TableStyle SelectorsStyle
		{
			get;
			set;
		}
		TableStyle CommandersStyle
		{
			get;
			set;
		}
		TableStyle HeaderStyle
		{
			get;
			set;
		}
		TableStyle FilterStyle
		{
			get;
			set;
		}
		TableStyle GroupingStyle
		{
			get;
			set;
		}
		TableStyle TotalsStyle
		{
			get;
			set;
		}
		TableStyle DetailsStyle
		{
			get;
			set;
		}
		TableStyle FooterStyle
		{
			get;
			set;
		}
		TableItemStyle SelectorsRowStyle
		{
			get;
			set;
		}
		TableItemStyle CommandersRowStyle
		{
			get;
			set;
		}
		TableItemStyle FilterRowStyle
		{
			get;
			set;
		}
		TableItemStyle HeaderRowStyle
		{
			get;
			set;
		}
		TableItemStyle GroupRowStyle
		{
			get;
			set;
		}
		TableItemStyle TotalsRowStyle
		{
			get;
			set;
		}
		TableItemStyle RowStyle
		{
			get;
			set;
		}
		TableItemStyle EditRowStyle
		{
			get;
			set;
		}
		TableItemStyle SelectedRowStyle
		{
			get;
			set;
		}
		TableItemStyle AlternatingRowStyle
		{
			get;
			set;
		}
		TableItemStyle FooterRowStyle
		{
			get;
			set;
		}
		TableItemStyle InvalidItemStyle
		{
			get;
			set;
		}
	}
}

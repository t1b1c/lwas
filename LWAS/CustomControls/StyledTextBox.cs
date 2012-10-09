/*
 * Copyright 2006-2012 TIBIC SOLUTIONS
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
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LWAS.CustomControls
{
	[Themeable(true)]
	public class StyledTextBox : TextBox
	{
		private Style _normalStyle = new Style();
		private Style _readOnlyStyle = new Style();
		[Themeable(true)]
		public Style NormalStyle
		{
			get
			{
				return this._normalStyle;
			}
			set
			{
				this._normalStyle = value;
				if (!this.ReadOnly)
				{
					base.MergeStyle(this._normalStyle);
				}
			}
		}
		[Themeable(true)]
		public Style ReadOnlyStyle
		{
			get
			{
				return this._readOnlyStyle;
			}
			set
			{
				this._readOnlyStyle = value;
				if (this.ReadOnly)
				{
					base.MergeStyle(this._readOnlyStyle);
				}
			}
		}
		public override bool ReadOnly
		{
			get
			{
				return base.ReadOnly;
			}
			set
			{
				base.ReadOnly = value;
				if (base.ReadOnly)
				{
					base.MergeStyle(this._readOnlyStyle);
				}
				else
				{
					base.MergeStyle(this._normalStyle);
				}
			}
		}
	}
}

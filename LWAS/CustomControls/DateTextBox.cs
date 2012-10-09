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
using System.Globalization;
using System.Web.UI;

namespace LWAS.CustomControls
{
	[Themeable(true)]
	public class DateTextBox : StyledTextBox
	{
		private DateTimeFormatInfo _info = new DateTimeFormatInfo();
		private DateTime? _date;
		private bool _hasTime = false;
		[Themeable(true)]
		public DateTimeFormatInfo Info
		{
			get
			{
				return this._info;
			}
			set
			{
				this._info = value;
			}
		}
		public object Date
		{
			get
			{
				object result;
				if (this._date.HasValue)
				{
					result = this._date.Value;
				}
				else
				{
					result = null;
				}
				return result;
			}
			set
			{
				if (null != value)
				{
					this.Text = value.ToString();
				}
				else
				{
					this._date = null;
				}
			}
		}
		[Themeable(true)]
		public bool HasTime
		{
			get
			{
				return this._hasTime;
			}
			set
			{
				this._hasTime = value;
			}
		}
		public override string Text
		{
			get
			{
				string result;
				if (this._date.HasValue)
				{
					if (this._hasTime)
					{
						DateTime value = this._date.Value;
						result = value.ToString("D", this._info);
					}
					else
					{
						DateTime value = this._date.Value;
						result = value.ToString("d", this._info);
					}
				}
				else
				{
					result = base.Text;
				}
				return result;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					DateTime date;
					if (DateTime.TryParse(value, this._info, DateTimeStyles.None, out date))
					{
						this._date = new DateTime?(date);
					}
					else
					{
						base.Text = value;
						this._date = null;
					}
				}
				else
				{
					base.Text = value;
					this._date = null;
				}
			}
		}
	}
}

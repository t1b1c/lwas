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
using System.Collections.Specialized;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LWAS.CustomControls
{
	public class DateBoundField : BoundField
	{
		private HiddenField valueHidden;
		public DateTime Value
		{
			get
			{
				DateTime ret = default(DateTime);
				if (!string.IsNullOrEmpty(this.valueHidden.Value))
				{
					DateTime.TryParse(this.valueHidden.Value, out ret);
				}
				return ret;
			}
			set
			{
				this.valueHidden.Value = value.ToString();
			}
		}
		public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
		{
			base.InitializeCell(cell, cellType, rowState, rowIndex);
			if (cellType == DataControlCellType.DataCell)
			{
				this.valueHidden = new HiddenField();
				cell.Controls.Add(this.valueHidden);
			}
		}
		protected override void OnDataBindField(object sender, EventArgs e)
		{
			try
			{
				base.OnDataBindField(sender, e);
			}
			catch
			{
			}
			object dataValue = this.GetValue(((Control)sender).NamingContainer);
			if (dataValue != null && !string.IsNullOrEmpty(dataValue.ToString()))
			{
				this.Value = (DateTime)dataValue;
			}
		}
		protected override string FormatDataValue(object dataValue, bool encode)
		{
			string format = this.DataFormatString;
			if (string.IsNullOrEmpty(format))
			{
				format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
			}
			string result;
			if (dataValue != null && !string.IsNullOrEmpty(dataValue.ToString()))
			{
				result = ((DateTime)dataValue).ToString(format);
			}
			else
			{
				result = null;
			}
			return result;
		}
		public override void ExtractValuesFromCell(IOrderedDictionary dictionary, DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly)
		{
			string dataField = this.DataField;
			if (dictionary.Contains(dataField))
			{
				dictionary[dataField] = this.Value;
			}
			else
			{
				dictionary.Add(dataField, this.Value);
			}
		}
	}
}

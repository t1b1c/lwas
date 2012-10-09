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
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace LWAS.CustomControls
{
	public class DropDownListField : BoundField
	{
		public string DataSourceID;
		public object DataSource;
		public string DataTextField;
		public string DataValueField;
		public override void ExtractValuesFromCell(IOrderedDictionary dictionary, DataControlFieldCell cell, DataControlRowState rowState, bool includeReadOnly)
		{
			if (cell.Controls.Count > 0)
			{
				DropDownList ddl = cell.Controls[0] as DropDownList;
				if (null == ddl)
				{
					throw new InvalidOperationException("DropDownField could not extract control.");
				}
				object selectedValue = ddl.SelectedValue;
				if (dictionary.Contains(this.DataField))
				{
					dictionary[this.DataField] = selectedValue;
				}
				else
				{
					dictionary.Add(this.DataField, selectedValue);
				}
			}
		}
		protected override void InitializeDataCell(DataControlFieldCell cell, DataControlRowState rowState)
		{
			DropDownList ddl = new DropDownList();
			ddl.Items.Add("");
			ddl.AppendDataBoundItems = true;
			if (!string.IsNullOrEmpty(this.DataSourceID) || null != this.DataSource)
			{
				if (!string.IsNullOrEmpty(this.DataSourceID))
				{
					ddl.DataSourceID = this.DataSourceID;
				}
				else
				{
					ddl.DataSource = this.DataSource;
				}
				ddl.DataTextField = this.DataTextField;
				ddl.DataValueField = this.DataValueField;
			}
			if (this.DataField.Length != 0)
			{
				ddl.DataBound += new EventHandler(this.OnDataBindField);
			}
			ddl.Enabled = false;
			if ((rowState & DataControlRowState.Edit) != DataControlRowState.Normal || (rowState & DataControlRowState.Insert) != DataControlRowState.Normal)
			{
				ddl.Enabled = true;
			}
			cell.Controls.Add(ddl);
		}
		protected override void OnDataBindField(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(this.DataField))
			{
				DropDownList ddl = (DropDownList)sender;
				IDataItemContainer container = ddl.NamingContainer as IDataItemContainer;
				DataRowView drv = null;
				if (container != null)
				{
					drv = (container.DataItem as DataRowView);
				}
				if (drv != null && null != ddl.Items.FindByValue(drv[this.DataField].ToString()))
				{
					ddl.SelectedValue = drv[this.DataField].ToString();
				}
			}
		}
		protected override DataControlField CreateField()
		{
			return base.CreateField();
		}
	}
}

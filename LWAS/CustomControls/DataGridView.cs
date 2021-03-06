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
using System.Collections.Specialized;
using System.Web.UI.WebControls;

namespace LWAS.CustomControls
{
	public class DataGridView : GridView
	{
		public IOrderedDictionary GetSelectedRowData()
		{
			IOrderedDictionary result;
			if (null == this.SelectedRow)
			{
				result = null;
			}
			else
			{
				OrderedDictionary fieldValues = new OrderedDictionary();
				foreach (object field in this.CreateColumns(null, false))
				{
					if (field is BoundField && !fieldValues.Contains(((BoundField)field).DataField))
					{
						fieldValues.Add(((BoundField)field).DataField, null);
					}
				}
				string[] dataKeyNames = this.DataKeyNames;
				for (int i = 0; i < dataKeyNames.Length; i++)
				{
					string key = dataKeyNames[i];
					if (!fieldValues.Contains(key))
					{
						fieldValues.Add(key, null);
					}
				}
				this.ExtractRowValues(fieldValues, this.SelectedRow, true, true);
				result = fieldValues;
			}
			return result;
		}
		public void Empty()
		{
			object dataSource = this.DataSource;
			this.DataSource = null;
			this.DataBind();
			this.DataSource = dataSource;
		}
	}
}

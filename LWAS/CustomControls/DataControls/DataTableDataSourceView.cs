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
using System.Collections;
using System.Data;
using System.Web.UI;

namespace LWAS.CustomControls.DataControls
{
	public class DataTableDataSourceView : DataSourceView
	{
		public static string DefaultViewName = "DefaultView";
		private DataTable dataTable = null;
		private string filter = "";
		internal DataTable DataTable
		{
			get
			{
				return this.dataTable;
			}
			set
			{
				this.dataTable = value;
			}
		}
		internal string Filter
		{
			get
			{
				return this.filter;
			}
			set
			{
				this.filter = value;
			}
		}
		public override bool CanDelete
		{
			get
			{
				return true;
			}
		}
		public override bool CanInsert
		{
			get
			{
				return true;
			}
		}
		public override bool CanUpdate
		{
			get
			{
				return true;
			}
		}
		public DataTableDataSourceView(IDataSource owner, string name) : base(owner, DataTableDataSourceView.DefaultViewName)
		{
		}
		protected override IEnumerable ExecuteSelect(DataSourceSelectArguments selectArgs)
		{
			DataView dataView = new DataView(this.DataTable);
			if (selectArgs.SortExpression != string.Empty)
			{
				dataView.Sort = selectArgs.SortExpression;
			}
			if (!string.IsNullOrEmpty(this.Filter))
			{
				dataView.RowFilter = this.Filter;
			}
			return dataView;
		}
		protected override int ExecuteDelete(IDictionary keys, IDictionary values)
		{
			object[] arrKeys = new object[keys.Count];
			keys.Values.CopyTo(arrKeys, 0);
			if (this.DataTable.Rows.Find(arrKeys) != null)
			{
				this.DataTable.Rows.Remove(this.DataTable.Rows.Find(arrKeys));
			}
			return 1;
		}
		protected override int ExecuteInsert(IDictionary values)
		{
			DataRow row = this.DataTable.NewRow();
			foreach (object key in values.Keys)
			{
				row[key.ToString()] = values[key];
			}
			this.DataTable.Rows.Add(row);
			return 1;
		}
		protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			int result;
			if (0 == keys.Count)
			{
				result = 0;
			}
			else
			{
				object[] arrKeys = new object[keys.Count];
				keys.Values.CopyTo(arrKeys, 0);
				DataRow row = this.DataTable.Rows.Find(arrKeys);
				foreach (object key in values.Keys)
				{
					row[key.ToString()] = values[key];
				}
				result = 1;
			}
			return result;
		}
	}
}

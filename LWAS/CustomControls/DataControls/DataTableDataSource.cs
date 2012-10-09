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
using System.Collections;
using System.Data;
using System.Web.UI;

namespace LWAS.CustomControls.DataControls
{
	public class DataTableDataSource : DataSourceControl
	{
		private DataTableDataSourceView view = null;
		public DataTable DataTable
		{
			get
			{
				return ((DataTableDataSourceView)this.GetView(string.Empty)).DataTable;
			}
			set
			{
				if (((DataTableDataSourceView)this.GetView(string.Empty)).DataTable != value)
				{
					((DataTableDataSourceView)this.GetView(string.Empty)).DataTable = value;
					this.RaiseDataSourceChangedEvent(EventArgs.Empty);
				}
			}
		}
		public string Filter
		{
			get
			{
				return ((DataTableDataSourceView)this.GetView(string.Empty)).Filter;
			}
			set
			{
				if (((DataTableDataSourceView)this.GetView(string.Empty)).Filter != value)
				{
					((DataTableDataSourceView)this.GetView(string.Empty)).Filter = value;
					this.RaiseDataSourceChangedEvent(EventArgs.Empty);
				}
			}
		}
		protected override DataSourceView GetView(string viewName)
		{
			if (null == this.view)
			{
				this.view = new DataTableDataSourceView(this, string.Empty);
			}
			return this.view;
		}
		protected override ICollection GetViewNames()
		{
			return new ArrayList(1)
			{
				DataTableDataSourceView.DefaultViewName
			};
		}
	}
}

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
using System.Collections;
using System.Data;
using System.Web.UI;

namespace LWAS.WebParts.Editors
{
	public class RoleUsersView : CRUDDataSourceView
	{
		private RolesDataSource source;
		private string role;
		public RoleUsersView(IDataSource owner, string viewName) : base(owner, viewName)
		{
			this.source = (owner as RolesDataSource);
			this.role = viewName;
		}
		protected override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments)
		{
			IEnumerable result;
			if (null == this.source)
			{
				result = null;
			}
			else
			{
				DataTable table = new DataTable();
				table.Columns.Add("Key");
				table.Columns.Add("Name");
				foreach (string user in this.source.ListUsers(this.role))
				{
					table.Rows.Add(new object[]
					{
						user, 
						user
					});
				}
				result = table.Rows;
			}
			return result;
		}
		protected override int ExecuteInsert(IDictionary values)
		{
			int result;
			if (null == this.source)
			{
				result = 0;
			}
			else
			{
				this.source.CreateUser(this.role, values["Name"].ToString());
				result = 1;
			}
			return result;
		}
		protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			int result;
			if (null == this.source)
			{
				result = 0;
			}
			else
			{
				this.source.RenameUser(this.role, values["Key"].ToString(), values["Name"].ToString());
				result = 1;
			}
			return result;
		}
		protected override int ExecuteDelete(IDictionary keys, IDictionary oldValues)
		{
			int result;
			if (null == this.source)
			{
				result = 0;
			}
			else
			{
				this.source.DeleteUser(this.role, keys["Key"].ToString());
				result = 1;
			}
			return result;
		}
	}
}

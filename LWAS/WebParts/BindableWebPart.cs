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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Web.UI;
using System.Web.UI.WebControls;

using LWAS.Extensible.Interfaces.DataBinding;
using LWAS.Extensible.Interfaces.Expressions;
using LWAS.Extensible.Interfaces.Validation;
using LWAS.Extensible.Interfaces.WebParts;

namespace LWAS.WebParts
{
	public class BindableWebPart : EditableWebPart, IBindableWebPart
	{
		private bool memberHasChanged = false;
		private IBinder _binder;
		private IDataSource _dataSource;
		private string _dataMember = string.Empty;
		private IValidationManager _validationManager;
		private IExpressionsManager _expressionsManager;
		private DataSourceView _dataSourceView;
		private IDictionary inserted = null;
		private IDictionary updated = null;
		private IDictionary deleted = null;
		public virtual IBinder Binder
		{
			get
			{
				return this._binder;
			}
			set
			{
				this._binder = value;
			}
		}
		public virtual IDataSource DataSource
		{
			get
			{
				return this._dataSource;
			}
			set
			{
				this._dataSource = value;
			}
		}
		public virtual string DataMember
		{
			get
			{
				return this._dataMember;
			}
			set
			{
				this._dataMember = value;
				this.memberHasChanged = true;
			}
		}
		public IValidationManager ValidationManager
		{
			get
			{
				return this._validationManager;
			}
			set
			{
				this._validationManager = value;
			}
		}
		public IExpressionsManager ExpressionsManager
		{
			get
			{
				return this._expressionsManager;
			}
			set
			{
				this._expressionsManager = value;
			}
		}
		public virtual DataSourceView DataSourceView
		{
			get
			{
				if (this._dataSourceView == null || this.memberHasChanged)
				{
					if (null == this._dataSource)
					{
						throw new InvalidOperationException("DataSource is null");
					}
					this._dataSourceView = this._dataSource.GetView(this._dataMember);
					if (null == this._dataSourceView)
					{
						throw new ArgumentException(string.Format("Member '{0}' not found", this._dataMember));
					}
				}
				return this._dataSourceView;
			}
			set
			{
				this._dataSourceView = value;
			}
		}
		protected virtual void DataSourceSelectCallback(IEnumerable data)
		{
			if (data is DbDataReader)
			{
				((DbDataReader)data).Close();
			}
		}
		protected virtual bool DataSourceOperationCallback(int affectedRows, Exception ex)
		{
			return null == ex;
		}
		protected virtual IDictionary TrimToParameters(ParameterCollection parameters, IDictionary values)
		{
			if (null == this._dataSource)
			{
				throw new InvalidOperationException("DataSource is null");
			}
			if (!(this._dataSource is Control))
			{
				throw new InvalidOperationException("DataSource is not a Control");
			}
			IDictionary result;
			if (null == values)
			{
				result = null;
			}
			else
			{
				Dictionary<string, object> prepared = new Dictionary<string, object>();
				IOrderedDictionary paramValues = parameters.GetValues(this.Context, this._dataSource as Control);
				if (null != parameters)
				{
					foreach (Parameter param in parameters)
					{
						prepared.Add(param.Name, paramValues[param.Name]);
					}
				}
				if (null != values)
				{
					foreach (DictionaryEntry entry in values)
					{
						if (prepared.ContainsKey(entry.Key.ToString()))
						{
							prepared[entry.Key.ToString()] = entry.Value;
						}
					}
				}
				result = prepared;
			}
			return result;
		}
		protected virtual void RecoverParameters(DbParameterCollection parameters, IDictionary values)
		{
			foreach (DbParameter param in parameters)
			{
				if (param.Direction == ParameterDirection.InputOutput || param.Direction == ParameterDirection.Output)
				{
					string key = param.ParameterName.Replace("@", "");
					if (values.Contains(key))
					{
						values[key] = param.Value;
					}
				}
			}
		}
		public virtual void Select(DataSourceSelectArguments args)
		{
			if (null != this._dataSource)
			{
				this.DataSourceView.Select(args, new DataSourceViewSelectCallback(this.DataSourceSelectCallback));
			}
		}
		public virtual void Insert(IDictionary values)
		{
			if (null != this._dataSource)
			{
				if (this._dataSource is SqlDataSource)
				{
					SqlDataSource sqlDataSource = this._dataSource as SqlDataSource;
					sqlDataSource.Inserted += new SqlDataSourceStatusEventHandler(this.BindableWebPart_Inserted);
					this.inserted = values;
					this.DataSourceView.Insert(this.TrimToParameters(sqlDataSource.InsertParameters, values), new DataSourceViewOperationCallback(this.DataSourceOperationCallback));
					sqlDataSource.Inserted -= new SqlDataSourceStatusEventHandler(this.BindableWebPart_Inserted);
				}
				else
				{
					this.DataSourceView.Insert(values, new DataSourceViewOperationCallback(this.DataSourceOperationCallback));
				}
			}
		}
		private void BindableWebPart_Inserted(object sender, SqlDataSourceStatusEventArgs e)
		{
			if (null != this.inserted)
			{
				this.RecoverParameters(e.Command.Parameters, this.inserted);
			}
		}
		public virtual void Update(IDictionary keys, IDictionary newValues, IDictionary oldValues)
		{
			if (null != this._dataSource)
			{
				if (this._dataSource is SqlDataSource)
				{
					SqlDataSource sqlDataSource = this._dataSource as SqlDataSource;
					sqlDataSource.Updated += new SqlDataSourceStatusEventHandler(this.sqlDataSource_Updated);
					this.updated = newValues;
					this.DataSourceView.Update(this.TrimToParameters(sqlDataSource.UpdateParameters, keys), this.TrimToParameters(sqlDataSource.UpdateParameters, newValues), oldValues, new DataSourceViewOperationCallback(this.DataSourceOperationCallback));
					sqlDataSource.Updated -= new SqlDataSourceStatusEventHandler(this.sqlDataSource_Updated);
				}
				else
				{
					this.DataSourceView.Update(keys, newValues, oldValues, new DataSourceViewOperationCallback(this.DataSourceOperationCallback));
				}
			}
		}
		private void sqlDataSource_Updated(object sender, SqlDataSourceStatusEventArgs e)
		{
			if (null != this.updated)
			{
				this.RecoverParameters(e.Command.Parameters, this.updated);
			}
		}
		public virtual void Delete(IDictionary keys, IDictionary oldValues)
		{
			if (null != this._dataSource)
			{
				if (this._dataSource is SqlDataSource)
				{
					SqlDataSource sqlDataSource = this._dataSource as SqlDataSource;
					sqlDataSource.Deleted += new SqlDataSourceStatusEventHandler(this.sqlDataSource_Deleted);
					this.deleted = keys;
					this.DataSourceView.Delete(this.TrimToParameters(sqlDataSource.DeleteParameters, keys), oldValues, new DataSourceViewOperationCallback(this.DataSourceOperationCallback));
					sqlDataSource.Deleted -= new SqlDataSourceStatusEventHandler(this.sqlDataSource_Deleted);
				}
				else
				{
					this.DataSourceView.Delete(keys, oldValues, new DataSourceViewOperationCallback(this.DataSourceOperationCallback));
				}
			}
		}
		private void sqlDataSource_Deleted(object sender, SqlDataSourceStatusEventArgs e)
		{
			if (null != this.deleted)
			{
				this.RecoverParameters(e.Command.Parameters, this.deleted);
			}
		}
	}
}

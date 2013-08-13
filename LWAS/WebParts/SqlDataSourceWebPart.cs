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
using System.Web.UI;
using System.Web.UI.WebControls;

using LWAS.Extensible.Exceptions;
using LWAS.Extensible.Interfaces.Monitoring;

namespace LWAS.WebParts
{
	public class SqlDataSourceWebPart : DataSourceProviderWebPart, IReporter
	{
		private IMonitor _monitor;
		private SqlDataSource sqlDataSource = new SqlDataSource();
		private bool _isReset = false;
		public IMonitor Monitor
		{
			get
			{
				return this._monitor;
			}
			set
			{
				this._monitor = value;
			}
		}
		public string Run
		{
			set
			{
				try
				{
					if (value != null)
					{
						if (!(value == "select"))
						{
							if (value == "delete")
							{
								this.sqlDataSource.Delete();
							}
						}
						else
						{
							this.sqlDataSource.Select(DataSourceSelectArguments.Empty);
						}
					}
				}
				catch (Exception ex)
				{
					this._monitor.Register(this, this._monitor.NewEventInstance("run error", null, ex, EVENT_TYPE.Error));
				}
			}
		}
		public override object DataSource
		{
			get
			{
				return this.sqlDataSource;
			}
			set
			{
			}
		}
		public bool IsReset
		{
			get
			{
				return this._isReset;
			}
			set
			{
				this._isReset = value;
				if (this._isReset)
				{
					this.OnReset();
				}
			}
		}
		public SqlDataSourceWebPart()
		{
			this.sqlDataSource.DataSourceMode = SqlDataSourceMode.DataReader;
			base.DataSource = this.sqlDataSource;
		}
		protected virtual void OnReset()
		{
			base.Initialize();
		}
		protected override void LoadViewState(object savedState)
		{
			object[] state = (object[])savedState;
			base.LoadViewState(state[0]);
			Hashtable defaultValues = (Hashtable)state[1];
			if (null != defaultValues)
			{
				foreach (string key in defaultValues.Keys)
				{
					if (null != defaultValues[key])
					{
						if (null != this.sqlDataSource.SelectParameters[key])
						{
							this.sqlDataSource.SelectParameters[key].DefaultValue = defaultValues[key].ToString();
						}
						if (null != this.sqlDataSource.InsertParameters[key])
						{
							this.sqlDataSource.InsertParameters[key].DefaultValue = defaultValues[key].ToString();
						}
						if (null != this.sqlDataSource.UpdateParameters[key])
						{
							this.sqlDataSource.UpdateParameters[key].DefaultValue = defaultValues[key].ToString();
						}
						if (null != this.sqlDataSource.DeleteParameters[key])
						{
							this.sqlDataSource.DeleteParameters[key].DefaultValue = defaultValues[key].ToString();
						}
					}
				}
			}
		}
		protected override object SaveViewState()
		{
			Hashtable defaultValues = new Hashtable();
			for (int i = 0; i < this.sqlDataSource.SelectParameters.Count; i++)
			{
				if (!defaultValues.ContainsKey(this.sqlDataSource.SelectParameters[i].Name) && null != this.sqlDataSource.SelectParameters[i].DefaultValue)
				{
					defaultValues.Add(this.sqlDataSource.SelectParameters[i].Name, this.sqlDataSource.SelectParameters[i].DefaultValue);
				}
			}
			for (int i = 0; i < this.sqlDataSource.InsertParameters.Count; i++)
			{
				if (!defaultValues.ContainsKey(this.sqlDataSource.InsertParameters[i].Name) && null != this.sqlDataSource.InsertParameters[i].DefaultValue)
				{
					defaultValues.Add(this.sqlDataSource.InsertParameters[i].Name, this.sqlDataSource.InsertParameters[i].DefaultValue);
				}
			}
			for (int i = 0; i < this.sqlDataSource.UpdateParameters.Count; i++)
			{
				if (!defaultValues.ContainsKey(this.sqlDataSource.UpdateParameters[i].Name) && null != this.sqlDataSource.UpdateParameters[i].DefaultValue)
				{
					defaultValues.Add(this.sqlDataSource.UpdateParameters[i].Name, this.sqlDataSource.UpdateParameters[i].DefaultValue);
				}
			}
			for (int i = 0; i < this.sqlDataSource.DeleteParameters.Count; i++)
			{
				if (!defaultValues.ContainsKey(this.sqlDataSource.DeleteParameters[i].Name) && null != this.sqlDataSource.DeleteParameters[i].DefaultValue)
				{
					defaultValues.Add(this.sqlDataSource.DeleteParameters[i].Name, this.sqlDataSource.DeleteParameters[i].DefaultValue);
				}
			}
			return new object[]
			{
				base.SaveViewState(), 
				defaultValues
			};
		}
		public override void Initialize()
		{
			if (null == this._monitor)
			{
				throw new MissingProviderException("Monitor");
			}
			base.Initialize();
		}
	}
}

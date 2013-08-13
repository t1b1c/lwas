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
using System.Collections.Generic;
using System.Configuration;
using System.Data;

using LWAS.Extensible.Interfaces.Monitoring;
using LWAS.Infrastructure;
using LWAS.Infrastructure.Security;

namespace LWAS.WebParts
{
	public class ContextWebPart : InitializableWebPart, IReporter
	{
		private string _adminMail;
        private string _url;
		private object _currentPart;
		private string _currentPath;
		private Dictionary<string, Dictionary<string, object>> _data = new Dictionary<string, Dictionary<string, object>>();
		private IMonitor _monitor;
		public string AdminMail
		{
			get
			{
				return this._adminMail;
			}
			set
			{
			}
		}
        public string Url
        {
            get { return _url; }
            set { }
        }
        public DateTime Now
        {
            get { return DateTime.Now; }
        }
		public object Login
		{
			get
			{
				return User.CurrentUser.Name;
			}
		}
		public object SetPart
		{
			set
			{
				this._currentPart = value;
			}
		}
		public string SetPath
		{
			set
			{
				this._currentPath = value;
			}
		}
		public string ValueOf
		{
			get
			{
				if (this._currentPart != null && !string.IsNullOrEmpty(this._currentPath))
				{
					object o = ReflectionServices.ExtractValue(this._currentPart, this._currentPath);
				}
				return null;
			}
		}
		public Dictionary<string, Dictionary<string, object>> Data
		{
			get
			{
				if (null == this._data)
				{
					throw new InvalidOperationException("No context data");
				}
				return this._data;
			}
		}
		public string DataKey
		{
			get;
			set;
		}
		public DataSet DataSource
		{
			set
			{
				if (null == this.DataKey) throw new InvalidOperationException("DataKey not set");

                if (value != null && value.Tables.Count > 0 && value.Tables[0].Rows.Count > 0)
                {
                    if (_data.ContainsKey(this.DataKey))
                        _data.Remove(this.DataKey);
                    this._data.Add(this.DataKey, ReflectionServices.ToDictionary(value.Tables[0].Rows[0]));
                }

                this.DataKey = null;
			}
		}
        public Dictionary<string, object> DataItem
        {
            set
            {
                if (null == this.DataKey) throw new InvalidOperationException("DataKey not set");

                if (value != null)
                {
                    if (_data.ContainsKey(this.DataKey))
                        _data.Remove(this.DataKey);
                    _data.Add(this.DataKey, value);
                }

                this.DataKey = null;
            }
        }
		public IMonitor Monitor
		{
			get
			{
				return this._monitor;
			}
			set
			{
				this._monitor = null;
			}
		}
        public object Temp
        {
            get;
            set;
        }
        public string NavigateTo
        {
            set
            {
                this.Page.Response.Redirect(value, false); // let the lifecycle to complete
            }
        }
		public ContextWebPart()
		{
			this.Hidden = true;
			this._adminMail = ConfigurationManager.AppSettings["SUPER_ROLE_MAIL"];
		}
		public override void Initialize()
		{
            base.Initialize();
            this._url = this.Page.Request.Url.GetLeftPart(UriPartial.Authority) + this.Page.ResolveUrl(this.Page.Request.ApplicationPath);
		}
	}
}

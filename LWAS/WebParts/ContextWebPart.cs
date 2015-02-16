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
using System.Web;

using LWAS.Extensible.Interfaces.Monitoring;
using LWAS.Infrastructure;
using LWAS.Infrastructure.Security;
using LWAS.Database;

namespace LWAS.WebParts
{
	public class ContextWebPart : InitializableWebPart, IReporter
	{
        private IMonitor _monitor;
        public IMonitor Monitor
        {
            get { return _monitor; }
            set { _monitor = null; }
        }

        public object Temp { get; set; }

        private string _adminMail;
        public string AdminMail
		{
			get { return _adminMail; }
			set { }
		}

        private string _url;
        public string Url
        {
            get { return _url; }
            set { }
        }

        public DateTime Now
        {
            get { return DateTime.Now; }
        }

        public DateTime Today
        {
            get { return DateTime.Today; }
        }

		public object Login
		{
			get { return User.CurrentUser.Name; }
		}

        Dictionary<string, Dictionary<string, object>> _data = new Dictionary<string, Dictionary<string, object>>();
        public Dictionary<string, Dictionary<string, object>> Data
		{
			get
			{
				if (null == _data)
					throw new InvalidOperationException("No context data");

                return _data;
			}
		}

		public string DataKey { get; set; }

		public DataSet DataSource
		{
			set
			{
				if (null == this.DataKey) throw new InvalidOperationException("DataKey not set");

                if (value != null && value.Tables.Count > 0)
                {
                    if (_data.ContainsKey(this.DataKey))
                        _data.Remove(this.DataKey);
                    this._data.Add(this.DataKey, ReflectionServices.ToDictionary(value.Tables[0]));
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

        ParametersCollection _navigateParameters;
        public ParametersCollection NavigateParameters
        {
            get
            {
                if (null == _navigateParameters)
                    _navigateParameters = new ParametersCollection();
                return _navigateParameters;
            }
        }

        public string NavigateTo
        {
            set
            {
                string destination = value;
                if (!String.IsNullOrEmpty(destination))
                {
                    destination = AddParametersToNavigateUrl(destination);

                    this.Page.Response.Redirect(destination, false); // let the lifecycle to complete
                }
            }
        }

		public ContextWebPart()
		{
			this.Hidden = true;
			_adminMail = ConfigurationManager.AppSettings["SUPER_ROLE_MAIL"];
		}

		public override void Initialize()
		{
            base.Initialize();
            _url = this.Page.Request.Url.GetLeftPart(UriPartial.Authority) + this.Page.ResolveUrl(this.Page.Request.ApplicationPath);
		}

        string AddParametersToNavigateUrl(string url)
        {
            bool first = true;
            if (url.IndexOf("?") < 0 && !this.NavigateParameters.IsEmpty)
                url += "?";
            else
                first = false;

            foreach (string parameter in this.NavigateParameters)
            {

                string pname = "{?}".Replace("?", parameter);
                object pval = this.NavigateParameters[parameter] ?? String.Empty;

                if (url.Contains(pname))
                    url = url.Replace(pname, HttpUtility.UrlEncode(pval.ToString()));
                else
                {
                    url += (first ? "" : "&");
                    url = url + HttpUtility.UrlEncode(parameter) + "=";
                    url += ((this.NavigateParameters[parameter] == null) ? string.Empty : HttpUtility.UrlEncode(pval.ToString()));
                }

                if (first)
                    first = false;
            }

            return url;
        }
	}
}

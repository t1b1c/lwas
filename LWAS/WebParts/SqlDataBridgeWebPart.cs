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
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Xml;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.IO;

using LWAS.CustomControls.DataControls;
using LWAS.Extensible.Exceptions;
using LWAS.Extensible.Interfaces.DataBinding;
using LWAS.Extensible.Interfaces.Monitoring;
using LWAS.Extensible.Interfaces.Storage;

namespace LWAS.WebParts
{
	public class SqlDataBridgeWebPart : EditableWebPart, IReporter
	{
        static object SyncRoot = new object();

		private IBinder _binder;
		private SqlDataBridge _dataBridge;
		private IMonitor _monitor;
		public Dictionary<string, string> ConnectionsRegistry = new Dictionary<string, string>();
		private DataSet _lastResult;
		private SqlDataReader _lastReader;
		private IStorageAgent _agent;
		public IBinder Binder
		{
			get { return this._binder; }
			set { this._binder = value; }
		}
		public SqlDataBridge DataBridge
		{
			get { return this._dataBridge; }
			set { this._dataBridge = value; }
		}
		public IMonitor Monitor
		{
			get { return this._monitor; }
			set { this._monitor = value; }
		}

        string defaultConnectionKey = null;
		public string Connection
		{
			get
			{
				return this._dataBridge.ConnectionString;
			}
			set
			{
				if (!string.IsNullOrEmpty(value) && this.ConnectionsRegistry.ContainsKey(value))
				{
					try
					{
						this._dataBridge.ConnectionString = this.ConnectionsRegistry[value];
                        if (String.IsNullOrEmpty(defaultConnectionKey))
                            defaultConnectionKey = value;
					}
					catch (Exception ex)
					{
						this._monitor.Register(this, this._monitor.NewEventInstance("open connection error", null, ex, EVENT_TYPE.Error));
					}
				}
                else   // empty means reset connection
                {
                    try
                    {
                        if (!String.IsNullOrEmpty(defaultConnectionKey))
                            _dataBridge.ConnectionString = this.ConnectionsRegistry[defaultConnectionKey];
                        else
                            this._dataBridge.ConnectionString = this.ConnectionsRegistry.Values.First();
                    }
                    catch (Exception ex)
                    {
                        this._monitor.Register(this, this._monitor.NewEventInstance("open connection error", null, ex, EVENT_TYPE.Error));
                    }

                }
			}
		}
		public DataSet LastResult
		{
			get
			{
				if (this._lastReader != null && !this._lastReader.IsClosed)
				{
					this._lastResult = new DataSet();
					this._lastResult.Load(this._lastReader, LoadOption.OverwriteChanges, new string[]
					{
						string.Empty
					});
				}
				else
				{
					this._lastResult = null;
				}
				return this._lastResult;
			}
		}
		public SqlDataReader LastReader
		{
			get
			{
				return this._lastReader;
			}
		}
		public string ExecuteKey
		{
			set
			{
				this._binder.Bind(value);
				this.OnExecute(value);
				this._binder.Bind(value);
				this.OnMilestone("done");
			}
		}
		public IStorageAgent Agent
		{
			get { return this._agent; }
			set { this._agent = value; }
		}

        public string[] RegisterCommand
        {
            set
            {
                string name = value[0];
                string sql = value[1];
                if (this.DataBridge.Commands.ContainsKey(name))
                    this.DataBridge.Commands.Remove(name);
                this.DataBridge.Commands.Add(name, new SqlCommand(sql));
                this.ExecuteKey = name;
            }
        }

		public SqlDataBridgeWebPart()
		{
			this.Hidden = true;
            _dataBridge = new SqlDataBridge();
            _dataBridge.ExecuteSql += _dataBridge_ExecuteSql;
		}

        void _dataBridge_ExecuteSql(object sender, SqlDataBridge.ExecuteSqlEventArgs e)
        {
            string sql = e.Command.CommandText;
            foreach (SqlParameter param in e.Command.Parameters)
                sql += String.Format(" {0}='{1}',", param.ParameterName, param.Value ?? "");

            this.Monitor.Register(this, this.Monitor.NewEventInstance("execute sql", null, sql, EVENT_TYPE.Trace));

        }
		protected void LoadConnections()
		{
			string file = ConfigurationManager.AppSettings["CONNECTIONS_FILE"];
			if (string.IsNullOrEmpty(file)) throw new ArgumentNullException("CONNECTIONS_FILE config not set");
            if (!Path.IsPathRooted(file))
                file = HttpContext.Current.Server.MapPath(file);

            Cache cache = HttpContext.Current.Cache;
            lock (SyncRoot)
            {
                XmlDocument doc = cache[file] as XmlDocument;

                if (null == doc)
                {
                    doc = new XmlDocument();
                    doc.LoadXml(this._agent.Read(file));
                    cache.Insert(file, doc, new CacheDependency(file));
                }
                XmlNode rootNode = doc.SelectSingleNode("connections");
                if (null == rootNode) throw new InvalidOperationException(string.Format("File '{0}' has no 'connections' node", file));

                foreach (XmlNode connNode in rootNode.ChildNodes)
                {
                    this.ConnectionsRegistry.Add(connNode.Attributes["key"].Value, connNode.Attributes["string"].Value);
                }
            }
		}
        public void Execute(string key)
        {
            OnExecute(key);
        }
		protected void OnExecute(string key)
		{
			this.EnsureReaderClosed();
			try
			{
				this._lastReader = this._dataBridge.ExecuteCommand(key);
			}
			catch (Exception ex)
			{
				string translated = base.Translator.Translate(ex.Message, ex.Message, null).Translation;
				this._monitor.Register(this, this._monitor.NewEventInstance(translated ?? ex.Message, EVENT_TYPE.Error));
			}
		}
		protected void EnsureReaderClosed()
		{
			if (this._lastReader != null && !this._lastReader.IsClosed)
			{
				this._lastReader.Close();
			}
		}
		protected override void OnUnload(EventArgs e)
		{
			this.EnsureReaderClosed();
			this._dataBridge.Close();
			base.OnUnload(e);
		}
		public override void Initialize()
		{
			if (null == this.Configuration) throw new MissingProviderException("Configuration"); 
			if (null == this.ConfigurationParser) throw new MissingProviderException("Configuration parser");
			if (null == this._binder) throw new MissingProviderException("Binder");
			if (null == this._monitor) throw new MissingProviderException("Monitor");
			if (null == this._agent) throw new MissingProviderException("Agent");

			this.ConfigurationParser.Parse(this);
			base.Initialize();
			this.LoadConnections();
		}
	}
}

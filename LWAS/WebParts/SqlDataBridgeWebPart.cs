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
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Xml;

using LWAS.CustomControls.DataControls;
using LWAS.Extensible.Exceptions;
using LWAS.Extensible.Interfaces.DataBinding;
using LWAS.Extensible.Interfaces.Monitoring;
using LWAS.Extensible.Interfaces.Storage;

namespace LWAS.WebParts
{
	public class SqlDataBridgeWebPart : EditableWebPart, IReporter
	{
		private IBinder _binder;
		private SqlDataBridge _dataBridge = new SqlDataBridge();
		private IMonitor _monitor;
		protected Dictionary<string, string> ConnectionsRegistry = new Dictionary<string, string>();
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
		}
		protected void LoadConnections()
		{
			string file = ConfigurationManager.AppSettings["CONNECTIONS_FILE"];
			if (string.IsNullOrEmpty(file))
			{
				throw new ArgumentNullException("CONNECTIONS_FILE config not set");
			}
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(this._agent.Read(file));
			XmlNode rootNode = doc.SelectSingleNode("connections");
			if (null == rootNode)
			{
				throw new InvalidOperationException(string.Format("File '{0}' has no 'connections' node", file));
			}
			foreach (XmlNode connNode in rootNode.ChildNodes)
			{
				this.ConnectionsRegistry.Add(connNode.Attributes["key"].Value, connNode.Attributes["string"].Value);
			}
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
			if (null == this.Configuration)
			{
				throw new MissingProviderException("Configuration");
			}
			if (null == this.ConfigurationParser)
			{
				throw new MissingProviderException("Configuration parser");
			}
			if (null == this._binder)
			{
				throw new MissingProviderException("Binder");
			}
			if (null == this._monitor)
			{
				throw new MissingProviderException("Monitor");
			}
			if (null == this._agent)
			{
				throw new MissingProviderException("Agent");
			}
			this.ConfigurationParser.Parse(this);
			base.Initialize();
			this.LoadConnections();
		}
	}
}

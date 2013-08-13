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
using System.Xml;

using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.Storage;

namespace LWAS.WebParts.Editors
{
	public class UndoManager
	{
		public class HistoryData
		{
			public DateTime Stamp;
			public string Tag;
			public IConfiguration Config;
			public HistoryData(string tag, IConfiguration config)
			{
				this.Tag = tag;
				this.Config = config;
			}
		}
		private IStorageContainer _container;
		private static UndoManager Instance;
		public Dictionary<string, LinkedList<UndoManager.HistoryData>> History = new Dictionary<string, LinkedList<UndoManager.HistoryData>>();
		public Dictionary<string, UndoManager.HistoryData> HystoryByTag = new Dictionary<string, UndoManager.HistoryData>();
		public IStorageContainer Container
		{
			get
			{
				return this._container;
			}
			set
			{
				this._container = value;
			}
		}
		static UndoManager()
		{
			UndoManager.Instance = new UndoManager();
		}
		public Dictionary<string, UndoManager.HistoryData> LoadHistory(string app, string screen, string part)
		{
			if (null == this._container)
			{
				throw new InvalidOperationException("Storage container not set");
			}
			string bin = ConfigurationManager.AppSettings["EDIT_REPO"];
			if (string.IsNullOrEmpty(bin))
			{
				throw new ApplicationException("'EDIT_REPO' web.config key not set");
			}
			string history = ConfigurationManager.AppSettings["EDIT_REPO_HISTORY"];
			if (string.IsNullOrEmpty(history))
			{
				throw new InvalidOperationException("EDIT_REPO_HISTORY key not set in config file");
			}
			IStorageContainer screenContainer = this._container.CreateContainer(bin).CreateContainer(app).CreateContainer(history).CreateContainer(screen);
			string key = part + ".xml";
			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(this._container.Agent.OpenStream(key));
				XmlNode root = doc.SelectSingleNode("records");
				if (null == root)
				{
					throw new InvalidOperationException("History file has no records node");
				}
			}
			finally
			{
				this._container.Agent.CloseStream(key);
			}
			return null;
		}
		public void AddHistory(string key, IConfiguration config)
		{
		}
		public void ChangeHistory(string key, string turnpointKey, IConfiguration config)
		{
		}
		public void Undo(string key)
		{
		}
		public void Redo(string key)
		{
		}
	}
}

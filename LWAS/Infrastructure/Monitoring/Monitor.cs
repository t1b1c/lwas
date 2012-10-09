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
using System.Configuration;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using LWAS.Extensible.Interfaces.Monitoring;

namespace LWAS.Infrastructure.Monitoring
{
	public class Monitor : IMonitor, IXmlSerializable
	{
		private bool _isDisabled = false;
		public EntriesList Entries = new EntriesList();
		private IRecordsCollection _records;
		private bool isMonitoring = false;
		public bool IsDisabled
		{
			get
			{
				return this._isDisabled;
			}
			set
			{
				this._isDisabled = value;
			}
		}
		public IRecordsCollection Records
		{
			get
			{
				if (null == this._records)
				{
					this._records = new RecordsCollection();
				}
				return this._records;
			}
		}
		public Monitor()
		{
			bool.TryParse(ConfigurationManager.AppSettings["DISABLE_MONITOR"], out this._isDisabled);
		}
		public void Start()
		{
			if (!this._isDisabled)
			{
				this.isMonitoring = true;
			}
		}
		public void Register(IReporter reporter, IEvent e)
		{
			if (!this._isDisabled)
			{
				if (!this.isMonitoring)
				{
					throw new InvalidOperationException("not monitoring");
				}
				if (null == reporter)
				{
					throw new ArgumentNullException("reporter");
				}
				if (null == e)
				{
					throw new ArgumentNullException("e");
				}
				this.Records.Add(new Record(reporter, e, this.Records.Count));
			}
		}
		public void Stop()
		{
			if (!this._isDisabled)
			{
				this.isMonitoring = false;
				this.Dump();
			}
		}
		protected void Dump()
		{
			if (!this._isDisabled && this.Records.Count > 0)
			{
				StringBuilder dump = new StringBuilder();
				using (XmlWriter writer = XmlWriter.Create(dump, new XmlWriterSettings
				{
					Indent = true
				}))
				{
					SerializationServices.Serialize(this, writer);
				}
				string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssff");
				this.Entries[timestamp] = dump.ToString();
			}
		}
		public string Dump(bool current)
		{
			string result;
			if (this._isDisabled)
			{
				result = string.Empty;
			}
			else
			{
				if (current)
				{
					this.Dump();
				}
				result = this.Entries.LastEntry();
			}
			return result;
		}
		public IEvent NewEventInstance(string key, EVENT_TYPE eventType)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}
			return new Event(key, eventType);
		}
		public IEvent NewEventInstance(string key, IEvent parent, EVENT_TYPE eventType)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}
			return new Event(key, parent, eventType);
		}
		public IEvent NewEventInstance(string key, IEvent parent, object data, EVENT_TYPE eventType)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}
			return new Event(key, parent, data, eventType);
		}
		public bool HasErrors()
		{
			bool result;
			foreach (IRecord e in this.Records)
			{
				if (e.Event.EventType == EVENT_TYPE.Error)
				{
					result = true;
					return result;
				}
			}
			result = false;
			return result;
		}
		public XmlSchema GetSchema()
		{
			return null;
		}
		public void ReadXml(XmlReader reader)
		{
			if (this._isDisabled)
			{
				return;
			}
		}
		public void WriteXml(XmlWriter writer)
		{
			if (!this._isDisabled)
			{
				this.Records.WriteXml(writer);
			}
		}
	}
}

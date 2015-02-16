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
        public bool IsDisabled
        {
            get { return _isDisabled; }
            set { _isDisabled = value; }
        }

        private IRecordsCollection _records;
        public IRecordsCollection Records
        {
            get
            {
                if (null == _records)
                    _records = new RecordsCollection();
                return _records;
            }
        }

        public EntriesList Entries { get; set; }
        public bool IsMonitoring { get; set; }

        public Monitor()
		{
			bool.TryParse(ConfigurationManager.AppSettings["DISABLE_MONITOR"], out _isDisabled);
            this.Entries = new EntriesList();
            this.IsMonitoring = false;
		}

		public void Start()
		{
			this.IsMonitoring = true;
		}

		public void Register(IReporter reporter, IEvent e)
		{
			if (!this.IsMonitoring) throw new InvalidOperationException("not monitoring");
			if (null == reporter) throw new ArgumentNullException("reporter");
			if (null == e) throw new ArgumentNullException("e");

			this.Records.Add(new Record(reporter, e, this.Records.Count));
		}

		public void Stop()
		{
            this.IsMonitoring = false;
			Dump();
		}

		protected void Dump()
		{
			if (!_isDisabled && this.Records.Count > 0)
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
			if (_isDisabled)
				result = string.Empty;
			else
			{
				if (current)
					this.Dump();
				result = this.Entries.LastEntry();
			}
			return result;
		}

		public IEvent NewEventInstance(string key, EVENT_TYPE eventType)
		{
			if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");

			return new Event(key, eventType);
		}

		public IEvent NewEventInstance(string key, IEvent parent, EVENT_TYPE eventType)
		{
			if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");

			return new Event(key, parent, eventType);
		}

		public IEvent NewEventInstance(string key, IEvent parent, object data, EVENT_TYPE eventType)
		{
			if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");

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
			if (_isDisabled)
				return;
		}

        public void WriteXml(XmlWriter writer)
		{
			if (!_isDisabled)
				this.Records.WriteXml(writer);
		}
	}
}

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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using LWAS.Extensible.Interfaces.Monitoring;

namespace LWAS.Infrastructure.Monitoring
{
	[Serializable]
	public class Record : IRecord, IXmlSerializable
	{
		private int _stamp;
		private IReporter _source;
		private IEvent _event;
		public int Stamp
		{
			get
			{
                return this._stamp;
			}
		}
		public IReporter Source
		{
			get
			{
				return this._source;
			}
			set
			{
				this._source = value;
			}
		}
		public IEvent Event
		{
			get
			{
				return this._event;
			}
			set
			{
				this._event = value;
			}
		}
		public Record(IReporter source, IEvent e, int aStamp)
		{
			if (null == source)
			{
				throw new ArgumentNullException("reporter");
			}
			if (null == e)
			{
				throw new ArgumentNullException("e");
			}
			this._source = source;
			this._event = e;
			this._stamp = aStamp;
		}
		public XmlSchema GetSchema()
		{
			return null;
		}
		public void ReadXml(XmlReader reader)
		{
		}
		public void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement("record");
			writer.WriteAttributeString("stamp", this._stamp.ToString());
			writer.WriteStartElement("source");
			writer.WriteAttributeString("title", this._source.Title);
			writer.WriteEndElement();
			this._event.WriteXml(writer);
			writer.WriteEndElement();
		}
	}
}

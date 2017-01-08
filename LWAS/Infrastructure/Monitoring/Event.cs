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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using LWAS.Extensible.Interfaces.Monitoring;

namespace LWAS.Infrastructure.Monitoring
{
	[Serializable]
	public class Event : IEvent, IXmlSerializable
	{
		private EVENT_TYPE _eventType = EVENT_TYPE.Unspecified;
		private string _key;
		private IEvent _parent;
		private object _data;
		public EVENT_TYPE EventType
		{
			get
			{
				return this._eventType;
			}
		}
		public string Key
		{
			get
			{
				return this._key;
			}
		}
		public IEvent Parent
		{
			get
			{
				return this._parent;
			}
		}
		public object Data
		{
			get
			{
				return this._data;
			}
		}
		public Event(string key, EVENT_TYPE type)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}
			this._key = key;
			this._eventType = type;
		}
		public Event(string key, IEvent parent, EVENT_TYPE type) : this(key, type)
		{
			this._parent = parent;
		}
		public Event(string key, IEvent parent, object data, EVENT_TYPE type) : this(key, parent, type)
		{
			this._data = data;
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
			if (null == writer)
			{
				throw new ArgumentNullException("writer");
			}
			writer.WriteStartElement("event");
			writer.WriteAttributeString("key", this._key);
			writer.WriteAttributeString("type", this._eventType.ToString());
			writer.WriteStartElement("parent");
			if (null != this._parent)
			{
				writer.WriteAttributeString("key", this._parent.Key);
			}
			writer.WriteEndElement();
			writer.WriteStartElement("data");
			if (null != this._data)
			{
				if (this._eventType == EVENT_TYPE.Unspecified)
				{
					writer.WriteAttributeString("type", this._data.GetType().AssemblyQualifiedName);
				}
				else
				{
					if (this._eventType == EVENT_TYPE.Info)
					{
						writer.WriteAttributeString("info", this._data.ToString());
					}
					else
					{
						if (this._eventType == EVENT_TYPE.Error || _eventType == EVENT_TYPE.ServerFailure)
						{
							Exception ex = this._data as Exception;
							if (null != this._data)
							{
								this.WriteException(writer, ex);
							}
						}
                        else if (this._eventType == EVENT_TYPE.Trace)
                            writer.WriteAttributeString("trace", this._data.ToString());
					}
				}
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
		private void WriteException(XmlWriter writer, Exception ex)
		{
			writer.WriteStartElement("error");
			writer.WriteAttributeString("message", ex.Message);
			if (null != ex.InnerException)
			{
				this.WriteException(writer, ex.InnerException);
			}
			writer.WriteEndElement();
		}
	}
}

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
using System.Xml.Serialization;

using LWAS.Extensible.Exceptions;
using LWAS.Extensible.Interfaces.Configuration;

namespace LWAS.Infrastructure.Configuration
{
	[Serializable]
	public class ConfigurationElement : ConfigurationType, IConfigurationElement, IConfigurationType, IXmlSerializable
	{
		private IConfigurationElementAttributesCollection _attributes;
		private IConfigurationElementsCollection _elements;
		public IConfigurationElementAttributesCollection Attributes
		{
			get
			{
				this.EnsureAttributes();
				return this._attributes;
			}
			set
			{
				this._attributes = new ConfigurationElementAttributesCollection(this, value);
			}
		}
		public IConfigurationElementsCollection Elements
		{
			get
			{
				this.EnsureElements();
				return this._elements;
			}
			set
			{
				this._elements = new ConfigurationElementsCollection(this, value);
			}
		}
		public ConfigurationElement(string key) : base(key)
		{
		}
		protected virtual void EnsureAttributes()
		{
			if (null == this._attributes)
			{
				this._attributes = new ConfigurationElementAttributesCollection(this);
			}
		}
		protected virtual void EnsureElements()
		{
			if (null == this._elements)
			{
				this._elements = new ConfigurationElementsCollection(this);
			}
		}
        public IConfigurationElement Clone()
        {
            ConfigurationElement clone = new ConfigurationElement(this.ConfigKey);
            clone.ConfigKey = this.ConfigKey;
            clone.Attributes = this.Attributes.Clone(clone);
            clone.Elements = this.Elements.Clone(clone);
            return clone;
        }
		public IConfigurationElementAttribute AddAttribute(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}
			this.Attributes.Add(key, new ConfigurationElementAttribute(key));
			return this.Attributes[key];
		}
		public IConfigurationElement AddElement(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}
			this.Elements.Add(key, new ConfigurationElement(key));
			return this.Elements[key];
		}
		public IConfigurationElementAttribute GetAttributeReference(string attribute)
		{
			if (string.IsNullOrEmpty(attribute))
			{
				throw new ArgumentNullException("attribute");
			}
			if (!this.Attributes.ContainsKey(attribute))
			{
				throw new MissingConfigurationException(attribute);
			}
			return this.Attributes[attribute];
		}
		public IConfigurationElement GetElementReference(string element)
		{
			if (string.IsNullOrEmpty(element))
			{
				throw new ArgumentNullException("element");
			}
			if (!this.Elements.ContainsKey(element))
			{
				throw new MissingConfigurationException(element);
			}
			return this.Elements[element];
		}
		public override void ReadXml(XmlReader reader)
		{
			if (null == reader)
			{
				throw new ArgumentNullException("reader");
			}
			if (reader.EOF)
			{
				throw new ArgumentException("reader is at EOF");
			}
			reader.MoveToElement();
			base.ReadXml(reader);
			reader.MoveToContent();
			int elementDepth = reader.Depth;
			if (reader.ReadToDescendant("attributes"))
			{
				this.EnsureAttributes();
				if (reader.ReadToDescendant("attribute"))
				{
					int depth = reader.Depth;
					do
					{
						ConfigurationElementAttribute attribute = new ConfigurationElementAttribute(string.Empty);
						attribute.ReadXml(reader);
						this._attributes.Add(attribute.ConfigKey, attribute);
						reader.MoveToElement();
						while (depth < reader.Depth && reader.Read())
						{
						}
					}
					while (reader.ReadToNextSibling("attribute"));
				}
			}
			while (elementDepth < reader.Depth - 1 && reader.Read())
			{
			}
			if (reader.ReadToNextSibling("elements"))
			{
				this.EnsureElements();
				if (reader.ReadToDescendant("element"))
				{
					int depth = reader.Depth;
					do
					{
						ConfigurationElement element = new ConfigurationElement(string.Empty);
						element.ReadXml(reader);
						this._elements.Add(element.ConfigKey, element);
						reader.MoveToElement();
						while (depth < reader.Depth && reader.Read())
						{
						}
					}
					while (reader.ReadToNextSibling("element"));
				}
			}
		}
		public override void WriteXml(XmlWriter writer)
		{
			if (null == writer)
			{
				throw new ArgumentNullException("writer");
			}
			writer.WriteStartElement("element");
			base.WriteXml(writer);
			this.EnsureAttributes();
			writer.WriteStartElement("attributes");
			foreach (IConfigurationElementAttribute attribute in this._attributes.Values)
			{
				attribute.WriteXml(writer);
			}
			writer.WriteEndElement();
			this.EnsureElements();
			writer.WriteStartElement("elements");
			foreach (IConfigurationElement element in this._elements.Values)
			{
				element.WriteXml(writer);
			}
			writer.WriteEndElement();
			writer.WriteEndElement();
		}
	}
}

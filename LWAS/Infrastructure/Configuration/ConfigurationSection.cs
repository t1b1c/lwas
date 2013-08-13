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
	public class ConfigurationSection : ConfigurationType, IConfigurationSection, IConfigurationType, IXmlSerializable
	{
		private IConfigurationElementsCollection _elements;
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
		public ConfigurationSection(string key) : base(key)
		{
		}
		protected virtual void EnsureElements()
		{
			if (null == this._elements)
			{
				this._elements = new ConfigurationElementsCollection(this);
			}
		}
        public IConfigurationSection Clone()
        {
            ConfigurationSection clone = new ConfigurationSection(this.ConfigKey);
            clone.Elements = this.Elements.Clone(clone);
            return clone;
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
		public IConfigurationElementAttribute GetAttributeReference(string element, string attribute)
		{
			if (string.IsNullOrEmpty(element))
			{
				throw new ArgumentNullException(element);
			}
			if (string.IsNullOrEmpty(attribute))
			{
				throw new ArgumentNullException(attribute);
			}
			IConfigurationElement elementRef = this.GetElementReference(element);
			return elementRef.GetAttributeReference(attribute);
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
			if (reader.ReadToDescendant("elements"))
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
			writer.WriteStartElement("section");
			base.WriteXml(writer);
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

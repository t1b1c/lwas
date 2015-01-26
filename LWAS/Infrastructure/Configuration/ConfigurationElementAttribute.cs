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
using System.Xml.Serialization;

using LWAS.Extensible.Interfaces.Configuration;

namespace LWAS.Infrastructure.Configuration
{
	[Serializable]
	public class ConfigurationElementAttribute : ConfigurationType, IConfigurationElementAttribute, IConfigurationType, IXmlSerializable
	{
		private object _value;
		public object Value
		{
			get
			{
				return this._value;
			}
			set
			{
				this._value = value;
			}
		}
		public ConfigurationElementAttribute(string key) : base(key)
		{
		}
        public IConfigurationElementAttribute Clone()
        {
            ConfigurationElementAttribute clone = new ConfigurationElementAttribute(this.ConfigKey);
            clone.ConfigKey = this.ConfigKey;
            clone.Value = this.Value;
            return clone;
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
			reader.MoveToElement();
			reader.MoveToAttribute("value");
			string temp = reader.Value;
			reader.MoveToElement();
			reader.MoveToAttribute("type");
			this._value = ReflectionServices.StrongTypeValue(temp, reader.Value);
		}
		public override void WriteXml(XmlWriter writer)
		{
			if (null == writer)
			{
				throw new ArgumentNullException("writer");
			}
			writer.WriteStartElement("attribute");
			base.WriteXml(writer);
			if (null == this._value)
			{
				this._value = string.Empty;
			}
			writer.WriteAttributeString("value", this._value.ToString());
			writer.WriteAttributeString("type", SerializationServices.ShortAssemblyQualifiedName(this._value.GetType().AssemblyQualifiedName));
			writer.WriteEndElement();
		}
	}
}

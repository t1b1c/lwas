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

using LWAS.Extensible.Interfaces.Configuration;

namespace LWAS.Infrastructure.Configuration
{
	[Serializable]
	public class ConfigurationType : IConfigurationType, IXmlSerializable
	{
		private string _configKey;
		public string ConfigKey
		{
			get
			{
				return this._configKey;
			}
			set
			{
				this._configKey = value;
			}
		}
		public ConfigurationType(string key)
		{
			this.ConfigKey = key;
		}
		public ConfigurationType(IConfigurationType anotherConfigurationType)
		{
			this.ConfigKey = anotherConfigurationType.ConfigKey;
		}
		public virtual XmlSchema GetSchema()
		{
			return null;
		}
		public virtual void ReadXml(XmlReader reader)
		{
			reader.MoveToAttribute("configKey");
			this._configKey = reader.Value;
		}
		public virtual void WriteXml(XmlWriter writer)
		{
			writer.WriteAttributeString("configKey", this._configKey);
		}
	}
}

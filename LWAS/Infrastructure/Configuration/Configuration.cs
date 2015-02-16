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
using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Configuration;

namespace LWAS.Infrastructure.Configuration
{
	[Serializable]
	public class Configuration : ConfigurationType, IConfiguration, IProvider, IInitializable, ILifetime, IConfigurationType, IXmlSerializable
	{
		public IConfigurationSectionsCollection _sections;
		private IConfiguration _configurationProvider;
		private int _initialization;
		private int _creation;
		private int _change;
		private int _completion;
		private RequestInitializationCallback _requestInitialization;
		public IConfigurationSectionsCollection Sections
		{
			get
			{
				this.EnsureSections();
				return this._sections;
			}
			set
			{
				this._sections = new ConfigurationSectionsCollection(this, value);
			}
		}
		public IConfiguration ConfigurationProvider
		{
			get
			{
				return this._configurationProvider;
			}
			set
			{
				this._configurationProvider = value;
			}
		}
		public int Initialization
		{
			get
			{
				return this._initialization;
			}
			set
			{
				this._initialization = value;
			}
		}
		public int Creation
		{
			get
			{
				return this._creation;
			}
			set
			{
				this._creation = value;
			}
		}
		public int Change
		{
			get
			{
				return this._change;
			}
			set
			{
				this._change = value;
			}
		}
		public int Completion
		{
			get
			{
				return this._completion;
			}
			set
			{
				this._completion = value;
			}
		}
		public RequestInitializationCallback RequestInitialization
		{
			get
			{
				return this._requestInitialization;
			}
			set
			{
				this._requestInitialization = value;
			}
		}
		public Configuration() : base(string.Empty)
		{
		}
		public Configuration(string key) : base(key)
		{
		}
		public Configuration(IConfiguration anotherConfig) : base(anotherConfig.ConfigKey)
		{
			this.ConfigurationProvider = anotherConfig.ConfigurationProvider;
			this.Sections = anotherConfig.Sections;
		}
		protected virtual void EnsureSections()
		{
			if (null == this._sections)
			{
				this._sections = new ConfigurationSectionsCollection(this);
			}
		}
        public IConfiguration Clone()
        {
            Configuration clone = new Configuration(this.ConfigKey);
            clone.Sections = this.Sections.Clone(clone);
            return clone;
        }
		public IConfiguration NewConfigurationInstance(string key)
		{
			return new Configuration(key);
		}
		public IConfigurationSection NewConfigurationSectionInstance(string key)
		{
			return new ConfigurationSection(key);
		}
		public IConfigurationElement NewConfigurationElementInstance(string key)
		{
			return new ConfigurationElement(key);
		}
		public IConfigurationElementAttribute NewConfigurationElementAttributeInstance(string key)
		{
			return new ConfigurationElementAttribute(key);
		}
		public IConfigurationSection AddSection(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}
			this.Sections.Add(key, new ConfigurationSection(key));
			return this.Sections[key];
		}
		public IConfigurationSection GetConfigurationSectionReference(string section)
		{
			if (string.IsNullOrEmpty(section))
			{
				throw new ArgumentNullException("section");
			}
			if (!this.Sections.ContainsKey(section))
			{
				throw new MissingConfigurationException(section);
			}
			return this.Sections[section];
		}
		public IConfigurationElement GetConfigurationElementReference(string section, string element)
		{
			if (string.IsNullOrEmpty(section))
			{
				throw new ArgumentNullException("section");
			}
			if (string.IsNullOrEmpty(element))
			{
				throw new ArgumentNullException("element");
			}
			IConfigurationSection sectionRef = this.GetConfigurationSectionReference(section);
			if (!sectionRef.Elements.ContainsKey(element))
			{
				throw new MissingConfigurationException(element);
			}
			return sectionRef.GetElementReference(element);
		}
		public IConfigurationElementAttribute GetConfigurationElementAttributeReference(string section, string element, string attribute)
		{
			if (string.IsNullOrEmpty(section))
			{
				throw new ArgumentNullException("section");
			}
			if (string.IsNullOrEmpty(element))
			{
				throw new ArgumentNullException("element");
			}
			if (string.IsNullOrEmpty(attribute))
			{
				throw new ArgumentNullException("attribute");
			}
			IConfigurationElement elementRef = this.GetConfigurationElementReference(section, element);
			return elementRef.GetAttributeReference(attribute);
		}
		public void Initialize()
		{
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
			if (reader.ReadToDescendant("sections"))
			{
				this.EnsureSections();
				if (reader.ReadToDescendant("section"))
				{
					int depth = reader.Depth;
					do
					{
						ConfigurationSection section = new ConfigurationSection(string.Empty);
						section.ReadXml(reader);
						this._sections.Add(section.ConfigKey, section);
						reader.MoveToElement();
						while (depth < reader.Depth && reader.Read())
						{
						}
					}
					while (reader.ReadToNextSibling("section"));
				}
			}
		}
		public override void WriteXml(XmlWriter writer)
		{
			if (null == writer)
			{
				throw new ArgumentNullException("writer");
			}
			base.WriteXml(writer);
			this.EnsureSections();
			writer.WriteStartElement("sections");
			foreach (IConfigurationSection section in this._sections.Values)
			{
				section.WriteXml(writer);
			}
			writer.WriteEndElement();
		}
	}
}

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
using System.Xml.Serialization;

namespace LWAS.Extensible.Interfaces.Configuration
{
	public interface IConfiguration : IProvider, IInitializable, ILifetime, IConfigurationType, IXmlSerializable
	{
		IConfigurationSectionsCollection Sections { get; set; }
        IConfiguration Clone();
		IConfiguration NewConfigurationInstance(string key);
		IConfigurationSection NewConfigurationSectionInstance(string key);
		IConfigurationElement NewConfigurationElementInstance(string key);
		IConfigurationElementAttribute NewConfigurationElementAttributeInstance(string key);
		IConfigurationSection AddSection(string key);
		IConfigurationSection GetConfigurationSectionReference(string section);
		IConfigurationElement GetConfigurationElementReference(string section, string element);
		IConfigurationElementAttribute GetConfigurationElementAttributeReference(string section, string element, string attribute);
	}
}

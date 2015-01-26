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
using System.Xml.Serialization;

namespace LWAS.Extensible.Interfaces.Configuration
{
	public interface IConfigurationElement : IConfigurationType, IXmlSerializable
	{
		IConfigurationElementAttributesCollection Attributes
		{
			get;
			set;
		}
		IConfigurationElementsCollection Elements
		{
			get;
			set;
		}
        IConfigurationElement Clone();
		IConfigurationElementAttribute AddAttribute(string key);
		IConfigurationElement AddElement(string key);
		IConfigurationElementAttribute GetAttributeReference(string attribute);
		IConfigurationElement GetElementReference(string element);
	}
}

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
using System.Configuration;

namespace LWAS.Drivers
{
	public class ComponentElement : ConfigurationElement
	{
		[ConfigurationProperty("name", IsRequired = true, IsKey = true)]
		public string Name
		{
			get
			{
				return (string)base["name"];
			}
			set
			{
				base["name"] = value;
			}
		}
		[ConfigurationProperty("setOnlyWhenNull", IsRequired = false)]
		public bool SetOnlyWhenNull
		{
			get
			{
				return this.Properties.Contains("setOnlyWhenNull") && (bool)base["setOnlyWhenNull"];
			}
			set
			{
				base["setOnlyWhenNull"] = value;
			}
		}
		[ConfigurationProperty("isSingleton", IsRequired = false)]
		public bool IsSingleton
		{
			get
			{
				return this.Properties.Contains("isSingleton") && (bool)base["isSingleton"];
			}
			set
			{
				base["isSingleton"] = value;
			}
		}
		[ConfigurationProperty("type", IsRequired = true)]
		public string Type
		{
			get
			{
				return (string)base["type"];
			}
			set
			{
				base["type"] = value;
			}
		}
		[ConfigurationProperty("member", IsRequired = false)]
		public string Member
		{
			get
			{
				return base["member"] as string;
			}
			set
			{
				base["member"] = value;
			}
		}
		[ConfigurationProperty("properties")]
		public PropertyElementCollection PropertiesCollection
		{
			get
			{
				return base["properties"] as PropertyElementCollection;
			}
		}
		[ConfigurationProperty("providers")]
		public ComponentElementCollection ProvidersCollection
		{
			get
			{
				return base["providers"] as ComponentElementCollection;
			}
		}
	}
}

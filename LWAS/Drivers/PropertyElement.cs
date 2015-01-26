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
	public class PropertyElement : ConfigurationElement
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
		[ConfigurationProperty("value", IsRequired = false)]
		public string Value
		{
			get
			{
				return base["value"].ToString();
			}
			set
			{
				base["value"] = value;
			}
		}
		[ConfigurationProperty("items")]
		public DuplicableComponentElementCollection ItemsCollection
		{
			get
			{
				return base["items"] as DuplicableComponentElementCollection;
			}
		}
	}
}

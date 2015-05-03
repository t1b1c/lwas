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
	[ConfigurationCollection(typeof(ComponentElement), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
	public class DuplicableComponentElementCollection : ConfigurationElementCollection
	{
		public override ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return ConfigurationElementCollectionType.AddRemoveClearMap;
			}
		}
		protected override ConfigurationElement CreateNewElement()
		{
			return new ComponentElement();
		}
		protected override object GetElementKey(ConfigurationElement element)
		{
			return element.GetHashCode();
		}
		public void Add(ComponentElement element)
		{
			if (null != element)
			{
				base.BaseAdd(element, false);
			}
		}
		public void Clear()
		{
			base.BaseClear();
		}
	}
}

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
using System.Configuration;

namespace LWAS.Drivers
{
	[ConfigurationCollection(typeof(PropertyElement), AddItemName = "property", CollectionType = ConfigurationElementCollectionType.BasicMap)]
	public class PropertyElementCollection : ConfigurationElementCollection
	{
		public override ConfigurationElementCollectionType CollectionType
		{
			get
			{
				return ConfigurationElementCollectionType.BasicMap;
			}
		}
		protected override string ElementName
		{
			get
			{
				return "property";
			}
		}
		public PropertyElement this[int index]
		{
			get
			{
				return (PropertyElement)base.BaseGet(index);
			}
			set
			{
				if (base.BaseGet(index) != null)
				{
					base.BaseRemoveAt(index);
				}
				this.BaseAdd(index, value);
			}
		}
		public new PropertyElement this[string Name]
		{
			get
			{
				return base.BaseGet(Name) as PropertyElement;
			}
		}
		protected override ConfigurationElement CreateNewElement()
		{
			return new PropertyElement();
		}
		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((PropertyElement)element).Name;
		}
		public int IndexOf(PropertyElement element)
		{
			return base.BaseIndexOf(element);
		}
		public void Add(PropertyElement element)
		{
			this.BaseAdd(element);
		}
		protected override void BaseAdd(ConfigurationElement element)
		{
			base.BaseAdd(element, false);
		}
		public void Remove(PropertyElement element)
		{
			if (base.BaseIndexOf(element) >= 0)
			{
				base.BaseRemove(element.Name);
			}
		}
		public void RemoveAt(int index)
		{
			base.BaseRemoveAt(index);
		}
		public void Remove(string name)
		{
			base.BaseRemove(name);
		}
		public void Clear()
		{
			base.BaseClear();
		}
		public bool Contains(string name)
		{
			return null != this[name];
		}
	}
}

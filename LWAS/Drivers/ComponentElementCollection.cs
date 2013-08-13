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
	[ConfigurationCollection(typeof(ComponentElement), AddItemName = "component", CollectionType = ConfigurationElementCollectionType.BasicMap)]
	public class ComponentElementCollection : ConfigurationElementCollection
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
				return "component";
			}
		}
		public ComponentElement this[int index]
		{
			get
			{
				return (ComponentElement)base.BaseGet(index);
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
		public new ComponentElement this[string Name]
		{
			get
			{
				return base.BaseGet(Name) as ComponentElement;
			}
		}
		protected override ConfigurationElement CreateNewElement()
		{
			return new ComponentElement();
		}
		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((ComponentElement)element).Name;
		}
		public int IndexOf(ComponentElement element)
		{
			return base.BaseIndexOf(element);
		}
		public void Add(ComponentElement element)
		{
			this.BaseAdd(element);
		}
		protected override void BaseAdd(ConfigurationElement element)
		{
			base.BaseAdd(element, false);
		}
		public void Remove(ComponentElement element)
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

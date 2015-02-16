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
using System.Collections;
using System.Collections.Generic;

using LWAS.Extensible.Interfaces.Configuration;

namespace LWAS.Infrastructure.Configuration
{
	public class ConfigurationElementsCollection : Dictionary<string, IConfigurationElement>, IConfigurationElementsCollection, IDictionary<string, IConfigurationElement>, ICollection<KeyValuePair<string, IConfigurationElement>>, IEnumerable<KeyValuePair<string, IConfigurationElement>>, IEnumerable
	{
		private IConfigurationType _parent;
		public IConfigurationType Parent
		{
			get
			{
				return this._parent;
			}
		}
		public ConfigurationElementsCollection(IConfigurationType parent)
		{
			if (!(parent is IConfigurationSection) && !(parent is IConfigurationElement))
			{
				throw new InvalidOperationException("Parent must be a section or an element");
			}
			this._parent = parent;
		}
		public ConfigurationElementsCollection(IConfigurationType parent, IConfigurationElementsCollection copy) : base(copy as Dictionary<string, IConfigurationElement>)
		{
			if (!(parent is IConfigurationSection) && !(parent is IConfigurationElement))
			{
				throw new InvalidOperationException("Parent must be a section or an element");
			}
			this._parent = parent;
		}
        public IConfigurationElementsCollection Clone(IConfigurationType parent)
        {
            ConfigurationElementsCollection clone = new ConfigurationElementsCollection(parent);
            foreach (IConfigurationElement element in this.Values)
                clone.Add(element.ConfigKey, element.Clone());
            return clone;
        }
		public void Replace(string oldKey, string newKey)
		{
			IConfigurationElement val = base[oldKey];
			base.Remove(oldKey);
			val.ConfigKey = newKey;
			base.Add(newKey, val);
		}
	}
}

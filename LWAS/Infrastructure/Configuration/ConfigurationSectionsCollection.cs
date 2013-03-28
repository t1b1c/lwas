/*
 * Copyright 2006-2012 TIBIC SOLUTIONS
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
	public class ConfigurationSectionsCollection : Dictionary<string, IConfigurationSection>, IConfigurationSectionsCollection, IDictionary<string, IConfigurationSection>, ICollection<KeyValuePair<string, IConfigurationSection>>, IEnumerable<KeyValuePair<string, IConfigurationSection>>, IEnumerable
	{
		private IConfiguration _parent;
		public IConfiguration Parent
		{
			get
			{
				return this._parent;
			}
		}
		public ConfigurationSectionsCollection(IConfiguration parent)
		{
			this._parent = parent;
		}
		public ConfigurationSectionsCollection(IConfiguration parent, IConfigurationSectionsCollection copy) : base(copy as Dictionary<string, IConfigurationSection>)
		{
			this._parent = parent;
		}
        public IConfigurationSectionsCollection Clone(IConfiguration parent)
        {
            ConfigurationSectionsCollection clone = new ConfigurationSectionsCollection(parent);
            foreach (IConfigurationSection section in this.Values)
                clone.Add(section.ConfigKey, section.Clone());
            return clone;
        }
		public void Replace(string oldKey, string newKey)
		{
			IConfigurationSection val = base[oldKey];
			base.Remove(oldKey);
			val.ConfigKey = newKey;
			base.Add(newKey, val);
		}
	}
}

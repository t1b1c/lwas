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
using System.Web.UI;

using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Infrastructure;

namespace LWAS.WebParts.Editors
{
	public class ConfigurationDataSourceView : DataSourceView
	{
		protected const string CONFIG_KEY = "configKey";
		private IConfiguration _configuration = null;
		private IConfigurationElementsCollection _item = null;
		public IConfiguration Configuration
		{
			get
			{
				return this._configuration;
			}
			set
			{
				this._configuration = value;
			}
		}
		protected virtual IConfigurationElementsCollection Item
		{
			get
			{
				return this._item;
			}
			set
			{
				this._item = value;
			}
		}
		public override bool CanDelete
		{
			get
			{
				return true;
			}
		}
		public override bool CanInsert
		{
			get
			{
				return true;
			}
		}
		public override bool CanUpdate
		{
			get
			{
				return true;
			}
		}
		public ConfigurationDataSourceView(IDataSource owner, string propertyName) : base(owner, propertyName)
		{
			if (null == owner)
			{
				throw new ArgumentNullException("owner");
			}
			if (!(owner is ConfigurationDataSource))
			{
				throw new InvalidOperationException("Owner is not a ConfigurationDataSource");
			}
			this._configuration = (owner as ConfigurationDataSource).Configuration;
			if (null == this._configuration)
			{
				throw new InvalidOperationException("DataSource does not have a Configuration");
			}
			this._item = (ReflectionServices.ExtractValue(this._configuration, propertyName) as IConfigurationElementsCollection);
			if (null == this._item)
			{
				throw new InvalidOperationException("Result is not a collection of configuration elements");
			}
		}
		protected override IEnumerable ExecuteSelect(DataSourceSelectArguments selectArgs)
		{
			Dictionary<string, object> ret = new Dictionary<string, object>();
			foreach (IConfigurationElement element in this._item.Values)
			{
				ret.Add("configKey", element.ConfigKey);
				foreach (IConfigurationElementAttribute attribute in element.Attributes.Values)
				{
					ret.Add(attribute.ConfigKey, attribute.Value);
				}
			}
			return ret;
		}
		protected override int ExecuteDelete(IDictionary keys, IDictionary values)
		{
			string configKey = (string)keys["configKey"];
			int result;
			if (this._item.Remove(configKey))
			{
				result = 1;
			}
			else
			{
				result = 0;
			}
			return result;
		}
		protected override int ExecuteInsert(IDictionary values)
		{
			string configKey = (string)values["configKey"];
			IConfigurationElement element = null;
			if (this._item.Parent is IConfigurationSection)
			{
				element = (this._item.Parent as IConfigurationSection).AddElement(configKey);
			}
			else
			{
				if (this._item.Parent is IConfigurationElement)
				{
					element = (this._item.Parent as IConfigurationElement).AddElement(configKey);
				}
			}
			int result;
			if (null != element)
			{
				foreach (object key in values.Keys)
				{
					if ("configKey" != key.ToString())
					{
						element.AddAttribute(key.ToString()).Value = values[key];
					}
				}
				result = 1;
			}
			else
			{
				result = 0;
			}
			return result;
		}
		protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			string configKey = (string)values["configKey"];
			IConfigurationElement element = this._item[configKey];
			int result;
			if (null != element)
			{
				foreach (object key in values.Keys)
				{
					if ("configKey" != key.ToString())
					{
						if (element.Attributes.ContainsKey(key.ToString()))
						{
							element.GetAttributeReference(key.ToString()).Value = key;
						}
						else
						{
							element.AddAttribute(key.ToString()).Value = values[key];
						}
					}
				}
				result = 1;
			}
			else
			{
				result = 0;
			}
			return result;
		}
	}
}

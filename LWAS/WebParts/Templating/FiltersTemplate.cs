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
using System.Collections;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.DataBinding;
using LWAS.Extensible.Interfaces.WebParts;
using LWAS.Infrastructure;

namespace LWAS.WebParts.Templating
{
	public class FiltersTemplate : TableTemplateBase
	{
		private static FiltersTemplate _instance = new FiltersTemplate();
		public static FiltersTemplate Instance
		{
			get
			{
				return FiltersTemplate._instance;
			}
		}
		public override void Create(Control container, IConfigurationType config, ITemplatable templatable, IEnumerable registry, IBinder binder, WebPartManager manager)
		{
			if (null == container)
			{
				throw new ArgumentNullException("container");
			}
			if (null == config)
			{
				throw new ArgumentNullException("config");
			}
			if (!(config is IConfigurationSection))
			{
				throw new ArgumentException("config must be an IConfigurationSection");
			}
			if (!(registry is ITemplatingItemsCollection))
			{
				throw new ArgumentException("registry must be an ITemplatingItemsCollection");
			}
			ITemplatingItemsCollection filters = registry as ITemplatingItemsCollection;
			IDictionary<string, IConfigurationElement> itemElements = (config as IConfigurationSection).Elements;
			if (itemElements.ContainsKey("filter"))
			{
				Table table;
				if (container is Table)
				{
					table = (container as Table);
				}
				else
				{
					table = new Table();
					table.ID = container.ID + "filter";
				}
				table.ApplyStyle(templatable.FilterStyle);
				for (int i = 0; i < filters.Count; i++)
				{
					foreach (IConfigurationElement element in itemElements["filter"].Elements.Values)
					{
						string prefix = "filter" + element.ConfigKey + i.ToString();
						this.DisplayItem(table, element, prefix, binder, filters[i], templatable.FilterRowStyle, templatable.InvalidItemStyle, manager);
					}
				}
				if (!(container is Table))
				{
					container.Controls.Add(table);
				}
			}
		}
		public override void Extract(Control container, IConfigurationType config, IEnumerable registry, WebPartManager manager)
		{
			if (null == container)
			{
				throw new ArgumentNullException("container");
			}
			if (null == config)
			{
				throw new ArgumentNullException("config");
			}
			if (!(config is IConfigurationSection))
			{
				throw new ArgumentException("config must be an IConfigurationSection");
			}
			if (!(registry is ITemplatingItemsCollection))
			{
				throw new ArgumentException("registry must be an ITemplatingItemsCollection");
			}
			ITemplatingItemsCollection filters = registry as ITemplatingItemsCollection;
			if (filters.Count != 0)
			{
				IDictionary<string, IConfigurationElement> itemElements = (config as IConfigurationSection).Elements;
				if (itemElements.ContainsKey("filter"))
				{
					for (int i = 0; i < filters.Count; i++)
					{
						Dictionary<string, object> data = filters[i].Data as Dictionary<string, object>;
						foreach (IConfigurationElement element in itemElements["filter"].Elements.Values)
						{
							string prefix = "filter" + element.ConfigKey + i.ToString();
							foreach (IConfigurationElement fieldElement in element.Elements.Values)
							{
								foreach (IConfigurationElement propertyElement in fieldElement.Elements.Values)
								{
									if (propertyElement.Attributes.ContainsKey("push"))
									{
										string propertyName = propertyElement.GetAttributeReference("member").Value.ToString();
										string sourcePropertyName = propertyElement.GetAttributeReference("push").Value.ToString();
										if (!data.ContainsKey(sourcePropertyName))
										{
											data.Add(sourcePropertyName, null);
										}
										string id = string.Concat(new string[]
										{
											container.ID, 
											"-", 
											element.ConfigKey, 
											"-", 
											prefix, 
											"-", 
											fieldElement.ConfigKey, 
											"-ctrl"
										});
										Control fieldControl = ReflectionServices.FindControlEx(id, container);
										if (null != fieldControl)
										{
											data[sourcePropertyName] = ReflectionServices.ExtractValue(fieldControl, propertyName);
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}
}

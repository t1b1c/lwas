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
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.WebParts;

namespace LWAS.WebParts.Templating
{
	public class CommandersTemplate : TableTemplateBase
	{
		private static CommandersTemplate _instance = new CommandersTemplate();
		public static CommandersTemplate Instance
		{
			get
			{
				return CommandersTemplate._instance;
			}
		}
		public override void Create(Control container, IConfigurationType config, ITemplatable templatable, IEnumerable registry, WebPartManager manager)
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
			if (!(registry is Dictionary<string, Control>))
			{
				throw new ArgumentException("registry must be a Dictionary<string,Control>");
			}
			Dictionary<string, Control> commanders = registry as Dictionary<string, Control>;
			commanders.Clear();
			IDictionary<string, IConfigurationElement> itemElements = (config as IConfigurationSection).Elements;
			if (itemElements.ContainsKey("commanders"))
			{
				Table table;
				if (container is Table)
				{
					table = (container as Table);
				}
				else
				{
					table = new Table();
				}
				if (string.IsNullOrEmpty(table.ID))
				{
					table.ID = container.ID + "commanders";
				}
				table.ApplyStyle(templatable.CommandersStyle);
				foreach (IConfigurationElement element in itemElements["commanders"].Elements.Values)
				{
					this.DisplayItem(table, element, element.ConfigKey, null, null, templatable.CommandersRowStyle, templatable.InvalidItemStyle, commanders, manager);
				}
				if (!(container is Table))
				{
					container.Controls.Add(table);
				}
			}
		}
	}
}

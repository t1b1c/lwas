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
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.WebParts;

namespace LWAS.WebParts.Templating
{
	public class FooterTemplate : TableTemplateBase
	{
		private static FooterTemplate _instance = new FooterTemplate();
		public static FooterTemplate Instance
		{
			get
			{
				return FooterTemplate._instance;
			}
		}
		public override void Create(Control container, IConfigurationType config, ITemplatable templatable, WebPartManager manager)
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
			IDictionary<string, IConfigurationElement> itemElements = (config as IConfigurationSection).Elements;
			if (itemElements.ContainsKey("footer"))
			{
				Table table;
				if (container is Table)
				{
					table = (container as Table);
				}
				else
				{
					table = new Table();
					table.ID = container.ID + "footer";
				}
				table.ApplyStyle(templatable.FooterStyle);
				foreach (IConfigurationElement element in itemElements["footer"].Elements.Values)
				{
					this.DisplayItem(table, element, "footer-" + element.ConfigKey, null, null, templatable.FooterRowStyle, templatable.InvalidItemStyle, manager);
				}
				if (!(container is Table))
				{
					container.Controls.Add(table);
				}
			}
		}
	}
}

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
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.DataBinding;
using LWAS.Extensible.Interfaces.WebParts;

namespace LWAS.WebParts.Templating
{
	public class TotalsTemplate : TableTemplateBase
	{
		private static TotalsTemplate _instance = new TotalsTemplate();
		public static TotalsTemplate Instance
		{
			get
			{
				return TotalsTemplate._instance;
			}
		}
		public override void Create(Control container, IConfigurationType config, ITemplatable templatable, IBinder binder, ITemplatingItem item, int itemIndex, WebPartManager manager)
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
			if ((config as IConfigurationSection).Elements.ContainsKey("totals"))
			{
				Table table;
				if (container is Table)
				{
					table = (container as Table);
				}
				else
				{
					table = new Table();
					table.ApplyStyle(templatable.TotalsStyle);
				}
				IConfigurationElement element = (config as IConfigurationSection).GetElementReference("totals");
				foreach (IConfigurationElement rowElement in element.Elements.Values)
				{
					this.DisplayItem(table, rowElement, "totals" + itemIndex.ToString(), binder, item, templatable.TotalsRowStyle, templatable.InvalidItemStyle, manager);
				}
				if (!(container is Table))
				{
					container.Controls.Add(table);
				}
			}
		}
	}
}

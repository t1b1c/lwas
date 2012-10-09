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
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.DataBinding;
using LWAS.Extensible.Interfaces.WebParts;

namespace LWAS.WebParts.Templating
{
	public class GroupingTemplate : TableTemplateBase
	{
		private static GroupingTemplate _instance = new GroupingTemplate();
		public static GroupingTemplate Instance
		{
			get
			{
				return GroupingTemplate._instance;
			}
		}
		public override void Create(Control container, IConfigurationType config, ITemplatable templatable, IBinder binder, ITemplatingItem item, int itemIndex, WebPartManager manager)
		{
			this.Create(container, config, templatable, binder, item, itemIndex, manager, false);
		}
		public virtual void Create(Control container, IConfigurationType config, ITemplatable templatable, IBinder binder, ITemplatingItem item, int itemIndex, WebPartManager manager, bool keepTable)
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
			Table containerTable;
			if (container is Table)
			{
				containerTable = (container as Table);
			}
			else
			{
				containerTable = new Table();
				containerTable.ID = container.ID + "grouping";
			}
			IConfigurationElement element = (config as IConfigurationSection).GetElementReference("grouping");
			Table table;
			if (keepTable)
			{
				table = containerTable;
			}
			else
			{
				TableRow tr = new TableRow();
				TableCell tc = new TableCell();
				table = new Table();
				tr.ID = "groupingTable" + itemIndex.ToString() + "row";
				tc.ID = "groupingTable" + itemIndex.ToString() + "cell";
				table.ID = "groupingTable" + itemIndex.ToString();
				tc.Controls.Add(table);
				tr.Cells.Add(tc);
				containerTable.Rows.Add(tr);
				if (element.Attributes.ContainsKey("span"))
				{
					int columnSpan = 1;
					int.TryParse(element.GetAttributeReference("span").Value.ToString(), out columnSpan);
					tc.ColumnSpan = columnSpan;
                }
                if (element.Attributes.ContainsKey("rowspan"))
                {
                    int rowSpan = 1;
                    int.TryParse(element.GetAttributeReference("rowspan").Value.ToString(), out rowSpan);
                    tc.RowSpan = rowSpan;
                }
			}
			table.ApplyStyle(templatable.GroupingStyle);
			foreach (IConfigurationElement rowElement in element.Elements.Values)
			{
				this.DisplayItem(table, rowElement, "grouping" + itemIndex.ToString(), binder, item, templatable.GroupRowStyle, templatable.InvalidItemStyle, manager);
			}
			if (!(container is Table))
			{
				container.Controls.Add(containerTable);
			}
		}
	}
}

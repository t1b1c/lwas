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
using LWAS.Extensible.Interfaces.DataBinding;
using LWAS.Extensible.Interfaces.Monitoring;
using LWAS.Extensible.Interfaces.WebParts;
using LWAS.Infrastructure;

namespace LWAS.WebParts.Templating
{
	public class TableTemplateBase
	{
		public virtual void Create(Control container, IConfigurationType config, ITemplatable templatable, WebPartManager manager)
		{
			throw new NotImplementedException();
		}
		public virtual void Create(Control container, IConfigurationType config, ITemplatable templatable, IEnumerable registry, WebPartManager manager)
		{
			throw new NotImplementedException();
		}
		public virtual void Create(Control container, IConfigurationType config, ITemplatable templatable, IEnumerable registry, IBinder binder, WebPartManager manager)
		{
			throw new NotImplementedException();
		}
		public virtual void Create(Control container, IConfigurationType config, ITemplatable templatable, IBinder binder, ITemplatingItem item, int itemIndex, WebPartManager manager)
		{
			throw new NotImplementedException();
		}
		public virtual void Create(Control container, IConfigurationType config, ITemplatable templatable, IEnumerable registry, IBinder binder, ITemplatingItem item, int itemIndex, WebPartManager manager)
		{
			throw new NotImplementedException();
		}
		public virtual void Extract(Control container, IConfigurationType config, IEnumerable registry, WebPartManager manager)
		{
			throw new NotImplementedException();
		}
		public virtual void Extract(Control container, IConfigurationType config, ITemplatable templatable, IBinder binder, IEnumerable registry, ITemplatingItem item, int itemIndex, int itemsCount, WebPartManager manager)
		{
			throw new NotImplementedException();
		}
		public virtual void DisplayItem(Table table, IConfigurationElement element, string index, IBinder binder, ITemplatingItem item, TableItemStyle style, TableItemStyle invalidStyle, WebPartManager manager)
		{
			this.DisplayItem(table, element, index, binder, item, style, invalidStyle, null, manager);
		}
		public virtual void DisplayItem(Table table, IConfigurationElement element, string index, IBinder binder, ITemplatingItem item, TableItemStyle style, TableItemStyle invalidStyle, Dictionary<string, Control> registry, WebPartManager manager)
		{
			string[] span = null;
			if (element.Attributes.ContainsKey("span") && null != element.GetAttributeReference("span").Value)
				span = element.GetAttributeReference("span").Value.ToString().Split(new char[] { ',' });

            string[] rowspan = null;
            if (element.Attributes.ContainsKey("rowspan") && null != element.GetAttributeReference("rowspan").Value)
                rowspan = element.GetAttributeReference("rowspan").Value.ToString().Split(new char[] { ',' });

            TableRow tr = new TableRow();
			tr.ID = string.Concat(new string[]
			{
				table.ID, 
				"-", 
				element.ConfigKey, 
				"-", 
				index
			});
			tr.Attributes["key"] = element.ConfigKey;
			tr.ApplyStyle(style);
			foreach (IConfigurationElementAttribute attribute in element.Attributes.Values)
			{
				if ("span" != attribute.ConfigKey && "rowspan" != attribute.ConfigKey)
				{
					tr.Style.Add(attribute.ConfigKey, attribute.Value.ToString());
				}
			}
			table.Rows.Add(tr);
			int count = 0;
			foreach (IConfigurationElement controlElement in element.Elements.Values)
			{
				TableCell tc = new TableCell();
				tc.ID = tr.ID + "-" + controlElement.ConfigKey;
				tc.Attributes["key"] = controlElement.ConfigKey;
				if (span != null && span.Length > count)
				{
					int columnSpan = 1;
					int.TryParse(span[count], out columnSpan);
					tc.ColumnSpan = columnSpan;
                    
                    if (rowspan != null && rowspan.Length > count)
                    {
                        int rowSpan = 1;
                        int.TryParse(rowspan[count], out rowSpan);
                        tc.RowSpan = rowSpan;
                    }
                    count++;
				}
				tr.Cells.Add(tc);
				string pullpush = null;
				foreach (IConfigurationElement propertyElement in controlElement.Elements.Values)
				{
					if (propertyElement.Attributes.ContainsKey("for") && "cell" == propertyElement.GetAttributeReference("for").Value.ToString() && propertyElement.Attributes.ContainsKey("member") && propertyElement.Attributes.ContainsKey("value"))
					{
						try
						{
							ReflectionServices.SetValue(tc, propertyElement.GetAttributeReference("member").Value.ToString(), propertyElement.GetAttributeReference("value").Value);
						}
						catch (Exception ex)
						{
							ControlFactory.Instance.Monitor.Register(ControlFactory.Instance, ControlFactory.Instance.Monitor.NewEventInstance("set cell attributes error", null, ex, EVENT_TYPE.Error));
						}
					}
					if (propertyElement.Attributes.ContainsKey("pull"))
					{
						pullpush = propertyElement.GetAttributeReference("pull").Value.ToString();
					}
					else
					{
						if (propertyElement.Attributes.ContainsKey("push"))
						{
							pullpush = propertyElement.GetAttributeReference("push").Value.ToString();
						}
					}
				}
				Control cellControl = ControlFactory.Instance.CreateControl(controlElement, index, binder, item, tc, invalidStyle, registry, manager);
				if (null != cellControl)
				{
					cellControl.ID = string.Concat(new string[]
					{
						table.ID, 
						"-", 
						element.ConfigKey, 
						"-", 
						index, 
						"-", 
						controlElement.ConfigKey, 
						"-ctrl"
					});
					tc.Controls.Add(cellControl);
					if (cellControl is BaseDataBoundControl && !string.IsNullOrEmpty(pullpush))
					{
						if (!item.BoundControls.ContainsKey(pullpush))
						{
							item.BoundControls.Add(pullpush, new List<BaseDataBoundControl>());
						}
						item.BoundControls[pullpush].Add((BaseDataBoundControl)cellControl);
					}
				}
			}
		}
	}
}

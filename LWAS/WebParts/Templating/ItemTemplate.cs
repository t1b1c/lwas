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

using LWAS.CustomControls;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.DataBinding;
using LWAS.Extensible.Interfaces.WebParts;
using LWAS.Infrastructure;

namespace LWAS.WebParts.Templating
{
	public class ItemTemplate : TableTemplateBase
	{
		private static ItemTemplate _instance = new ItemTemplate();
		public static ItemTemplate Instance
		{
			get
			{
				return ItemTemplate._instance;
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
			IDictionary<string, IConfigurationElement> itemElements = (config as IConfigurationSection).Elements;
			Table table;
			if (container is Table)
			{
				table = (container as Table);
			}
			else
			{
				table = new Table();
            }
            table.ApplyStyle(templatable.DetailsStyle);

            Panel statusPanel = new Panel();
            HiddenField hiddenReadOnly = new HiddenField();
            hiddenReadOnly.ID = itemIndex.ToString() + "-readOnly";
            hiddenReadOnly.Value = item.IsReadOnly.ToString();
            statusPanel.Controls.Add(hiddenReadOnly);
            HiddenField hiddenNew = new HiddenField();
            hiddenNew.ID = itemIndex.ToString() + "-new";
            hiddenNew.Value = item.IsNew.ToString();
            statusPanel.Controls.Add(hiddenNew);
            HiddenField hiddenCurrent = new HiddenField();
            hiddenCurrent.ID = itemIndex.ToString() + "-current";
            hiddenCurrent.Value = item.IsCurrent.ToString();
            statusPanel.Controls.Add(hiddenCurrent);
            HiddenField hiddenHasChanges = new HiddenField();
            hiddenHasChanges.ID = itemIndex.ToString() + "-hasChanges";
            hiddenHasChanges.Value = item.HasChanges.ToString();
            statusPanel.Controls.Add(hiddenHasChanges);
            HiddenField hiddenIsValid = new HiddenField();
            hiddenIsValid.ID = itemIndex.ToString() + "-isValid";
            hiddenIsValid.Value = item.IsValid.ToString();
            statusPanel.Controls.Add(hiddenIsValid);
            HiddenField hiddenInvalidMember = new HiddenField();
            hiddenInvalidMember.ID = itemIndex.ToString() + "-invalidMember";
            hiddenInvalidMember.Value = item.InvalidMember;
            statusPanel.Controls.Add(hiddenInvalidMember);

			if (null != item)
			{
				item.BoundControls.Clear();
			}
			foreach (IConfigurationElement element in itemElements.Values)
			{
				if (!("selectors" == element.ConfigKey) && !("commanders" == element.ConfigKey) && !("filter" == element.ConfigKey) && !("header" == element.ConfigKey) && !("footer" == element.ConfigKey) && !("grouping" == element.ConfigKey) && !("totals" == element.ConfigKey))
				{
					if (item.IsCurrent && !item.IsReadOnly)
					{
						this.DisplayItem(table, element, itemIndex.ToString(), binder, item, templatable.EditRowStyle, templatable.InvalidItemStyle, manager);
					}
					else
					{
						if (item.IsCurrent)
						{
							this.DisplayItem(table, element, itemIndex.ToString(), binder, item, templatable.SelectedRowStyle, templatable.InvalidItemStyle, manager);
						}
						else
						{
							if (itemIndex % 2 == 0)
							{
								this.DisplayItem(table, element, itemIndex.ToString(), binder, item, templatable.RowStyle, templatable.InvalidItemStyle, manager);
							}
							else
							{
								this.DisplayItem(table, element, itemIndex.ToString(), binder, item, templatable.AlternatingRowStyle, templatable.InvalidItemStyle, manager);
							}
						}
					}
				}
			}
			if (table.Rows.Count > 0)
			{
                if (!(table.Rows[0].Cells.Count > 0))
                {
                    TableCell firstcell = new TableCell();
                    firstcell.ID = table.Rows[0].ID + "firstcell";
                    table.Rows[0].Cells.Add(firstcell);
                }

                table.Rows[0].Cells[0].Controls.Add(statusPanel);
			}
		}
		public override void Extract(Control container, IConfigurationType config, ITemplatable templatable, IBinder binder, IEnumerable registry, ITemplatingItem item, int itemIndex, int itemsCount, WebPartManager manager)
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
				throw new ArgumentException("registry must be ITemplatingItemsCollection");
			}
			ITemplatingItemsCollection items = registry as ITemplatingItemsCollection;
			items.Clear();
			IDictionary<string, IConfigurationElement> itemElements = (config as IConfigurationSection).Elements;
			for (int i = 0; i < itemsCount; i++)
			{
				item = new TemplatingItem();
				Control hiddenReadOnly = ReflectionServices.FindControlEx(i.ToString() + "-readOnly", container);
				if (null != hiddenReadOnly)
				{
					bool isReadOnly = false;
					bool.TryParse(((HiddenField)hiddenReadOnly).Value, out isReadOnly);
					item.IsReadOnly = isReadOnly;
				}
				Control hiddenNew = ReflectionServices.FindControlEx(i.ToString() + "-new", container);
				if (null != hiddenNew)
				{
					bool isNew = false;
					bool.TryParse(((HiddenField)hiddenNew).Value, out isNew);
					item.IsNew = isNew;
				}
				Control hiddenCurrent = ReflectionServices.FindControlEx(i.ToString() + "-current", container);
				if (null != hiddenCurrent)
				{
					bool isCurrent = false;
					bool.TryParse(((HiddenField)hiddenCurrent).Value, out isCurrent);
					item.IsCurrent = isCurrent;
				}
				Control hiddenHasChanges = ReflectionServices.FindControlEx(i.ToString() + "-hasChanges", container);
				if (null != hiddenHasChanges)
				{
					bool hasChanges = false;
					bool.TryParse(((HiddenField)hiddenHasChanges).Value, out hasChanges);
					item.HasChanges = hasChanges;
				}
				Control hiddenIsValid = ReflectionServices.FindControlEx(i.ToString() + "-isValid", container);
				if (null != hiddenIsValid)
				{
					bool isValid = false;
					bool.TryParse(((HiddenField)hiddenIsValid).Value, out isValid);
					item.IsValid = isValid;
				}
				Control hiddenInvalidMember = ReflectionServices.FindControlEx(i.ToString() + "-invalidMember", container);
				if (null != hiddenInvalidMember)
				{
					item.InvalidMember = ((HiddenField)hiddenInvalidMember).Value;
				}
				this.PopulateItem(container, config, item, i.ToString());
				items.Add(item);
			}
		}
		public virtual void PopulateItem(Control container, IConfigurationType config, ITemplatingItem item, string index)
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
			item.Data = new Dictionary<string, object>();
			Dictionary<string, object> data = item.Data as Dictionary<string, object>;
			IDictionary<string, IConfigurationElement> itemElements = (config as IConfigurationSection).Elements;
			foreach (IConfigurationElement element in itemElements.Values)
			{
				if (!("selectors" == element.ConfigKey) && !("commanders" == element.ConfigKey) && !("filter" == element.ConfigKey) && !("header" == element.ConfigKey) && !("footer" == element.ConfigKey))
				{
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
								if (!string.IsNullOrEmpty(index))
								{
									string id = string.Concat(new string[]
									{
										container.ID, 
										"-", 
										element.ConfigKey, 
										"-", 
										index, 
										"-", 
										fieldElement.ConfigKey, 
										"-ctrl"
									});
									Control fieldControl = ReflectionServices.FindControlEx(id, container);
									if (null != fieldControl)
									{
										object val = ReflectionServices.ExtractValue(fieldControl, propertyName);
										if (fieldControl is DateTextBox && val == null && propertyName == "Date")
										{
											val = ((DateTextBox)fieldControl).Text;
										}
										data[sourcePropertyName] = val;
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

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

using LWAS.CustomControls;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.DataBinding;
using LWAS.Infrastructure;

namespace LWAS.WebParts.Parsers
{
	public class FormViewBuilder
	{
		private IDictionary<string, object> _boundControls = new Dictionary<string, object>();
		private IDictionary<string, object> _dataSources = new Dictionary<string, object>();
		public void Build(FormView form, IConfigurationSection section, IBinder binder, IDictionary<string, object> boundControls, IDictionary<string, object> dataSources)
		{
			if (null == form)
			{
				throw new ArgumentNullException("form");
			}
			if (null == section)
			{
				throw new ArgumentNullException("section");
			}
			if (null == binder)
			{
				throw new ArgumentNullException("binder");
			}
			this._boundControls = boundControls;
			this._boundControls.Clear();
			this._dataSources = dataSources;
			this._dataSources.Clear();
			form.EditItemTemplate = new TemplateHelper(this.BuildItemTemplate(section, binder, FormViewMode.Edit), section);
		}
		private Table BuildItemTemplate(IConfigurationSection section, IBinder binder, FormViewMode mode)
		{
			Table table = new Table();
			table.ID = "table";
			foreach (IConfigurationElement rowElement in section.Elements.Values)
			{
				string[] span = null;
				if (rowElement.Attributes.ContainsKey("span"))
					span = rowElement.GetAttributeReference("span").Value.ToString().Split(new char[] { ',' });

                string[] rowspan = null;
                if (rowElement.Attributes.ContainsKey("rowspan"))
                    rowspan = rowElement.GetAttributeReference("rowspan").Value.ToString().Split(new char[] { ',' });

                TableRow tr = new TableRow();
				tr.ID = "tr" + table.Rows.Count.ToString();
				int count = 0;
				foreach (IConfigurationElement controlElement in rowElement.Elements.Values)
				{
					TableCell tc = new TableCell();
					tc.ID = tr.ID + "tc" + tr.Cells.Count;
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
                    string type = null;
                    if (controlElement.Attributes.ContainsKey("type"))
                        type = controlElement.GetAttributeReference("type").Value.ToString();
                    if (!String.IsNullOrEmpty(type))
                    {
                        Control cellControl = this.CreateControl(type, mode);
                        cellControl.ID = tc.ID + controlElement.ConfigKey;
                        foreach (IConfigurationElement controlPropertyElement in controlElement.Elements.Values)
                        {
                            string propertyName = controlPropertyElement.GetAttributeReference("member").Value.ToString();
                            if (controlPropertyElement.Attributes.ContainsKey("value"))
                            {
                                ReflectionServices.SetValue(cellControl, propertyName, controlPropertyElement.GetAttributeReference("value").Value);
                            }
                            if (controlPropertyElement.Attributes.ContainsKey("isList") && (bool)controlPropertyElement.GetAttributeReference("isList").Value)
                            {
                                ListItemCollection list = ReflectionServices.ExtractValue(cellControl, propertyName) as ListItemCollection;
                                if (null == list)
                                {
                                    throw new InvalidOperationException(string.Format("Member '{0}' is not a ListItemCollection", propertyName));
                                }
                                if (controlPropertyElement.Attributes.ContainsKey("hasEmpty") && (bool)controlPropertyElement.GetAttributeReference("hasEmpty").Value)
                                {
                                    if (list is ListItemCollection)
                                        ((ListItemCollection)list).Add("");
                                }
                                foreach (IConfigurationElement listItemElement in controlPropertyElement.Elements.Values)
                                {
                                    list.Add(new ListItem(listItemElement.GetAttributeReference("text").Value.ToString(), listItemElement.GetAttributeReference("value").Value.ToString()));
                                }
                            }
                            if (controlPropertyElement.Attributes.ContainsKey("pull"))
                            {
                                string pull = controlPropertyElement.GetAttributeReference("pull").Value.ToString();
                                IBindingItem bindingItem = binder.NewBindingItemInstance();
                                bindingItem.Source = null;
                                bindingItem.SourceProperty = pull;
                                bindingItem.Target = cellControl;
                                bindingItem.TargetProperty = propertyName;
                                binder.BindingItems.Add(bindingItem);
                                if (cellControl is BaseDataBoundControl)
                                {
                                    this._boundControls.Add(pull, cellControl);
                                    if (!this._dataSources.ContainsKey(pull))
                                    {
                                        this._dataSources.Add(pull, null);
                                    }
                                }
                            }
                        }
                        tc.Controls.Add(cellControl);
                    }
					tr.Cells.Add(tc);
				}
				table.Rows.Add(tr);
			}
			return table;
		}
		private Control CreateControl(string type, FormViewMode mode)
		{
			Control result;
			if (mode == FormViewMode.ReadOnly)
			{
				result = this.CreateReadOnlyControl(type);
			}
			else
			{
				result = this.CreateEditableControl(type);
			}
			return result;
		}
		private Control CreateEditableControl(string type)
		{
			Control control;
			switch (type)
			{
				case "Text":
				{
					control = new StyledTextBox();
					break;
				}
				case "Date":
				{
					control = new MaskedCalendar();
					break;
				}
                case "Number":
                {
                    control = new NumberTextBox();
                    break;
                }
				case "DropDownList":
				{
					control = new StatelessDropDownList();
					break;
				}
				case "Link":
				{
					control = new HyperLink();
					break;
				}
				case "CheckBox":
				{
					control = new CheckBox();
					break;
				}
				case "Label":
				{
					control = new Label();
					((Label)control).Text = "[label]";
					break;
				}
				case "Hidden":
				{
					control = new HiddenField();
					break;
				}
				default:
				{
                	throw new ArgumentException("Unknown form view field type " + type);
				}
			}
			return control;
		}
		private Control CreateReadOnlyControl(string type)
		{
			Control control;
			switch (type)
			{
				case "Text":
				{
					control = new TextBox();
					break;
				}
				case "Date":
				{
					control = new MaskedCalendar();
					break;
                }
                case "Number":
                {
                    control = new NumberTextBox();
                    break;
                }
				case "DropDownList":
				{
					control = new StatelessDropDownList();
					break;
				}
				case "Link":
				{
					control = new HyperLink();
					break;
				}
				case "CheckBox":
				{
					control = new CheckBox();
					break;
				}
				case "Label":
				{
					control = new Label();
					((Label)control).Text = "[label]";
					break;
				}
				case "Hidden":
				{
					control = new HiddenField();
					break;
				}
				default:
				{
			        throw new ArgumentException("Unknown form view field type " + type);
				}
			}
			return control;
		}
	}
}

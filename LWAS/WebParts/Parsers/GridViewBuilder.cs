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
using System.Web.UI.WebControls;

using LWAS.CustomControls;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Infrastructure;

namespace LWAS.WebParts.Parsers
{
	public class GridViewBuilder
	{
		private IDictionary<string, object> _boundControls = new Dictionary<string, object>();
		private IDictionary<string, object> _dataSources = new Dictionary<string, object>();
		public void Build(GridView grid, IConfigurationSection section, IDictionary<string, object> boundControls, IDictionary<string, object> dataSources)
		{
			this._boundControls = boundControls;
			this._dataSources = dataSources;
			if (null == grid)
			{
				throw new ArgumentNullException("grid");
			}
			if (null == section)
			{
				throw new ArgumentNullException("section");
			}
			foreach (IConfigurationElement element in section.Elements.Values)
			{
				if ("keys" == element.ConfigKey)
				{
					grid.DataKeyNames = element.GetAttributeReference("names").Value.ToString().Split(new char[]
					{
						','
					});
				}
				else
				{
					BoundField field = this.GetField(element);
					if (null != field)
					{
						grid.Columns.Add(field);
					}
					this.ParseProperties(field, element);
					if (field is DropDownListField && !string.IsNullOrEmpty(field.DataField))
					{
						this._boundControls.Add(field.DataField, field);
						if (!this._dataSources.ContainsKey(field.DataField))
						{
							this._dataSources.Add(field.DataField, null);
						}
					}
				}
			}
		}
        private BoundField GetField(IConfigurationElement element)
        {
            if (element.Attributes.ContainsKey("type"))
            {
                string type = element.Attributes["type"].Value.ToString();

                BoundField field;
                switch (type)
                {
                    case "Text":
                        {
                            field = new BoundField();
                            break;
                        }
                    case "Label":
                        {
                            field = new BoundField();
                            field.ReadOnly = true;
                            break;
                        }
                    case "Date":
                        {
                            field = new DateBoundField();
                            break;
                        }
                    case "Number":
                        {
                            field = new NumberBoundField();
                            break;
                        }
                    case "DropDownList":
                        {
                            field = new DropDownListField();
                            break;
                        }
                    case "CheckBox":
                    case "Check":
                        {
                            field = new CheckBoxField();
                            break;
                        }
                    case "Hidden":
                        {
                            field = new BoundField();
                            field.ItemStyle.CssClass = "hidden";
                            break;
                        }
                    default:
                        {
                            throw new ArgumentException("Unknown grid view field type '" + element.Attributes["type"].Value.ToString() + "'");
                        }
                }
                if (!(field is CheckBoxField))
                {
                    field.HtmlEncode = false;
                }
                if (element.Attributes.ContainsKey("DataMember"))
                    field.DataField = element.Attributes["DataMember"].Value.ToString(); ;
                if (element.Attributes.ContainsKey("HeaderText"))
                    field.HeaderText = element.Attributes["HeaderText"].Value.ToString(); ;
                if (element.Attributes.ContainsKey("FormatString"))
                {
                    try
                    {
                        field.DataFormatString = element.Attributes["FormatString"].Value.ToString();
                    }
                    catch
                    {
                    }
                }
                if (field is DropDownListField)
                {
                    if (element.Attributes.ContainsKey("DataTextField"))
                    {
                        ((DropDownListField)field).DataTextField = element.GetAttributeReference("DataTextField").Value.ToString();
                    }
                    if (element.Attributes.ContainsKey("DataValueField"))
                    {
                        ((DropDownListField)field).DataValueField = element.GetAttributeReference("DataValueField").Value.ToString();
                    }
                }
                return field;
            }
            return null;
        }
		private void ParseProperties(BoundField target, IConfigurationElement propertiesConfig)
		{
			foreach (IConfigurationElement element in propertiesConfig.Elements.Values)
			{
				try
				{
					ReflectionServices.SetValue(target, element.GetAttributeReference("member").Value.ToString(), element.GetAttributeReference("value").Value, true);
				}
				catch
				{
				}
			}
		}
	}
}

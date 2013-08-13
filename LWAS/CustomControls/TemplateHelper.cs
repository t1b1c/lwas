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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.UI;

using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Infrastructure;

namespace LWAS.CustomControls
{
	public class TemplateHelper : IBindableTemplate, ITemplate
	{
		private Control innerControl;
		private IConfigurationSection fieldsSection;
		public TemplateHelper(Control aInnerControl, IConfigurationSection aFieldsSection)
		{
			this.innerControl = aInnerControl;
			this.fieldsSection = aFieldsSection;
		}
		public virtual void InstantiateIn(Control container)
		{
			if (this.innerControl != null)
			{
				container.Controls.Add(this.innerControl);
			}
		}
		public IOrderedDictionary ExtractValues(Control container)
		{
			if (null == this.innerControl)
			{
				throw new InvalidOperationException("InnerControl not set");
			}
			if (null == this.fieldsSection)
			{
				throw new InvalidOperationException("FieldsSection not set");
			}
			OrderedDictionary values = new OrderedDictionary();
			Dictionary<string, Control> map = new Dictionary<string, Control>();
			this.BuildControlsMap(map, this.innerControl);
			int rcount = 0;
			foreach (IConfigurationElement rowElement in this.fieldsSection.Elements.Values)
			{
				int ccount = 0;
				foreach (IConfigurationElement fieldElement in rowElement.Elements.Values)
				{
					string id = string.Concat(new object[]
					{
						"tr", 
						rcount, 
						"tc", 
						ccount, 
						fieldElement.ConfigKey
					});
					if (!map.ContainsKey(id))
					{
						throw new ApplicationException("Can't find control " + id);
					}
					Control fieldControl = map[id];
					foreach (IConfigurationElement propertyElement in fieldElement.Elements.Values)
					{
						if (propertyElement.Attributes.ContainsKey("pull") && propertyElement.Attributes.ContainsKey("member"))
						{
							string pull = propertyElement.GetAttributeReference("pull").Value.ToString();
							string member = propertyElement.GetAttributeReference("member").Value.ToString();
							if (!values.Contains(pull))
							{
								values.Add(pull, ReflectionServices.ExtractValue(fieldControl, member));
							}
						}
						if (propertyElement.Attributes.ContainsKey("push") && propertyElement.Attributes.ContainsKey("member"))
						{
							string push = propertyElement.GetAttributeReference("push").Value.ToString();
							string member = propertyElement.GetAttributeReference("member").Value.ToString();
							if (!values.Contains(push))
							{
								values.Add(push, ReflectionServices.ExtractValue(fieldControl, member));
							}
						}
					}
					ccount++;
				}
				rcount++;
			}
			return values;
		}
		public void BuildControlsMap(Dictionary<string, Control> map)
		{
			this.BuildControlsMap(map, this.innerControl);
		}
		private void BuildControlsMap(Dictionary<string, Control> map, Control parent)
		{
			foreach (Control child in parent.Controls)
			{
				if (!string.IsNullOrEmpty(child.ID) && !map.ContainsKey(child.ID))
				{
					map.Add(child.ID, child);
				}
				this.BuildControlsMap(map, child);
			}
		}
	}
}

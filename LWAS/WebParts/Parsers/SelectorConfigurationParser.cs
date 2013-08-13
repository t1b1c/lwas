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

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.DataBinding;
using LWAS.Infrastructure;

namespace LWAS.WebParts.Parsers
{
	public class SelectorConfigurationParser : ConfigurationParser
	{
		public override IResult Parse(object source)
		{
			SelectorWebPart selectorWebPart = source as SelectorWebPart;
			if (null == selectorWebPart)
			{
				throw new ArgumentException("source is not a SelectorWebPart");
			}
			IConfiguration config = selectorWebPart.Configuration;
			if (null == config)
			{
				throw new ArgumentException("source has no configuration");
			}
			IBinder binder = selectorWebPart.Binder;
			if (null == binder)
			{
				throw new ArgumentException("source has no binder");
			}
			if (config.Sections.ContainsKey("SelectorProperties"))
			{
				this.ParseProperties(selectorWebPart.Selector, config.GetConfigurationSectionReference("SelectorProperties"));
			}
			if (config.Sections.ContainsKey("CriteriaFields"))
			{
				new FormViewBuilder().Build(selectorWebPart.Selector.Criteria, config.GetConfigurationSectionReference("CriteriaFields"), binder, selectorWebPart.Selector.BoundControls, selectorWebPart.Selector.DataSources);
				new DataTableBuilder().Build(selectorWebPart.Selector.CriteriaStorage, config.GetConfigurationSectionReference("CriteriaFields"));
				string[] keys = new string[selectorWebPart.Selector.CriteriaStorage.PrimaryKey.Length];
				for (int i = 0; i < selectorWebPart.Selector.CriteriaStorage.PrimaryKey.Length; i++)
				{
					keys[i] = selectorWebPart.Selector.CriteriaStorage.PrimaryKey[i].ColumnName;
				}
				selectorWebPart.Selector.Criteria.DataKeyNames = keys;
			}
			if (config.Sections.ContainsKey("ResultsFields"))
			{
				new GridViewBuilder().Build(selectorWebPart.Selector.Results, config.GetConfigurationSectionReference("ResultsFields"), selectorWebPart.Selector.BoundControls, selectorWebPart.Selector.DataSources);
			}
			return null;
		}
		private void ParseProperties(Control target, IConfigurationSection propertiesConfigSection)
		{
			foreach (IConfigurationElement element in propertiesConfigSection.Elements.Values)
			{
				ReflectionServices.SetValue(target, element.GetAttributeReference("name").Value.ToString(), element.GetAttributeReference("value").Value);
			}
		}
	}
}

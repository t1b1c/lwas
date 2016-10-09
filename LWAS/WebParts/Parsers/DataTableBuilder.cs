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
using System.Data;

using LWAS.Extensible.Interfaces.Configuration;

namespace LWAS.WebParts.Parsers
{
	public class DataTableBuilder
	{
		public void Build(DataTable table, IConfigurationType config)
		{
			if (null == table)
			{
				throw new ArgumentNullException("table");
			}
			if (null == config)
			{
				throw new ArgumentNullException("config");
			}
			IDictionary<string, IConfigurationElement> elements = null;
			if (config is IConfigurationSection)
			{
				elements = ((IConfigurationSection)config).Elements;
			}
			else
			{
				if (config is IConfigurationElement)
				{
					elements = ((IConfigurationElement)config).Elements;
				}
			}
			if (null == elements)
			{
				throw new ArgumentException("config has no Elements");
			}
			Dictionary<string, object> values = new Dictionary<string, object>();
			if (!table.Columns.Contains("PK"))
			{
				DataColumn pk = table.Columns.Add("PK", typeof(int));
				pk.AutoIncrement = true;
				pk.AutoIncrementSeed = 1L;
				pk.AutoIncrementStep = 1L;
				table.Constraints.Add("PK", pk, true);
			}
			values.Add("PK", null);
			List<DataColumn> columnsToRemove = new List<DataColumn>();
			foreach (DataColumn column in table.Columns)
			{
				columnsToRemove.Add(column);
			}
			foreach (IConfigurationElement rowElement in elements.Values)
			{
				foreach (IConfigurationElement controlElement in rowElement.Elements.Values)
				{
					foreach (IConfigurationElement propertyElement in controlElement.Elements.Values)
					{
                        if (!table.Columns.Contains(controlElement.ConfigKey))
                        {
                            table.Columns.Add(controlElement.ConfigKey);
                            columnsToRemove.Remove(table.Columns[controlElement.ConfigKey]);
                        }

						if (propertyElement.Attributes.ContainsKey("pull"))
						{
							string pull = propertyElement.GetAttributeReference("pull").Value.ToString();
							if (!table.Columns.Contains(pull))
							{
								table.Columns.Add(pull);
								columnsToRemove.Remove(table.Columns[pull]);
							}
						}
						if (propertyElement.Attributes.ContainsKey("push"))
						{
							string push = propertyElement.GetAttributeReference("push").Value.ToString();
							if (!table.Columns.Contains(push))
							{
								table.Columns.Add(push);
								columnsToRemove.Remove(table.Columns[push]);
							}
						}
					}
				}
			}
			foreach (DataColumn column in columnsToRemove)
			{
				if (column.ColumnName != "PK")
				{
					table.Columns.Remove(column);
				}
			}
		}
	}
}

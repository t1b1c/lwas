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
using System.Data.SqlClient;
using System.Web.UI.WebControls.WebParts;

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.DataBinding;
using LWAS.Infrastructure;

namespace LWAS.WebParts.Parsers
{
	public class SqlDataBridgeConfigurationParser : ConfigurationParser
	{
		public override IResult Parse(object source)
		{
			if (null == source)
			{
				throw new ArgumentNullException("source");
			}
			SqlDataBridgeWebPart sqlDataBridgeWebPart = source as SqlDataBridgeWebPart;
			if (null == sqlDataBridgeWebPart)
			{
				throw new ArgumentException("source is not a SqlDataSourceWebPart");
			}
			IConfiguration configuration = sqlDataBridgeWebPart.Configuration;
			if (null == configuration)
			{
				throw new ArgumentException("source has no configuration");
			}
			if (configuration.Sections.ContainsKey("commands"))
			{
				sqlDataBridgeWebPart.DataBridge.Commands = new Dictionary<string, SqlCommand>();
				IConfigurationSection commands = configuration.GetConfigurationSectionReference("commands");
				foreach (IConfigurationElement command in commands.Elements.Values)
				{
					sqlDataBridgeWebPart.DataBridge.Commands.Add(command.ConfigKey, this.BuildCommand(command, sqlDataBridgeWebPart.Binder, WebPartManager.GetCurrentWebPartManager(sqlDataBridgeWebPart.Page)));
				}
			}
			return null;
		}
		private SqlCommand BuildCommand(IConfigurationElement commandElement, IBinder binder, WebPartManager manager)
		{
			SqlCommand command = new SqlCommand();
			foreach (IConfigurationElementAttribute attribute in commandElement.Attributes.Values)
			{
				ReflectionServices.SetValue(command, attribute.ConfigKey, attribute.Value);
			}
			IBindingItemsCollection bindingParams = binder.NewBindingItemCollectionInstance();
			binder.BindingSet.Add(commandElement.ConfigKey, bindingParams);
			foreach (IConfigurationElement parameterElement in commandElement.Elements.Values)
			{
				SqlParameter parameter = new SqlParameter();
				foreach (IConfigurationElementAttribute parameterAttribute in parameterElement.Attributes.Values)
				{
					if ("bind" == parameterAttribute.ConfigKey)
					{
						string bindstring = parameterAttribute.Value.ToString();
						bool isOutput = parameterElement.Attributes.ContainsKey("Direction") && 
                            ("Output" == parameterElement.GetAttributeReference("Direction").Value.ToString() ||
                            "InputOutput" == parameterElement.GetAttributeReference("Direction").Value.ToString());
						if (bindstring.Contains("."))
						{
							string sourcestring = bindstring.Substring(0, bindstring.IndexOf("."));
							IBindingItem bindingItem = binder.NewBindingItemInstance();
							if (!isOutput)
							{
								bindingItem.Source = manager.FindControl(sourcestring);
								bindingItem.SourceProperty = bindstring.Substring(sourcestring.Length + 1);
								bindingItem.Target = parameter;
								bindingItem.TargetProperty = "Value";
							}
							else
							{
								bindingItem.Target = manager.FindControl(sourcestring);
								bindingItem.TargetProperty = bindstring.Substring(sourcestring.Length + 1);
								bindingItem.Source = parameter;
								bindingItem.SourceProperty = "Value";
							}
							bindingParams.Add(bindingItem);
						}
					}
					else
					{
						ReflectionServices.SetValue(parameter, parameterAttribute.ConfigKey, parameterAttribute.Value);
					}
				}
				if (null == parameter.Value)
				{
					parameter.Value = DBNull.Value;
				}
				command.Parameters.Add(parameter);
			}
			return command;
		}
	}
}

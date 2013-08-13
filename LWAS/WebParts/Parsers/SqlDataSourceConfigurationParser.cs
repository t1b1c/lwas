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
using System.Web.UI.WebControls;

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Infrastructure;

namespace LWAS.WebParts.Parsers
{
	public class SqlDataSourceConfigurationParser : ConfigurationParser
	{
		public override IResult Parse(object source)
		{
			if (null == source)
			{
				throw new ArgumentNullException("source");
			}
			SqlDataSourceWebPart sqlDataSourceWebPart = source as SqlDataSourceWebPart;
			if (null == sqlDataSourceWebPart)
			{
				throw new ArgumentException("source is not a SqlDataSourceWebPart");
			}
			SqlDataSource sqlDataSource = sqlDataSourceWebPart.DataSource as SqlDataSource;
			if (null == sqlDataSource)
			{
				throw new ArgumentException("source has no SqlDataSource as DataSource");
			}
			IConfiguration configuration = sqlDataSourceWebPart.Configuration;
			if (null == configuration)
			{
				throw new ArgumentException("source has no configuration");
			}
			this.BuildSelectCommand(sqlDataSource, configuration.GetConfigurationSectionReference("select"));
			this.BuildInsertCommand(sqlDataSource, configuration.GetConfigurationSectionReference("insert"));
			this.BuildUpdateCommand(sqlDataSource, configuration.GetConfigurationSectionReference("update"));
			this.BuildDeleteCommand(sqlDataSource, configuration.GetConfigurationSectionReference("delete"));
			return null;
		}
		private void BuildSelectCommand(SqlDataSource sqlDataSource, IConfigurationSection section)
		{
			sqlDataSource.SelectCommand = section.GetAttributeReference("Command", "text").Value.ToString();
			sqlDataSource.SelectCommandType = (SqlDataSourceCommandType)ReflectionServices.StrongTypeValue(section.GetAttributeReference("Command", "type").Value.ToString(), typeof(SqlDataSourceCommandType));
			if (section.Elements.Keys.Contains("Parameters"))
			{
				this.BuildCommand(section.GetElementReference("Parameters"), sqlDataSource.SelectParameters);
			}
		}
		private void BuildInsertCommand(SqlDataSource sqlDataSource, IConfigurationSection section)
		{
			sqlDataSource.InsertCommand = section.GetAttributeReference("Command", "text").Value.ToString();
			sqlDataSource.InsertCommandType = (SqlDataSourceCommandType)ReflectionServices.StrongTypeValue(section.GetAttributeReference("Command", "type").Value.ToString(), typeof(SqlDataSourceCommandType));
			if (section.Elements.Keys.Contains("Parameters"))
			{
				this.BuildCommand(section.GetElementReference("Parameters"), sqlDataSource.InsertParameters);
			}
		}
		private void BuildUpdateCommand(SqlDataSource sqlDataSource, IConfigurationSection section)
		{
			sqlDataSource.UpdateCommand = section.GetAttributeReference("Command", "text").Value.ToString();
			sqlDataSource.UpdateCommandType = (SqlDataSourceCommandType)ReflectionServices.StrongTypeValue(section.GetAttributeReference("Command", "type").Value.ToString(), typeof(SqlDataSourceCommandType));
			if (section.Elements.Keys.Contains("Parameters"))
			{
				this.BuildCommand(section.GetElementReference("Parameters"), sqlDataSource.UpdateParameters);
			}
		}
		private void BuildDeleteCommand(SqlDataSource sqlDataSource, IConfigurationSection section)
		{
			sqlDataSource.DeleteCommand = section.GetAttributeReference("Command", "text").Value.ToString();
			sqlDataSource.DeleteCommandType = (SqlDataSourceCommandType)ReflectionServices.StrongTypeValue(section.GetAttributeReference("Command", "type").Value.ToString(), typeof(SqlDataSourceCommandType));
			if (section.Elements.Keys.Contains("Parameters"))
			{
				this.BuildCommand(section.GetElementReference("Parameters"), sqlDataSource.DeleteParameters);
			}
		}
		private void BuildCommand(IConfigurationElement parametersSection, ParameterCollection parameters)
		{
			parameters.Clear();
			foreach (IConfigurationElement parameterElement in parametersSection.Elements.Values)
			{
				Parameter commandParameter = new Parameter();
				foreach (IConfigurationElement propertyElement in parameterElement.Elements.Values)
				{
					ReflectionServices.SetValue(commandParameter, propertyElement.GetAttributeReference("name").Value.ToString(), propertyElement.GetAttributeReference("value").Value);
				}
				parameters.Add(commandParameter);
			}
		}
	}
}

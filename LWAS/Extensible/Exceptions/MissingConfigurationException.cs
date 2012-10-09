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

using LWAS.Extensible.Interfaces.Configuration;

namespace LWAS.Extensible.Exceptions
{
	public class MissingConfigurationException : Exception
	{
		private string _message = string.Empty;
		public override string Message
		{
			get
			{
				return this._message;
			}
		}
		public MissingConfigurationException(string missing)
		{
			this._message = string.Format("Missing {0}", missing);
		}
		public MissingConfigurationException(string missing, IConfigurationType configSource)
		{
			this._message = string.Format("Missing {0} in configuration source {1}", missing, configSource.ConfigKey);
		}
		public MissingConfigurationException(IConfigurationType configSection)
		{
			this._message = string.Format("Missing section {0}", configSection.ConfigKey);
		}
		public MissingConfigurationException(IConfigurationType configChild, IConfigurationType configParent)
		{
			this._message = string.Format("Missing section {0} in {1}", configChild.ConfigKey, configParent.ConfigKey);
		}
		public MissingConfigurationException(IConfigurationType configChild, IConfigurationType configParent, IConfigurationType configSource)
		{
			this._message = string.Format("Missing element {0} in {1} in {2}", configChild.ConfigKey, configParent.ConfigKey, configSource.ConfigKey);
		}
		public MissingConfigurationException(IConfigurationType configElementAttribute, IConfigurationType configElement, IConfigurationType configSection, IConfigurationType configSource)
		{
			this._message = string.Format("Missing attribute {0} of element {1} in section {2} in configuration source {3}", new object[]
			{
				configElementAttribute.ConfigKey, 
				configElement.ConfigKey, 
				configSection.ConfigKey, 
				configSource.ConfigKey
			});
		}
	}
}

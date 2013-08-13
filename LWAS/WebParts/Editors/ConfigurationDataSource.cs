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
using System.Web.UI;

using LWAS.Extensible.Interfaces.Configuration;

namespace LWAS.WebParts.Editors
{
	public class ConfigurationDataSource : DataSourceControl
	{
		private IConfiguration _configuration = null;
		public IConfiguration Configuration
		{
			get
			{
				return this._configuration;
			}
			set
			{
				this._configuration = value;
			}
		}
		protected override DataSourceView GetView(string propertyName)
		{
			return new ConfigurationDataSourceView(this, propertyName);
		}
		protected override ICollection GetViewNames()
		{
			return null;
		}
	}
}

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
using System.Web.UI.WebControls.WebParts;

using LWAS.Extensible.Exceptions;
using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.DataSources;

namespace LWAS.WebParts
{
	public class DataSourceProviderWebPart : EditableWebPart, IDataSourceProvider, IInitializable, ILifetime
	{
		private object _dataSource;
		public virtual object DataSource
		{
			get
			{
				return this._dataSource;
			}
			set
			{
				this._dataSource = value;
				if (null != this._dataSource)
				{
					this.OnMilestone("source");
				}
			}
		}
		public DataSourceProviderWebPart()
		{
			this.Hidden = true;
			this.ExportMode = WebPartExportMode.All;
		}
		public override void Initialize()
		{
			if (null == this.Configuration)
			{
				throw new MissingProviderException("configuration provider");
			}
			if (null == this.ConfigurationParser)
			{
				throw new MissingProviderException("configuration parser");
			}
			this.ConfigurationParser.Parse(this);
			base.Initialize();
		}
	}
}

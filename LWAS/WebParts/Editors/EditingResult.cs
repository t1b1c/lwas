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

using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.Editors;

namespace LWAS.WebParts.Editors
{
	public class EditingResult : IEditingResult
	{
		private IConfiguration _configuration;
		private IConfigurationType _levelEdited;
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
		public IConfigurationType LevelEdited
		{
			get
			{
				return this._levelEdited;
			}
			set
			{
				this._levelEdited = value;
			}
		}
		public EditingResult()
		{
			this._configuration = null;
			this._levelEdited = this._configuration;
		}
		public EditingResult(IEditingResult anotherResult)
		{
			this.Configuration = anotherResult.Configuration;
			this.LevelEdited = anotherResult.LevelEdited;
		}
	}
}

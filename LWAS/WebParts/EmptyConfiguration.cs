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

using LWAS.Extensible.Exceptions;
using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Configuration;

namespace LWAS.WebParts
{
	public class EmptyConfiguration : IProvider, IInitializable, ILifetime
	{
		private IConfiguration _configurationProvider;
		private int _initialization;
		private int _creation;
		private int _change;
		private int _completion;
		private RequestInitializationCallback _requestInitialization;
		public IConfiguration ConfigurationProvider
		{
			get
			{
				return this._configurationProvider;
			}
			set
			{
				this._configurationProvider = value;
			}
		}
		public int Initialization
		{
			get
			{
				return this._initialization;
			}
			set
			{
				this._initialization = value;
			}
		}
		public int Creation
		{
			get
			{
				return this._creation;
			}
			set
			{
				this._creation = value;
			}
		}
		public int Change
		{
			get
			{
				return this._change;
			}
			set
			{
				this._change = value;
			}
		}
		public int Completion
		{
			get
			{
				return this._completion;
			}
			set
			{
				this._completion = value;
			}
		}
		public RequestInitializationCallback RequestInitialization
		{
			get
			{
				return this._requestInitialization;
			}
			set
			{
				this._requestInitialization = value;
			}
		}
		public virtual void Initialize()
		{
			if (null == this._configurationProvider)
			{
				throw new MissingConfigurationProviderException();
			}
		}
	}
}

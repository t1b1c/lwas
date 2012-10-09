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

using LWAS.Extensible.Interfaces.Storage;

namespace LWAS.Infrastructure.Storage
{
	public abstract class BaseContainer : IStorageContainer
	{
		public abstract IStorageAgent Agent
		{
			get;
		}
		public abstract string Key
		{
			get;
			set;
		}
		public abstract void Delete();
		public abstract IStorageContainer CreateContainer(string key);
		public abstract bool HasKey(string key);
		public abstract bool IsEmpty();
	}
}

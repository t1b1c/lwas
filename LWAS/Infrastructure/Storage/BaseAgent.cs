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
using System.Collections.Generic;
using System.IO;

using LWAS.Extensible.Interfaces.Storage;

namespace LWAS.Infrastructure.Storage
{
	public abstract class BaseAgent : IStorageAgent
	{
		public abstract Stream OpenStream(string key);
		public abstract void CloseStream(string key);
		public abstract string Read(string key);
		public abstract void Write(string key, string content);
		public abstract void Erase(string key);
		public abstract IList<string> List();
		public abstract bool HasKey(string key);
		public abstract void ReplaceKey(string oldkey, string newkey);
		public abstract void CleanUp();
        public abstract string Sanitize(string key);
	}
}

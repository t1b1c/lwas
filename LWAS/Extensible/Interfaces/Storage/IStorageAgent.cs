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
using System.IO;

namespace LWAS.Extensible.Interfaces.Storage
{
	public interface IStorageAgent
	{
		Stream OpenStream(string key);
		void CloseStream(string key);
		string Read(string key);
		void Write(string key, string content);
		void Erase(string key);
		IList<string> List();
        IEnumerable<string> ListAll(string filter);
		bool HasKey(string key);
		void ReplaceKey(string oldkey, string newkey);
		void CleanUp();
        string Sanitize(string key);
	}
}

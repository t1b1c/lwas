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
using System.Configuration;
using System.IO;
using System.Web;

using LWAS.Extensible.Interfaces.Storage;

namespace LWAS.Infrastructure.Personalization
{
	public class PersonalizationStorage
	{
		public delegate string RootPath();
		private string _screen;
		private IStorageAgent _agent;
		public static PersonalizationStorage Instance;
		public virtual string Screen
		{
			get
			{
				return this._screen;
			}
			set
			{
				this._screen = value;
			}
		}
		public IStorageAgent Agent
		{
			get
			{
				return this._agent;
			}
			set
			{
				this._agent = value;
			}
		}
		private PersonalizationStorage()
		{
		}
		static PersonalizationStorage()
		{
			PersonalizationStorage.Instance = new PersonalizationStorage();
			PersonalizationStorage.Instance.Screen = ConfigurationManager.AppSettings["SCREEN"];
		}
		public virtual string BuildPath(string key)
		{
			string path = string.Empty;
			if (HttpContext.Current.Items.Contains("rootCallback"))
			{
				path = ((PersonalizationStorage.RootPath)HttpContext.Current.Items["rootCallback"])();
			}
			return Path.Combine(path, key + ".xml");
		}
		public virtual string Read(string key, string template)
		{
			if (null == this._agent)
			{
				throw new InvalidOperationException("Agent not set");
			}
			string storagekey = this.BuildPath(key);
			string result;
			if (this._agent.HasKey(storagekey))
			{
				result = this._agent.Read(storagekey);
			}
			else
			{
				result = this._agent.Read(this.BuildPath(template));
			}
			return result;
		}
		public virtual void Write(string key, string contents)
		{
			if (null == this._agent)
			{
				throw new InvalidOperationException("Agent not set");
			}
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException(key);
			}
			string file = this.BuildPath(key);
			this._agent.Write(file, contents);
		}
	}
}

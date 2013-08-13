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
using System.IO;
using System.Web;

using LWAS.Extensible.Interfaces.Storage;

namespace LWAS.Infrastructure.Storage
{
	public class DirectoryContainer : BaseContainer
	{
		private IStorageAgent _agent;
		private string _key;
		protected string parent;
		public override IStorageAgent Agent
		{
			get
			{
				return this._agent;
			}
		}
		public override string Key
		{
			get
			{
				return this._key;
			}
			set
			{
				this.CreateOrRename(value);
			}
		}
		public DirectoryContainer()
		{
			this.parent = string.Empty;
			this._key = string.Empty;
			this._agent = new FileAgent();
		}
		public DirectoryContainer(string parentKey, string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}
			this.parent = ((parentKey == null) ? string.Empty : parentKey);
			this.CreateOrRename(key);
		}
		protected virtual void CreateOrRename(string newkey)
		{
			if (string.IsNullOrEmpty(newkey))
			{
				throw new ArgumentNullException("newkey");
			}
			string keyWithPath = this.GetFullPath(this._key);
			string newkeyWithPath = this.GetFullPath(newkey);
			if (string.IsNullOrEmpty(this._key))
			{
				if (!Directory.Exists(newkeyWithPath))
				{
					Directory.CreateDirectory(newkeyWithPath);
				}
			}
			else
			{
				if (this._key != newkey)
				{
					Directory.Move(keyWithPath, newkeyWithPath);
				}
			}
			this._key = newkey;
			this._agent = new FileAgent(Path.Combine(this.parent, this._key));
		}
		protected virtual string GetFullPath(string key)
		{
			string result;
			if (string.IsNullOrEmpty(key))
			{
				result = string.Empty;
			}
			else
			{
				if (string.IsNullOrEmpty(this.parent))
				{
                    if (!Path.IsPathRooted(key))
                        result = HttpContext.Current.Server.MapPath(key);
                    else
                        result = key;
				}
				else
				{
                    if (!Path.IsPathRooted(Path.Combine(this.parent, key)))
                        result = HttpContext.Current.Server.MapPath(Path.Combine(this.parent, key));
                    else
                        result = Path.Combine(this.parent, key);
				}
			}
			return result;
		}
		public override void Delete()
		{
			string keyWithPath = this.GetFullPath(this._key);
			if (Directory.Exists(keyWithPath))
			{
				Directory.Delete(keyWithPath, true);
			}
		}
		public override IStorageContainer CreateContainer(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}
			return new DirectoryContainer(Path.Combine(this.parent, this._key), key);
		}
		public override bool HasKey(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}
			string keyWithPath = this.GetFullPath(Path.Combine(this._key, key));
			return Directory.Exists(keyWithPath);
		}
		public override bool IsEmpty()
		{
			string keyWithPath = this.GetFullPath(this._key);
			bool empty = false;
			empty = (Directory.GetDirectories(keyWithPath).Length == 0);
			if (empty)
			{
				empty = (Directory.GetFiles(keyWithPath).Length == 0);
			}
			return empty;
		}
		public override string ToString()
		{
			return this.GetFullPath(this._key);
		}
	}
}

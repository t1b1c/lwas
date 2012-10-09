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
using System.Threading;

using LWAS.Infrastructure.Security;

namespace LWAS.Infrastructure.Storage
{
	public class UserDataAgent : FileAgent
	{
		protected override string GetFileWithPath(string file)
		{
			return Path.Combine(UserData.Instance.RootPath, file);
		}
		public override IList<string> List()
		{
			List<string> keys = new List<string>();
			object syncRoot;
			Monitor.Enter(syncRoot = FileAgent.SyncRoot);
			try
			{
				string path = UserData.Instance.RootPath;
				if (Directory.Exists(path))
				{
					string[] files = Directory.GetFiles(path);
					for (int i = 0; i < files.Length; i++)
					{
						string file = files[i];
						keys.Add(Path.GetFileNameWithoutExtension(file));
					}
				}
			}
			finally
			{
				Monitor.Exit(syncRoot);
			}
			return keys;
		}
	}
}

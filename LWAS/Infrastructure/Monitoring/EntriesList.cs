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
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web;

using LWAS.Infrastructure.Security;

namespace LWAS.Infrastructure.Monitoring
{
	public class EntriesList : IEnumerable<string>
	{
		private int _logSize = 1;
		private string repoPath;
		private List<string> entries;
		public int LogSize
		{
			get
			{
				return this._logSize;
			}
		}
		public string this[string entry]
		{
			get
			{
				string result;
				if (!this.entries.Contains(entry))
				{
					result = null;
				}
				else
				{
					string file = Path.Combine(this.repoPath, entry + ".xml");
					if (!File.Exists(file))
					{
						result = null;
					}
					else
					{
						result = File.ReadAllText(file);
					}
				}
				return result;
			}
			set
			{
				File.WriteAllText(Path.Combine(this.repoPath, entry + ".xml"), value);
				this.InitEntries();
			}
		}
		public EntriesList()
		{
			int.TryParse(ConfigurationManager.AppSettings["MONITOR_LOG_SIZE"], out this._logSize);
			this.repoPath = ConfigurationManager.AppSettings["MONITOR_REPO"];
			if (string.IsNullOrEmpty(this.repoPath))
			{
				throw new InvalidProgramException("Monitor has no repo path");
			}
			this.repoPath = HttpContext.Current.Server.MapPath(this.repoPath);
			this.repoPath = Path.Combine(this.repoPath, User.CurrentUser.Name);
			if (!Directory.Exists(this.repoPath))
			{
				Directory.CreateDirectory(this.repoPath);
			}
			this.InitEntries();
		}
		private void InitEntries()
		{
			string[] phisicalEntries = Directory.GetFiles(this.repoPath);
			if (phisicalEntries.Length > this._logSize)
			{
				for (int i = 0; i < phisicalEntries.Length - this._logSize; i++)
				{
					File.Delete(phisicalEntries[i]);
				}
				phisicalEntries = Directory.GetFiles(this.repoPath);
			}
			this.entries = new List<string>();
			string[] array = phisicalEntries;
			for (int j = 0; j < array.Length; j++)
			{
				string entry = array[j];
				this.entries.Add(Path.GetFileNameWithoutExtension(entry));
			}
		}
		public string LastEntry()
		{
			string result;
			if (this.entries.Count == 0)
			{
				result = null;
			}
			else
			{
				result = this[this.entries[this.entries.Count - 1]];
			}
			return result;
		}

        public IEnumerator<string> GetEnumerator()
        {
            return this.entries.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            foreach (string file in Directory.GetFiles(this.repoPath))
                File.Delete(file);
            entries.Clear();
        }
    }
}

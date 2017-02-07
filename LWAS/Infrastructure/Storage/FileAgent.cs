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
using System.Threading;
using System.Web;
using System.Text.RegularExpressions;

using LWAS.Extensible.Interfaces;

namespace LWAS.Infrastructure.Storage
{
	public class FileAgent : BaseAgent, IWatchdogSubscriber
	{
		protected static object SyncRoot = new object();
		protected Dictionary<string, Stream> Streams = new Dictionary<string, Stream>();
		protected string container;
		private IWatchdog _watchdog;
		public IWatchdog Watchdog
		{
			get
			{
				return this._watchdog;
			}
			set
			{
				_watchdog = value;
                if (null != _watchdog)
                    this._watchdog.WatchIt += new EventHandler(this._watchdog_WatchIt);
			}
		}
		public FileAgent()
		{
		}
		public FileAgent(string containerKey)
		{
			this.container = containerKey;
		}
		protected virtual string GetFileWithPath(string file)
		{
			string result;
			try
			{
				if (string.IsNullOrEmpty(this.container))
				{
                    if (!Path.IsPathRooted(file))
                        result = HttpContext.Current.Server.MapPath(file);
                    else
                        result = file;
				}
				else
				{
                    if (!Path.IsPathRooted(Path.Combine(this.container, file)))
                        result = HttpContext.Current.Server.MapPath(Path.Combine(this.container, file));
                    else
                        result = Path.Combine(this.container, file);
				}
			}
			catch (InvalidOperationException ex)
			{
				result = null;
			}
			return result;
		}
		public override Stream OpenStream(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}
			object syncRoot;
			Monitor.Enter(syncRoot = FileAgent.SyncRoot);
			Stream result;
			try
			{
				if (null != this.Streams && this.Streams.ContainsKey(key))
				{
					this.CloseStream(key);
				}
				string file = this.GetFileWithPath(key);
				Stream stream = File.Open(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
				if (null == stream)
				{
					throw new ApplicationException(string.Format("Couldn't open file '{0}'", file));
				}
				if (null == this.Streams)
				{
					this.Streams = new Dictionary<string, Stream>();
				}
				this.Streams.Add(key, stream);
				result = stream;
			}
			finally
			{
				Monitor.Exit(syncRoot);
			}
			return result;
		}
		public override void CloseStream(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}
			if (this.Streams.ContainsKey(key))
			{
				try
				{
					this.Streams[key].Close();
				}
				finally
				{
					this.Streams.Remove(key);
				}
			}
		}
		public override string Read(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}
			string file = this.GetFileWithPath(key);
			object syncRoot;
			Monitor.Enter(syncRoot = FileAgent.SyncRoot);
			string result;
			try
			{
				if (File.Exists(file))
				{
					result = File.ReadAllText(file);
				}
				else
				{
					result = string.Empty;
				}
			}
			finally
			{
				Monitor.Exit(syncRoot);
			}
			return result;
		}
		public override void Write(string key, string content)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException(key);
			}
			string file = this.GetFileWithPath(key);
			object syncRoot;
			Monitor.Enter(syncRoot = FileAgent.SyncRoot);
			try
			{
				if (!Directory.Exists(Path.GetDirectoryName(file)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(file));
				}
				File.WriteAllText(file, content);
			}
			finally
			{
				Monitor.Exit(syncRoot);
			}
		}
		public void Write(string key, Stream content)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException(key);
			}
			Stream fileStream = this.OpenStream(key);
			object syncRoot;
			Monitor.Enter(syncRoot = FileAgent.SyncRoot);
            try
            {
                FileAgent.CopyStream(content, fileStream);
            }
            catch (Exception ex)
            {
            }
			finally
			{
				Monitor.Exit(syncRoot);
			}
			this.CloseStream(key);
		}
		public static void CopyStream(Stream input, Stream output)
		{
			byte[] buffer = new byte[8192];
			int len;
			while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
			{
				output.Write(buffer, 0, len);
			}
		}
		public override void Erase(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}
			string file = this.GetFileWithPath(key);
			object syncRoot;
			Monitor.Enter(syncRoot = FileAgent.SyncRoot);
			try
			{
				if (File.Exists(file))
				{
					File.Delete(file);
				}
			}
			finally
			{
				Monitor.Exit(syncRoot);
			}
		}
		public override IList<string> List()
		{
            if (null == this.Streams)
                return new List<string>();
			string[] keys = new string[this.Streams.Count];
			this.Streams.Keys.CopyTo(keys, 0);
			return keys;
		}

        public override IEnumerable<string> ListAll(string filter)
        {
            if (String.IsNullOrEmpty(container))
                yield break;
            string path = container;
            if (!Path.IsPathRooted(path))
                path = HttpContext.Current.Server.MapPath(path);
            foreach (string f in Directory.GetFiles(path, filter))
                yield return Path.GetFileName(f);
        }

		public override bool HasKey(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				throw new ArgumentNullException("key");
			}
			return File.Exists(this.GetFileWithPath(key));
		}
		public override void ReplaceKey(string oldkey, string newkey)
		{
			if (string.IsNullOrEmpty(oldkey))
			{
				throw new ArgumentNullException("oldkey");
			}
			if (string.IsNullOrEmpty(newkey))
			{
				throw new ArgumentNullException("newkey");
			}
			if (this.HasKey(oldkey))
			{
				object syncRoot;
				Monitor.Enter(syncRoot = FileAgent.SyncRoot);
				try
				{
					Directory.Move(this.GetFileWithPath(oldkey), this.GetFileWithPath(newkey));
				}
				finally
				{
					Monitor.Exit(syncRoot);
				}
			}
		}
		private void _watchdog_WatchIt(object sender, EventArgs e)
		{
			this.CleanUp();
		}
		public override void CleanUp()
		{
            if (null == this.Streams) return;

			foreach (string key in this.List())
			{
				if (this.Streams.ContainsKey(key))
				{
					try
					{
						this.Streams[key].Close();
					}
					finally
					{
						this.Streams.Remove(key);
					}
				}
			}
			this.Streams = null;
		}

        public override string Sanitize(string key)
        {
            string temp = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalid = string.Format(@"[{0}]+", temp);
            return Regex.Replace(key, invalid, "_");
        }
	}
}

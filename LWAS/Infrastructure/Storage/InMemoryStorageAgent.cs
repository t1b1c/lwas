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
using System.Linq;
using System.Web.Caching;

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Storage;

namespace LWAS.Infrastructure.Storage
{
    public class InMemoryStorageAgent : IStorageAgent
    {
        static object SyncRoot = new object();
        readonly string CACHEKEY;
        Dictionary<string, string> Data;
        Cache cache;

        public InMemoryStorageAgent()
        {
            this.CACHEKEY = HttpContext.Current.User.Identity.Name + ".InMemoryStorageAgent";
            cache = HttpContext.Current.Cache;
            lock(SyncRoot)
            {
                this.Data = cache[this.CACHEKEY] as Dictionary<string, string>;
                if (null == this.Data)
                    this.Data = new Dictionary<string, string>();
            }
        }

        public Stream OpenStream(string key)
        {
            throw new NotImplementedException();
        }

        public void CloseStream(string key)
        {
            throw new NotImplementedException();
        }

        public string Read(string key)
        {
            return this.Data[key];
        }

        public void Write(string key, string content)
        {
            this.Data[key] = content;
        }

        public void Erase(string key)
        {
            if (this.Data.ContainsKey(key))
                this.Data.Remove(key);
        }

        public IList<string> List()
        {
            return this.Data.Keys.ToList();
        }

        public IEnumerable<string> ListAll(string filter)
        {
            throw new NotImplementedException();
        }

        public bool HasKey(string key)
        {
            return this.Data.ContainsKey(key);
        }

        public void ReplaceKey(string oldkey, string newkey)
        {
            if (this.Data.ContainsKey(oldkey))
            {
                this.Data.Add(newkey, this.Data[oldkey]);
                this.Data.Remove(oldkey);
            }
        }

        public void CleanUp()
        {
            lock(SyncRoot)
            {
                cache.Remove(this.CACHEKEY);
                cache.Insert(this.CACHEKEY, this.Data);
            }
        }

        public string Sanitize(string key)
        {
            string temp = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalid = string.Format(@"[{0}]+", temp);
            return Regex.Replace(key, invalid, "_");
        }
    }
}

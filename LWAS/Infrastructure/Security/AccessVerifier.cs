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
using System.Web.Security;
using System.Xml;
using System.Web;
using System.Web.Caching;

using LWAS.Extensible.Interfaces.Storage;

namespace LWAS.Infrastructure.Security
{
	public class AccessVerifier
	{
        static object SyncRoot = new object();
        string key;
        public static AccessVerifier Instance;

        Dictionary<string, List<string>> Access
        {
            get
            {
                Cache cache = HttpRuntime.Cache;
                lock (SyncRoot)
                {
                    Dictionary<string, List<string>> access = cache[key] as Dictionary<string, List<string>>;
                    if (null == access)
                    {
                        access = LoadAcces();
                        cache.Insert(key, access, new CacheDependency(HttpContext.Current.Server.MapPath(key)));
                    }
                    return access;
                }
            }
        }

        private IStorageAgent _agent;
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

        static AccessVerifier()
        {
            AccessVerifier.Instance = new AccessVerifier();
        }

        public AccessVerifier()
        {
            this.key = ConfigurationManager.AppSettings["ACCESS"];
            if (string.IsNullOrEmpty(this.key))
            {
                throw new InvalidOperationException("ACCESS key not set in config file");
            }
        }

        public virtual Dictionary<string, List<string>> LoadAcces()
        {
            this._agent = AccessVerifier.Instance.Agent;
            if (null == this._agent) { throw new InvalidOperationException("Agent not set"); }
            XmlDocument doc = new XmlDocument();
            
            try
            {
                doc.Load(this._agent.OpenStream(this.key));
            }
            finally
            {
                this._agent.CloseStream(this.key);
            }

            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();

            XmlNode root = doc.SelectSingleNode("access");
            if (null == root) throw new InvalidOperationException("Access file has no access node"); 
        
            foreach (XmlNode node in root.ChildNodes)
            {
                string name = node.Attributes["key"].Value;
                if (string.IsNullOrEmpty(name)) throw new InvalidOperationException(string.Format("Found a screen node without a key in '{0}'", this.key));

                if (!result.ContainsKey(name))
                    result.Add(name, new List<string>());

                List<string> screenMembers = result[name];
                foreach (XmlNode role in node.ChildNodes)
                {
                    if (null == role.Attributes["name"]) throw new InvalidOperationException(string.Format("The screen '{0}' has a role without name", node.Name));

                    if (!screenMembers.Contains(role.Attributes["name"].Value))
                        screenMembers.Add(role.Attributes["name"].Value);
                }
            }
            return result;
        }

        public bool HasAccess(string screen)
        {
            return HasAccess(User.CurrentUser.Name, screen);
        }

        public bool HasAccess(string user, string screen)
        {
            if (this.Access.ContainsKey(screen))
            {
                foreach (string role in this.Access[screen])
                    if (RoleManager.Instance.IsUserInRole(user, role))
                        return true;
            }
            return false;
        }
	}
}

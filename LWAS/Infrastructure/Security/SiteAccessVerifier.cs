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
using System.Web;
using System.Web.Security;
using System.Web.Caching;
using System.Xml;

using LWAS.Extensible.Interfaces.Storage;

namespace LWAS.Infrastructure.Security
{
	public class SiteAccessVerifier
	{
        static object SyncRoot = new object();
        string key;
        public static SiteAccessVerifier Instance;

        Dictionary<string, Dictionary<string, List<string>>> Access
        {
            get
            {
                Cache cache = HttpRuntime.Cache;
                lock (SyncRoot)
                {
                    Dictionary<string, Dictionary<string, List<string>>> access = cache[key] as Dictionary<string, Dictionary<string, List<string>>>;
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

        static SiteAccessVerifier()
        {
            SiteAccessVerifier.Instance = new SiteAccessVerifier();
        }

        public SiteAccessVerifier()
        {
            this.key = ConfigurationManager.AppSettings["ACCESS"];
            if (string.IsNullOrEmpty(this.key))
            {
                throw new InvalidOperationException("ACCESS key not set in config file");
            }
        }

        public virtual Dictionary<string, Dictionary<string, List<string>>> LoadAcces()
        {
            this._agent = SiteAccessVerifier.Instance.Agent;
            if (null == this._agent) throw new InvalidOperationException("Agent not set");

            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(this._agent.OpenStream(this.key));
            }
            finally
            {
                this._agent.CloseStream(this.key);
            }

            Dictionary<string, Dictionary<string, List<string>>> access = new Dictionary<string, Dictionary<string, List<string>>>();

            XmlNode root = doc.SelectSingleNode("access");
            if (null == root)
                throw new InvalidOperationException("Access file has no access node");
            
            foreach (XmlNode appNode in root.ChildNodes)
            {
                if ("app" != appNode.Name) throw new InvalidOperationException(String.Format("expected app but found '{0}' in '{1}'", appNode.Name, key));

                string app = appNode.Attributes["key"].Value;
                if (!access.ContainsKey(app))
                    access.Add(app, new Dictionary<string, List<string>>());

                foreach (XmlNode screenNode in appNode.ChildNodes)
                {
                    if ("screen" != screenNode.Name) throw new InvalidOperationException(String.Format("expected screen but found '{0}' in app '{1}' in '{2}'", screenNode.Name, app, key));

                    string screen = screenNode.Attributes["key"].Value;
                    if (!access[app].ContainsKey(screen))
                        access[app].Add(screen, new List<string>());

                    List<string> screenMembers = access[app][screen];
                    foreach (XmlNode role in screenNode.ChildNodes)
                    {
                        if ("role" != role.Name) throw new InvalidOperationException(String.Format("expected role but found '{0}' in screen '{1}' in app '{2}' in '{3}'", role.Name, screen, app, key));

                        if (!screenMembers.Contains(role.Attributes["name"].Value))
                        {
                            screenMembers.Add(role.Attributes["name"].Value);
                        }
                    }
                }
            }
            return access;
        }

        public bool HasAccess(string app, string screen)
        {
            return HasAccess(app, screen, User.CurrentUser.Name);
        }

        public bool HasAccess(string app, string screen, string user)
        {
            if (this.Access.ContainsKey(app))
            {
                if (this.Access[app].ContainsKey(screen))
                {
                    foreach (string role in this.Access[app][screen])
                        if (RoleManager.Instance.IsUserInRole(user, role))
                            return true;
                }
            }
            return false;
        }
	}
}

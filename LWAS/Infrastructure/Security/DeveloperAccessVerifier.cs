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
using System.Xml;
using System.Web.Caching;

using LWAS.Extensible.Interfaces.Storage;

namespace LWAS.Infrastructure.Security
{
	public class DeveloperAccessVerifier
	{
        static object SyncRoot = new object();
        string key;
        public static DeveloperAccessVerifier Instance;

        private IStorageAgent _agent;
        public IStorageAgent Agent
        {
            get { return this._agent; }
            set { this._agent = value; }
        }

        Dictionary<string, List<string>> Owners
        {
            get
            {
                Cache cache = HttpRuntime.Cache;
                lock (SyncRoot)
                {
                    Dictionary<string, List<string>> access = cache[key] as Dictionary<string, List<string>>;
                    if (null == access)
                    {
                        access = LoadOwners();
                        cache.Insert(key, access, new CacheDependency(HttpContext.Current.Server.MapPath(key)));
                    }
                    return access;
                }
            }
        }

        static DeveloperAccessVerifier()
        {
            DeveloperAccessVerifier.Instance = new DeveloperAccessVerifier();
        }

        public DeveloperAccessVerifier()
        {
            key = ConfigurationManager.AppSettings["ACCESS_DEVELOPERS"];
            if (String.IsNullOrEmpty(key)) throw new InvalidOperationException("ACCESS_DEVELOPERS key not set in config file");
        }

        public virtual Dictionary<string, List<string>> LoadOwners()
        {
            if (null == _agent) throw new InvalidOperationException("Agent not set");
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(this._agent.OpenStream(key));
            }
            finally
            {
                this._agent.CloseStream(key);
            }

            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();

            XmlNode root = doc.SelectSingleNode("owners");
            if (null == root)
                throw new InvalidOperationException("Owners file has no owners node");
            
            foreach (XmlNode appNode in root.ChildNodes)
            {
                string app = appNode.Attributes["name"].Value;
                result.Add(app, new List<string>());
                foreach (XmlNode ownerNode in appNode.ChildNodes)
                    result[app].Add(ownerNode.Attributes["name"].Value);
            }

            return result;
        }

        public bool IsOwner(string application)
        {
            return IsOwner(application, User.CurrentUser.Name);
        }

        public bool IsOwner(string application, string user)
        {
            if (!this.Owners.ContainsKey(application)) return false;

            return this.Owners[application].Contains(user);
        }

        public bool HasAccess(string app, string screen)
        {
            return HasAccess(app, screen, User.CurrentUser.Name);
        }

        public bool HasAccess(string app, string screen, string user)
        {
            string editRepo = ConfigurationManager.AppSettings["EDIT_REPO"];
            if (String.IsNullOrEmpty(editRepo)) throw new InvalidOperationException("EDIT_REPO key not set in config file");
            string accessRepo = ConfigurationManager.AppSettings["EDIT_REPO_ROLES"];
            if (String.IsNullOrEmpty(accessRepo)) throw new InvalidOperationException("EDIT_REPO_ROLES key not set in config file");
            string accessFile = ConfigurationManager.AppSettings["EDIT_ACCESS"];
            if (String.IsNullOrEmpty(accessFile)) throw new InvalidOperationException("EDIT_ACCESS key not set in config file");
            string rolesFile = ConfigurationManager.AppSettings["EDIT_ROLES"];
            if (String.IsNullOrEmpty(accessFile)) throw new InvalidOperationException("EDIT_ROLES key not set in config file");

            string fileA = System.IO.Path.Combine(editRepo, app);
            fileA = System.IO.Path.Combine(fileA, accessRepo);
            fileA = System.IO.Path.Combine(fileA, accessFile);

            XmlDocument docAccess = new XmlDocument();
            if (_agent.HasKey(fileA))
                docAccess.LoadXml(this._agent.Read(fileA));
            else
                return true;

            string fileR = System.IO.Path.Combine(editRepo, app);
            fileR = System.IO.Path.Combine(fileR, accessRepo);
            fileR = System.IO.Path.Combine(fileR, rolesFile);

            XmlDocument docRoles = new XmlDocument();
            if (_agent.HasKey(fileR))
                docRoles.Load(this._agent.OpenStream(fileR));
            else
                return true;

            XmlNode rootAccess = docAccess.SelectSingleNode("access");
            XmlNode rootRoles = docRoles.SelectSingleNode("roles");
            XmlNode screenAccessNode = rootAccess.SelectSingleNode("descendant::screen[attribute::key='" + screen + "']");
            if (null != screenAccessNode)
            {
                foreach (XmlNode roleAccessNode in screenAccessNode.ChildNodes)
                {
                    XmlNode roleUsersNode = rootRoles.SelectSingleNode("descendant::role[attribute::name='" + roleAccessNode.Attributes["name"].Value + "']");
                    if (null != roleUsersNode)
                        if (null != roleUsersNode.SelectSingleNode("descendant::user[attribute::name='" + user + "']"))
                            return true;
                }
            }

            return false;
        }
	}
}

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
using System.Configuration;
using System.Web.Security;
using System.Xml;
using System.Web;
using System.Web.Caching;

using LWAS.Extensible.Interfaces.Storage;

namespace LWAS.Infrastructure.Security
{
	public class RoleManager : RoleProvider
    {	
        object SyncRoot = new object();
		private string key = string.Empty;
		private IStorageAgent _agent;
		public static RoleManager Instance;
		private string _applicationName;
		private Dictionary<string, List<string>> Roles
		{
			get
            {
                Cache cache = HttpRuntime.Cache;
                lock (SyncRoot)
                {
                    Dictionary<string, List<string>> roles = cache[key] as Dictionary<string, List<string>>;
                    if (null == roles)
                    {
                        roles = LoadRoles();
                        cache.Insert(key, roles, new CacheDependency(HttpContext.Current.Server.MapPath(key)));
                    }
                    return roles;
                }
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
		public override string ApplicationName
		{
			get
			{
				return this._applicationName;
			}
			set
			{
				this._applicationName = value;
			}
		}
		static RoleManager()
		{
			RoleManager.Instance = new RoleManager();
		}
		public RoleManager()
		{
			this.key = ConfigurationManager.AppSettings["ROLES"];
			if (string.IsNullOrEmpty(this.key))
			{
				throw new InvalidOperationException("ROLES key not set in config file");
			}
		}
		public virtual Dictionary<string, List<string>> LoadRoles()
		{
			this._agent = RoleManager.Instance.Agent;
			if (null == this._agent)  throw new InvalidOperationException("Agent not set"); 

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

			XmlNode root = doc.SelectSingleNode("roles");
			if (null == root) { throw new InvalidOperationException("Roles files has no roles node"); }

			foreach (XmlNode node in root.ChildNodes)
			{
				string name = node.Attributes["name"].Value;
				if (string.IsNullOrEmpty(name)) throw new InvalidOperationException(string.Format("Found a role node without a name in '{0}'", this.key));
                
                if (!result.ContainsKey(name))
                    result.Add(name, new List<string>());

                List<string> roleMembers = result[name];
				foreach (XmlNode user in node.ChildNodes)
				{
					if (null == user.Attributes["name"]) throw new InvalidOperationException(string.Format("The role '{0}' has a user without name", node.Name));

					if (!roleMembers.Contains(user.Attributes["name"].Value))
						roleMembers.Add(user.Attributes["name"].Value);
				}
			}
            return result;
		}
		public override void AddUsersToRoles(string[] usernames, string[] roleNames)
		{
			for (int i = 0; i < roleNames.Length; i++)
			{
				string role = roleNames[i];
				if (!this.Roles.ContainsKey(role))
				{
					this.Roles.Add(role, new List<string>());
				}
				for (int j = 0; j < usernames.Length; j++)
				{
					string user = usernames[j];
					if (!this.Roles[role].Contains(user))
					{
						this.Roles[role].Add(user);
					}
				}
			}
		}
		public override void CreateRole(string roleName)
		{
			if (!this.Roles.ContainsKey(roleName))
			{
				this.Roles.Add(roleName, new List<string>());
			}
		}
		public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
		{
			if (this.Roles.ContainsKey(roleName))
			{
				if (throwOnPopulatedRole && this.Roles[roleName].Count > 0)
				{
					throw new InvalidOperationException(string.Format("Role '{0}' has users assignet to it", roleName));
				}
				this.Roles.Remove(roleName);
			}
			return true;
		}
		public override string[] FindUsersInRole(string roleName, string usernameToMatch)
		{
			List<string> found = new List<string>();
			if (this.Roles.ContainsKey(roleName))
			{
				foreach (string user in this.Roles[roleName])
				{
					if (user.Contains(usernameToMatch))
					{
						found.Add(user);
					}
				}
			}
			return found.ToArray();
		}
		public override string[] GetAllRoles()
		{
			string[] ret = new string[this.Roles.Count];
			this.Roles.Keys.CopyTo(ret, 0);
			return ret;
		}
		public override string[] GetRolesForUser(string username)
		{
			List<string> found = new List<string>();
			foreach (string role in this.Roles.Keys)
			{
				foreach (string user in this.Roles[role])
				{
					if (user == username && !found.Contains(role))
					{
						found.Add(role);
					}
				}
			}
			return found.ToArray();
		}
		public override string[] GetUsersInRole(string roleName)
		{
			string[] result;
			if (!this.Roles.ContainsKey(roleName))
			{
				result = null;
			}
			else
			{
				result = this.Roles[roleName].ToArray();
			}
			return result;
		}
		public override bool IsUserInRole(string username, string roleName)
		{
			return this.Roles.ContainsKey(roleName) && this.Roles[roleName].Contains(username);
		}
		public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
		{
			for (int i = 0; i < roleNames.Length; i++)
			{
				string role = roleNames[i];
				if (this.Roles.ContainsKey(role))
				{
					for (int j = 0; j < usernames.Length; j++)
					{
						string user = usernames[j];
						if (this.Roles[role].Contains(user))
						{
							this.Roles[role].Remove(user);
						}
					}
				}
			}
		}
		public override bool RoleExists(string roleName)
		{
			return this.Roles.ContainsKey(roleName);
		}
	}
}

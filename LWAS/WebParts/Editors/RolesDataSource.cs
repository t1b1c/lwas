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
using System.Collections;
using System.Web.UI;
using System.Xml;

using LWAS.Extensible.Interfaces.Storage;
using LWAS.Infrastructure.Storage;
using LWAS.Infrastructure.Security;

namespace LWAS.WebParts.Editors
{
	public class RolesDataSource : DataSourceControl
    {
        public string ApplicationName { get; set; }

		private string _rolesFile;
		private IStorageAgent _agent;
		public IStorageAgent Agent
		{
			get
			{
				return this._agent;
			}
		}
		public RolesDataSource(string rolesFile, string application)
		{
			this._rolesFile = rolesFile;
            this.ApplicationName = application;
			this._agent = new FileAgent();
			if (!this._agent.HasKey(rolesFile))
			{
				XmlDocument doc = new XmlDocument();
				doc.AppendChild(doc.CreateNode(XmlNodeType.XmlDeclaration, "", ""));
				doc.AppendChild(doc.CreateNode(XmlNodeType.Element, "roles", ""));
				this._agent.Write(this._rolesFile, string.Empty);
				this.SaveRolesFile(doc);
			}
		}
		protected XmlDocument LoadRolesFile()
		{
			XmlDocument doc = new XmlDocument();
            doc.LoadXml(this._agent.Read(this._rolesFile));
			return doc;
		}
		protected void SaveRolesFile(XmlDocument doc)
		{
			this._agent.Erase(this._rolesFile);
            try
            {
                doc.Save(this._agent.OpenStream(this._rolesFile));
            }
            finally
            {
                this._agent.CloseStream(this._rolesFile);
            }
		}
		public IEnumerable ListRoles()
		{
			ArrayList ret = new ArrayList();
			XmlDocument doc = this.LoadRolesFile();
			XmlNode root = doc.SelectSingleNode("roles");
			if (null == root)
			{
				throw new InvalidOperationException("Roles file has no roles node");
			}
			foreach (XmlNode roleNode in root.ChildNodes)
			{
				if ("role" == roleNode.Name)
				{
					ret.Add(roleNode.Attributes["name"].Value);
				}
			}
			return ret;
		}
		public void CreateRole(string name)
		{
            if (!String.IsNullOrEmpty(this.ApplicationName) && !DeveloperAccessVerifier.Instance.IsOwner(this.ApplicationName))
                throw new InvalidOperationException(String.Format("Failed to setup roles for the application '{0}'. You don't have access to that application.", this.ApplicationName));
            
            XmlDocument doc = this.LoadRolesFile();
			XmlNode root = doc.SelectSingleNode("roles");
			if (null == root)
			{
				throw new InvalidOperationException("Roles file has no roles node");
			}
			XmlNode node = doc.CreateNode(XmlNodeType.Element, "role", "");
			XmlAttribute nameAttribute = doc.CreateAttribute("name");
			nameAttribute.Value = name;
			node.Attributes.Append(nameAttribute);
			root.AppendChild(node);
			this.SaveRolesFile(doc);
		}
		public void RenameRole(string oldname, string newname)
		{
            if (!String.IsNullOrEmpty(this.ApplicationName) && !DeveloperAccessVerifier.Instance.IsOwner(this.ApplicationName))
                throw new InvalidOperationException(String.Format("Failed to setup roles for the application '{0}'. You don't have access to that application.", this.ApplicationName));

            if (!(oldname == newname))
			{
				XmlDocument doc = this.LoadRolesFile();
				XmlNode root = doc.SelectSingleNode("roles");
				if (null == root)
				{
					throw new InvalidOperationException("Roles file has no roles node");
				}
				XmlNode roleNode = root.SelectSingleNode("child::role[attribute::name='" + oldname + "']");
				if (null == roleNode)
				{
					throw new ArgumentException(string.Format("Can't find role '{0}'", oldname));
				}
				roleNode.Attributes["name"].Value = newname;
				this.SaveRolesFile(doc);
			}
		}
		public void DeleteRole(string name)
		{
            if (!String.IsNullOrEmpty(this.ApplicationName) && !DeveloperAccessVerifier.Instance.IsOwner(this.ApplicationName))
                throw new InvalidOperationException(String.Format("Failed to setup roles for the application '{0}'. You don't have access to that application.", this.ApplicationName));

            XmlDocument doc = this.LoadRolesFile();
			XmlNode root = doc.SelectSingleNode("roles");
			if (null == root)
			{
				throw new InvalidOperationException("Roles file has no roles node");
			}
			XmlNode roleNode = root.SelectSingleNode("child::role[attribute::name='" + name + "']");
			if (null == roleNode)
			{
				throw new ArgumentException(string.Format("Can't find role '{0}'", name));
			}
			root.RemoveChild(roleNode);
			this.SaveRolesFile(doc);
		}
		public IEnumerable ListUsers(string role)
		{
			ArrayList ret = new ArrayList();
			XmlDocument doc = this.LoadRolesFile();
			XmlNode root = doc.SelectSingleNode("roles");
			if (null == root)
			{
				throw new InvalidOperationException("Roles file has no roles node");
			}
			XmlNode roleNode = root.SelectSingleNode("child::role[attribute::name='" + role + "']");
			if (null != roleNode)
			{
				foreach (XmlNode userNode in roleNode.ChildNodes)
				{
					if ("user" == userNode.Name)
					{
						ret.Add(userNode.Attributes["name"].Value);
					}
				}
			}
			return ret;
		}
		public void CreateUser(string role, string name)
		{
            if (!String.IsNullOrEmpty(this.ApplicationName) && !DeveloperAccessVerifier.Instance.IsOwner(this.ApplicationName))
                throw new InvalidOperationException(String.Format("Failed to setup roles for the application '{0}'. You don't have access to that application.", this.ApplicationName));

            XmlDocument doc = this.LoadRolesFile();
			XmlNode root = doc.SelectSingleNode("roles");
			if (null == root)
			{
				throw new InvalidOperationException("Roles file has no roles node");
			}
			XmlNode roleNode = root.SelectSingleNode("child::role[attribute::name='" + role + "']");
			if (null == roleNode)
			{
				throw new ArgumentException(string.Format("Can't find role '{0}'", role));
			}
			XmlNode node = doc.CreateNode(XmlNodeType.Element, "user", "");
			XmlAttribute nameAttribute = doc.CreateAttribute("name");
			nameAttribute.Value = name;
			node.Attributes.Append(nameAttribute);
			roleNode.AppendChild(node);
			this.SaveRolesFile(doc);
		}
		public void RenameUser(string role, string oldname, string newname)
		{
            if (!String.IsNullOrEmpty(this.ApplicationName) && !DeveloperAccessVerifier.Instance.IsOwner(this.ApplicationName))
                throw new InvalidOperationException(String.Format("Failed to setup roles for the application '{0}'. You don't have access to that application.", this.ApplicationName));

            if (!(oldname == newname))
			{
				XmlDocument doc = this.LoadRolesFile();
				XmlNode root = doc.SelectSingleNode("roles");
				if (null == root)
				{
					throw new InvalidOperationException("Roles file has no roles node");
				}
				XmlNode roleNode = root.SelectSingleNode("child::role[attribute::name='" + role + "']");
				if (null == roleNode)
				{
					throw new ArgumentException(string.Format("Can't find role '{0}'", role));
				}
				XmlNode userNode = roleNode.SelectSingleNode("child::user[attribute::name='" + oldname + "']");
				if (null != userNode)
				{
					userNode.Attributes["name"].Value = newname;
				}
				this.SaveRolesFile(doc);
			}
		}
		public void DeleteUser(string role, string name)
		{
            if (!String.IsNullOrEmpty(this.ApplicationName) && !DeveloperAccessVerifier.Instance.IsOwner(this.ApplicationName))
                throw new InvalidOperationException(String.Format("Failed to setup roles for the application '{0}'. You don't have access to that application.", this.ApplicationName));

            XmlDocument doc = this.LoadRolesFile();
			XmlNode root = doc.SelectSingleNode("roles");
			if (null == root)
			{
				throw new InvalidOperationException("Roles file has no roles node");
			}
			XmlNode roleNode = root.SelectSingleNode("child::role[attribute::name='" + role + "']");
			if (null == roleNode)
			{
				throw new ArgumentException(string.Format("Can't find role '{0}'", role));
			}
			XmlNode userNode = roleNode.SelectSingleNode("child::user[attribute::name='" + name + "']");
			if (null != userNode)
			{
				roleNode.RemoveChild(userNode);
			}
			this.SaveRolesFile(doc);
		}
		protected override DataSourceView GetView(string viewName)
		{
			DataSourceView result;
			if ("/" == viewName)
			{
				result = new RolesView(this, viewName);
			}
			else
			{
				if (!string.IsNullOrEmpty(viewName))
				{
					result = new RoleUsersView(this, viewName);
				}
				else
				{
					result = new NoEditView(this, string.Empty);
				}
			}
			return result;
		}
	}
}

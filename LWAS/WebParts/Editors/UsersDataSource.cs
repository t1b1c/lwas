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
using System.Collections;
using System.Web.Security;
using System.Web.UI;
using System.Xml;

using LWAS.Extensible.Interfaces.Storage;
using LWAS.Infrastructure.Storage;
using LWAS.Infrastructure.Security;

namespace LWAS.WebParts.Editors
{
	public class UsersDataSource : DataSourceControl
    {
        public string ApplicationName { get; set; }

		private string _usersFile;
		private IStorageAgent _agent;
		public IStorageAgent Agent
		{
			get
			{
				return this._agent;
			}
		}

        public bool VerifyDeveloperAccess { get; set; }

        public UsersDataSource(string usersFile, string application)
		{
            this._usersFile = usersFile;
            this.ApplicationName = application;
			this._agent = new FileAgent();
			if (!this._agent.HasKey(usersFile))
			{
				XmlDocument doc = new XmlDocument();
				doc.AppendChild(doc.CreateNode(XmlNodeType.XmlDeclaration, "", ""));
				doc.AppendChild(doc.CreateNode(XmlNodeType.Element, "users", ""));
				this._agent.Write(this._usersFile, string.Empty);
				this.SaveUsersFile(doc);
			}
		}
        public UsersDataSource(string usersFile, string application, bool verifyDeveloperAccess)
            : this(usersFile, application)
        {
            this.VerifyDeveloperAccess = verifyDeveloperAccess;
        }
		protected XmlDocument LoadUsersFile()
		{
			XmlDocument doc = new XmlDocument();
            doc.LoadXml(this._agent.Read(this._usersFile));
			return doc;
		}
		protected void SaveUsersFile(XmlDocument doc)
		{
			this._agent.Erase(this._usersFile);
            try
            {
                doc.Save(this._agent.OpenStream(this._usersFile));
            }
            finally
            {
                this._agent.CloseStream(this._usersFile);
            }
		}
		public IEnumerable ListUsers()
		{
			ArrayList ret = new ArrayList();
			XmlDocument doc = this.LoadUsersFile();
			XmlNode root = doc.SelectSingleNode("users");
			if (null == root)
			{
				throw new InvalidOperationException("Users file has no users node");
			}
			foreach (XmlNode roleNode in root.ChildNodes)
			{
				if ("user" == roleNode.Name)
				{
					ret.Add(roleNode.Attributes["name"].Value);
				}
			}
			return ret;
		}
		public void CreateUser(string name)
		{
            if (this.VerifyDeveloperAccess && !DeveloperAccessVerifier.Instance.IsOwner(this.ApplicationName))
                throw new InvalidOperationException(String.Format("Failed to setup users for the application '{0}'. You don't have access to that application.", this.ApplicationName));

            XmlDocument doc = this.LoadUsersFile();
			XmlNode root = doc.SelectSingleNode("users");
			if (null == root)
			{
				throw new InvalidOperationException("Users file has no users node");
			}
			XmlNode node = doc.CreateNode(XmlNodeType.Element, "user", "");
			XmlAttribute nameAttribute = doc.CreateAttribute("name");
			XmlAttribute pwdAttribute = doc.CreateAttribute("pwd");
			nameAttribute.Value = name;
			pwdAttribute.Value = FormsAuthentication.HashPasswordForStoringInConfigFile(this.GenerateStrongPassword(), "SHA1");
			node.Attributes.Append(nameAttribute);
			node.Attributes.Append(pwdAttribute);
			root.AppendChild(node);
			this.SaveUsersFile(doc);
		}
		public void RenameUser(string oldname, string newname)
		{
            if (this.VerifyDeveloperAccess && !DeveloperAccessVerifier.Instance.IsOwner(this.ApplicationName))
                throw new InvalidOperationException(String.Format("Failed to setup users for the application '{0}'. You don't have access to that application.", this.ApplicationName));

            if (!(oldname == newname))
			{
				XmlDocument doc = this.LoadUsersFile();
				XmlNode root = doc.SelectSingleNode("users");
				if (null == root)
				{
					throw new InvalidOperationException("Users file has no users node");
				}
				XmlNode roleNode = root.SelectSingleNode("child::user[attribute::name='" + oldname + "']");
				if (null == roleNode)
				{
					throw new ArgumentException(string.Format("Can't find user '{0}'", oldname));
				}
				roleNode.Attributes["name"].Value = newname;
				this.SaveUsersFile(doc);
			}
		}
		public void DeleteUser(string name)
		{
            if (this.VerifyDeveloperAccess && !DeveloperAccessVerifier.Instance.IsOwner(this.ApplicationName))
                throw new InvalidOperationException(String.Format("Failed to setup users for the application '{0}'. You don't have access to that application.", this.ApplicationName));

            XmlDocument doc = this.LoadUsersFile();
			XmlNode root = doc.SelectSingleNode("users");
			if (null == root)
			{
				throw new InvalidOperationException("Users file has no users node");
			}
			XmlNode roleNode = root.SelectSingleNode("child::user[attribute::name='" + name + "']");
			if (null == roleNode)
			{
				throw new ArgumentException(string.Format("Can't find user '{0}'", name));
			}
			root.RemoveChild(roleNode);
			this.SaveUsersFile(doc);
		}
		public void ResetPassword(string name, string password)
		{
            if (this.VerifyDeveloperAccess && !DeveloperAccessVerifier.Instance.IsOwner(this.ApplicationName))
                throw new InvalidOperationException(String.Format("Failed to setup users for the application '{0}'. You don't have access to that application.", this.ApplicationName));

            string hash = FormsAuthentication.HashPasswordForStoringInConfigFile(password, "SHA1");
			XmlDocument doc = this.LoadUsersFile();
			XmlNode root = doc.SelectSingleNode("users");
			if (null == root)
			{
				throw new InvalidOperationException("Users file has no users node");
			}
			XmlNode roleNode = root.SelectSingleNode("child::user[attribute::name='" + name + "']");
			if (null == roleNode)
			{
				throw new ArgumentException(string.Format("Can't find user '{0}'", name));
			}
			roleNode.Attributes["pwd"].Value = hash;
			this.SaveUsersFile(doc);
		}
		public string GenerateStrongPassword()
		{
			return Membership.GeneratePassword(8, 3);
		}
		protected override DataSourceView GetView(string viewName)
		{
			return new UsersView(this, viewName);
		}
	}
}

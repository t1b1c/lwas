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
using System.Web.UI;
using System.Xml;

using LWAS.Extensible.Interfaces.Storage;
using LWAS.Infrastructure.Storage;
using LWAS.Infrastructure.Security;

namespace LWAS.WebParts.Editors
{
	public class AccessDataSource : DataSourceControl
	{
        public string ApplicationName { get; set; }

        private IStorageAgent _agent;
		public IStorageAgent Agent
		{
			get
			{
				return this._agent;
			}
		}

        private string _accessFile;
        public AccessDataSource(string accessFile, string application)
		{
			this._accessFile = accessFile;
            this.ApplicationName = application;
            this._agent = new FileAgent();

			if (!this._agent.HasKey(this._accessFile))
			{
				XmlDocument doc = new XmlDocument();
				doc.AppendChild(doc.CreateNode(XmlNodeType.XmlDeclaration, "", ""));
				doc.AppendChild(doc.CreateNode(XmlNodeType.Element, "access", ""));
				this._agent.Write(this._accessFile, string.Empty);
				this.SaveAccessFile(doc);
			}
		}
		protected XmlDocument LoadAccessFile()
		{
			XmlDocument doc = new XmlDocument();
            doc.LoadXml(this._agent.Read(this._accessFile));
			return doc;
		}
		protected void SaveAccessFile(XmlDocument doc)
		{
			this._agent.Erase(this._accessFile);
            try
            {
                doc.Save(this._agent.OpenStream(this._accessFile));
            }
            finally
            {
                this._agent.CloseStream(this._accessFile);
            }
		}
		public IEnumerable ListAccess()
		{
			ArrayList ret = new ArrayList();
			XmlDocument doc = this.LoadAccessFile();
			XmlNode root = doc.SelectSingleNode("access");
			if (null == root) throw new InvalidOperationException("Access file has no access node");

			foreach (XmlNode accessNode in root.ChildNodes)
			{
				if ("screen" == accessNode.Name && (DeveloperAccessVerifier.Instance.IsOwner(this.ApplicationName) ||
                                                    DeveloperAccessVerifier.Instance.HasAccess(this.ApplicationName, accessNode.Attributes["key"].Value)))
				{
					ret.Add(accessNode.Attributes["key"].Value);
				}
			}
			return ret;
		}
		public void CreateAccess(string screen)
		{
            if (!(DeveloperAccessVerifier.Instance.IsOwner(this.ApplicationName) ||
                  DeveloperAccessVerifier.Instance.HasAccess(this.ApplicationName, screen)))
                throw new InvalidOperationException(String.Format("Failed to setup the access for the screen '{0}' from application '{1}'. You don't have access to that screen.", screen, this.ApplicationName));

			XmlDocument doc = this.LoadAccessFile();
			XmlNode root = doc.SelectSingleNode("access");
			if (null == root)
			{
				throw new InvalidOperationException("Access file has no access node");
			}
			XmlNode node = doc.CreateNode(XmlNodeType.Element, "screen", "");
			XmlAttribute keyAttribute = doc.CreateAttribute("key");
			keyAttribute.Value = screen;
			node.Attributes.Append(keyAttribute);
			root.AppendChild(node);
			this.SaveAccessFile(doc);
		}
		public void ReplaceScreen(string oldscreen, string newscreen)
		{
            if (!(DeveloperAccessVerifier.Instance.IsOwner(this.ApplicationName) ||
                  DeveloperAccessVerifier.Instance.HasAccess(this.ApplicationName, oldscreen)))
                throw new InvalidOperationException(String.Format("Failed to setup the access for the screen '{0}' from application '{1}'. You don't have access to that screen.", oldscreen, this.ApplicationName));
            
            if (!(oldscreen == newscreen))
			{
				XmlDocument doc = this.LoadAccessFile();
				XmlNode root = doc.SelectSingleNode("access");
				if (null == root)
				{
					throw new InvalidOperationException("Access file has no access node");
				}
				XmlNode accessNode = root.SelectSingleNode("child::screen[attribute::key='" + oldscreen + "']");
				if (null == accessNode)
				{
					throw new ArgumentException(string.Format("Can't find access '{0}'", oldscreen));
				}
				accessNode.Attributes["key"].Value = newscreen;
				this.SaveAccessFile(doc);
			}
		}
		public void DeleteAccess(string screen)
		{
            if (!(DeveloperAccessVerifier.Instance.IsOwner(this.ApplicationName) ||
                  DeveloperAccessVerifier.Instance.HasAccess(this.ApplicationName, screen)))
                throw new InvalidOperationException(String.Format("Failed to setup the access for the screen '{0}' from application '{1}'. You don't have access to that screen.", screen, this.ApplicationName));
            
            XmlDocument doc = this.LoadAccessFile();
			XmlNode root = doc.SelectSingleNode("access");
			if (null == root)
			{
				throw new InvalidOperationException("Access file has no access node");
			}
			XmlNode accessNode = root.SelectSingleNode("child::screen[attribute::key='" + screen + "']");
			if (null == accessNode)
			{
				throw new ArgumentException(string.Format("Can't find access '{0}'", screen));
			}
			root.RemoveChild(accessNode);
			this.SaveAccessFile(doc);
		}
		public IEnumerable ListRoles(string screen)
		{
			ArrayList ret = new ArrayList();
			XmlDocument doc = this.LoadAccessFile();
			XmlNode root = doc.SelectSingleNode("access");
			if (null == root)
			{
				throw new InvalidOperationException("Access file has no access node");
			}
			XmlNode accessNode = root.SelectSingleNode("child::screen[attribute::key='" + screen + "']");
			if (null != accessNode)
			{
				foreach (XmlNode roleNode in accessNode.ChildNodes)
				{
					if ("role" == roleNode.Name)
					{
						ret.Add(roleNode.Attributes["name"].Value);
					}
				}
			}
			return ret;
		}
		public void AddRole(string screen, string role)
		{
            if (!(DeveloperAccessVerifier.Instance.IsOwner(this.ApplicationName) ||
                  DeveloperAccessVerifier.Instance.HasAccess(this.ApplicationName, screen)))
                throw new InvalidOperationException(String.Format("Failed to setup the access for the screen '{0}' from application '{1}'. You don't have access to that screen.", screen, this.ApplicationName));
            
            XmlDocument doc = this.LoadAccessFile();
			XmlNode root = doc.SelectSingleNode("access");
			if (null == root)
			{
				throw new InvalidOperationException("Access file has no access node");
			}
			XmlNode screenNode = root.SelectSingleNode("child::screen[attribute::key='" + screen + "']");
			if (null == screenNode)
			{
				throw new ArgumentException(string.Format("Can't find access '{0}'", screen));
			}
			XmlNode node = doc.CreateNode(XmlNodeType.Element, "role", "");
			XmlAttribute nameAttribute = doc.CreateAttribute("name");
			nameAttribute.Value = role;
			node.Attributes.Append(nameAttribute);
			screenNode.AppendChild(node);
			this.SaveAccessFile(doc);
		}
        public void AddFirstRole(string screen, string role)
        {
            if (!(DeveloperAccessVerifier.Instance.IsOwner(this.ApplicationName) ||
                  DeveloperAccessVerifier.Instance.HasAccess(this.ApplicationName, screen)))
                throw new InvalidOperationException(String.Format("Failed to setup the access for the screen '{0}' from application '{1}'. You don't have access to that screen.", screen, this.ApplicationName));

            XmlDocument doc = this.LoadAccessFile();
            XmlNode root = doc.SelectSingleNode("access");
            if (null == root)
            {
                throw new InvalidOperationException("Access file has no access node");
            }
            XmlNode screenNode = root.SelectSingleNode("child::screen[attribute::key='" + screen + "']");
            if (null == screenNode)
            {
                throw new ArgumentException(string.Format("Can't find access '{0}'", screen));
            }
            XmlNode node = doc.CreateNode(XmlNodeType.Element, "role", "");
            XmlAttribute nameAttribute = doc.CreateAttribute("name");
            nameAttribute.Value = role;
            node.Attributes.Append(nameAttribute);
            if (screenNode.ChildNodes.Count > 0)
                screenNode.InsertBefore(node, screenNode.FirstChild);
            else
                screenNode.AppendChild(node);
            this.SaveAccessFile(doc);
        }
		public void ReplaceRole(string screen, string oldrole, string newrole)
		{
            if (!(DeveloperAccessVerifier.Instance.IsOwner(this.ApplicationName) ||
                  DeveloperAccessVerifier.Instance.HasAccess(this.ApplicationName, screen)))
                throw new InvalidOperationException(String.Format("Failed to setup the access for the screen '{0}' from application '{1}'. You don't have access to that screen.", screen, this.ApplicationName));
            
            if (!(oldrole == newrole))
			{
				XmlDocument doc = this.LoadAccessFile();
				XmlNode root = doc.SelectSingleNode("access");
				if (null == root)
				{
					throw new InvalidOperationException("Access file has no access node");
				}
				XmlNode screenNode = root.SelectSingleNode("child::screen[attribute::key='" + screen + "']");
				if (null == screenNode)
				{
					throw new ArgumentException(string.Format("Can't find access '{0}'", screen));
				}
				XmlNode roleNode = screenNode.SelectSingleNode("child::role[attribute::name='" + oldrole + "']");
				if (null != roleNode)
				{
					roleNode.Attributes["name"].Value = newrole;
				}
				this.SaveAccessFile(doc);
			}
		}
		public void RemoveRole(string screen, string role)
		{
            if (!(DeveloperAccessVerifier.Instance.IsOwner(this.ApplicationName) ||
                  DeveloperAccessVerifier.Instance.HasAccess(this.ApplicationName, screen)))
                throw new InvalidOperationException(String.Format("Failed to setup the access for the screen '{0}' from application '{1}'. You don't have access to that screen.", screen, this.ApplicationName));
            
            XmlDocument doc = this.LoadAccessFile();
			XmlNode root = doc.SelectSingleNode("access");
			if (null == root)
			{
				throw new InvalidOperationException("Access file has no access node");
			}
			XmlNode screenNode = root.SelectSingleNode("child::screen[attribute::key='" + screen + "']");
			if (null == screenNode)
			{
				throw new ArgumentException(string.Format("Can't find access '{0}'", screen));
			}
			XmlNode roleNode = screenNode.SelectSingleNode("child::role[attribute::name='" + role + "']");
			if (null != roleNode)
			{
				screenNode.RemoveChild(roleNode);
			}
			this.SaveAccessFile(doc);
		}
		protected override DataSourceView GetView(string viewName)
		{
			DataSourceView result;
			if ("/" == viewName)
			{
				result = new AccessView(this, viewName);
			}
			else
			{
				if (!string.IsNullOrEmpty(viewName))
				{
					result = new AccessRolesView(this, viewName);
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

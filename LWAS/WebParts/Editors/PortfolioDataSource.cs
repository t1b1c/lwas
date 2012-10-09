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
using System.Collections.Generic;
using System.Web.UI;
using System.Xml;

using LWAS.Extensible.Interfaces.Storage;
using LWAS.Infrastructure.Storage;
using LWAS.Infrastructure.Security;

namespace LWAS.WebParts.Editors
{
	public class PortfolioDataSource : DataSourceControl
	{
		private string _portfolio;
        public string Portfolio
        {
            get { return _portfolio; }
        }
		private string _repoPath;
        public string RepoPath
        {
            get { return _repoPath; }
        }
		private string _repoFile;
        public string RepoFile
        {
            get { return _repoFile; }
        }
		private IStorageContainer _container;
		public IStorageContainer Container
		{
			get { return this._container; }
		}
        private string _accessContainer;
        private string _accessFile;
        private string _ownersFile;

        public string Filter { get; set; }

        bool _ignoreOwnership = false;

		public PortfolioDataSource(string portfolio, string repoPath, string repoFile)
		{
			this._portfolio = portfolio;
			this._repoPath = repoPath;
			this._repoFile = repoFile;
			this._container = new DirectoryContainer(string.Empty, repoPath);
		}
        public PortfolioDataSource(string portfolio, string repoPath, string repoFile, string accessContainer, string accessFile, bool ignoreOwnership, string ownersFile)
            : this(portfolio, repoPath, repoFile)
        {
            this._accessContainer = accessContainer;
            this._accessFile = accessFile;
            this._ignoreOwnership = ignoreOwnership;
            this._ownersFile = ownersFile;
        }
		protected XmlDocument LoadRepoFile()
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(this._container.Agent.Read(this._repoFile));
			return doc;
		}
		protected void SaveRepoFile(XmlDocument doc)
		{
			this._container.Agent.Erase(this._repoFile);
            try
            {
                doc.Save(this._container.Agent.OpenStream(this._repoFile));
            }
            finally
            {
                this._container.Agent.CloseStream(this._repoFile);
            }
		}

        protected XmlDocument LoadOwnersFile()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml((new FileAgent()).Read(this._ownersFile));
            return doc;
        }
        protected void SaveOwnersFile(XmlDocument doc)
        {
            FileAgent agent = new FileAgent();
            agent.Erase(this._ownersFile);
            try
            {
                doc.Save(agent.OpenStream(this._ownersFile));
            }
            finally
            {
                agent.CloseStream(this._ownersFile);
            }
        }

		public IEnumerable ListApplications()
		{
			if (null == this._container)
			{
				throw new InvalidOperationException("Storage container not set");
			}
			ArrayList ret = new ArrayList();
			XmlDocument doc = this.LoadRepoFile();
			XmlNode root = doc.SelectSingleNode("apps");
			if (null == root)
			{
				throw new InvalidOperationException("Repository file has no apps node");
			}
			foreach (XmlNode appNode in root.ChildNodes)
			{
				if ("app" == appNode.Name && null != appNode.Attributes["key"])
				{
                    if (_ignoreOwnership || DeveloperAccessVerifier.Instance.IsOwner(appNode.Attributes["key"].Value))
                        ret.Add(appNode.Attributes["key"].Value);
				}
			}
			return ret;
		}
		public string ApplicationConnectionString(string app)
		{
			XmlDocument doc = this.LoadRepoFile();
			XmlNode root = doc.SelectSingleNode("apps");
			if (null == root)
			{
				throw new InvalidOperationException("Repository file has no apps node");
			}
			XmlNode appNode = root.SelectSingleNode("child::app[attribute::key='" + app + "']");
			string result;
			if (appNode != null && null != appNode.Attributes["connectionString"])
			{
				result = appNode.Attributes["connectionString"].Value;
			}
			else
			{
				result = null;
			}
			return result;
		}
		public void CreateApplication(string name)
		{
			if (this._container.HasKey(name))
				throw new ArgumentException(string.Format("Duplicate repository '{0}' found", name));

			XmlDocument doc = this.LoadRepoFile();
			XmlNode root = doc.SelectSingleNode("apps");
			if (null == root) throw new InvalidOperationException("Repository file has no apps node");

			XmlNode node = doc.CreateNode(XmlNodeType.Element, "app", "");
			XmlAttribute keyAttribute = doc.CreateAttribute("key");
			keyAttribute.Value = name;
			node.Attributes.Append(keyAttribute);
			root.AppendChild(node);
			this._container.CreateContainer(name).CreateContainer(this._portfolio);
			this.SaveRepoFile(doc);

            AddOwner(name, User.CurrentUser.Name);
		}
        public void AddOwner(string application, string owner)
        {
            XmlDocument doc = this.LoadOwnersFile();
            XmlNode root = doc.SelectSingleNode("owners");
            if (null == root) throw new InvalidOperationException("Owners file has no owners node");

            XmlNode node = root.SelectSingleNode("child::app[attribute::name='" + application + "']");
            if (node == null)
            {
                node = doc.CreateNode(XmlNodeType.Element, "app", "");
                XmlAttribute keyAttribute = doc.CreateAttribute("name");
                keyAttribute.Value = application;
                node.Attributes.Append(keyAttribute);
                root.AppendChild(node);
            }

            XmlNode userNode = node.SelectSingleNode("child::user[attribute::name='" + owner + "']");
            if (userNode == null)
            {
                userNode = doc.CreateNode(XmlNodeType.Element, "user", "");
                XmlAttribute userAttribute = doc.CreateAttribute("name");
                userAttribute.Value = owner;
                userNode.Attributes.Append(userAttribute);

                node.AppendChild(userNode);
                this.SaveOwnersFile(doc);
            }

        }
		public void RenameApplication(string oldname, string newname)
		{
			if (!(oldname == newname))
			{
				if (!this._container.HasKey(oldname)) throw new ArgumentException(string.Format("Can't find repository '{0}'", oldname));
				if (this._container.HasKey(newname)) throw new ArgumentException(string.Format("Duplicate repository '{0}' found", newname));

				XmlDocument doc = this.LoadRepoFile();
				XmlNode root = doc.SelectSingleNode("apps");
				if (null == root) throw new InvalidOperationException("Repository file has no apps node");

				XmlNode appNode = root.SelectSingleNode("child::app[attribute::key='" + oldname + "']");
				if (null == appNode) throw new ArgumentException(string.Format("Can't find application '{0}'", new object[0]), oldname);

				appNode.Attributes["key"].Value = newname;
                this._container.CreateContainer(oldname).Key = newname;

                this.SaveRepoFile(doc);

                XmlDocument ownersdoc = this.LoadOwnersFile();
                XmlNode ownersroot = ownersdoc.SelectSingleNode("owners");
                if (null == ownersroot) throw new InvalidOperationException("Owners file has no owners node");

                XmlNode node = ownersroot.SelectSingleNode("child::app[attribute::name='" + oldname + "']");
                if (null != node)
                {
                    node.Attributes["name"].Value = newname;
                    SaveOwnersFile(ownersdoc);
                }
			}
		}
		public void DeleteApplication(string name)
		{
            if (!DeveloperAccessVerifier.Instance.IsOwner(name)) throw new InvalidOperationException(String.Format("Failed to delete the application '{0}'. You don't own that application.", name));
			if (!this._container.HasKey(name)) throw new ArgumentException(string.Format("Can't find repository '{0}'", name));

			XmlDocument doc = this.LoadRepoFile();
			XmlNode root = doc.SelectSingleNode("apps");
			if (null == root) throw new InvalidOperationException("Repository file has no apps node");

			XmlNode appNode = root.SelectSingleNode("child::app[attribute::key='" + name + "']");
			if (null == appNode) throw new ArgumentException(string.Format("Can't find application '{0}'", new object[0]), name);

			this._container.CreateContainer(name).Delete();
            root.RemoveChild(appNode);
            this.SaveRepoFile(doc);

            XmlDocument ownersdoc = this.LoadOwnersFile();
            XmlNode ownersroot = ownersdoc.SelectSingleNode("owners");
            if (null == ownersroot) throw new InvalidOperationException("Owners file has no owners node");

            XmlNode node = ownersroot.SelectSingleNode("child::app[attribute::name='" + name + "']");
            if (null != node)
            {
                ownersroot.RemoveChild(node);
                SaveOwnersFile(ownersdoc);
            }
		}
		public IEnumerable ListScreens(string application)
		{
			ArrayList ret = new ArrayList();
			XmlDocument doc = this.LoadRepoFile();
			XmlNode root = doc.SelectSingleNode("apps");
			if (null == root)
			{
				throw new InvalidOperationException("Repository file has no apps node");
			}
			XmlNode appNode = root.SelectSingleNode("child::app[attribute::key='" + application + "']");
			if (null != appNode)
			{
				foreach (XmlNode screenNode in appNode.ChildNodes)
				{
					if ("screen" == screenNode.Name)
					{
                        if (DeveloperAccessVerifier.Instance.IsOwner(appNode.Attributes["key"].Value) ||
                            DeveloperAccessVerifier.Instance.HasAccess(appNode.Attributes["key"].Value, screenNode.Attributes["key"].Value))
                            if (String.IsNullOrEmpty(this.Filter) || screenNode.Attributes["key"].Value.ToLower().Contains(this.Filter.ToLower()))
                                ret.Add(screenNode.Attributes["key"].Value);
					}
				}
			}
			return ret;
		}
        public IEnumerable ListScreensAll(string application)
        {
            ArrayList ret = new ArrayList();
            XmlDocument doc = this.LoadRepoFile();
            XmlNode root = doc.SelectSingleNode("apps");
            if (null == root)
            {
                throw new InvalidOperationException("Repository file has no apps node");
            }
            XmlNode appNode = root.SelectSingleNode("child::app[attribute::key='" + application + "']");
            if (null != appNode)
                foreach (XmlNode screenNode in appNode.ChildNodes)
                    if ("screen" == screenNode.Name)
                        ret.Add(screenNode.Attributes["key"].Value);

            return ret;
        }
        public void CreateScreen(string application, string name, string role)
        {
            if (!DeveloperAccessVerifier.Instance.IsOwner(application)) throw new InvalidOperationException(String.Format("Failed to create screen '{0}' in application '{1}'. You don't own that application.", name, application));

            CreateScreen(application, name);
            RoleToScreen(application, name, role);
        }
		public void CreateScreen(string application, string name)
		{
            if (!DeveloperAccessVerifier.Instance.IsOwner(application)) throw new InvalidOperationException(String.Format("Failed to create screen '{0}' in application '{1}'. You don't own that application.", name, application));
            if (!this._container.HasKey(application)) throw new ArgumentException(string.Format("Can't find repository '{0}'", application));

			IStorageContainer portfolioContainer = this._container.CreateContainer(application).CreateContainer(this._portfolio);
			string file = name + ".xml";
			if (portfolioContainer.Agent.HasKey(file)) throw new ArgumentException(string.Format("Duplicate screen '{0}' found in repository '{1}'", name, application));

			XmlDocument doc = this.LoadRepoFile();
			XmlNode root = doc.SelectSingleNode("apps");
			if (null == root) throw new InvalidOperationException("Repository file has no apps node");

			XmlNode appNode = root.SelectSingleNode("child::app[attribute::key='" + application + "']");
			if (null == appNode) throw new ArgumentException(string.Format("Can't find application '{0}'", new object[0]), application);

			XmlNode node = doc.CreateNode(XmlNodeType.Element, "screen", "");
			XmlAttribute keyAttribute = doc.CreateAttribute("key");
			keyAttribute.Value = name;
			node.Attributes.Append(keyAttribute);
			appNode.AppendChild(node);
			portfolioContainer.Agent.Write(file, string.Empty);
			this.SaveRepoFile(doc);
		}
        public void RenameScreen(string application, string oldname, string newname, string role)
        {
            if (!DeveloperAccessVerifier.Instance.IsOwner(application)) throw new InvalidOperationException(String.Format("Failed to rename screen '{0}' from application '{1}'. You don't own that application.", oldname, application));

            RenameScreen(application, oldname, newname);
            RoleToScreen(application, oldname, newname, role);
        }
		public void RenameScreen(string application, string oldname, string newname)
		{
            if (!DeveloperAccessVerifier.Instance.IsOwner(application)) throw new InvalidOperationException(String.Format("Failed to rename screen '{0}' from application '{1}'. You don't own that application.", oldname, application));
            
            if (!(oldname == newname))
			{
				if (!this._container.HasKey(application)) throw new ArgumentException(string.Format("Can't find repository '{0}'", application));

				IStorageContainer portfolioContainer = this._container.CreateContainer(application).CreateContainer(this._portfolio);
				string oldfile = oldname + ".xml";
				if (!portfolioContainer.Agent.HasKey(oldfile)) throw new ArgumentException(string.Format("Can't find screen '{0}' in repository '{1}'", oldname, application));

				string newfile = newname + ".xml";
				if (portfolioContainer.Agent.HasKey(newfile)) throw new ArgumentException(string.Format("Duplicate screen '{0}' found in repository '{1}'", newname, application));

				XmlDocument doc = this.LoadRepoFile();
				XmlNode root = doc.SelectSingleNode("apps");
				if (null == root) throw new InvalidOperationException("Repository file has no apps node");

				XmlNode appNode = root.SelectSingleNode("child::app[attribute::key='" + application + "']");
				if (null == appNode) throw new ArgumentException(string.Format("Can't find application '{0}'", new object[0]), application);

				XmlNode screenNode = appNode.SelectSingleNode("child::screen[attribute::key='" + oldname + "']");
				if (null != screenNode)
					screenNode.Attributes["key"].Value = newname;

				portfolioContainer.Agent.ReplaceKey(oldfile, newfile);
				this.SaveRepoFile(doc);
			}
		}
        public void DeleteScreen(string application, string name, string role)
        {
            if (!DeveloperAccessVerifier.Instance.IsOwner(application)) throw new InvalidOperationException(String.Format("Failed to delete screen '{0}' from application '{1}'. You don't own that application.", name, application));

            RemoveRole(application, name, role);
            DeleteScreen(application, name);
        }
		public void DeleteScreen(string application, string name)
		{
            if (!DeveloperAccessVerifier.Instance.IsOwner(application)) throw new InvalidOperationException(String.Format("Failed to delete screen '{0}' from application '{1}'. You don't own that application.", name, application));
            if (!this._container.HasKey(application)) throw new ArgumentException(string.Format("Can't find repository '{0}'", application));

			IStorageContainer portfolioContainer = this._container.CreateContainer(application).CreateContainer(this._portfolio);
			string file = name + ".xml";
			if (!portfolioContainer.Agent.HasKey(file)) throw new ArgumentException(string.Format("Can't find screen '{0}' in repository '{1}'", name, application));

			XmlDocument doc = this.LoadRepoFile();
			XmlNode root = doc.SelectSingleNode("apps");
			if (null == root) throw new InvalidOperationException("Repository file has no apps node");

			XmlNode appNode = root.SelectSingleNode("child::app[attribute::key='" + application + "']");
			if (null == appNode) throw new ArgumentException(string.Format("Can't find application '{0}'", new object[0]), application);

			XmlNode screenNode = appNode.SelectSingleNode("child::screen[attribute::key='" + name + "']");
			if (null != screenNode)
				appNode.RemoveChild(screenNode);
			if (portfolioContainer.HasKey(name))
				portfolioContainer.CreateContainer(name).Delete();
			if (portfolioContainer.Agent.HasKey(name + "_proxies.xml"))
				portfolioContainer.Agent.Erase(name + "_proxies.xml");
			if (portfolioContainer.Agent.HasKey(name + "_containers.xml"))
				portfolioContainer.Agent.Erase(name + "_containers.xml");

			portfolioContainer.Agent.Erase(file);
			if (portfolioContainer.IsEmpty())
				portfolioContainer.Delete();

			this.SaveRepoFile(doc);
		}
		public void ClearScreen(string application, string name)
		{
            if (!DeveloperAccessVerifier.Instance.IsOwner(application) && !DeveloperAccessVerifier.Instance.HasAccess(application, name)) 
                throw new InvalidOperationException(String.Format("Failed to clear screen '{0}' from application '{1}'. You can't access that screen.", name, application));

			if (!this._container.HasKey(application)) throw new ArgumentException(string.Format("Can't find repository '{0}'", application));

			IStorageContainer portfolioContainer = this._container.CreateContainer(application).CreateContainer(this._portfolio);
			string file = name + ".xml";
			if (!portfolioContainer.Agent.HasKey(file)) throw new ArgumentException(string.Format("Can't find screen '{0}' in repository '{1}'", name, application));

			if (portfolioContainer.HasKey(name))
                portfolioContainer.CreateContainer(name).Delete();
			if (portfolioContainer.Agent.HasKey(name + "_proxies.xml"))
                portfolioContainer.Agent.Erase(name + "_proxies.xml");
			if (portfolioContainer.Agent.HasKey(name + "_containers.xml"))
                portfolioContainer.Agent.Erase(name + "_containers.xml");
		}

        public void CopyScreen(string application, string sourcescreen, string screen)
        {
            if (!DeveloperAccessVerifier.Instance.IsOwner(application) && !DeveloperAccessVerifier.Instance.HasAccess(application, sourcescreen))
                throw new InvalidOperationException(String.Format("Failed to copy screen '{0}' from application '{1}'. You can't access that screen.", sourcescreen, application));

            if (!this._container.HasKey(application)) throw new ArgumentException(string.Format("Can't find repository '{0}'", application));

            IStorageContainer portfolioContainer = this._container.CreateContainer(application).CreateContainer(this._portfolio);
            string file = sourcescreen + ".xml";
            if (!portfolioContainer.Agent.HasKey(file)) throw new ArgumentException(string.Format("Can't find screen '{0}' in repository '{1}'", sourcescreen, application));

            string content = portfolioContainer.Agent.Read(file);
            CreateScreen(application, screen);
            portfolioContainer.Agent.Write(screen + ".xml", content);
        }

        protected AccessDataSource CreateAccessDataSource(string application)
        {
            if (String.IsNullOrEmpty(_repoPath)) throw new InvalidOperationException("Repo path not set");
            if (String.IsNullOrEmpty(_accessContainer)) throw new InvalidOperationException("Access container not set");
            if (String.IsNullOrEmpty(_accessFile)) throw new InvalidOperationException("Access file not set");
            if (String.IsNullOrEmpty(application)) throw new ArgumentNullException("application");

            string afp = System.IO.Path.Combine(_repoPath, application);
            afp = System.IO.Path.Combine(afp, _accessContainer);
            afp = System.IO.Path.Combine(afp, _accessFile);

            return new AccessDataSource(afp, application);
        }

        public string RoleForScreen(string application, string screen)
        {            
            AccessDataSource ads = CreateAccessDataSource(application);
            ArrayList roles = ads.ListRoles(screen) as ArrayList;
            if (roles.Count > 0)
                return roles[0] as string;
            else
                return null;
        }

        public void RoleToScreen(string application, string screen, string role)
        {
            AccessDataSource ads = CreateAccessDataSource(application);
            ArrayList screens = ads.ListAccess() as ArrayList;
            if (!screens.Contains(screen))
                ads.CreateAccess(screen);
            ArrayList roles = ads.ListRoles(screen) as ArrayList;
            if (!roles.Contains(role))
                ads.AddFirstRole(screen, role);
        }

        public void RoleToScreen(string application, string oldname, string screen, string role)
        {
            AccessDataSource ads = CreateAccessDataSource(application);
            ads.ReplaceScreen(oldname, screen);
            RoleToScreen(application, screen, role);
        }

        public void RemoveRole(string application, string screen, string role)
        {
            AccessDataSource ads = CreateAccessDataSource(application);
            ads.RemoveRole(screen, role);
        }

		protected override DataSourceView GetView(string viewName)
		{
			DataSourceView result;
			if ("/" == viewName)
			{
				result = new ApplicationsView(this, viewName);
			}
			else
			{
				if (!string.IsNullOrEmpty(viewName))
				{
                    if (!String.IsNullOrEmpty(_accessContainer) && !String.IsNullOrEmpty(_accessFile))
                        result = new ScreensWithAccessView(this, viewName);
                    else
                        result = new ScreensView(this, viewName);
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

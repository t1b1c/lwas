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
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using System.Web.Caching;

using LWAS.Extensible.Exceptions;
using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Configuration;
using LWAS.Extensible.Interfaces.Editors;
using LWAS.Extensible.Interfaces.Storage;
using LWAS.Extensible.Interfaces.WebParts;
using LWAS.Infrastructure;
using LWAS.Infrastructure.Security;

namespace LWAS.WebParts.Editors
{
	public class BuildManager : EditorPart, IEditor, IProvider, IInitializable, ILifetime
	{
		public class EditorIdentification
		{
			public string Key;
			public string Text;
			public string Url;
			public EditorIdentification(string key, string text, string url)
			{
				this.Key = key;
				this.Text = text;
				this.Url = url;
			}
		}
        private static object SyncRoot = new Object();
		private IStorageContainer _container;
		private string _text;
		private object _target;
		private EditingResult _result;
		private IConfiguration _configurationProvider;
		private int _initialization;
		private int _creation;
		private int _change;
		private int _completion;
		private RequestInitializationCallback _requestInitialization;
		public static BuildManager Instance;
		private static bool areEditorsLoaded;
		private static Dictionary<string, BuildManager.EditorIdentification> _editors;
		protected Panel InstructionsPanel = new Panel();
		public event EditorEventHandler EditComplete;
		public IStorageContainer Container
		{
			get
			{
				return this._container;
			}
			set
			{
				this._container = value;
			}
		}
		public virtual string Text
		{
			get
			{
				return this._text;
			}
			set
			{
				this._text = value;
				this.Title = this._text;
			}
		}
		public virtual object Target
		{
			get
			{
				return this._target;
			}
			set
			{
				this._target = value;
			}
		}
		public virtual IEditingResult Result
		{
			get
			{
				return this._result;
			}
			set
			{
				this._result = new EditingResult(value);
			}
		}
		public virtual IConfiguration ConfigurationProvider
		{
			get
			{
				return this._configurationProvider;
			}
			set
			{
				this._configurationProvider = value;
			}
		}
		public int Initialization
		{
			get
			{
				return this._initialization;
			}
			set
			{
				this._initialization = value;
			}
		}
		public int Creation
		{
			get
			{
				return this._creation;
			}
			set
			{
				this._creation = value;
			}
		}
		public int Change
		{
			get
			{
				return this._change;
			}
			set
			{
				this._change = value;
			}
		}
		public int Completion
		{
			get
			{
				return this._completion;
			}
			set
			{
				this._completion = value;
			}
		}
		public RequestInitializationCallback RequestInitialization
		{
			get
			{
				return this._requestInitialization;
			}
			set
			{
				this._requestInitialization = value;
			}
		}
		public static Dictionary<string, BuildManager.EditorIdentification> Editors
		{
			get
			{
				if (!BuildManager.areEditorsLoaded)
				{
					BuildManager.LoadEditors();
					BuildManager.areEditorsLoaded = true;
				}
				return BuildManager._editors;
			}
		}
		public virtual void Initialize()
		{
		}
		static BuildManager()
		{
			BuildManager.areEditorsLoaded = false;
			BuildManager._editors = new Dictionary<string, BuildManager.EditorIdentification>();
			BuildManager.Instance = new BuildManager();
		}
		protected static void LoadEditors()
		{
			if (null == BuildManager.Instance.Container)
			{
				throw new InvalidOperationException("Storage container not set");
			}
			string key = ConfigurationManager.AppSettings["EDITORS"];
			if (string.IsNullOrEmpty(key))
			{
				throw new ApplicationException("'EDITORS' web.config key not set");
			}
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(BuildManager.Instance.Container.Agent.Read(key));
			XmlNode root = doc.SelectSingleNode("editors");
			if (null == root)
			{
				throw new InvalidOperationException("Editors configuration file has no editors node");
			}
			foreach (XmlNode node in root.ChildNodes)
			{
				if (null == node.Attributes["key"])
				{
					throw new ApplicationException(string.Format("Invalid editors registry '{0}'. Missing attribute 'key' on node '{1}'", key, node.Name));
				}
				if (null == node.Attributes["text"])
				{
					throw new ApplicationException(string.Format("Invalid editors registry '{0}'. Missing attribute 'text' on node '{1}'", key, node.Name));
				}
				if (null == node.Attributes["url"])
				{
					throw new ApplicationException(string.Format("Invalid editors registry '{0}'. Missing attribute 'url' on node '{1}'", key, node.Name));
				}
				if (!BuildManager._editors.ContainsKey(node.Attributes["key"].Value))
				{
					BuildManager._editors.Add(node.Attributes["key"].Value, new BuildManager.EditorIdentification(node.Attributes["key"].Value, node.Attributes["text"].Value, node.Attributes["url"].Value));
				}
			}
		}
		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			this.Controls.Add(this.InstructionsPanel);
		}
		public override void SyncChanges()
		{
			string key;
			if (base.WebPartToEdit is ISymbolWebPart)
			{
				string symbolOf = ((ISymbolWebPart)base.WebPartToEdit).SymbolOf;
				if (symbolOf.Contains(","))
				{
					key = symbolOf.Substring(0, symbolOf.IndexOf(","));
				}
				else
				{
					key = symbolOf;
				}
			}
			else
			{
				key = base.WebPartToEdit.ToString();
			}
			if (string.IsNullOrEmpty(key))
			{
				throw new InvalidOperationException("Cannot retrieve editor's key");
			}
			if (!BuildManager.Editors.ContainsKey(key))
			{
				throw new InvalidOperationException(string.Format("Unkown editor '{0}'", key));
			}
			IConfigurableWebPart target = base.WebPartToEdit as IConfigurableWebPart;
			if (null == target)
			{
				throw new WrongWebPartTypeException(typeof(IConfigurableWebPart).Name);
			}
			string app = base.WebPartToEdit.Page.Request["a"];
			string screen = base.WebPartToEdit.Page.Request["screen"];
			string part = base.WebPartToEdit.ID;
			if (string.IsNullOrEmpty(screen))
			{
				screen = base.WebPartToEdit.Page.Request["s"];
			}
			if (base.WebPartToEdit is ISymbolWebPart)
			{
				BuildManager.SetConfigurationStatic(app + "." + base.WebPartToEdit.ID, target.Configuration);
			}
			else
			{
				BuildManager.SetConfiguration(app, screen, part, target.Configuration, false);
			}
			IEditableWebPart editableTarget = base.WebPartToEdit as IEditableWebPart;
			if (null != editableTarget)
			{
				editableTarget.BeginEdit();
			}
			this.DisplayInstructions(BuildManager.Editors[key], app, screen, part);
		}
		public static void SetConfiguration(string app, string screen, string part, IConfiguration config, bool overwrite)
		{
			if (null == BuildManager.Instance.Container)
			{
				throw new InvalidOperationException("Storage container not set");
			}
			if (null == config)
			{
				throw new ArgumentNullException("config");
			}
			string bin = ConfigurationManager.AppSettings["EDIT_REPO"];
			if (string.IsNullOrEmpty(bin))
			{
				throw new ApplicationException("'EDIT_REPO' web.config key not set");
			}
			string portfolio = ConfigurationManager.AppSettings["EDIT_REPO_PORTFOLIO"];
			if (string.IsNullOrEmpty(portfolio))
			{
				throw new InvalidOperationException("EDIT_REPO_PORTFOLIO key not set in config file");
			}
			IStorageContainer screenContainer = BuildManager.Instance.Container.CreateContainer(bin).CreateContainer(app).CreateContainer(portfolio).CreateContainer(screen);
			string key = part + ".xml";
			if (!screenContainer.Agent.HasKey(key) || overwrite)
			{
				StringBuilder content = new StringBuilder();
				using (StringWriter stringWriter = new StringWriter(content))
				{
					using (XmlTextWriter writer = new XmlTextWriter(stringWriter))
					{
						writer.Formatting = Formatting.Indented;
						writer.WriteStartDocument();
						writer.WriteStartElement("edit");
						writer.WriteAttributeString("type", SerializationServices.ShortAssemblyQualifiedName(config.GetType().AssemblyQualifiedName));
						SerializationServices.Serialize(config, writer);
						writer.WriteEndElement();
					}
				}
				if (screenContainer.Agent.HasKey(key))
				{
					screenContainer.Agent.Erase(key);
				}
				screenContainer.Agent.Write(key, content.ToString());
			}
		}
		public static void SetConfigurationStatic(string key, IConfiguration config)
		{
			if (null == BuildManager.Instance.Container)
			{
				throw new InvalidOperationException("Storage container not set");
			}
			if (null == config)
			{
				throw new ArgumentNullException("config");
			}
			string userData = ConfigurationManager.AppSettings["USER_DATA"];
			if (string.IsNullOrEmpty(userData))
			{
				throw new ApplicationException("'USER_DATA' web.config key not set");
			}
			string userEditRepo = ConfigurationManager.AppSettings["EDIT_USER_REPO"];
			if (string.IsNullOrEmpty(userEditRepo))
			{
				throw new InvalidOperationException("EDIT_USER_REPO key not set in config file");
			}
			IStorageContainer userRepoContainer = BuildManager.Instance.Container.CreateContainer(userData).CreateContainer(User.CurrentUser.Name).CreateContainer(userEditRepo);
			key += ".xml";
			StringBuilder content = new StringBuilder();
			using (StringWriter stringWriter = new StringWriter(content))
			{
				using (XmlTextWriter writer = new XmlTextWriter(stringWriter))
				{
					writer.Formatting = Formatting.Indented;
					writer.WriteStartDocument();
					writer.WriteStartElement("edit");
                    writer.WriteAttributeString("type", SerializationServices.ShortAssemblyQualifiedName(config.GetType().AssemblyQualifiedName));
					SerializationServices.Serialize(config, writer);
					writer.WriteEndElement();
				}
			}
			if (userRepoContainer.Agent.HasKey(key))
			{
				userRepoContainer.Agent.Erase(key);
			}
			userRepoContainer.Agent.Write(key, content.ToString());
		}
		public virtual void DisplayInstructions(BuildManager.EditorIdentification editor, string app, string screen, string part)
		{
			if (null == editor)
			{
				throw new ArgumentNullException("editor");
			}
			this.Title = editor.Text + " : " + part;
			this.InstructionsPanel.Controls.Clear();
			this.InstructionsPanel.Controls.Add(new LiteralControl("Click "));
			HyperLink link = new HyperLink();
			link.Text = "here";
			string url = editor.Url;
			url = this.AddQueryParam(url, "a", app);
			url = this.AddQueryParam(url, "s", screen);
			url = this.AddQueryParam(url, "p", part);
			link.NavigateUrl = url;
			this.InstructionsPanel.Controls.Add(link);
			this.InstructionsPanel.Controls.Add(new LiteralControl(" to open the editor page"));
		}
		private string AddQueryParam(string url, string param, string value)
		{
			if (!url.Contains("?"))
			{
				url += "?";
			}
			if (!url.EndsWith("?") && !url.EndsWith("&"))
			{
				url += "&";
			}
			url = url + param + "=" + value;
			return url;
		}
		public override bool ApplyChanges()
		{
			IConfigurableWebPart target = base.WebPartToEdit as IConfigurableWebPart;
			if (null == target)
			{
				throw new WrongWebPartTypeException(typeof(IConfigurableWebPart).Name);
			}
			string app = base.WebPartToEdit.Page.Request["a"];
			string screen = base.WebPartToEdit.Page.Request["screen"];
			string part = base.WebPartToEdit.ID;
			if (string.IsNullOrEmpty(screen))
			{
				screen = base.WebPartToEdit.Page.Request["s"];
			}
			target.Configuration = BuildManager.GetConfiguration(app, screen, part);
			IEditableWebPart editableTarget = base.WebPartToEdit as IEditableWebPart;
			if (null != editableTarget)
			{
				editableTarget.EndEdit();
			}
			if (null != this.EditComplete)
			{
				this.EditComplete(this, EditorEventArgs.Empty);
			}
			return true;
		}

        public static void ApplyChanges(string app, string screen, string part)
        {
        }

		public static IConfiguration GetConfiguration(string app, string screen, string part)
		{
            IConfiguration cachedconfig = GetConfigurationFromCache(app + "." + screen + "." + part);
            if (null != cachedconfig)
                return cachedconfig;

			if (null == BuildManager.Instance.Container)
			{
				throw new InvalidOperationException("Storage container not set");
			}
			string bin = ConfigurationManager.AppSettings["EDIT_REPO"];
			if (string.IsNullOrEmpty(bin))
			{
				throw new ApplicationException("'EDIT_REPO' web.config key not set");
			}
			string portfolio = ConfigurationManager.AppSettings["EDIT_REPO_PORTFOLIO"];
			if (string.IsNullOrEmpty(portfolio))
			{
				throw new InvalidOperationException("EDIT_REPO_PORTFOLIO key not set in config file");
			}
			IStorageContainer screenContainer = BuildManager.Instance.Container.CreateContainer(bin).CreateContainer(app).CreateContainer(portfolio).CreateContainer(screen);
			string key = part + ".xml";
			IConfiguration result;
			if (!screenContainer.Agent.HasKey(key))
			{
				result = null;
			}
			else
			{
				object config = null;
				using (StringReader stringReader = new StringReader(screenContainer.Agent.Read(key)))
				{
					using (XmlTextReader reader = new XmlTextReader(stringReader))
					{
						reader.MoveToContent();
						if ("edit" != reader.Name)
						{
							throw new ApplicationException(string.Format("Expected 'edit' but found '{0}' as a start element in '{1}'", reader.Name, key));
						}
						if (reader.MoveToAttribute("type"))
						{
							Type type = Type.GetType(reader.Value);
							if (null == type)
							{
								throw new ApplicationException(string.Format("Failed to create type '{0}'", reader.Value));
							}
							reader.MoveToContent();
							config = SerializationServices.Deserialize(type, reader);
						}
					}
				}
				result = (config as IConfiguration);
			}

            AddConfigurationToCache(app + "." + screen + "." + part, result, Path.Combine(screenContainer.ToString(), key));

			return result;
		}

		public static IConfiguration GetConfigurationStatic(string key)
		{
			if (null == BuildManager.Instance.Container)
			{
				throw new InvalidOperationException("Storage container not set");
			}
			string userData = ConfigurationManager.AppSettings["USER_DATA"];
			if (string.IsNullOrEmpty(userData))
			{
				throw new ApplicationException("'USER_DATA' web.config key not set");
			}
			string userEditRepo = ConfigurationManager.AppSettings["EDIT_USER_REPO"];
			if (string.IsNullOrEmpty(userEditRepo))
			{
				throw new InvalidOperationException("EDIT_USER_REPO key not set in config file");
			}
			IStorageContainer userRepoContainer = BuildManager.Instance.Container.CreateContainer(userData).CreateContainer(User.CurrentUser.Name).CreateContainer(userEditRepo);
			key += ".xml";
			IConfiguration result;
			if (!userRepoContainer.Agent.HasKey(key))
			{
				result = null;
			}
			else
			{
				object config = null;
				using (StringReader stringReader = new StringReader(userRepoContainer.Agent.Read(key)))
				{
					using (XmlTextReader reader = new XmlTextReader(stringReader))
					{
						reader.MoveToContent();
						if ("edit" != reader.Name)
						{
							throw new ApplicationException(string.Format("Expected 'edit' but found '{0}' as a start element in '{1}'", reader.Name, key));
						}
						if (reader.MoveToAttribute("type"))
						{
							Type type = Type.GetType(reader.Value);
							if (null == type)
							{
								throw new ApplicationException(string.Format("Failed to create type '{0}'", reader.Value));
							}
							reader.MoveToContent();
							config = SerializationServices.Deserialize(type, reader);
						}
					}
				}
				result = (config as IConfiguration);
			}
			return result;
		}
		public static void RegisterScreenParts(WebPartManager manager, string app, string screen)
		{
			foreach (WebPart part in manager.WebParts)
			{
				if (part is IConfigurableWebPart)
				{
					BuildManager.SetConfiguration(app, screen, part.ID, ((IConfigurableWebPart)part).Configuration, false);
				}
			}
		}
		public static void InstantiateProxies(string app, string screen, WebPartManager manager, WebPartZone zone)
		{
			if (null == BuildManager.Instance.Container)
			{
				throw new InvalidOperationException("Storage container not set");
			}
			string bin = ConfigurationManager.AppSettings["EDIT_REPO"];
			if (string.IsNullOrEmpty(bin))
			{
				throw new ApplicationException("'EDIT_REPO' web.config key not set");
			}
			string portfolio = ConfigurationManager.AppSettings["EDIT_REPO_PORTFOLIO"];
			if (string.IsNullOrEmpty(portfolio))
			{
				throw new InvalidOperationException("EDIT_REPO_PORTFOLIO key not set in config file");
			}
			IStorageContainer portfolioContainer = BuildManager.Instance.Container.CreateContainer(bin).CreateContainer(app).CreateContainer(portfolio);
            string cachedkey = app + "." + screen + "..crawler";
            PersonalizationCrawler crawler = GetCrawlerFromCache(cachedkey);
            if (null == crawler)
            {
                string fileToMonitor = Path.Combine(portfolioContainer.ToString(), screen + ".xml");
                crawler = new PersonalizationCrawler(portfolioContainer.Agent, screen + ".xml");
                AddCrawlerToCache(cachedkey, crawler, fileToMonitor);
            }
			foreach (PersonalizationCrawler.PartInfo partInfo in crawler.Proxies)
			{
				WebPart part = manager.WebParts[partInfo.Id];
				if (null == part)
				{
					part = (ReflectionServices.CreateInstance(partInfo.Name) as WebPart);
				}
				if (null == part.Zone)
				{
					part = manager.AddWebPart(part, zone, zone.WebParts.Count);
					part.ID = partInfo.Id;
					part.Title = partInfo.Title;
				}
				part.AllowClose = false;
				part.AllowHide = false;
				part.AllowMinimize = false;
				if (part is IConfigurableWebPart)
				{
					((IConfigurableWebPart)part).Configuration = BuildManager.GetConfiguration(app, screen, partInfo.Id);
				}
			}
		}
		public static List<string> ListContainers(string app, string screen)
		{
			if (null == BuildManager.Instance.Container)
			{
				throw new InvalidOperationException("Storage container not set");
			}
			List<string> list = new List<string>();
			string bin = ConfigurationManager.AppSettings["EDIT_REPO"];
			if (string.IsNullOrEmpty(bin))
			{
				throw new ApplicationException("'EDIT_REPO' web.config key not set");
			}
			string portfolio = ConfigurationManager.AppSettings["EDIT_REPO_PORTFOLIO"];
			if (string.IsNullOrEmpty(portfolio))
			{
				throw new InvalidOperationException("EDIT_REPO_PORTFOLIO key not set in config file");
			}
			IStorageContainer portfolioContainer = BuildManager.Instance.Container.CreateContainer(bin).CreateContainer(app).CreateContainer(portfolio);
            string cachedkey = app + "." + screen + "..crawler";
            PersonalizationCrawler crawler = GetCrawlerFromCache(cachedkey);
            if (null == crawler)
            {
                string fileToMonitor = Path.Combine(portfolioContainer.ToString(), screen + ".xml");
                if (portfolioContainer.Agent.HasKey(fileToMonitor))
                {
                    try
                    {
                        crawler = new PersonalizationCrawler(portfolioContainer.Agent, screen + ".xml");
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException(String.Format("Failed to crawl {0}.{1}", app, screen), ex);
                    }
                    AddCrawlerToCache(cachedkey, crawler, fileToMonitor);
                }
            }
			if (null != crawler)
			{
				foreach (PersonalizationCrawler.PartInfo partInfo in crawler.Containers)
				{
					list.Add(partInfo.Id);
				}
			}
			return list;
		}
		public static List<string> ListParts(string app, string screen, IList<string> zones)
		{
			if (null == BuildManager.Instance.Container)
			{
				throw new InvalidOperationException("Storage container not set");
			}
			List<string> list = new List<string>();
			string bin = ConfigurationManager.AppSettings["EDIT_REPO"];
			if (string.IsNullOrEmpty(bin))
			{
				throw new ApplicationException("'EDIT_REPO' web.config key not set");
			}
			string portfolio = ConfigurationManager.AppSettings["EDIT_REPO_PORTFOLIO"];
			if (string.IsNullOrEmpty(portfolio))
			{
				throw new InvalidOperationException("EDIT_REPO_PORTFOLIO key not set in config file");
			}
			IStorageContainer portfolioContainer = BuildManager.Instance.Container.CreateContainer(bin).CreateContainer(app).CreateContainer(portfolio);
            string cachedkey = app + "." + screen + "..crawler";
            PersonalizationCrawler crawler = GetCrawlerFromCache(cachedkey);
            if (null == crawler)
            {
                string fileToMonitor = Path.Combine(portfolioContainer.ToString(), screen + ".xml");
                crawler = new PersonalizationCrawler(portfolioContainer.Agent, screen + ".xml");
                AddCrawlerToCache(cachedkey, crawler, fileToMonitor);
            }
            foreach (PersonalizationCrawler.PartInfo partInfo in crawler.Parts)
			{
				if (zones == null || zones.Contains(partInfo.Zone))
				{
					list.Add(partInfo.Id);
				}
			}
			return list;
		}
		public static List<string> ListMilestones(string app, string screen, string part)
		{
            Cache cache = HttpRuntime.Cache;
            string cachekey = app + "." + screen + "." + part + ".milestones";
            List<string> result = cache[cachekey] as List<string>;
            if (null == result)
            {
                if (null == BuildManager.Instance.Container)
                {
                    throw new InvalidOperationException("Storage container not set");
                }
                string bin = ConfigurationManager.AppSettings["EDIT_REPO"];
                if (string.IsNullOrEmpty(bin))
                {
                    throw new ApplicationException("'EDIT_REPO' web.config key not set");
                }
                string portfolio = ConfigurationManager.AppSettings["EDIT_REPO_PORTFOLIO"];
                if (string.IsNullOrEmpty(portfolio))
                {
                    throw new InvalidOperationException("EDIT_REPO_PORTFOLIO key not set in config file");
                }
                IStorageContainer portfolioContainer = BuildManager.Instance.Container.CreateContainer(bin).CreateContainer(app).CreateContainer(portfolio);
                string cachedkey = app + "." + screen + "..crawler";
                PersonalizationCrawler crawler = GetCrawlerFromCache(cachedkey);
                if (null == crawler)
                {
                    string fileToMonitor = Path.Combine(portfolioContainer.ToString(), screen + ".xml");
                    crawler = new PersonalizationCrawler(portfolioContainer.Agent, screen + ".xml");
                    AddCrawlerToCache(cachedkey, crawler, fileToMonitor);
                }

                foreach (PersonalizationCrawler.PartInfo partInfo in crawler.Parts)
                {
                    if (partInfo.Id == part)
                    {
                        result = BuildManager.ListMilestones(partInfo.Name.Substring(0, partInfo.Name.IndexOf(",")));
                        string fileToMonitor = Path.Combine(portfolioContainer.ToString(), screen + ".xml");
                        cache.Insert(cachekey, result, new CacheDependency(fileToMonitor));
                        return result;
                    }
                }
            }
			return result;
		}
		public static List<string> ListMilestones(string partType)
		{
			if (null == BuildManager.Instance.Container)
			{
				throw new InvalidOperationException("Storage container not set");
			}
			string key = ConfigurationManager.AppSettings["MILESTONES"];
			if (string.IsNullOrEmpty(key))
			{
				throw new ApplicationException("'MILESTONES' web.config key not set");
			}
			List<string> list = new List<string>();
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(BuildManager.Instance.Container.Agent.Read(key));
			XmlNode root = doc.SelectSingleNode("parts");
			if (null == root)
			{
				throw new InvalidOperationException("Milestones configuration file has no parts node");
			}
			XmlNode partNode = root.SelectSingleNode("child::part[attribute::key='" + partType + "']");
			if (null != partNode)
			{
				foreach (XmlNode node in partNode.ChildNodes)
				{
					if ("milestone" == node.Name && null != node.Attributes["key"])
					{
						list.Add(node.Attributes["key"].Value);
					}
				}
			}
			return list;
		}
		public static List<string> ListCustomMilestones(string app, string screen, string workflowPart, string partid)
		{
			List<string> list = new List<string>();
			List<string> stdmilestones = BuildManager.ListMilestones(app, screen, partid);
			if (null == stdmilestones)
			{
				stdmilestones = new List<string>();
			}
			IConfiguration config = BuildManager.GetConfiguration(app, screen, partid);
			if (config != null && config.Sections.ContainsKey("Template"))
			{
                foreach (IConfigurationElement templateElement in config.GetConfigurationSectionReference("Template").Elements.Values)
                {
                    if ("selectors" == templateElement.ConfigKey ||
                        "commanders" == templateElement.ConfigKey ||
                        "filter" == templateElement.ConfigKey ||
                        "header" == templateElement.ConfigKey ||
                        "footer" == templateElement.ConfigKey ||
                        "grouping" == templateElement.ConfigKey ||
                        "totals" == templateElement.ConfigKey)
                    {
                        foreach(IConfigurationElement element in templateElement.Elements.Values)
                            ListCustomCommands(element, stdmilestones, list);
                    }
                    else
                        ListCustomCommands(templateElement, stdmilestones, list);
                }
			}

            config = BuildManager.GetConfiguration(app, screen, workflowPart);
			if (config != null && config.Sections.ContainsKey("flows"))
			{
				foreach (IConfigurationElement jobElement in config.GetConfigurationSectionReference("flows").Elements.Values)
				{
					if (jobElement.Elements.ContainsKey("transits"))
					{
						foreach (IConfigurationElement transitElement in jobElement.GetElementReference("transits").Elements.Values)
						{
							if (transitElement.Elements.ContainsKey("source") && transitElement.Elements.ContainsKey("destination"))
							{
								IConfigurationElement sourceElement = transitElement.GetElementReference("source");
								IConfigurationElement destinationElement = transitElement.GetElementReference("destination");
								if (destinationElement.Attributes.ContainsKey("id") && destinationElement.GetAttributeReference("id").Value != null && partid == destinationElement.GetAttributeReference("id").Value.ToString() && destinationElement.Attributes.ContainsKey("member") && destinationElement.GetAttributeReference("member").Value != null && "Milestone" == destinationElement.GetAttributeReference("member").Value.ToString() && sourceElement.Attributes.ContainsKey("value") && sourceElement.GetAttributeReference("value").Value != null && !string.IsNullOrEmpty(sourceElement.GetAttributeReference("value").Value.ToString()))
								{
                                    string[] cmds = sourceElement.GetAttributeReference("value").Value.ToString().Split(',');
                                    foreach (string command in cmds)
                                        if (!stdmilestones.Contains(command) && !list.Contains(command))
                                            list.Add(command);
                                }
							}
						}
					}
				}
			}
			return list;
		}

        static void ListCustomCommands(IConfigurationElement templateElement, List<string> stdmilestones, List<string> list)
        {
            foreach (IConfigurationElement controlElement in templateElement.Elements.Values)
            {
                foreach (IConfigurationElement propertyElement in controlElement.Elements.Values)
                {
                    if (propertyElement.Attributes.ContainsKey("command") && propertyElement.GetAttributeReference("command").Value != null && !string.IsNullOrEmpty(propertyElement.GetAttributeReference("command").Value.ToString()))
                    {
                        string[] cmds = propertyElement.GetAttributeReference("command").Value.ToString().Split(',');
                        foreach (string command in cmds)
                            if (!stdmilestones.Contains(command) && !list.Contains(command))
                                list.Add(command);
                    }
                    if (propertyElement.Attributes.ContainsKey("member") && propertyElement.GetAttributeReference("member").Value != null && "CommandName" == propertyElement.GetAttributeReference("member").Value.ToString() && propertyElement.Attributes.ContainsKey("value") && propertyElement.GetAttributeReference("value").Value != null && !string.IsNullOrEmpty(propertyElement.GetAttributeReference("value").Value.ToString()))
                    {
                        string[] cmds = propertyElement.GetAttributeReference("value").Value.ToString().Split(',');
                        foreach (string command in cmds)
                            if (!stdmilestones.Contains(command) && !list.Contains(command))
                                list.Add(command);
                    }
                }
            }
        }

        public static List<string> ListContextKeys(string app, string screen, string workflowPart, string partid)
        {
            List<string> list = new List<string>();
            IConfiguration config = BuildManager.GetConfiguration(app, screen, workflowPart);
            if (config != null && config.Sections.ContainsKey("flows"))
            {
                foreach (IConfigurationElement jobElement in config.GetConfigurationSectionReference("flows").Elements.Values)
                {
                    if (jobElement.Elements.ContainsKey("transits"))
                    {
                        foreach (IConfigurationElement transitElement in jobElement.GetElementReference("transits").Elements.Values)
                        {
                            if (transitElement.Elements.ContainsKey("source") && transitElement.Elements.ContainsKey("destination"))
                            {
                                IConfigurationElement sourceElement = transitElement.GetElementReference("source");
                                IConfigurationElement destinationElement = transitElement.GetElementReference("destination");
                                if (destinationElement.Attributes.ContainsKey("id") && 
                                    destinationElement.GetAttributeReference("id").Value != null && 
                                    partid == destinationElement.GetAttributeReference("id").Value.ToString() && 
                                    destinationElement.Attributes.ContainsKey("member") && 
                                    destinationElement.GetAttributeReference("member").Value != null && 
                                    "DataKey" == destinationElement.GetAttributeReference("member").Value.ToString() && 
                                    sourceElement.Attributes.ContainsKey("value") && 
                                    sourceElement.GetAttributeReference("value").Value != null && 
                                    !String.IsNullOrEmpty(sourceElement.GetAttributeReference("value").Value.ToString()))
                                {
                                    string datakey = sourceElement.GetAttributeReference("value").Value.ToString();
                                    list.Add(datakey);
                                }
                            }
                        }
                    }
                }
            }
            return list;
        }
		public static PersonalizationCrawler.PartInfo PartInfo(string app, string screen, string part)
		{
			if (null == BuildManager.Instance.Container)
			{
				throw new InvalidOperationException("Storage container not set");
			}
			string bin = ConfigurationManager.AppSettings["EDIT_REPO"];
			if (string.IsNullOrEmpty(bin))
			{
				throw new ApplicationException("'EDIT_REPO' web.config key not set");
			}
			string portfolio = ConfigurationManager.AppSettings["EDIT_REPO_PORTFOLIO"];
			if (string.IsNullOrEmpty(portfolio))
			{
				throw new InvalidOperationException("EDIT_REPO_PORTFOLIO key not set in config file");
			}
			IStorageContainer portfolioContainer = BuildManager.Instance.Container.CreateContainer(bin).CreateContainer(app).CreateContainer(portfolio);
            string cachedkey = app + "." + screen + "..crawler";
            PersonalizationCrawler crawler = GetCrawlerFromCache(cachedkey);
            if (null == crawler)
            {
                string fileToMonitor = Path.Combine(portfolioContainer.ToString(), screen + ".xml");
                crawler = new PersonalizationCrawler(portfolioContainer.Agent, screen + ".xml");
                AddCrawlerToCache(cachedkey, crawler, fileToMonitor);
            }
            PersonalizationCrawler.PartInfo result;
			foreach (PersonalizationCrawler.PartInfo partInfo in crawler.Parts)
			{
				if (partInfo.Id == part)
				{
					result = partInfo;
					return result;
				}
			}
			result = null;
			return result;
		}
		public static List<string> ListConnections()
		{
			string file = ConfigurationManager.AppSettings["CONNECTIONS_FILE"];
			if (string.IsNullOrEmpty(file))
			{
				throw new ArgumentNullException("CONNECTIONS_FILE config not set");
			}
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(BuildManager.Instance.Container.Agent.Read(file));
			XmlNode rootNode = doc.SelectSingleNode("connections");
			if (null == rootNode)
			{
				throw new InvalidOperationException(string.Format("File '{0}' has no 'connections' node", file));
			}
			List<string> list = new List<string>();
			foreach (XmlNode connNode in rootNode.ChildNodes)
			{
				list.Add(connNode.Attributes["key"].Value);
			}
			return list;
        }

        private static IConfiguration GetConfigurationFromCache(string key)
        {
            Cache cache = HttpRuntime.Cache;
            return cache[key] as IConfiguration;
        }

        private static void AddConfigurationToCache(string key, IConfiguration config, string fileToMonitor)
        {
            if (null == config)
                return;

            Cache cache = HttpRuntime.Cache;
            lock (SyncRoot)
            {
                cache.Remove(key);
                cache.Insert(key, config, new CacheDependency(fileToMonitor));
            }
        }

        private static PersonalizationCrawler GetCrawlerFromCache(string key)
        {
            Cache cache = HttpRuntime.Cache;
            return cache[key] as PersonalizationCrawler;
        }

        private static void AddCrawlerToCache(string key, PersonalizationCrawler crawler, string fileToMonitor)
        {
            if (null == crawler)
                return;

            Cache cache = HttpRuntime.Cache;
            lock (SyncRoot)
            {
                cache.Remove(key);
                cache.Insert(key, crawler, new CacheDependency(fileToMonitor));
            }
        }
	}
}

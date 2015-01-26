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
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Collections.Generic;

using LWAS.Extensible.Interfaces.Storage;

namespace LWAS.CustomControls
{
    public class Breadcrumb : CompositeControl
    {
        public class ErrorEventArgs : EventArgs
        {
            public Exception Exception { get; set; }
        }

        static object SyncRoot = new object();
        Cache cache;

        UpdatePanelDynamic updatePanel;
        Panel container;
        HiddenField pathHidden;
        public IStorageAgent Agent { get; set; }
        public event EventHandler<ErrorEventArgs> Error;
        public event EventHandler Displayed;
        public event EventHandler Changed;

        public string Key { get; set; }
        public string Separator { get; set; }
        public string Path
        {
            get { return pathHidden.Value; }
            set { pathHidden.Value = value; }
        }
        public bool ShowRoot { get; set; }
        public bool IgnoreRoot { get; set; }

        public XDocument Source { get; set; }

        public Breadcrumb()
        {
            cache = HttpRuntime.Cache;
            this.ShowRoot = true;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.Page.Load += new EventHandler(Page_Load);
            this.Page.LoadComplete += new EventHandler(Page_LoadComplete);

            EnsureChildControls();
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            updatePanel = new UpdatePanelDynamic();
            updatePanel.ID = "updatePanel";
            this.Controls.Add(updatePanel);

            pathHidden = new HiddenField();
            pathHidden.ID = "pathHidden";
            updatePanel.ContentTemplateContainer.Controls.Add(pathHidden);

            container = new Panel();
            container.ID = "container";
            container.CssClass = "breadcrumb_container";
            updatePanel.ContentTemplateContainer.Controls.Add(container);
        }

        void Page_Load(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(this.Key))
                Display();
        }

        void Page_LoadComplete(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(this.Key))
                Display();
        }

        public void Display()
        {
            try
            {
                container.Controls.Clear();
                XDocument doc = ConfigurationFromCache();

                XElement root = null;
                if (this.IgnoreRoot)
                    root = doc.Elements().First();
                else
                    root = doc.Element("root");
                
                if (null == root)
                    throw new InvalidOperationException("Breadcrumb root not found");

                if (String.IsNullOrEmpty(this.Path))
                    this.Path = "/" + root.Name;
                IEnumerable<XElement> elements = root.XPathSelectElement(this.Path)
                                                     .AncestorsAndSelf();

                foreach (XElement element in elements)
                {
                    if (null == element.Attribute("name")) throw new InvalidOperationException(String.Format("Element '{0}' does not contain attribute 'name'", element.Name.LocalName));
                    string name = element.Attribute("name").Value;

                    if ("root" == name && !this.ShowRoot)
                        continue;

                    Menu menu = new Menu()
                    {
                        ID = ToID(name),
                        DisplayMode = BulletedListDisplayMode.LinkButton,
                        Label = name,
                        Argument = ToXPath(element),
                        ActiveLabel = true
                    };

                    foreach (XElement sibling in element.ElementsBeforeSelf())
                    {
                        if (null == sibling.Attribute("name")) throw new InvalidOperationException(String.Format("Element '{0}' does not contain attribute 'name'", sibling.Name.LocalName));
                        ListItem li = new ListItem(sibling.Attribute("name").Value, ToXPath(sibling));
                        menu.Commands.Add(li);
                    }
                    foreach (XElement sibling in element.ElementsAfterSelf())
                    {
                        if (null == sibling.Attribute("name")) throw new InvalidOperationException(String.Format("Element '{0}' does not contain attribute 'name'", sibling.Name.LocalName));
                        ListItem li = new ListItem(sibling.Attribute("name").Value, ToXPath(sibling));
                        menu.Commands.Add(li);
                    }

                    menu.MenuClick += (s, e) =>
                        {
                            this.Path = e.CommandValue;
                            if (null != this.Changed)
                                Changed(this, new EventArgs());
                        };

                    container.Controls.AddAt(0, menu);

                    if (root == element)
                        break;

                    container.Controls
                             .AddAt(0, new Label()
                    {
                        Text = this.Separator,
                        CssClass = "breadcrumb_separator"
                    });
                }
                if (null != this.Displayed)
                    Displayed(this, null);
            }
            catch (Exception ex)
            {
                if (null != this.Error)
                    Error(this, new ErrorEventArgs() { Exception = ex });
                else
                    throw ex;
            }

        }

        XDocument ConfigurationFromCache()
        {
            if (null != this.Source)
                return this.Source;

            if (null == this.Agent) throw new InvalidOperationException("Agent not set");

            lock (SyncRoot)
            {
                this.Source = cache[this.Key] as XDocument;
                if (null == this.Source)
                {
                    this.Source = XDocument.Parse(this.Agent.Read(this.Key));
                    ConfigurationToCache(this.Source);
                }
                return this.Source;
            }
        }

        void ConfigurationToCache(XDocument doc)
        {
            cache.Remove(this.Key);
            cache.Insert(this.Key, doc, new CacheDependency(this.Key));
        }

        string ToXPath(XElement element)
        {
            if (element == null) throw new ArgumentNullException("element");

            var ancestors = from e in element.Ancestors()
                            select "/*[@name='" + e.Attribute("name").Value + "']";

            return string.Concat(ancestors.Reverse().ToArray()) + "/*[@name='" + element.Attribute("name").Value + "']";
        }

        string ToID(string name)
        {
            return name.Replace(" ", "-");
        }
    }
}

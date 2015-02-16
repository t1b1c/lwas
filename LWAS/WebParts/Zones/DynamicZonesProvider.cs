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
using System.Web;
using System.Web.UI;
using System.Web.Caching;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml;

using LWAS.Extensible.Interfaces;
using LWAS.Extensible.Interfaces.Storage;
using LWAS.Extensible.Interfaces.Routing;

using LWAS.Infrastructure;

namespace LWAS.WebParts.Zones
{
	public class DynamicZonesProvider : IZonesProvider
    {
        public class ZoneInfo
        {
            public string ParentID { get; set; }
        }

        static object SyncRoot = new object();
        Cache cache = HttpRuntime.Cache;
        string configFile;
        string screenKey;

        public Page Page { get; set; }
        public IStorageAgent Agent { get; set; }

        public IDictionary<string, IList<WebPartZone>> Zones
        {
            get 
            {
                lock (SyncRoot)
                {
                    IDictionary<string, IList<WebPartZone>> zones = cache[configFile] as IDictionary<string, IList<WebPartZone>>;
                    if (null == zones)
                        zones = LoadConfig();
                    return zones;
                }
            }
        }

        public Dictionary<WebPartZone, ZoneInfo> Extras = new Dictionary<WebPartZone, ZoneInfo>();

        string Screen
        {
            get { return this.Page.Request.QueryString[screenKey]; }
        }

        HtmlForm _form;
        HtmlForm TheForm
        {
            get
            {
                if (null == _form)
                    _form = FindTheForm(this.Page);
                return _form;
            }
        }

        HtmlForm FindTheForm(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
                if (ctrl is HtmlForm)
                    return ctrl as HtmlForm;

            foreach (Control child in parent.Controls)
            {
                HtmlForm test = FindTheForm(child);
                if (null != test)
                    return test;
            }

            return null;
        }

        Manager _manager;
        Manager Manager
        {
            get
            {
                if (null == _manager)
                    _manager = WebPartManager.GetCurrentWebPartManager(this.Page) as Manager;
                return _manager;
            }
        }

        private IDictionary<string, IList<WebPartZone>> LoadConfig()
        {
            if (null == this.Agent) throw new InvalidOperationException("Agent not set");

            Dictionary<string, IList<WebPartZone>> zones = new Dictionary<string, IList<WebPartZone>>();
            XmlDocument doc = new XmlDocument();
            if (!this.Agent.HasKey(configFile))
                CreateEmptyConfig();
            doc.LoadXml(this.Agent.Read(configFile));
            XmlNode rootNode = doc.SelectSingleNode("zones");
            if (null == rootNode) throw new ApplicationException(String.Format("The config file '{0}' has no zones element", configFile));
            foreach (XmlNode node in rootNode.ChildNodes)
            {
                if (null == node.Attributes["id"]) throw new ApplicationException("Missing attribute 'id' in zone's configuration element");
                if (null == node.Attributes["title"]) throw new ApplicationException("Missing attribute 'title' in zone's configuration element");
                if (null == node.Attributes["screen"]) throw new ApplicationException("Missing attribute 'screen' in zone's configuration element");
                if (null == node.Attributes["type"]) throw new ApplicationException("Missing attribute 'type' in zone's configuration element");

                string id = node.Attributes["id"].Value;
                string title = node.Attributes["title"].Value;
                string screen = node.Attributes["screen"].Value;
                string type = node.Attributes["type"].Value;
                WebPartZone zone = ReflectionServices.CreateInstance(type) as WebPartZone;
                if (null == zone) throw new ApplicationException(String.Format("Failed to create WebPartZone for zone '{0}' from type '{1}'", id, type));

                zone.ID = id;
                zone.HeaderText = title;
                if (!zones.ContainsKey(screen))
                    zones.Add(screen, new List<WebPartZone>());
                zones[screen].Add(zone);

                if (!this.Extras.ContainsKey(zone))
                    this.Extras.Add(zone, new ZoneInfo());
                ZoneInfo zoneInfo = this.Extras[zone];
                foreach (XmlNode extraNode in node.ChildNodes)
                {
                    if ("parent" == extraNode.Name && null != extraNode.Attributes["id"])
                        zoneInfo.ParentID = extraNode.Attributes["id"].Value;
                }
            }

            cache.Remove(configFile);
            cache.Insert(configFile, zones, new CacheDependency(configFile));

            return zones;
        }

        private void CreateEmptyConfig()
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", null, null));
            doc.AppendChild(doc.CreateElement("zones"));
            try
            {
                this.Agent.Write(configFile, "");
                using (XmlTextWriter writer = new XmlTextWriter(this.Agent.OpenStream(configFile), null))
                {
                    writer.Formatting = Formatting.Indented;
                    doc.WriteTo(writer);
                }
            }
            finally
            {
                this.Agent.CloseStream(configFile);
            }
        }

        public void RegisterZones()
        {
            configFile = this.Manager.RoutingManager.SettingsRoutes["ZONES_CONFIG"].Path;
            if (String.IsNullOrEmpty(configFile)) throw new ApplicationException("ZONES_CONFIG not defined in config file");
            screenKey = this.Manager.RoutingManager.SettingsRoutes["SCREEN"].OriginalPath;
            if (String.IsNullOrEmpty(screenKey)) throw new ApplicationException("SCREEN not defined in config file");
            if (null == this.Page) throw new InvalidOperationException("Page not set");

            string screen = this.Screen;
            if (String.IsNullOrEmpty(screen) || !this.Zones.ContainsKey(screen))
                return;

            foreach (WebPartZone zone in this.Zones[screen])
            {
                string parentid = this.Extras[zone].ParentID;
                if (!String.IsNullOrEmpty(parentid))
                {
                    Control parent = ReflectionServices.FindControlEx(parentid, this.TheForm);
                    if (null != parent)
                    {
                        parent.Controls.Add(zone);
                        continue;
                    }
                }
                this.TheForm.Controls.Add(zone);
            }
        }

    }
}

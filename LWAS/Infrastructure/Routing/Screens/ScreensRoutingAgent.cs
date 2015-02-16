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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Xml;
using System.Xml.Linq;

using LWAS.Extensible.Interfaces.Routing;
using LWAS.Extensible.Interfaces.Storage;

using LWAS.Infrastructure.Storage;

namespace LWAS.Infrastructure.Routing.Screens
{
    public class ScreensRoutingAgent : BaseAgent
    {
        public IStorageAgent Agent { get; set; }

        public ScreensRoutingAgent()
        {
            this.Agent = new FileAgent();
        }

        public override void Load(IRoutingManager manager)
        {
            if (null == manager) throw new ArgumentNullException("manager");
            if (null == this.Agent) throw new ApplicationException("Storage agent not set");
            string routes_config = manager.SettingsRoutes["ROUTES_CONFIG"].Path;
            if (String.IsNullOrEmpty(routes_config)) throw new ApplicationException("ROUTES_CONFIG not set");

            if (!this.Agent.HasKey(routes_config))
                CreateEmptyConfigFile(routes_config);

            XDocument doc = XDocument.Parse(this.Agent.Read(routes_config));

            XElement routesElement = doc.Element("routes");
            if (null != routesElement)
            {
                XAttribute appRouteAttribute = routesElement.Attribute("name");
                if (null != appRouteAttribute && !String.IsNullOrEmpty(appRouteAttribute.Value))
                {
                    string name = appRouteAttribute.Value;
                    manager.ApplicationsRoutes.Add(new ApplicationRoute(name));
                }
            }

            IEnumerable<XElement> leaves = doc.Descendants()
                                              .Where(e => !e.HasElements && e.Name != "routes");
            foreach (XElement element in leaves)
            {
                string name = element.Attribute("name").Value;
                string path = GetPath(element);
                manager.ScreensRoutes.Add(new ScreenRoute(name, path));
            }
        }

        string GetPath(XElement element)
        {
            var parents = new List<string>();
            var node = element;
            while (node != null && "routes" != node.Name)
            {
                parents.Add(node.Attribute("name").Value.ToString());
                node = node.Parent;
            }
            parents.Reverse();
            return string.Join("/", parents.ToArray<string>());
        }

        public override void Save(IRoute route)
        {
            throw new NotImplementedException();
        }

        void CreateEmptyConfigFile(string file)
        {
            XDocument doc = new XDocument(new XDeclaration("1.0", null, null),
                                          new XElement("routes",
                                                new XAttribute("name", "")
                                            )
                                         );
            try
            {
                this.Agent.Write(file, "");
                using (XmlTextWriter writer = new XmlTextWriter(this.Agent.OpenStream(file), null))
                {
                    writer.Formatting = Formatting.Indented;
                    doc.WriteTo(writer);
                }
            }
            finally
            {
                this.Agent.CloseStream(file);
            }
        }
    }
}

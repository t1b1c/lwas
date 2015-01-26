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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.IO;

using LWAS.Extensible.Interfaces.Storage;
using LWAS.Extensible.Interfaces.Expressions;

namespace LWAS.Database
{
    public class ViewsManager
    {
        string configFile;
        IStorageAgent agent;

        public ViewsCollection Views { get; set; }
        public TablesCollection Tables { get; set; }
        public RelationsCollection Relations { get; set; }
        public IExpressionsManager ExpressionsManager { get; set; }

        public ViewsManager(string aConfigFile, IStorageAgent anAgent, IExpressionsManager expressionsManager)
        {
            if (String.IsNullOrEmpty(aConfigFile)) throw new ArgumentNullException("aConfigFile");
            if (null == anAgent) throw new ArgumentNullException("anAgent");
            if (null == expressionsManager) throw new ArgumentNullException("expressionsManager");

            configFile = aConfigFile;
            agent = anAgent;
            this.ExpressionsManager = expressionsManager;

            this.Views = new ViewsCollection(this);
            this.Tables = new TablesCollection(this);
            this.Relations = new RelationsCollection(this);

            LoadConfiguration();
        }

        public ViewsManager(XmlReader reader, IExpressionsManager expressionsManager)
        {
            if (null == expressionsManager) throw new ArgumentNullException("expressionsManager");
            if (null == reader) throw new ArgumentNullException("reader");

            this.ExpressionsManager = expressionsManager;

            this.Views = new ViewsCollection(this);
            this.Tables = new TablesCollection(this);
            this.Relations = new RelationsCollection(this);

            XElement root = XDocument.ReadFrom(reader) as XElement;
            if (null == root) throw new ArgumentException("Failed to load the root element");
            FromXml(root);
        }

        public void LoadConfiguration()
        {
            if (!agent.HasKey(configFile))
                CreateEmptyConfiguration();

            XDocument doc = XDocument.Parse(agent.Read(configFile));
            XElement root = doc.Element("config");
            FromXml(root);
        }

        private void CreateEmptyConfiguration()
        {
            XDocument doc = new XDocument(new XDeclaration("1.0", null, null),
                                          new XElement("config",
                                              new XElement("views"),
                                              new XElement("tables"),
                                              new XElement("relations")
                                              )
                                         );
            try
            {
                agent.Write(configFile, "");
                using (XmlTextWriter writer = new XmlTextWriter(agent.OpenStream(configFile), null))
                {
                    writer.Formatting = Formatting.Indented;
                    doc.WriteTo(writer);
                }
            }
            finally
            {
                agent.CloseStream(configFile);
            }
        }

        public void SaveConfiguration()
        {
            agent.Erase(configFile);
            using (XmlTextWriter writer = new XmlTextWriter(agent.OpenStream(configFile), Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                ToXml(writer);
            }
        }

        public void ToXml(XmlTextWriter writer)
        {
            if (null == writer) throw new ArgumentNullException("writer");
            
            writer.WriteStartElement("config");

            this.Views.ToXml(writer);
            this.Tables.ToXml(writer);
            this.Relations.ToXml(writer);

            writer.WriteEndElement();   // config
        }

        public void FromXml(XElement element)
        {
            if (null == element) throw new ArgumentNullException("element");

            this.Views.Clear();
            this.Tables.Clear();
            this.Relations.Clear();

            this.Tables.FromXml(element.Element("tables"));
            this.Relations.FromXml(element.Element("relations"));
            this.Views.FromXml(element.Element("views"));
        }

        public void ToSql(StringBuilder builder)
        {
            if (null == builder) throw new ArgumentNullException("builder");
        }
    }
}
